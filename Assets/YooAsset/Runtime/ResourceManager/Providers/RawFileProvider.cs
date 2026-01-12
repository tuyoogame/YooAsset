
namespace YooAsset
{
    /// <summary>
    /// 原生文件提供者，负责加载原始文件资源。
    /// </summary>
    internal sealed class RawFileProvider : ProviderBase
    {
        public RawFileProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo) : base(manager, providerKey, assetInfo)
        {
        }
        protected override void InternalProcessBundleHandle()
        {
            SetSuccess();
        }
    }
}