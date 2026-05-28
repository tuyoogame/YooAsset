using System;

namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存加载原生资源包操作
    /// </summary>
    internal sealed class EBCLoadVirtualRawBundleOperation : EBCLoadBundleBaseOperation
    {
        public EBCLoadVirtualRawBundleOperation(EditorBundleCache fileCache, PackageBundle bundle)
            : base(fileCache, bundle) { }

        protected override void CreateBundleHandle()
        {
            string editorFilePath = EditorFileSystemHelper.GetEditorFilePath(_bundle);
            if (string.IsNullOrEmpty(editorFilePath))
            {
                SetError($"Editor file path is null. Bundle: '{_bundle.BundleName}'.");
                return;
            }

            try
            {
                var rawBundle = RawBundleHelper.LoadFromFile(editorFilePath);

                SetResult();
                BundleHandle = new VirtualRawBundleHandle(_bundle, rawBundle);
            }
            catch (Exception ex)
            {
                SetError($"Failed to read raw bundle file: {ex.Message}.");
                YooLogger.LogError(Error);
            }
        }
    }
}
