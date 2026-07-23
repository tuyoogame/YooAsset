using System;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统，统一管理普通 Web 和 Mini Game 平台的远程资源加载。
    /// 通过 IWebPlatformStrategy 隔离不同平台的 AssetBundle 请求、提取和卸载行为。
    /// </summary>
    internal class WebNetworkFileSystem : IFileSystem
    {
        /// <summary>
        /// Web 文件缓存系统
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

        /// <summary>
        /// 平台策略
        /// </summary>
        public IWebPlatformStrategy PlatformStrategy { get; private set; }

        /// <summary>
        /// 预下载策略
        /// </summary>
        public IWebPreloadStrategy PreloadStrategy { get; private set; }

        /// <summary>
        /// 预下载冲突协调器
        /// </summary>
        internal PreloadConflictController PreloadConflict { get; private set; }

        /// <inheritdoc />
        public string PackageName { get; private set; }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：UnityWebRequest 创建委托
        /// </summary>
        public UnityWebRequestCreator WebRequestCreator { get; private set; }

        /// <summary>
        /// 自定义参数：禁用 Unity 内置网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; private set; } = false;

        /// <summary>
        /// 自定义参数：最大并发连接数
        /// </summary>
        public int DownloadMaxConcurrency { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：每帧发起的最大请求数
        /// </summary>
        public int DownloadMaxRequestsPerFrame { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：下载的资源包数据的校验级别
        /// </summary>
        public EFileVerifyLevel DownloadVerifyLevel { get; private set; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 远程服务接口
        /// </summary>
        public IRemoteService RemoteService { get; private set; }

        /// <summary>
        /// 自定义参数：AssetBundle 解密器
        /// </summary>
        public IBundleDecryptor AssetBundleDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：RawBundle 解密器
        /// </summary>
        public IBundleDecryptor RawBundleDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：ArchiveBundle 解密器
        /// </summary>
        public IBundleDecryptor ArchiveBundleDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：资源清单解密器
        /// </summary>
        public IManifestDecryptor ManifestDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：下载重试判定策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; private set; }

        /// <summary>
        /// 自定义参数：URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; private set; }
        #endregion

        public WebNetworkFileSystem()
        {
        }

        /// <inheritdoc />
        public virtual FSInitializeOperation InitializeAsync()
        {
            var operation = new WNFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new WNFSRequestPackageVersionOperation(this, options.AppendTimeTicks, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new WNFSLoadPackageManifestOperation(this, options.PackageVersion, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new WNFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options)
        {
            var operation = new FSEnsurePackageBundleFailureOperation($"{nameof(WebNetworkFileSystem)} does not support ensure bundle file operation.");
            return operation;
        }
        /// <inheritdoc />
        public virtual FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            if (PreloadStrategy == null)
            {
                var operation = new FSDownloadBundleCompleteOperation($"{nameof(WebNetworkFileSystem)} does not support download operation.");
                return operation;
            }

            return new WNFSDownloadBundleOperation(this, options);
        }
        /// <inheritdoc />
        public virtual FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            var operation = new FSClearCacheCompleteOperation();
            return operation;
        }

        /// <inheritdoc />
        public virtual void SetParameter(string paramName, object value)
        {
            if (paramName == nameof(EFileSystemParameter.DownloadBackend))
            {
                DownloadBackend = FileSystemHelper.CastParameter<IDownloadBackend>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.UnityWebRequestCreator))
            {
                WebRequestCreator = FileSystemHelper.CastParameter<UnityWebRequestCreator>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DisableUnityWebCache))
            {
                DisableUnityWebCache = FileSystemHelper.CastParameter<bool>(paramName, value);
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
            else if (paramName == nameof(EFileSystemParameter.DownloadWatchdogTimeout))
            {
                // 小游戏平台的 UnityWebRequest 可能无法返回可靠的下载字节数，因此不支持看门狗机制。
                YooLogger.LogError($"{nameof(EFileSystemParameter.DownloadWatchdogTimeout)} is not supported by {nameof(WebNetworkFileSystem)}.");
            }
            else if (paramName == nameof(EFileSystemParameter.FileVerifyLevel))
            {
                DownloadVerifyLevel = FileSystemHelper.CastParameter<EFileVerifyLevel>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.RemoteService))
            {
                RemoteService = FileSystemHelper.CastParameter<IRemoteService>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.AssetBundleDecryptor))
            {
                AssetBundleDecryptor = FileSystemHelper.CastParameter<IBundleDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.RawBundleDecryptor))
            {
                RawBundleDecryptor = FileSystemHelper.CastParameter<IBundleDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.ArchiveBundleDecryptor))
            {
                ArchiveBundleDecryptor = FileSystemHelper.CastParameter<IBundleDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.ManifestDecryptor))
            {
                ManifestDecryptor = FileSystemHelper.CastParameter<IManifestDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadRetryPolicy))
            {
                DownloadRetryPolicy = FileSystemHelper.CastParameter<IDownloadRetryPolicy>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadUrlPolicy))
            {
                DownloadUrlPolicy = FileSystemHelper.CastParameter<IDownloadUrlPolicy>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.WebPlatformStrategy))
            {
                PlatformStrategy = FileSystemHelper.CastParameter<IWebPlatformStrategy>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.WebPreloadStrategy))
            {
                PreloadStrategy = FileSystemHelper.CastParameter<IWebPreloadStrategy>(paramName, value);
            }
            else
            {
                throw new ArgumentException($"Unrecognized parameter name: '{paramName}'.", nameof(paramName));
            }
        }
        /// <inheritdoc />
        public virtual void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend(WebRequestCreator);

            // 创建默认的下载重试策略
            if (DownloadRetryPolicy == null)
                DownloadRetryPolicy = new DefaultDownloadRetryPolicy();

            // 创建默认的 URL 选择策略
            if (DownloadUrlPolicy == null)
                DownloadUrlPolicy = new DefaultDownloadUrlPolicy();

            // 创建默认的平台策略
            if (PlatformStrategy == null)
                PlatformStrategy = new DefaultWebPlatformStrategy(WebRequestCreator);

            // 创建预下载冲突协调器
            // 注意：配置了预下载策略即启用加载/预下载冲突协调，未配置时门禁空转。
            PreloadConflict = new PreloadConflictController(PreloadStrategy != null);

            // 创建文件缓存系统
            var cacheConfig = new WebNetworkBundleCache.Configuration(
                disableUnityWebCache: DisableUnityWebCache,
                downloadVerifyLevel: DownloadVerifyLevel,
                assetBundleDecryptor: AssetBundleDecryptor,
                rawBundleDecryptor: RawBundleDecryptor,
                archiveBundleDecryptor: ArchiveBundleDecryptor,
                platformStrategy: PlatformStrategy,
                remoteService: RemoteService,
                downloadBackend: DownloadBackend,
                downloadRetryPolicy: DownloadRetryPolicy,
                downloadUrlPolicy: DownloadUrlPolicy);
            BundleCache = new WebNetworkBundleCache(packageName, cacheConfig);
        }
        /// <inheritdoc />
        public virtual void OnDestroy()
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

            PreloadConflict = null;
        }

        /// <inheritdoc />
        public virtual bool CanAcceptBundle(PackageBundle bundle)
        {
            // 注意：保底加载！
            return true;
        }
        /// <inheritdoc />
        public virtual bool IsDownloadRequired(PackageBundle bundle)
        {
            // 注意：未配置预下载策略时，文件系统不支持主动下载。
            if (PreloadStrategy == null)
                return false;

            return IsBundleCached(bundle) == false;
        }
        /// <inheritdoc />
        public virtual bool IsUnpackRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public virtual bool IsImportRequired(PackageBundle bundle)
        {
            return false;
        }

        #region 内部方法
        internal bool IsBundleCached(PackageBundle bundle)
        {
            // 注意：调用方必须保证已配置预下载策略。
            var candidateUrls = RemoteService.GetRemoteUrls(bundle.GetFileName());
            var args = new WebPreloadQueryArgs(bundle, candidateUrls);
            return PreloadStrategy.IsBundleCached(args);
        }
        #endregion
    }
}
