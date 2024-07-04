
namespace YooAsset
{
    internal interface IPlayMode
    {
        /// <summary>
        /// 当前激活的清单
        /// </summary>
        PackageManifest ActiveManifest { set; get; }

        /// <summary>
        /// 更新游戏模式
        /// </summary>
        void UpdatePlayMode();

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 预下载指定版本的包裹内容
        /// </summary>
        PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout);

        /// <summary>
        /// 清空所有文件
        /// </summary>
        ClearAllBundleFilesOperation ClearAllBundleFilesAsync();

        /// <summary>
        /// 清空未使用的文件
        /// </summary>
        ClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync();

        // 下载相关
        ResourceDownloaderOperation CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout);
        ResourceDownloaderOperation CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout);
        ResourceDownloaderOperation CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout);

        // 解压相关
        ResourceUnpackerOperation CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout);
        ResourceUnpackerOperation CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout);

        // 导入相关
        ResourceImporterOperation CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout);
    }
}