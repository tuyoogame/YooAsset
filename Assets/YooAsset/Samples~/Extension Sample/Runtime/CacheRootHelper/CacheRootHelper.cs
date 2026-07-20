using YooAsset;

/// <summary>
/// 缓存目录帮助类
/// </summary>
public static class CacheRootHelper
{
    /// <summary>
    /// 获取 YooAsset 默认的缓存根目录
    /// 注意：与平台相关，与内部逻辑保持一致。
    /// </summary>
    /// <returns>默认缓存根目录的绝对路径</returns>
    public static string GetDefaultCacheRoot()
    {
        return YooAssetConfiguration.GetDefaultCacheRoot();
    }

    /// <summary>
    /// 沙盒文件系统的清单子目录名称
    /// </summary>
    public static string ManifestFolderName
    {
        get { return SandboxFileSystemConsts.ManifestFilesFolderName; }
    }
}
