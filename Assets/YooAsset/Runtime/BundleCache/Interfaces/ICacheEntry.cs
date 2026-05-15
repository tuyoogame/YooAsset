
namespace YooAsset
{
    /// <summary>
    /// 缓存条目接口
    /// </summary>
    internal interface ICacheEntry
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        string BundleGuid { get; }
    }
}
