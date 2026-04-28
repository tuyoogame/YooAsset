using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 资源包加载操作
    /// </summary>
    internal sealed class LoadBundleOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckConcurrency,
            LoadBundleFile,
            Done,
        }

        private readonly ResourceManager _resourceManager;
        private readonly List<ProviderBase> _providers = new List<ProviderBase>(100);
        private readonly List<ProviderBase> _removeList = new List<ProviderBase>(100);
        private FSLoadPackageBundleOperation _loadPackageBundleOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 资源包文件信息
        /// </summary>
        public BundleInfo LoadBundleInfo { private set; get; }

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;

        /// <summary>
        /// 是否已收到强制销毁请求
        /// </summary>
        public bool ForceDestroyRequested { private set; get; } = false;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 资源包句柄
        /// </summary>
        public IBundleHandle BundleHandle { set; get; }

        internal LoadBundleOperation(ResourceManager resourceManager, BundleInfo bundleInfo)
        {
            _resourceManager = resourceManager;
            LoadBundleInfo = bundleInfo;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckConcurrency;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (ForceDestroyRequested)
            {
                if (_steps == ESteps.CheckConcurrency)
                {
                    _steps = ESteps.Done;
                    SetError("Bundle loader force destroyed during package destruction.");
                    return;
                }

                if (_steps == ESteps.LoadBundleFile)
                {
                    // 注意：终止下载器
                    if (_loadPackageBundleOp != null)
                        _loadPackageBundleOp.ShouldAbortDownload = true;
                }

                // 注意：其它条件的情况下，继续往下走，等底层操作自然退出。
            }

            if (_steps == ESteps.CheckConcurrency)
            {
                if (IsWaitForCompletion)
                {
                    _steps = ESteps.LoadBundleFile;
                }
                else
                {
                    if (_resourceManager.IsBundleLoadingBusy())
                        return;
                    _steps = ESteps.LoadBundleFile;
                }
            }

            if (_steps == ESteps.LoadBundleFile)
            {
                if (_loadPackageBundleOp == null)
                {
                    // 统计计数增加
                    _resourceManager.IncrementBundleLoadingCounter();
                    _loadPackageBundleOp = LoadBundleInfo.CreateBundleLoader();
                    _loadPackageBundleOp.StartOperation();
                    AddChildOperation(_loadPackageBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadPackageBundleOp.WaitForCompletion();

                _loadPackageBundleOp.UpdateOperation();
                if (_loadPackageBundleOp.IsDone == false)
                    return;

                if (_loadPackageBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadPackageBundleOp.BundleHandle == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"Bundle loaded successfully but the handle is null. Bundle: '{LoadBundleInfo.Bundle.BundleName}'.");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        BundleHandle = _loadPackageBundleOp.BundleHandle;
                        SetResult();
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadPackageBundleOp.Error);
                }

                // 统计计数减少
                _resourceManager.DecrementBundleLoadingCounter();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
        protected override string InternalGetDescription()
        {
            return $"BundleName: {LoadBundleInfo.Bundle.BundleName}";
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 销毁资源包加载器并释放资源包
        /// </summary>
        /// <remarks>
        /// 该方法是幂等的，重复调用不会重复释放资源。
        /// </remarks>
        public void DestroyLoader()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            // 注意：正在加载中的任务不可以销毁
            if (_steps == ESteps.LoadBundleFile)
                throw new YooInternalException($"Cannot destroy loader while loading bundle: '{LoadBundleInfo.Bundle.BundleName}'.");

            if (RefCount > 0)
                throw new YooInternalException($"Cannot destroy loader with non-zero ref count {RefCount}: '{LoadBundleInfo.Bundle.BundleName}'.");

            if (BundleHandle != null)
                BundleHandle.UnloadBundle();

            if (IsDone == false)
            {
                _steps = ESteps.Done;
                SetError("Bundle loader destroyed.");
            }
        }

        /// <summary>
        /// 强制销毁资源包加载器
        /// </summary>
        /// <remarks>
        /// 该方法是幂等的，仅用于全局 Destroy 场景，重复调用不会重复释放资源。
        /// </remarks>
        public void ForceDestroyLoader()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            if (_steps == ESteps.LoadBundleFile)
            {
                _loadPackageBundleOp.WaitForCompletion();
                if (_loadPackageBundleOp.Status == EOperationStatus.Succeeded)
                    BundleHandle = _loadPackageBundleOp.BundleHandle;
            }

            if (BundleHandle != null)
                BundleHandle.UnloadBundle();

            if (IsDone == false)
            {
                _steps = ESteps.Done;
                SetError("Bundle loader destroyed.");
            }
        }

        /// <summary>
        /// 请求强制销毁
        /// </summary>
        public void RequestForceDestroy()
        {
            ForceDestroyRequested = true;
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        /// <returns>如果可以安全销毁则返回 true</returns>
        public bool CanDestroyLoader()
        {
            if (IsReleasable() == false)
                return false;

            // YOOASSET_LEGACY_DEPENDENCY
            // 检查引用链上的资源包是否已经全部销毁
            // 注意：互相引用的资源包无法卸载！
            if (LoadBundleInfo.Bundle.ReferrerBundleIDs.Count > 0)
            {
                foreach (var bundleID in LoadBundleInfo.Bundle.ReferrerBundleIDs)
                {
#if YOOASSET_EXPERIMENTAL
                    if (_resourceManager.CheckBundleReleasable(bundleID) == false)
                        return false;
#else
                    if (_resourceManager.CheckBundleDestroyed(bundleID) == false)
                        return false;
#endif
                }
            }

            return true;
        }

        /// <summary>
        /// 是否可以释放
        /// </summary>
        /// <returns>如果引用计数为零且未在加载中则返回 true</returns>
        public bool IsReleasable()
        {
            // 注意：正在加载中的任务不可以销毁
            if (_steps == ESteps.LoadBundleFile)
                return false;

            if (RefCount > 0)
                return false;

            return true;
        }

        /// <summary>
        /// 添加附属的资源提供者
        /// </summary>
        /// <param name="provider">要关联的资源提供者</param>
        public void AddProvider(ProviderBase provider)
        {
            if (_providers.Contains(provider) == false)
                _providers.Add(provider);
        }

        /// <summary>
        /// 尝试销毁资源提供者
        /// </summary>
        public void TryDestroyProviders()
        {
            // 获取移除列表
            _removeList.Clear();
            foreach (var provider in _providers)
            {
                if (provider.CanDestroyProvider())
                {
                    _removeList.Add(provider);
                }
            }

            // 销毁资源提供者
            foreach (var provider in _removeList)
            {
                _providers.Remove(provider);
                provider.DestroyProvider();
            }

            // 移除资源提供者
            if (_removeList.Count > 0)
            {
                _resourceManager.RemoveBundleProviders(_removeList);
                _removeList.Clear();
            }
        }

    }
}