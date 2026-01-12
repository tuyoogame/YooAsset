
namespace YooAsset
{
    /// <summary>
    /// 覆盖安装清理模式
    /// </summary>
    public enum EInstallCleanupMode
    {
        /// <summary>
        /// 不做任何处理
        /// </summary>
        None = 0,

        /// <summary>
        /// 清理所有缓存文件（包含资源文件和清单文件）
        /// </summary>
        ClearAllCacheFiles = 1,

        /// <summary>
        /// 清理所有缓存的资源文件
        /// </summary>
        ClearAllBundleFiles = 2,

        /// <summary>
        /// 清理所有缓存的清单文件
        /// </summary>
        ClearAllManifestFiles = 3,
    }
}