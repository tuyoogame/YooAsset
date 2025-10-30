using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class LoadBundleFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckConcurrency,
            LoadBundleFile,
            Done,
        }

        private readonly ResourceManager _resManager;
        private readonly List<ProviderOperation> _providers = new List<ProviderOperation>(100);
        private readonly List<ProviderOperation> _removeList = new List<ProviderOperation>(100);
        private FSLoadBundleOperation _loadBundleOp;
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
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 下载进度
        /// </summary>
        public float DownloadProgress { set; get; } = 0;

        /// <summary>
        /// 下载大小
        /// </summary>
        public long DownloadedBytes { set; get; } = 0;

        /// <summary>
        /// 加载结果
        /// </summary>
        public BundleResult Result { set; get; }


        internal LoadBundleFileOperation(ResourceManager resourceManager, BundleInfo bundleInfo)
        {
            _resManager = resourceManager;
            LoadBundleInfo = bundleInfo;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckConcurrency;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckConcurrency)
            {
                if (IsWaitForAsyncComplete)
                {
                    _steps = ESteps.LoadBundleFile;
                }
                else
                {
                    if (_resManager.BundleLoadingIsBusy())
                        return;
                    _steps = ESteps.LoadBundleFile;
                }
            }

            if (_steps == ESteps.LoadBundleFile)
            {
                if (_loadBundleOp == null)
                {
                    // 统计计数增加
                    _resManager.BundleLoadingCounter++;
                    _loadBundleOp = LoadBundleInfo.LoadBundleFile();
                    _loadBundleOp.StartOperation();
                    AddChildOperation(_loadBundleOp);
                }

                if (IsWaitForAsyncComplete)
                    _loadBundleOp.WaitForAsyncComplete();

                _loadBundleOp.UpdateOperation();
                DownloadProgress = _loadBundleOp.DownloadProgress;
                DownloadedBytes = _loadBundleOp.DownloadedBytes;
                if (_loadBundleOp.IsDone == false)
                    return;

                if (_loadBundleOp.Status == EOperationStatus.Succeed)
                {
                    if (_loadBundleOp.Result == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The bundle loader result is null ! {LoadBundleInfo.Bundle.BundleName}";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Result = _loadBundleOp.Result;
                        Status = EOperationStatus.Succeed;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBundleOp.Error;
                }

                // 统计计数减少
                _resManager.BundleLoadingCounter--;
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
        internal override string InternalGetDesc()
        {
            return $"BundleName : {LoadBundleInfo.Bundle.BundleName}";
        }

        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void DestroyLoader()
        {
            IsDestroyed = true;

            // 注意：正在加载中的任务不可以销毁
            if (_steps == ESteps.LoadBundleFile)
                throw new Exception($"Bundle file loader is not done : {LoadBundleInfo.Bundle.BundleName}");

            if (RefCount > 0)
                throw new Exception($"Bundle file loader ref is not zero : {LoadBundleInfo.Bundle.BundleName}");

            if (Result != null)
                Result.UnloadBundleFile();

            if (IsDone == false)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Bundle loader destroyed !";
            }
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroyLoader()
        {
            if (CanReleasableLoader() == false)
                return false;

            // YOOASSET_LEGACY_DEPENDENCY
            // 检查引用链上的资源包是否已经全部销毁
            // 注意：互相引用的资源包无法卸载！
            if (LoadBundleInfo.Bundle.ReferenceBundleIDs.Count > 0)
            {
                foreach (var bundleID in LoadBundleInfo.Bundle.ReferenceBundleIDs)
                {
#if YOOASSET_EXPERIMENTAL
                    if (_resManager.CheckBundleReleasable(bundleID) == false)
                        return false;
#else
                    if (_resManager.CheckBundleDestroyed(bundleID) == false)
                        return false;
#endif
                }
            }

            return true;
        }

        /// <summary>
        /// 是否可以释放
        /// </summary>
        public bool CanReleasableLoader()
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
        public void AddProvider(ProviderOperation provider)
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
                _resManager.RemoveBundleProviders(_removeList);
                _removeList.Clear();
            }
        }

        /// <summary>
        /// 尝试终止加载器
        /// </summary>
        public void TryAbortLoader()
        {
            if (IsDone == false)
            {
                if (_steps == ESteps.CheckConcurrency)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Abort bundle loader !";
                }

                if (_steps == ESteps.LoadBundleFile)
                {
                    // 注意：终止下载器
                    if (_loadBundleOp != null)
                        _loadBundleOp.AbortDownloadFile = true;
                }
            }
        }
    }
}