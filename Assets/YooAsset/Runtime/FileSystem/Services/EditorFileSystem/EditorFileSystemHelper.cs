
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件系统工具类
    /// </summary>
    internal static class EditorFileSystemHelper
    {
        /// <summary>
        /// 获取编辑器环境下资源包对应的源文件路径
        /// </summary>
        public static string GetEditorFilePath(PackageBundle bundle)
        {
            if (bundle.MainAssets.Count == 0)
                return string.Empty;

            var packageAsset = bundle.MainAssets[0];
            return packageAsset.AssetPath;
        }
    }
}