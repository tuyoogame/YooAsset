
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存加载虚拟归档资源包操作
    /// </summary>
    internal sealed class EBCLoadVirtualArchiveBundleOperation : EBCLoadBundleBaseOperation
    {
        public EBCLoadVirtualArchiveBundleOperation(EditorBundleCache fileCache, PackageBundle bundle)
            : base(fileCache, bundle) { }

        protected override void CreateBundleHandle()
        {
            if (_bundle.MainAssets.Count == 0)
            {
                SetError($"Virtual archive bundle has no main assets. Bundle: '{_bundle.BundleName}'.");
                return;
            }

            var archiveAssetPaths = new HashSet<string>();
            foreach (var packageAsset in _bundle.MainAssets)
            {
                archiveAssetPaths.Add(packageAsset.AssetPath);
            }

            var virtualArchiveBundle = new VirtualArchiveBundle(archiveAssetPaths);
            string virtualBundlePath = $"VirtualArchiveBundle://{_bundle.BundleName}";

            SetResult();
            BundleHandle = new VirtualArchiveBundleHandle(virtualBundlePath, _bundle, virtualArchiveBundle);
        }
    }
}
