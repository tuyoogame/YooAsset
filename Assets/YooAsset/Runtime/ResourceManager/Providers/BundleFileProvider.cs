
namespace YooAsset
{
    /// <summary>
    /// 资源包文件提供者，负责加载资源包文件。
    /// </summary>
    internal sealed class BundleFileProvider : ProviderBase
    {
        public BundleFileProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo) : base(manager, providerKey, assetInfo)
        {
        }
        protected override void InternalProcessBundleHandle()
        {
            SetSuccess();
        }
    }
}