#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - Operation 类型
// 提供 v2.3 的 InitializeParameters 类族、Operation 包装类、ImportFileInfo 和 GameAsyncOperation。

using System;

namespace YooAsset
{
    #region InitializeParameters
    /// <summary>
    /// v2.3 初始化参数基类
    /// </summary>
    [Obsolete("Use specific Options classes (e.g. EditorSimulateModeOptions) instead.")]
    public abstract class InitializeParameters
    {
        public int BundleLoadingMaxConcurrency = int.MaxValue;
        public bool AutoUnloadBundleWhenUnused = false;
        public bool WebGLForceSyncLoadAsset = false;
    }

    /// <summary>
    /// v2.3 编辑器模拟模式初始化参数
    /// </summary>
    [Obsolete("Use EditorSimulateModeOptions instead.")]
    public class EditorSimulateModeParameters : InitializeParameters
    {
        public FileSystemParameters EditorFileSystemParameters;
    }

    /// <summary>
    /// v2.3 离线运行模式初始化参数
    /// </summary>
    [Obsolete("Use OfflinePlayModeOptions instead.")]
    public class OfflinePlayModeParameters : InitializeParameters
    {
        public FileSystemParameters BuildinFileSystemParameters;
    }

    /// <summary>
    /// v2.3 联机运行模式初始化参数
    /// </summary>
    [Obsolete("Use HostPlayModeOptions instead.")]
    public class HostPlayModeParameters : InitializeParameters
    {
        public FileSystemParameters BuildinFileSystemParameters;
        public FileSystemParameters CacheFileSystemParameters;
    }

    /// <summary>
    /// v2.3 WebGL 运行模式初始化参数
    /// </summary>
    [Obsolete("Use WebPlayModeOptions instead.")]
    public class WebPlayModeParameters : InitializeParameters
    {
        public FileSystemParameters WebServerFileSystemParameters;
        public FileSystemParameters WebRemoteFileSystemParameters;
    }

    /// <summary>
    /// v2.3 自定义运行模式初始化参数
    /// </summary>
    [Obsolete("Use CustomPlayModeOptions instead.")]
    public class CustomPlayModeParameters : InitializeParameters
    {
        public readonly System.Collections.Generic.List<FileSystemParameters> FileSystemParameterList = new System.Collections.Generic.List<FileSystemParameters>();
    }
    #endregion

    #region LegacyOperationWrapper
    /// <summary>
    /// v2.3 Operation 包装基类，将 v3 的 AsyncOperationBase 转发为旧类型。
    /// </summary>
    [Obsolete("Use v3 operation types directly.")]
    public class LegacyOperationWrapper : AsyncOperationBase
    {
        private bool _isDone = false;
        protected readonly AsyncOperationBase _operation;

        internal LegacyOperationWrapper(AsyncOperationBase op)
        {
            _operation = op;
        }
        protected override void InternalStart()
        {
        }
        protected override void InternalUpdate()
        {
            if (_isDone)
                return;

            if (IsWaitForCompletion)
                _operation.WaitForCompletion();

            _operation.UpdateOperation();
            Progress = _operation.Progress;
            if (_operation.IsDone == false)
                return;

            _isDone = true;
            if (_operation.Status == EOperationStatus.Succeeded)
                SetResult();
            else
                SetError(_operation.Error);
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
    #endregion

    #region Legacy Operations
    [Obsolete("Use InitializePackageOperation instead.")]
    public class InitializationOperation : LegacyOperationWrapper
    {
        internal InitializationOperation(InitializePackageOperation op) : base(op) { }
    }

    [Obsolete("Use DestroyPackageOperation instead.")]
    public class DestroyOperation : LegacyOperationWrapper
    {
        internal DestroyOperation(DestroyPackageOperation op) : base(op) { }
    }

    [Obsolete("Use LoadPackageManifestOperation instead.")]
    public class UpdatePackageManifestOperation : LegacyOperationWrapper
    {
        internal UpdatePackageManifestOperation(LoadPackageManifestOperation op) : base(op) { }
    }

    [Obsolete("Use PrefetchManifestOperation instead.")]
    public class PreDownloadContentOperation : LegacyOperationWrapper
    {
        private readonly PrefetchManifestOperation _prefetchOp;

        internal PreDownloadContentOperation(PrefetchManifestOperation op) : base(op)
        {
            _prefetchOp = op;
        }

        [Obsolete("Use PrefetchManifestOperation.CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(downloadingMaxNumber, failedTryAgain);
            return _prefetchOp.CreateResourceDownloader(options);
        }

        [Obsolete("Use PrefetchManifestOperation.CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            string[] tags = new string[] { tag };
            var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
            return _prefetchOp.CreateResourceDownloader(options);
        }

        [Obsolete("Use PrefetchManifestOperation.CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
            return _prefetchOp.CreateResourceDownloader(options);
        }
    }
    #endregion

    #region ImportFileInfo
    /// <summary>
    /// v2.3 导入文件信息
    /// </summary>
    [Obsolete("Use ImportBundleInfo instead.")]
    public struct ImportFileInfo
    {
        public string FilePath;
        public string BundleName;
        public string BundleGUID;
    }
    #endregion

    #region GameAsyncOperation
    /// <summary>
    /// v2.3 游戏异步操作基类（v3 已移除）
    /// </summary>
    [Obsolete("GameAsyncOperation has been removed in v3. Use AsyncOperationBase directly.")]
    public abstract class GameAsyncOperation : AsyncOperationBase
    {
        protected override void InternalStart()
        {
            OnStart();
        }
        protected override void InternalUpdate()
        {
            OnUpdate();
        }
        protected override void InternalAbort()
        {
            OnAbort();
        }
        protected override void InternalWaitForCompletion()
        {
            OnWaitForAsyncComplete();
        }

        protected abstract void OnStart();
        protected abstract void OnUpdate();
        protected abstract void OnAbort();
        protected virtual void OnWaitForAsyncComplete() { }

        protected new bool IsBusy
        {
            get { return AsyncOperationSystem.IsBusy; }
        }

        protected void Abort()
        {
            AbortOperation();
        }
    }
    #endregion
}
#endif
