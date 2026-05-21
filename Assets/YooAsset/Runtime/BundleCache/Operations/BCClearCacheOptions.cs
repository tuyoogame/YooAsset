
namespace YooAsset
{
    /// <summary>
    /// 清理缓存的操作选项
    /// </summary>
    internal readonly struct BCClearCacheOptions
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

        public BCClearCacheOptions(string clearMethod, object clearParameter, PackageManifest manifest)
        {
            ClearMethod = clearMethod;
            ClearParameter = clearParameter;
            Manifest = manifest;
        }
    }
}
