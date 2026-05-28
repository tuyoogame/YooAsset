using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统，管理沙盒目录下的资源文件存储与访问。
    /// </summary>
    internal class SandboxFileSystem : IFileSystem
    {
        /// <summary>
        /// 临时文件路径映射表
        /// </summary>
        protected readonly Dictionary<string, string> _tempFilePathMapping = new Dictionary<string, string>(10000);

        /// <summary>
        /// 包裹根目录路径
        /// </summary>
        protected string _packageRoot;

        /// <summary>
        /// 临时文件根目录路径
        /// </summary>
        protected string _tempFilesRoot;

        /// <summary>
        /// 缓存清单文件根目录路径
        /// </summary>
        protected string _cacheManifestFilesRoot;

        /// <summary>
        /// 缓存资源包文件根目录路径
        /// </summary>
        protected string _cacheBundleFilesRoot;

        /// <summary>
        /// 沙盒 Bundle 缓存系统
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
        /// 自定义参数：覆盖安装缓存清理模式
        /// </summary>
        public EInstallCleanupMode InstallCleanupMode { get; private set; } = EInstallCleanupMode.None;

        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验级别
        /// </summary>
        public EFileVerifyLevel FileVerifyLevel { get; private set; } = EFileVerifyLevel.Low;

        /// <summary>
        /// 自定义参数：初始化时缓存文件校验的最大并发数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐值为处理器数两倍）。过大的值可能导致线程池任务过多，影响系统稳定性。
        /// </remarks>
        public int FileVerifyMaxConcurrency { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：禁用边玩边下机制
        /// </summary>
        public bool DisableOnDemandDownload { get; private set; } = false;

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
        /// 自定义参数：下载任务的看门狗机制超时时间
        /// </summary>
        public int DownloadWatchdogTimeout { get; private set; } = 0;

        /// <summary>
        /// 自定义参数：启用断点续传的最小尺寸
        /// </summary>
        public long ResumeDownloadMinimumSize { get; private set; } = long.MaxValue;

        /// <summary>
        /// 自定义参数：远程服务接口的实例类
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
        /// 自定义参数：AssetBundle 备用解密器
        /// </summary>
        public IBundleMemoryDecryptor AssetBundleFallbackDecryptor { get; private set; }

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
        public SandboxFileSystem()
        {
        }
        /// <inheritdoc />
        public FSInitializeOperation InitializeAsync()
        {
            var operation = new SFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new SFSRequestPackageVersionOperation(this, options.AppendTimeTicks, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new SFSLoadPackageManifestOperation(this, options.PackageVersion, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new SFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options)
        {
            var operation = new SFSEnsurePackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var downloader = new SFSDownloadBundleOperation(this, options);
            return downloader;
        }
        /// <inheritdoc />
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            if (options.ClearMethod == ClearCacheMethods.ClearAllManifestFiles)
            {
                var operation = new SFSClearAllCacheManifestOperation(this);
                return operation;
            }
            else if (options.ClearMethod == ClearCacheMethods.ClearUnusedManifestFiles)
            {
                var operation = new SFSClearUnusedCacheManifestOperation(this, options.Manifest);
                return operation;
            }
            else
            {
                var operation = new SFSClearCacheOperation(this, options);
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
            else if (paramName == nameof(EFileSystemParameter.InstallCleanupMode))
            {
                InstallCleanupMode = FileSystemHelper.CastParameter<EInstallCleanupMode>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.FileVerifyLevel))
            {
                FileVerifyLevel = FileSystemHelper.CastParameter<EFileVerifyLevel>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.FileVerifyMaxConcurrency))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                if (convertValue > 32)
                {
                    YooLogger.LogWarning($"FILE_VERIFY_MAX_CONCURRENCY value {convertValue} is too large, clamped to 32. Recommended range: 1 - 32.");
                }

                // 限制在合理范围内：1-32                                                                            
                FileVerifyMaxConcurrency = Mathf.Clamp(convertValue, 1, 32);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadDisableOndemand))
            {
                DisableOnDemandDownload = FileSystemHelper.CastParameter<bool>(paramName, value);
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
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                DownloadWatchdogTimeout = Mathf.Max(convertValue, 0);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadResumeMinimumSize))
            {
                ResumeDownloadMinimumSize = FileSystemHelper.CastParameter<long>(paramName, value);
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
            else if (paramName == nameof(EFileSystemParameter.AssetBundleFallbackDecryptor))
            {
                AssetBundleFallbackDecryptor = FileSystemHelper.CastParameter<IBundleMemoryDecryptor>(paramName, value);
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
                _packageRoot = GetDefaultCachePackageRoot(packageName);
            else
                _packageRoot = packageRoot;

            _cacheBundleFilesRoot = PathUtility.Combine(_packageRoot, SandboxFileSystemConsts.BundleFilesFolderName);
            _cacheManifestFilesRoot = PathUtility.Combine(_packageRoot, SandboxFileSystemConsts.ManifestFilesFolderName);
            _tempFilesRoot = PathUtility.Combine(_packageRoot, SandboxFileSystemConsts.TempFilesFolderName);

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend(WebRequestCreator);

            // 创建默认的下载重试策略
            if (DownloadRetryPolicy == null)
                DownloadRetryPolicy = new DefaultDownloadRetryPolicy();

            // 创建默认的 URL 选择策略
            if (DownloadUrlPolicy == null)
                DownloadUrlPolicy = new DefaultDownloadUrlPolicy();

            // 创建文件缓存系统
            {
                var cacheConfig = new SandboxBundleCache.Configuration(
                    fileVerifyMaxConcurrency: FileVerifyMaxConcurrency,
                    fileVerifyLevel: FileVerifyLevel,
                    assetBundleDecryptor: AssetBundleDecryptor,
                    rawBundleDecryptor: RawBundleDecryptor,
                    archiveBundleDecryptor: ArchiveBundleDecryptor,
                    assetBundleFallbackDecryptor: AssetBundleFallbackDecryptor);
                BundleCache = new SandboxBundleCache(PackageName, _cacheBundleFilesRoot, cacheConfig);
            }
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
            // 注意：保底加载！
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
            return BundleCache.IsCached(bundle.BundleGuid) == false;
        }

        #region 内部方法
        /// <summary>
        /// 获取默认的缓存包裹根目录
        /// </summary>
        public string GetDefaultCachePackageRoot(string packageName)
        {
            string rootDirectory = YooAssetConfiguration.GetDefaultCacheRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }

        /// <summary>
        /// 获取缓存清单文件的根目录
        /// </summary>
        public string GetCacheManifestFilesRoot()
        {
            return _cacheManifestFilesRoot;
        }

        /// <summary>
        /// 获取缓存包裹哈希文件路径
        /// </summary>
        public string GetCachePackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_cacheManifestFilesRoot, fileName);
        }

        /// <summary>
        /// 获取缓存包裹清单文件路径
        /// </summary>
        public string GetCachePackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_cacheManifestFilesRoot, fileName);
        }

        /// <summary>
        /// 获取沙盒应用程序水印文件路径
        /// </summary>
        public string GetSandboxAppFootprintFilePath()
        {
            return PathUtility.Combine(_cacheManifestFilesRoot, SandboxFileSystemConsts.AppFootprintFileName);
        }

        /// <summary>
        /// 获取临时文件路径
        /// </summary>
        public string GetTempFilePath(PackageBundle bundle)
        {
            if (_tempFilePathMapping.TryGetValue(bundle.BundleGuid, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_tempFilesRoot, bundle.BundleGuid);
                _tempFilePathMapping.Add(bundle.BundleGuid, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 删除所有缓存的资源文件
        /// </summary>
        public void DeleteAllBundleFiles()
        {
            if (Directory.Exists(_cacheBundleFilesRoot))
            {
                Directory.Delete(_cacheBundleFilesRoot, true);
            }
        }

        /// <summary>
        /// 删除所有缓存的清单文件
        /// </summary>
        public void DeleteAllManifestFiles()
        {
            if (Directory.Exists(_cacheManifestFilesRoot))
            {
                Directory.Delete(_cacheManifestFilesRoot, true);
            }
        }

        /// <summary>
        /// 删除所有缓存的临时文件
        /// </summary>
        public void DeleteAllTempFiles()
        {
            if (Directory.Exists(_tempFilesRoot))
            {
                Directory.Delete(_tempFilesRoot, true);
            }
        }
        #endregion
    }
}

