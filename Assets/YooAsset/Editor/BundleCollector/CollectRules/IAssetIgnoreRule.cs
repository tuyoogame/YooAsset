
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源忽略规则接口
    /// </summary>
    public interface IAssetIgnoreRule
    {
        /// <summary>
        /// 检查是否为忽略文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>如果忽略该资源返回 true</returns>
        bool IsIgnoreAsset(EditorAssetInfo assetInfo);
    }
}
