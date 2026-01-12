
namespace YooAsset
{
    /// <summary>
    /// 远端资源文件命名风格
    /// </summary>
    public enum EFileNameStyle
    {
        /// <summary>
        /// 哈希值名称
        /// </summary>
        HashName = 0,

        /// <summary>
        /// 资源包名称（不推荐）
        /// </summary>
        BundleName = 1,

        /// <summary>
        /// 资源包名称 + 哈希值名称
        /// </summary>
        BundleName_HashName = 2,
    }
}