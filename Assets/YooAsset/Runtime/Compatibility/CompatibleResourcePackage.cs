#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - ResourcePackage 兼容方法
// 通过 partial class 为 ResourcePackage 补充 v2.3 的旧接口。

using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// v2.3 缓存清理模式（v3 改用字符串参数）
    /// </summary>
    [Obsolete("Use string-based ClearCacheOptions instead.")]
    public enum EFileClearMode
    {
        ClearAllBundleFiles,
        ClearUnusedBundleFiles,
        ClearBundleFilesByLocations,
        ClearBundleFilesByTags,
        ClearAllManifestFiles,
        ClearUnusedManifestFiles,
    }

    public partial class ResourcePackage
    {
        #region 初始化 / 销毁
        [Obsolete("Use InitializePackageAsync(options) instead.")]
        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            if (parameters is EditorSimulateModeParameters esp)
            {
                var options = new EditorSimulateModeOptions();
                options.BundleLoadingMaxConcurrency = esp.BundleLoadingMaxConcurrency;
                options.AutoUnloadBundleWhenUnused = esp.AutoUnloadBundleWhenUnused;
                options.WebGLForceSyncLoadAsset = esp.WebGLForceSyncLoadAsset;
                options.EditorFileSystemParameters = esp.EditorFileSystemParameters;
                var operation = InitializePackageAsync(options);
                var wrapper = new InitializationOperation(operation);
                AsyncOperationSystem.StartOperation(PackageName, wrapper);
                return wrapper;
            }
            else if (parameters is OfflinePlayModeParameters opp)
            {
                var options = new OfflinePlayModeOptions();
                options.BundleLoadingMaxConcurrency = opp.BundleLoadingMaxConcurrency;
                options.AutoUnloadBundleWhenUnused = opp.AutoUnloadBundleWhenUnused;
                options.WebGLForceSyncLoadAsset = opp.WebGLForceSyncLoadAsset;
                options.BuiltinFileSystemParameters = opp.BuildinFileSystemParameters;
                var operation = InitializePackageAsync(options);
                var wrapper = new InitializationOperation(operation);
                AsyncOperationSystem.StartOperation(PackageName, wrapper);
                return wrapper;
            }
            else if (parameters is HostPlayModeParameters hpp)
            {
                var options = new HostPlayModeOptions();
                options.BundleLoadingMaxConcurrency = hpp.BundleLoadingMaxConcurrency;
                options.AutoUnloadBundleWhenUnused = hpp.AutoUnloadBundleWhenUnused;
                options.WebGLForceSyncLoadAsset = hpp.WebGLForceSyncLoadAsset;
                options.BuiltinFileSystemParameters = hpp.BuildinFileSystemParameters;
                options.CacheFileSystemParameters = hpp.CacheFileSystemParameters;
                var operation = InitializePackageAsync(options);
                var wrapper = new InitializationOperation(operation);
                AsyncOperationSystem.StartOperation(PackageName, wrapper);
                return wrapper;
            }
            else if (parameters is WebPlayModeParameters wpp)
            {
                var options = new WebPlayModeOptions();
                options.BundleLoadingMaxConcurrency = wpp.BundleLoadingMaxConcurrency;
                options.AutoUnloadBundleWhenUnused = wpp.AutoUnloadBundleWhenUnused;
                options.WebGLForceSyncLoadAsset = wpp.WebGLForceSyncLoadAsset;
                options.WebServerFileSystemParameters = wpp.WebServerFileSystemParameters;
                options.WebNetworkFileSystemParameters = wpp.WebRemoteFileSystemParameters;
                var operation = InitializePackageAsync(options);
                var wrapper = new InitializationOperation(operation);
                AsyncOperationSystem.StartOperation(PackageName, wrapper);
                return wrapper;
            }
            else if (parameters is CustomPlayModeParameters cpp)
            {
                var options = new CustomPlayModeOptions();
                options.BundleLoadingMaxConcurrency = cpp.BundleLoadingMaxConcurrency;
                options.AutoUnloadBundleWhenUnused = cpp.AutoUnloadBundleWhenUnused;
                options.WebGLForceSyncLoadAsset = cpp.WebGLForceSyncLoadAsset;
                foreach (var fsp in cpp.FileSystemParameterList)
                    options.FileSystemParameterList.Add(fsp);
                var operation = InitializePackageAsync(options);
                var wrapper = new InitializationOperation(operation);
                AsyncOperationSystem.StartOperation(PackageName, wrapper);
                return wrapper;
            }
            else
            {
                throw new NotImplementedException($"Unsupported InitializeParameters type: {parameters.GetType().Name}");
            }
        }

        [Obsolete("Use DestroyPackageAsync() instead.")]
        public DestroyOperation DestroyAsync()
        {
            var operation = DestroyPackageAsync();
            var wrapper = new DestroyOperation(operation);
            AsyncOperationSystem.StartOperation(AsyncOperationSystem.GlobalSchedulerName, wrapper);
            return wrapper;
        }
        #endregion

        #region 版本 / 清单
        [Obsolete("Use RequestPackageVersionAsync(RequestPackageVersionOptions) instead.")]
        public RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var options = new RequestPackageVersionOptions(appendTimeTicks, timeout);
            return RequestPackageVersionAsync(options);
        }

        [Obsolete("Use LoadPackageManifestAsync(LoadPackageManifestOptions) instead.")]
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            var options = new LoadPackageManifestOptions(packageVersion, timeout);
            var operation = LoadPackageManifestAsync(options);
            var wrapper = new UpdatePackageManifestOperation(operation);
            AsyncOperationSystem.StartOperation(PackageName, wrapper);
            return wrapper;
        }

        [Obsolete("Use PrefetchManifestAsync(PrefetchManifestOptions) instead.")]
        public PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout = 60)
        {
            var options = new PrefetchManifestOptions(packageVersion, timeout);
            var op = PrefetchManifestAsync(options);
            var wrapper = new PreDownloadContentOperation(op);
            AsyncOperationSystem.StartOperation(PackageName, wrapper);
            return wrapper;
        }
        #endregion

        #region 缓存清理
        [Obsolete("Use ClearCacheAsync(ClearCacheOptions) instead.")]
        public ClearCacheOperation ClearCacheFilesAsync(string fileClearMode, object clearParam = null)
        {
            var options = new ClearCacheOptions(fileClearMode, clearParam);
            return ClearCacheAsync(options);
        }

        [Obsolete("Use ClearCacheAsync(ClearCacheOptions) instead.")]
        public ClearCacheOperation ClearCacheFilesAsync(EFileClearMode clearMode, object clearParam = null)
        {
            var options = new ClearCacheOptions(clearMode.ToString(), clearParam);
            return ClearCacheAsync(options);
        }
        #endregion

        #region 资源卸载
        [Obsolete("Use UnloadUnusedAssetsAsync(UnloadUnusedAssetsOptions) instead.")]
        public UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync(int loopCount)
        {
            var options = new UnloadUnusedAssetsOptions(loopCount);
            return UnloadUnusedAssetsAsync(options);
        }
        #endregion

        #region 资源查询
        [Obsolete("Use GetDownloadSize() instead.")]
        public bool IsNeedDownloadFromRemote(string location)
        {
            return GetDownloadSize(location) > 0;
        }

        [Obsolete("Use GetDownloadSize() instead.")]
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            return GetDownloadSize(assetInfo) > 0;
        }

        [Obsolete("Use IsLocationValid() instead.")]
        public bool CheckLocationValid(string location)
        {
            return IsLocationValid(location);
        }

        [Obsolete("Use GetAssetInfoByGuid() instead.")]
        public AssetInfo GetAssetInfoByGUID(string assetGUID)
        {
            return GetAssetInfoByGuid(assetGUID);
        }

        [Obsolete("Use GetAssetInfoByGuid() instead.")]
        public AssetInfo GetAssetInfoByGUID(string assetGUID, Type type)
        {
            return GetAssetInfoByGuid(assetGUID, type);
        }
        #endregion

        #region 资源下载
        [Obsolete("Use CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            string[] tags = new string[] { tag };
            var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(ResourceDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(string location, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            var options = new BundleDownloaderOptions(assetInfo, recursiveDownload, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain)
        {
            return CreateBundleDownloader(location, false, downloadingMaxNumber, failedTryAgain);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }
            var options = new BundleDownloaderOptions(assetInfos.ToArray(), recursiveDownload, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
        {
            return CreateBundleDownloader(locations, false, downloadingMaxNumber, failedTryAgain);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
            var options = new BundleDownloaderOptions(assetInfos, recursiveDownload, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain)
        {
            return CreateBundleDownloader(assetInfo, false, downloadingMaxNumber, failedTryAgain);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new BundleDownloaderOptions(assetInfos, recursiveDownload, downloadingMaxNumber, failedTryAgain);
            return CreateResourceDownloader(options);
        }

        [Obsolete("Use CreateResourceDownloader(BundleDownloaderOptions) instead.")]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            return CreateBundleDownloader(assetInfos, false, downloadingMaxNumber, failedTryAgain);
        }
        #endregion

        #region 资源解压
        [Obsolete("Use CreateResourceUnpacker(ResourceUnpackerOptions) instead.")]
        public ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceUnpackerOptions(unpackingMaxNumber, failedTryAgain);
            return CreateResourceUnpacker(options);
        }

        [Obsolete("Use CreateResourceUnpacker(ResourceUnpackerOptions) instead.")]
        public ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            string[] tags = new string[] { tag };
            var options = new ResourceUnpackerOptions(tags, unpackingMaxNumber, failedTryAgain);
            return CreateResourceUnpacker(options);
        }

        [Obsolete("Use CreateResourceUnpacker(ResourceUnpackerOptions) instead.")]
        public ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceUnpackerOptions(tags, unpackingMaxNumber, failedTryAgain);
            return CreateResourceUnpacker(options);
        }
        #endregion

        #region 资源导入
        [Obsolete("Use CreateResourceImporter(BundleImporterOptions) instead.")]
        public ResourceImporterOperation CreateResourceImporter(string[] filePaths, int importerMaxNumber, int failedTryAgain)
        {
            ImportFileInfo[] fileInfos = new ImportFileInfo[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                ImportFileInfo fileInfo = new ImportFileInfo();
                fileInfo.FilePath = filePaths[i];
                fileInfos[i] = fileInfo;
            }
            return CreateResourceImporter(fileInfos, importerMaxNumber, failedTryAgain);
        }

        [Obsolete("Use CreateResourceImporter(BundleImporterOptions) instead.")]
        public ResourceImporterOperation CreateResourceImporter(ImportFileInfo[] fileInfos, int importerMaxNumber, int failedTryAgain)
        {
            ImportBundleInfo[] bundleInfos = new ImportBundleInfo[fileInfos.Length];
            for (int i = 0; i < fileInfos.Length; i++)
            {
                bundleInfos[i] = new ImportBundleInfo(
                    filePath: fileInfos[i].FilePath,
                    bundleName: fileInfos[i].BundleName,
                    bundleGuid: fileInfos[i].BundleGUID);
            }
            var options = new BundleImporterOptions(bundleInfos, importerMaxNumber, failedTryAgain);
            return CreateResourceImporter(options);
        }
        #endregion

        #region 资源包文件加载
        [Obsolete("Use LoadBundleFileSync(AssetInfo) instead.")]
        public BundleFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            return LoadBundleFileSync(assetInfo);
        }

        [Obsolete("Use LoadBundleFileSync(string) instead.")]
        public BundleFileHandle LoadRawFileSync(string location)
        {
            return LoadBundleFileSync(location);
        }

        [Obsolete("Use LoadBundleFileAsync(AssetInfo, uint) instead.")]
        public BundleFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            return LoadBundleFileAsync(assetInfo, priority);
        }

        [Obsolete("Use LoadBundleFileAsync(string, uint) instead.")]
        public BundleFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            return LoadBundleFileAsync(location, priority);
        }
        #endregion
    }
}
#endif
