using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace YooAsset
{
    internal abstract class ProviderOperation : AsyncOperationBase
    {
        protected enum ESteps
        {
            None = 0,
            StartBundleLoader,
            WaitBundleLoader,
            ProcessBundleResult,
            Done,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderGUID { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { private set; get; }

        /// <summary>
        /// 获取的资源对象
        /// </summary>
        public UnityEngine.Object AssetObject { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] SubAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

        /// <summary>
        /// 获取的资源包对象
        /// </summary>
        public BundleResult BundleResultObject { protected set; get; }

        /// <summary>
        /// 加载的场景名称
        /// </summary>
        public string SceneName { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;


        private ESteps _steps = ESteps.None;
        protected readonly ResourceManager _resManager;
        private readonly LoadBundleFileOperation _mainBundleLoader;
        private readonly List<LoadBundleFileOperation> _bundleLoaders = new List<LoadBundleFileOperation>(10);
        private readonly HashSet<HandleBase> _handles = new HashSet<HandleBase>();

        public ProviderOperation(ResourceManager manager, string providerGUID, AssetInfo assetInfo)
        {
            _resManager = manager;
            ProviderGUID = providerGUID;
            MainAssetInfo = assetInfo;

            if (string.IsNullOrEmpty(providerGUID) == false)
            {
                // 主资源包加载器
                _mainBundleLoader = manager.CreateMainBundleFileLoader(assetInfo);
                _mainBundleLoader.AddProvider(this);
                _bundleLoaders.Add(_mainBundleLoader);

                // 依赖资源包加载器集合
                var dependLoaders = manager.CreateDependBundleFileLoaders(assetInfo);
                if (dependLoaders.Count > 0)
                    _bundleLoaders.AddRange(dependLoaders);

                // 增加引用计数
                foreach (var bundleLoader in _bundleLoaders)
                {
                    bundleLoader.Reference();
                }
            }
        }
        internal override void InternalStart()
        {
            _steps = ESteps.StartBundleLoader;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

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
                if (IsWaitForAsyncComplete)
                {
                    foreach (var bundleLoader in _bundleLoaders)
                    {
                        bundleLoader.WaitForAsyncComplete();
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

                    if (bundleLoader.Status != EOperationStatus.Succeed)
                    {
                        InvokeCompletion(bundleLoader.Error, EOperationStatus.Failed);
                        return;
                    }
                }

                // 检测加载结果
                BundleResultObject = _mainBundleLoader.Result;
                if (BundleResultObject == null)
                {
                    string error = $"Loaded bundle result is null !";
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.ProcessBundleResult;
            }

            if (_steps == ESteps.ProcessBundleResult)
            {
                ProcessBundleResult();
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
            return $"AssetPath : {MainAssetInfo.AssetPath}";
        }
        protected abstract void ProcessBundleResult();

        /// <summary>
        /// 销毁资源提供者
        /// </summary>
        public void DestroyProvider()
        {
            IsDestroyed = true;

            // 检测是否为正常销毁
            if (IsDone == false)
            {
                Error = "User abort !";
                Status = EOperationStatus.Failed;
            }

            // 减少引用计数
            foreach (var bundleLoader in _bundleLoaders)
            {
                bundleLoader.Release();
            }
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroyProvider()
        {
            // 注意：在进行资源加载过程时不可以销毁
            if (_steps == ESteps.ProcessBundleResult)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 创建资源句柄
        /// </summary>
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
        public void ReleaseHandle(HandleBase handle)
        {
            if (RefCount <= 0)
                throw new System.Exception("Should never get here !");

            if (_handles.Remove(handle) == false)
                throw new System.Exception("Should never get here !");

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 释放所有资源句柄
        /// </summary>
        public void ReleaseAllHandles()
        {
            List<HandleBase> tempers = _handles.ToList();
            foreach (var handle in tempers)
            {
                handle.Release();
            }
        }

        /// <summary>
        /// 结束流程
        /// </summary>
        protected void InvokeCompletion(string error, EOperationStatus status)
        {
            _steps = ESteps.Done;
            Error = error;
            Status = status;

            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            // 注意：回调方法如果发生异常，会阻断列表里的后续回调方法！
            List<HandleBase> tempers = _handles.ToList();
            foreach (var hande in tempers)
            {
                if (hande.IsValid)
                {
                    hande.InvokeCallback();
                }
            }
        }

        /// <summary>
        /// 获取下载报告
        /// </summary>
        public DownloadStatus GetDownloadStatus()
        {
            DownloadStatus status = new DownloadStatus();
            foreach (var bundleLoader in _bundleLoaders)
            {
                status.TotalBytes += bundleLoader.LoadBundleInfo.Bundle.FileSize;
                status.DownloadedBytes += bundleLoader.DownloadedBytes;
            }

            if (status.TotalBytes == 0)
                throw new System.Exception("Should never get here !");

            status.IsDone = status.DownloadedBytes == status.TotalBytes;
            status.Progress = (float)status.DownloadedBytes / status.TotalBytes;
            return status;
        }

        #region 调试信息
        /// <summary>
        /// 出生的场景
        /// </summary>
        public string SpawnScene = string.Empty;

        [Conditional("DEBUG")]
        public void InitProviderDebugInfo()
        {
            SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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