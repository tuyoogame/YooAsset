
namespace YooAsset
{
    /// <summary>
    /// 清理缓存的操作选项
    /// </summary>
    internal readonly struct FSClearCacheOptions
    {
        /// <summary>
        /// 清理方式
        /// </summary>
        public string ClearMethod { get; }

        /// <summary>
        /// 附加参数
        /// </summary>
        public object ClearParameter { get; }

        /// <summary>
        /// 资源清单
        /// </summary>
        public PackageManifest Manifest { get; }

        public FSClearCacheOptions(string clearMethod, object clearParameter, PackageManifest manifest)
        {
            ClearMethod = clearMethod;
            ClearParameter = clearParameter;
            Manifest = manifest;
        }

        /// <summary>
        /// 转换为 BundleCache 的清理选项
        /// </summary>
        public BCClearCacheOptions ConvertTo()
        {
            return new BCClearCacheOptions(
                clearMethod: ClearMethod,
                clearParameter: ClearParameter,
                manifest: Manifest);
        }
    }
}
