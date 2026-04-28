using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace YooAsset
{
    /// <summary>
    /// 资源提供者基类，负责管理资源的加载和句柄创建。
    /// </summary>
    internal abstract class ProviderBase : AsyncOperationBase
    {
        private enum ESteps
        {
            None = 0,
            StartBundleLoader,
            WaitBundleLoader,
            ProcessBundleHandle,
            Done,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderKey { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { private set; get; }

        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object AssetObject { protected set; get; }

        /// <summary>
        /// 加载的所有资源对象
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 加载的子资源对象
        /// </summary>
        public UnityEngine.Object[] SubAssetObjects { protected set; get; }

        /// <summary>
        /// 场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

        /// <summary>
        /// 加载的资源包句柄
        /// </summary>
        public IBundleHandle LoadedBundleHandle { protected set; get; }

        /// <summary>
        /// 加载的场景名称
        /// </summary>
        public string LoadedSceneName { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;

        /// <summary>
        /// 是否已收到强制销毁请求
        /// </summary>
        public bool ForceDestroyRequested { private set; get; } = false;

        /// <summary>
        /// 加载任务是否进行中
        /// </summary>
        private bool IsLoading
        {
            get
            {
                return _steps == ESteps.WaitBundleLoader || _steps == ESteps.ProcessBundleHandle;
            }
        }

        private ESteps _steps = ESteps.None;
        protected readonly ResourceManager _resourceManager;
        private readonly LoadBundleOperation _mainBundleLoader;
        private readonly List<LoadBundleOperation> _bundleLoaders = new List<LoadBundleOperation>(10);
        private readonly HashSet<HandleBase> _handles = new HashSet<HandleBase>();

        /// <summary>
        /// 创建资源提供者实例
        /// </summary>
        /// <param name="manager">资源管理器</param>
        /// <param name="providerKey">提供者唯一标识符</param>
        /// <param name="assetInfo">资源信息</param>
        public ProviderBase(ResourceManager manager, string providerKey, AssetInfo assetInfo)
        {
            _resourceManager = manager;
            ProviderKey = providerKey;
            MainAssetInfo = assetInfo;

            // 注意: 以下 bundle loader 初始化不可移至 InternalStart()，
            // 否则共享 loader 的引用计数会出错，导致资源被提前卸载。
            if (string.IsNullOrEmpty(providerKey) == false)
            {
                _mainBundleLoader = manager.GetOrCreateMainBundleLoader(assetInfo);
                _mainBundleLoader.AddProvider(this);
                _bundleLoaders.Add(_mainBundleLoader);

                var dependLoaders = manager.GetOrCreateDependBundleLoaders(assetInfo);
                if (dependLoaders.Count > 0)
                    _bundleLoaders.AddRange(dependLoaders);

                // 增加引用计数
                foreach (var bundleLoader in _bundleLoaders)
                {
                    bundleLoader.Reference();
                }
            }
        }
        protected override void InternalStart()
        {
            _steps = ESteps.StartBundleLoader;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (ForceDestroyRequested)
            {
                if (IsLoading == false)
                {
                    SetFail("Provider force destroyed during package destruction.");
                    return;
                }

                // 注意：已进入加载阶段则继续等待自然完成
            }

            // 注意：未在加载中的任务可以挂起
            if (IsLoading == false)
            {
                if (RefCount <= 0)
                    return;
            }

            if (_steps == ESteps.StartBundleLoader)
            {
                foreach (var bundleLoader in _bundleLoaders)
                {
                    bundleLoader.StartOperation();
                    AddChildOperation(bundleLoader);
                }
                _steps = ESteps.WaitBundleLoader;
            }

            if (_steps == ESteps.WaitBundleLoader)
            {
                if (IsWaitForCompletion)
                {
                    foreach (var bundleLoader in _bundleLoaders)
                    {
                        bundleLoader.WaitForCompletion();
                    }
                }

                // 更新资源包加载器
                foreach (var bundleLoader in _bundleLoaders)
                {
                    bundleLoader.UpdateOperation();
                }

                // 检测加载是否完成
                foreach (var bundleLoader in _bundleLoaders)
                {
                    if (bundleLoader.IsDone == false)
                        return;

                    if (bundleLoader.Status != EOperationStatus.Succeeded)
                    {
                        SetFail(bundleLoader.Error);
                        return;
                    }
                }

                // 检测加载结果
                LoadedBundleHandle = _mainBundleLoader.BundleHandle;
                if (LoadedBundleHandle == null)
                {
                    SetFail($"Loaded bundle handle is null. Asset: '{MainAssetInfo.AssetPath}'.");
                    return;
                }

                _steps = ESteps.ProcessBundleHandle;
            }

            if (_steps == ESteps.ProcessBundleHandle)
            {
                InternalProcessBundleHandle();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
        protected override string InternalGetDescription()
        {
            return $"AssetPath: {MainAssetInfo.AssetPath}";
        }

        /// <summary>
        /// 处理资源包句柄，由子类实现具体逻辑。
        /// </summary>
        protected abstract void InternalProcessBundleHandle();

        /// <summary>
        /// 销毁资源提供者
        /// </summary>
        /// <remarks>
        /// 该方法是幂等的，重复调用不会重复释放资源。
        /// </remarks>
        public void DestroyProvider()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            // 检测是否为正常销毁
            if (IsDone == false)
            {
                _steps = ESteps.Done;
                SetError("Operation was aborted.");
            }

            // 减少引用计数
            foreach (var bundleLoader in _bundleLoaders)
            {
                bundleLoader.Release();
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
        /// <returns>如果引用计数为零且未在加载中则返回 true</returns>
        public bool CanDestroyProvider()
        {
            // 注意：正在加载中的任务不可以销毁
            if (IsLoading)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 创建资源句柄
        /// </summary>
        /// <typeparam name="T">句柄类型，必须是 HandleBase 的派生类型。</typeparam>
        /// <returns>创建的资源句柄</returns>
        public T CreateHandle<T>() where T : HandleBase
        {
            // 引用计数增加
            RefCount++;

            HandleBase handle = HandleFactory.CreateHandle(this, typeof(T));
            _handles.Add(handle);
            return handle as T;
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        /// <param name="handle">要释放的资源句柄</param>
        public void ReleaseHandle(HandleBase handle)
        {
            if (RefCount <= 0)
                throw new YooInternalException($"Attempting to release handle when RefCount is already zero. Asset: '{MainAssetInfo.AssetPath}'.");

            if (_handles.Remove(handle) == false)
                throw new YooInternalException($"Handle not found in cache list. Asset: '{MainAssetInfo.AssetPath}'.");

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 释放所有资源句柄
        /// </summary>
        public void ReleaseAllHandles()
        {
            List<HandleBase> tempHandles = _handles.ToList();
            foreach (var handle in tempHandles)
            {
                handle.Release();
            }
        }

        /// <summary>
        /// 尝试卸载资源包
        /// </summary>
        public void TryUnloadBundle()
        {
            if (_resourceManager.AutoUnloadBundleWhenUnused)
            {
                _resourceManager.TryUnloadUnusedAsset(MainAssetInfo, 10);
            }
        }

        /// <summary>
        /// 设置成功状态并触发完成回调
        /// </summary>
        protected void SetSuccess()
        {
            _steps = ESteps.Done;
            SetResult();
            InvokeCompletion();
        }

        /// <summary>
        /// 设置失败状态并触发完成回调
        /// </summary>
        /// <param name="error">错误信息</param>
        protected void SetFail(string error)
        {
            _steps = ESteps.Done;
            SetError(error);
            InvokeCompletion();
        }

        private void InvokeCompletion()
        {
            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            List<HandleBase> tempHandles = _handles.ToList();
            foreach (var handle in tempHandles)
            {
                if (handle.IsValid)
                {
                    try
                    {
                        handle.InvokeCallback();
                    }
                    catch (Exception ex)
                    {
                        YooLogger.LogError($"Exception in completion callback: {ex}.");
                    }
                }
            }
        }

        #region 调试信息
        /// <summary>
        /// 资源加载时的活跃场景名称
        /// </summary>
        public string SceneName { get; set; } = string.Empty;

        /// <summary>
        /// 初始化资源提供者的调试信息
        /// </summary>
        [Conditional("DEBUG")]
        public void InitProviderDebugInfo()
        {
            SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        internal List<string> GetDebugDependBundles()
        {
            List<string> result = new List<string>(_bundleLoaders.Count);
            foreach (var bundleLoader in _bundleLoaders)
            {
                var packageBundle = bundleLoader.LoadBundleInfo.Bundle;
                result.Add(packageBundle.BundleName);
            }
            return result;
        }
        #endregion
    }
}