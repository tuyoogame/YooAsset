
namespace YooAsset.Editor
{
    /// <summary>
    /// 搜索错误类型
    /// </summary>
    public enum ECollectAssetSearchError
    {
        /// <summary>
        /// 无错误
        /// </summary>
        None,

        /// <summary>
        /// 输入路径为空
        /// </summary>
        InputPathEmpty,

        /// <summary>
        /// 输入路径缺少 Assets/ 路径前缀
        /// </summary>
        InputPathMissingAssetsPrefix,

        /// <summary>
        /// 输入路径以斜杠结尾
        /// </summary>
        InputPathEndsWithSlash,

        /// <summary>
        /// 输入路径缺少文件扩展名
        /// </summary>
        InputPathMissingExtension,

        /// <summary>
        /// 输入路径的是文件夹路径
        /// </summary>
        InputPathIsFolder,

        /// <summary>
        /// 资源文件不存在
        /// </summary>
        AssetPathNotExists,
    }
}