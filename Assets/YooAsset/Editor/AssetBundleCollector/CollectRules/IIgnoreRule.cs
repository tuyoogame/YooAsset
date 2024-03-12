
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源忽略规则接口
    /// </summary>
    public interface  IIgnoreRule
    {
        bool IsIgnore(AssetInfo assetInfo);
    }
}