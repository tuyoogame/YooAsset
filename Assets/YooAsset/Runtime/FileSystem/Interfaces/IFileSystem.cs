
namespace YooAsset
{
    /// <summary>
    /// 文件系统接口
    /// </summary>
    internal interface IFileSystem
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        /// <returns>初始化操作句柄</returns>
        FSInitializeOperation InitializeAsync();

        /// <summary>
        /// 查询包裹版本
        /// </summary>
        /// <param name="options">请求版本的选项参数</param>
        /// <returns>请求包裹版本操作句柄</returns>
        FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options);

        /// <summary>
        /// 加载包裹清单
        /// </summary>
        /// <param name="options">加载清单的选项参数</param>
        /// <returns>加载包裹清单操作句柄</returns>
        FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options);

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="options">加载资源包的选项参数</param>
        /// <returns>加载资源包操作句柄</returns>
        FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options);

        /// <summary>
        /// 确保资源包文件已就绪
        /// </summary>
        /// <param name="options">确保资源包已就绪的选项参数</param>
        /// <returns>确保资源包已就绪的操作句柄</returns>
        FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options);

        /// <summary>
        /// 下载资源包
        /// </summary>
        /// <param name="options">下载资源包的选项参数</param>
        /// <returns>下载资源包操作句柄</returns>
        FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options);

        /// <summary>
        /// 清理缓存文件
        /// </summary>
        /// <param name="options">清理缓存的选项参数</param>
        /// <returns>清理缓存操作句柄</returns>
        FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options);


        /// <summary>
        /// 设置自定义参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">参数值</param>
        void SetParameter(string paramName, object value);
        
        /// <summary>
        /// 执行文件系统创建时的初始化回调
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageRoot">包裹根目录</param>
        void OnCreate(string packageName, string packageRoot);

        /// <summary>
        /// 执行文件系统销毁时的清理回调
        /// </summary>
        void OnDestroy();


        /// <summary>
        /// 判断是否可以接管指定的资源包
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>如果可以接受该资源包则返回 true，否则返回 false。</returns>
        bool CanAcceptBundle(PackageBundle bundle);

        /// <summary>
        /// 检查是否需要下载指定的资源包
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>如果需要下载该资源包则返回 true，否则返回 false。</returns>
        bool IsDownloadRequired(PackageBundle bundle);

        /// <summary>
        /// 检查是否需要解压指定的资源包
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>如果需要解压该资源包则返回 true，否则返回 false。</returns>
        bool IsUnpackRequired(PackageBundle bundle);

        /// <summary>
        /// 检查是否需要导入指定的资源包
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>如果需要导入该资源包则返回 true，否则返回 false。</returns>
        bool IsImportRequired(PackageBundle bundle);
    }
}