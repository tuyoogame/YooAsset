
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存加载虚拟资源包操作
    /// </summary>
    internal sealed class EBCLoadVirtualAssetBundleOperation : EBCLoadBundleBaseOperation
    {
        public EBCLoadVirtualAssetBundleOperation(EditorBundleCache fileCache, PackageBundle bundle)
            : base(fileCache, bundle) { }

        protected override void CreateBundleHandle()
        {
            SetResult();
            BundleHandle = new VirtualAssetBundleHandle(_bundle);
        }
    }
}
