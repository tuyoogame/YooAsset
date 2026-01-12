namespace YooAsset.Editor
{
    /// <summary>
    /// 包管理工具类
    /// </summary>
    public static class EditorPackageUtility
    {
        /// <summary>
        /// 获取通过 PackageManager 安装的 YooAsset 版本号
        /// </summary>
        /// <returns>版本号字符串，未找到时为空字符串</returns>
        public static string GetPackageManagerYooVersion()
        {
            UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(YooAssets).Assembly);
            if (packageInfo != null)
                return packageInfo.version;
            else
                return string.Empty;
        }
    }
}
