using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 内置文件系统，管理内置（StreamingAssets）目录下的资源文件访问。
    /// </summary>
    internal class BuiltinFileSystem : IFileSystem
    {
        /// <summary>
        /// 内置资源文件路径映射表
        /// </summary>
        protected readonly Dictionary<string, string> _builtinFilePathMapping = new Dictionary<string, string>(10000);

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
        /// 解压清单文件根目录路径
        /// </summary>
        protected string _unpackManifestFilesRoot;

        /// <summary>
        /// 解压资源包文件根目录路径
        /// </summary>
        protected string _unpackBundleFilesRoot;

        /// <summary>
        /// 内置 Bundle 缓存系统
        /// </summary>
        public IBundleCache BuiltinBundleCache { get; private set; }

        /// <summary>
        /// 解压 Bundle 缓存系统
        /// </summary>
        public IBundleCache UnpackBundleCache { get; private set; }

        /// <summary>
        /// 解压调度器
        /// </summary>
        public DownloadSchedulerOperation UnpackScheduler { get; internal set; }

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
        /// 自定义参数：初始化的时候缓存文件校验最大并发数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐值为处理器数两倍）。过大的值可能导致线程池任务过多，影响系统稳定性。
        /// </remarks>
        public int FileVerifyMaxConcurrency { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：拷贝内置清单
        /// </summary>
        public bool CopyBuiltinPackageManifest { get; private set; } = false;

        /// <summary>
        /// 自定义参数：拷贝内置清单的目标目录
        /// </summary>
        /// <remarks>
        /// 注意：该参数为空的时候，会获取默认的沙盒目录。
        /// </remarks>
        public string CopyBuiltinPackageManifestDestRoot { get; private set; }

        /// <summary>
        /// 自定义参数：解压文件系统的根目录
        /// </summary>
        public string UnpackFileSystemRoot { get; private set; }

        /// <summary>
        /// 自定义参数：最大并发连接数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐范围 1-32）
        /// </remarks>
        public int UnpackMaxConcurrency { get; private set; } = 8;

        /// <summary>
        /// 自定义参数：每帧发起的最大请求数
        /// </summary>
        /// <remarks>
        /// 默认值：8（推荐范围 1-32）。避免单帧发起过多请求导致卡顿。
        /// </remarks>
        public int UnpackMaxRequestsPerFrame { get; private set; } = 8;

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
        /// 自定义参数：内置资源包解包策略
        /// </summary>
        internal IBundleUnpackPolicy BundleUnpackPolicy { get; private set; }

        /// <summary>
        /// 自定义参数：内置文件访问器
        /// </summary>
        internal IBuiltinFileAccessor BuiltinFileAccessor { get; private set; }
        #endregion

        /// <summary>
        /// 创建实例
        /// </summary>
        public BuiltinFileSystem()
        {
        }
        /// <inheritdoc />
        public FSInitializeOperation InitializeAsync()
        {
            var operation = new BFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new BFSRequestPackageVersionOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new BFSLoadPackageManifestOperation(this, options.PackageVersion);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new BFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options)
        {
            var operation = new BFSEnsurePackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var operation = new BFSDownloadBundleOperation(this, options);
            return operation;
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
                var operation = new BFSClearCacheOperation(this, options);
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
            else if (paramName == nameof(EFileSystemParameter.CopyBuiltinPackageManifest))
            {
                CopyBuiltinPackageManifest = FileSystemHelper.CastParameter<bool>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.CopyBuiltinPackageManifestDestRoot))
            {
                CopyBuiltinPackageManifestDestRoot = FileSystemHelper.CastParameter<string>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.UnpackFileSystemRoot))
            {
                UnpackFileSystemRoot = FileSystemHelper.CastParameter<string>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadMaxConcurrency))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                if (convertValue > 32)
                {
                    YooLogger.LogWarning($"DOWNLOAD_MAX_CONCURRENCY value {convertValue} is too large, clamped to 32. Recommended range: 1 - 32.");
                }

                // 限制在合理范围内：1-32          
                UnpackMaxConcurrency = Mathf.Clamp(convertValue, 1, 32);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadMaxRequestPerFrame))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                if (convertValue > 32)
                {
                    YooLogger.LogWarning($"DOWNLOAD_MAX_REQUEST_PER_FRAME value {convertValue} is too large, clamped to 32. Recommended range: 1 - 32.");
                }

                // 限制在合理范围内：1-32          
                UnpackMaxRequestsPerFrame = Mathf.Clamp(convertValue, 1, 32);
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
            else if (paramName == nameof(EFileSystemParameter.BundleUnpackPolicy))
            {
                BundleUnpackPolicy = FileSystemHelper.CastParameter<IBundleUnpackPolicy>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.BuiltinFileAccessor))
            {
                BuiltinFileAccessor = FileSystemHelper.CastParameter<IBuiltinFileAccessor>(paramName, value);
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
                _packageRoot = GetDefaultBuiltinPackageRoot(packageName);
            else
                _packageRoot = packageRoot;

            // 设置根目录
            string unpackRoot;
            if (string.IsNullOrEmpty(UnpackFileSystemRoot))
                unpackRoot = GetDefaultUnpackPackageRoot(packageName);
            else
                unpackRoot = UnpackFileSystemRoot;
            _unpackManifestFilesRoot = PathUtility.Combine(unpackRoot, BuiltinFileSystemConsts.UnpackManifestFilesFolderName);
            _unpackBundleFilesRoot = PathUtility.Combine(unpackRoot, BuiltinFileSystemConsts.UnpackBundleFilesFolderName);
            _tempFilesRoot = PathUtility.Combine(unpackRoot, BuiltinFileSystemConsts.UnpackTempFilesFolderName);

            // 创建默认的解包策略
            if (BundleUnpackPolicy == null)
                BundleUnpackPolicy = new DefaultBundleUnpackPolicy();

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend(WebRequestCreator);

            // 创建内置文件缓存系统
            {
                var cacheConfig = new BuiltinBundleCache.Configuration(
                    assetBundleDecryptor: AssetBundleDecryptor,
                    rawBundleDecryptor: RawBundleDecryptor,
                    archiveBundleDecryptor: ArchiveBundleDecryptor,
                    downloadBackend: DownloadBackend);
                BuiltinBundleCache = new BuiltinBundleCache(packageName, _packageRoot, cacheConfig);
            }

            // 创建沙盒文件缓存系统
            {
                var cacheConfig = new SandboxBundleCache.Configuration(
                    fileVerifyMaxConcurrency: FileVerifyMaxConcurrency,
                    fileVerifyLevel: FileVerifyLevel,
                    assetBundleDecryptor: AssetBundleDecryptor,
                    rawBundleDecryptor: RawBundleDecryptor,
                    archiveBundleDecryptor: ArchiveBundleDecryptor,
                    assetBundleFallbackDecryptor: AssetBundleFallbackDecryptor);
                UnpackBundleCache = new SandboxBundleCache(packageName, _unpackBundleFilesRoot, cacheConfig);
            }
        }
        /// <inheritdoc />
        public void OnDestroy()
        {
            if (BuiltinBundleCache != null)
            {
                BuiltinBundleCache.Dispose();
                BuiltinBundleCache = null;
            }

            if (UnpackBundleCache != null)
            {
                UnpackBundleCache.Dispose();
                UnpackBundleCache = null;
            }

            if (UnpackScheduler != null)
            {
                UnpackScheduler.AbortOperation();
                UnpackScheduler = null;
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
            return BuiltinBundleCache.IsCached(bundle.BundleGuid);
        }
        /// <inheritdoc />
        public bool IsDownloadRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public bool IsUnpackRequired(PackageBundle bundle)
        {
            if (CanAcceptBundle(bundle) == false)
                return false;

            if (IsUnpackBundle(bundle) == false)
                return false;

            return UnpackBundleCache.IsCached(bundle.BundleGuid) == false;
        }
        /// <inheritdoc />
        public bool IsImportRequired(PackageBundle bundle)
        {
            return false;
        }

        #region 内部方法
        /// <summary>
        /// 通过策略判定指定资源包是否为需要解包的类型
        /// </summary>
        internal bool IsUnpackBundle(PackageBundle bundle)
        {
            var unpackInfo = new BundleUnpackInfo(bundle);
            return BundleUnpackPolicy.IsUnpackBundle(unpackInfo);
        }

        /// <summary>
        /// 获取默认的内置包裹根目录
        /// </summary>
        public string GetDefaultBuiltinPackageRoot(string packageName)
        {
            string rootDirectory = YooAssetConfiguration.GetDefaultBuiltinRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }

        /// <summary>
        /// 获取内置文件路径
        /// </summary>
        public string GetBuiltinBundleFilePath(PackageBundle bundle)
        {
            if (_builtinFilePathMapping.TryGetValue(bundle.BundleGuid, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_packageRoot, bundle.GetFileName());
                _builtinFilePathMapping.Add(bundle.BundleGuid, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取内置包裹版本文件路径
        /// </summary>
        public string GetBuiltinPackageVersionFilePath()
        {
            string fileName = YooAssetConfiguration.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取内置包裹哈希文件路径
        /// </summary>
        public string GetBuiltinPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取内置包裹清单文件路径
        /// </summary>
        public string GetBuiltinPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetConfiguration.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }

        /// <summary>
        /// 获取沙盒应用程序水印文件路径
        /// </summary>
        public string GetSandboxAppFootprintFilePath()
        {
            return PathUtility.Combine(_unpackManifestFilesRoot, SandboxFileSystemConsts.AppFootprintFileName);
        }

        /// <summary>
        /// 删除所有缓存的资源文件
        /// </summary>
        public void DeleteAllBundleFiles()
        {
            if (Directory.Exists(_unpackBundleFilesRoot))
            {
                Directory.Delete(_unpackBundleFilesRoot, true);
            }
        }

        /// <summary>
        /// 删除所有缓存的清单文件
        /// </summary>
        public void DeleteAllManifestFiles()
        {
            if (Directory.Exists(_unpackManifestFilesRoot))
            {
                Directory.Delete(_unpackManifestFilesRoot, true);
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

        /// <summary>
        /// 获取默认的解压根目录
        /// </summary>
        public string GetDefaultUnpackPackageRoot(string packageName)
        {
            string rootDirectory = YooAssetConfiguration.GetDefaultCacheRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }

        /// <summary>
        /// 获取解压的临时文件路径
        /// </summary>
        public string GetUnpackTempFilePath(PackageBundle bundle)
        {
            if (_tempFilePathMapping.TryGetValue(bundle.BundleGuid, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_tempFilesRoot, bundle.BundleGuid);
                _tempFilePathMapping.Add(bundle.BundleGuid, filePath);
            }
            return filePath;
        }
        #endregion
    }
}
