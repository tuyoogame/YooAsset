using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// Web服务端文件系统，管理 WebServer 平台的文件系统访问。
    /// </summary>
    internal class WebServerFileSystem : IFileSystem
    {
        /// <summary>
        /// Web 文件路径映射表
        /// </summary>
        protected readonly Dictionary<string, string> _webFilePathMapping = new Dictionary<string, string>(10000);

        /// <summary>
        /// 包裹根目录路径
        /// </summary>
        protected string _packageRoot = string.Empty;

        /// <summary>
        /// Web Bundle 缓存系统
        /// </summary>
        public IBundleCache BundleCache { get; private set; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; private set; }

        /// <summary>
        /// 平台策略
        /// </summary>
        public IWebPlatformStrategy PlatformStrategy { get; private set; }

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
        /// 自定义参数：下载任务的看门狗机制超时时间
        /// </summary>
        public int DownloadWatchdogTimeout { get; private set; } = 0;

        /// <summary>
        /// 自定义参数：下载的资源包数据的校验级别
        /// </summary>
        public EFileVerifyLevel DownloadVerifyLevel { get; private set; } = EFileVerifyLevel.Middle;

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

        /// <summary>
        /// 创建实例
        /// </summary>
        public WebServerFileSystem()
        {
        }
        /// <inheritdoc />
        public FSInitializeOperation InitializeAsync()
        {
            var operation = new WSFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new WSFSRequestPackageVersionOperation(this, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new WSFSLoadPackageManifestOperation(this, options.PackageVersion, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new WSFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options)
        {
            var operation = new FSEnsurePackageBundleFailureOperation($"{nameof(WebServerFileSystem)} does not support ensure bundle file operation.");
            return operation;
        }
        /// <inheritdoc />
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var operation = new FSDownloadBundleCompleteOperation($"{nameof(WebServerFileSystem)} does not support download operation.");
            return operation;
        }
        /// <inheritdoc />
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            var operation = new FSClearCacheCompleteOperation();
            return operation;
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
            else if (paramName == nameof(EFileSystemParameter.DisableUnityWebCache))
            {
                DisableUnityWebCache = FileSystemHelper.CastParameter<bool>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadWatchdogTimeout))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                DownloadWatchdogTimeout = Mathf.Max(convertValue, 0);
            }
            else if (paramName == nameof(EFileSystemParameter.FileVerifyLevel))
            {
                DownloadVerifyLevel = FileSystemHelper.CastParameter<EFileVerifyLevel>(paramName, value);
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
                _packageRoot = GetDefaultWebPackageRoot(packageName);
            else
                _packageRoot = packageRoot;

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

            // 创建Web文件缓存系统
            var cacheConfig = new WebServerBundleCache.Configuration(
                watchdogTimeout: DownloadWatchdogTimeout,
                disableUnityWebCache: DisableUnityWebCache,
                downloadVerifyLevel: DownloadVerifyLevel,
                assetBundleDecryptor: AssetBundleDecryptor,
                rawBundleDecryptor: RawBundleDecryptor,
                archiveBundleDecryptor: ArchiveBundleDecryptor,
                platformStrategy: PlatformStrategy,
                downloadBackend: DownloadBackend,
                downloadRetryPolicy: DownloadRetryPolicy,
                downloadUrlPolicy: DownloadUrlPolicy);
            BundleCache = new WebServerBundleCache(packageName, _packageRoot, cacheConfig);
        }
        /// <inheritdoc />
        public void OnDestroy()
        {
            if (BundleCache != null)
            {
                BundleCache.Dispose();
                BundleCache = null;
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
            return BundleCache.IsCached(bundle.BundleGuid);
        }
        /// <inheritdoc />
        public bool IsDownloadRequired(PackageBundle bundle)
        {
            return false;
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
        protected string GetDefaultWebPackageRoot(string packageName)
        {
            string rootDirectory = YooAssetConfiguration.GetDefaultBuiltinRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }
        /// <summary>
        /// 获取Web包裹版本文件路径
        /// </summary>
        public string GetWebPackageVersionFilePath()
        {
            string fileName = YooAssetConfiguration.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取Web包裹哈希文件路径
        /// </summary>
        public string GetWebPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取Web包裹清单文件路径
        /// </summary>
        public string GetWebPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        #endregion
    }
}
