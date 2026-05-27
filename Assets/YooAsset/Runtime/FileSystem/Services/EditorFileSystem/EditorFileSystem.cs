using System;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 模拟文件系统，管理编辑器模式下的模拟文件系统。
    /// </summary>
    internal class EditorFileSystem : IFileSystem
    {
        /// <summary>
        /// 包裹根目录路径
        /// </summary>
        protected string _packageRoot;

        /// <summary>
        /// 虚拟文件缓存系统
        /// </summary>
        public IBundleCache BundleCache { get; private set; }

        /// <summary>
        /// 下载调度器
        /// </summary>
        public DownloadSchedulerOperation DownloadScheduler { get; internal set; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; private set; }

        /// <inheritdoc />
        public string PackageName { get; private set; }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：UnityWebRequest 创建委托
        /// </summary>
        public UnityWebRequestCreator WebRequestCreator { get; private set; }

        /// <summary>
        /// 自定义参数：模拟WebGL平台模式
        /// </summary>
        public bool VirtualWebGLMode { get; private set; } = false;

        /// <summary>
        /// 自定义参数：模拟虚拟下载模式
        /// </summary>
        public bool VirtualDownloadMode { get; private set; } = false;

        /// <summary>
        /// 自定义参数：模拟虚拟下载的网速（单位：字节）
        /// </summary>
        /// <remarks>
        /// 默认值：1024
        /// </remarks>
        public int VirtualDownloadSpeed { get; private set; } = 1024;

        /// <summary>
        /// 自定义参数：最大并发连接数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐范围 1-32）。过大的并发数可能被服务器限流，也会增加本地资源消耗。
        /// </remarks>
        public int DownloadMaxConcurrency { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：每帧发起的最大请求数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐范围 1-32）。避免单帧发起过多请求导致卡顿。
        /// </remarks>
        public int DownloadMaxRequestsPerFrame { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：异步模拟加载最小帧数
        /// </summary>
        /// <remarks>
        /// 默认值：1
        /// </remarks>
        public int AsyncSimulateMinFrame { get; private set; } = 1;

        /// <summary>
        /// 自定义参数：异步模拟加载最大帧数
        /// </summary>
        /// <remarks>
        /// 默认值：1
        /// </remarks>
        public int AsyncSimulateMaxFrame { get; private set; } = 1;
        #endregion

        /// <summary>
        /// 创建实例
        /// </summary>
        public EditorFileSystem()
        {
        }
        /// <inheritdoc />
        public FSInitializeOperation InitializeAsync()
        {
            var operation = new EFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new EFSRequestPackageVersionOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new EFSLoadPackageManifestOperation(this, options.PackageVersion);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new EFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options)
        {
            var operation = new EFSEnsurePackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var downloader = new EFSDownloadBundleOperation(this, options);
            return downloader;
        }
        /// <inheritdoc />
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            if (options.ClearMethod == ClearCacheMethods.ClearAllManifestFiles)
            {
                var operation = new FSClearCacheCompleteOperation();
                return operation;
            }
            else if (options.ClearMethod == ClearCacheMethods.ClearUnusedManifestFiles)
            {
                var operation = new FSClearCacheCompleteOperation();
                return operation;
            }
            else
            {
                var operation = new EFSClearCacheOperation(this, options);
                return operation;
            }
        }

        /// <inheritdoc />
        public void SetParameter(string paramName, object value)
        {
            if (paramName == nameof(EFileSystemParameter.DownloadBackend))
            {
                DownloadBackend = FileSystemHelper.CastParameter<IDownloadBackend>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.UnityWebRequestCreator))
            {
                WebRequestCreator = FileSystemHelper.CastParameter<UnityWebRequestCreator>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.VirtualWebglMode))
            {
                VirtualWebGLMode = FileSystemHelper.CastParameter<bool>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.VirtualDownloadMode))
            {
                VirtualDownloadMode = FileSystemHelper.CastParameter<bool>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.VirtualDownloadSpeed))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                VirtualDownloadSpeed = Mathf.Clamp(convertValue, 1, int.MaxValue);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadMaxConcurrency))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                if (convertValue > 32)
                {
                    YooLogger.LogWarning($"DOWNLOAD_MAX_CONCURRENCY value {convertValue} is too large, clamped to 32. Recommended range: 1 - 32.");
                }

                // 限制在合理范围内：1-32          
                DownloadMaxConcurrency = Mathf.Clamp(convertValue, 1, 32);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadMaxRequestPerFrame))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                if (convertValue > 32)
                {
                    YooLogger.LogWarning($"DOWNLOAD_MAX_REQUEST_PER_FRAME value {convertValue} is too large, clamped to 32. Recommended range: 1 - 32.");
                }

                // 限制在合理范围内：1-32          
                DownloadMaxRequestsPerFrame = Mathf.Clamp(convertValue, 1, 32);
            }
            else if (paramName == nameof(EFileSystemParameter.AsyncSimulateMinFrame))
            {
                AsyncSimulateMinFrame = FileSystemHelper.CastParameter<int>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.AsyncSimulateMaxFrame))
            {
                AsyncSimulateMaxFrame = FileSystemHelper.CastParameter<int>(paramName, value);
            }
            else
            {
                throw new ArgumentException($"Unrecognized parameter name: '{paramName}'.", nameof(paramName));
            }
        }
        /// <inheritdoc />
        public void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(packageRoot))
                throw new System.ArgumentException($"{nameof(EditorFileSystem)} package root is null or empty.");

            _packageRoot = packageRoot;

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend(WebRequestCreator);

            // 创建编辑器文件缓存系统
            if (AsyncSimulateMinFrame > AsyncSimulateMaxFrame)
                AsyncSimulateMinFrame = AsyncSimulateMaxFrame;
            var cacheConfig = new EditorBundleCache.Configuration(
                virtualDownloadMode: VirtualDownloadMode,
                virtualWebGLMode: VirtualWebGLMode,
                asyncSimulateMinFrame: AsyncSimulateMinFrame,
                asyncSimulateMaxFrame: AsyncSimulateMaxFrame);
            string cacheRoot = GetEditorBundleCacheRoot();
            BundleCache = new EditorBundleCache(packageName, cacheRoot, cacheConfig);
        }
        /// <inheritdoc />
        public void OnDestroy()
        {
            if (BundleCache != null)
            {
                BundleCache.Dispose();
                BundleCache = null;
            }

            if (DownloadScheduler != null)
            {
                DownloadScheduler.AbortOperation();
                DownloadScheduler = null;
            }

            if (DownloadBackend != null)
            {
                DownloadBackend.Dispose();
                DownloadBackend = null;
            }
        }

        /// <inheritdoc />
        public bool CanAcceptBundle(PackageBundle bundle)
        {
            return true;
        }
        /// <inheritdoc />
        public bool IsDownloadRequired(PackageBundle bundle)
        {
            return BundleCache.IsCached(bundle.BundleGuid) == false;
        }
        /// <inheritdoc />
        public bool IsUnpackRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public bool IsImportRequired(PackageBundle bundle)
        {
            return false;
        }

        #region 内部方法
        /// <summary>
        /// 获取编辑器缓存根目录路径
        /// </summary>
        private string GetEditorBundleCacheRoot()
        {
            string root = YooAssetConfiguration.GetEditorCacheRoot();
            return PathUtility.Combine(root, PackageName, EditorFileSystemConsts.CacheFolderName);
        }

        /// <summary>
        /// 获取编辑器包裹版本文件路径
        /// </summary>
        public string GetEditorPackageVersionFilePath()
        {
            string fileName = YooAssetConfiguration.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取编辑器包裹哈希文件路径
        /// </summary>
        public string GetEditorPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取编辑器包裹清单文件路径
        /// </summary>
        public string GetEditorPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        #endregion
    }
}
