using System;
using System.IO;

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
            string editorFilePath = EditorFileSystemHelper.GetEditorFilePath(_bundle);
            if (string.IsNullOrEmpty(editorFilePath))
            {
                SetError($"Editor file path is null. Bundle: '{_bundle.BundleName}'.");
                return;
            }

            SetResult();
            BundleHandle = new VirtualAssetBundleHandle(editorFilePath, _bundle);
        }
    }
}
