
namespace YooAsset
{
    /// <summary>
    /// 清理缓存的操作选项
    /// </summary>
    public readonly struct ClearCacheOptions
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
        /// 创建清理缓存选项实例
        /// </summary>
        /// <param name="clearMethod">清理方式</param>
        public ClearCacheOptions(string clearMethod)
        {
            ClearMethod = clearMethod;
            ClearParameter = null;
        }

        /// <summary>
        /// 创建清理缓存选项实例
        /// </summary>
        /// <param name="clearMethod">清理方式</param>
        /// <param name="clearParam">附加参数</param>
        public ClearCacheOptions(string clearMethod, object clearParam)
        {
            ClearMethod = clearMethod;
            ClearParameter = clearParam;
        }

        /// <summary>
        /// 转换为 FileSystem 的清理缓存选项
        /// </summary>
        /// <param name="manifest">资源清单</param>
        internal FSClearCacheOptions ConvertTo(PackageManifest manifest)
        {
            return new FSClearCacheOptions(
                clearMethod: ClearMethod,
                clearParameter: ClearParameter,
                manifest: manifest);
        }
    }
}