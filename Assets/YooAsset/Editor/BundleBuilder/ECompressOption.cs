
namespace YooAsset.Editor
{
    /// <summary>
    /// AssetBundle压缩选项
    /// </summary>
    public enum ECompressOption
    {
        /// <summary>
        /// 不压缩
        /// </summary>
        Uncompressed = 0,

        /// <summary>
        /// LZMA 压缩格式
        /// </summary>
        LZMA,

        /// <summary>
        /// LZ4 压缩格式
        /// </summary>
        LZ4,
    }
}