
namespace YooAsset
{
    /// <summary>
    /// 资源包信息类
    /// </summary>
    internal class BundleInfo
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _importFilePath;

        /// <summary>
        /// 资源包描述
        /// </summary>
        public readonly PackageBundle Bundle;


        /// <summary>
        /// 创建资源包信息实例
        /// </summary>
        /// <param name="fileSystem">所属文件系统</param>
        /// <param name="bundle">资源包描述</param>
        public BundleInfo(IFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            Bundle = bundle;
            _importFilePath = null;
        }

        /// <summary>
        /// 创建资源包信息实例（带导入路径）
        /// </summary>
        /// <param name="fileSystem">所属文件系统</param>
        /// <param name="bundle">资源包描述</param>
        /// <param name="importFilePath">导入文件路径</param>
        public BundleInfo(IFileSystem fileSystem, PackageBundle bundle, string importFilePath)
        {
            _fileSystem = fileSystem;
            Bundle = bundle;
            _importFilePath = importFilePath;
        }

        /// <summary>
        /// 创建资源包加载器
        /// </summary>
        /// <returns>返回资源包加载操作对象</returns>
        public FSLoadPackageBundleOperation CreateBundleLoader()
        {
            var options = new FSLoadPackageBundleOptions(Bundle);
            return _fileSystem.LoadPackageBundleAsync(options);
        }

        /// <summary>
        /// 创建资源包文件确保器
        /// </summary>
        /// <returns>返回确保资源包文件就绪的操作对象</returns>
        public FSEnsurePackageBundleOperation CreateBundleEnsurer()
        {
            var options = new FSEnsurePackageBundleOptions(Bundle);
            return _fileSystem.EnsurePackageBundleAsync(options);
        }

        /// <summary>
        /// 创建资源包下载器
        /// </summary>
        /// <param name="retryCount">下载失败后的重试次数</param>
        /// <returns>返回文件下载操作对象</returns>
        public FSDownloadBundleOperation CreateBundleDownloader(int retryCount)
        {
            var options = new FSDownloadBundleOptions(Bundle, retryCount, _importFilePath);
            return _fileSystem.DownloadBundleAsync(options);
        }

        /// <summary>
        /// 是否需要从远端下载
        /// </summary>
        /// <returns>如果需要下载返回true，否则返回false。</returns>
        public bool IsDownloadRequired()
        {
            return _fileSystem.IsDownloadRequired(Bundle);
        }

        /// <summary>
        /// 获取资源包的组合唯一标识
        /// </summary>
        /// <returns>返回由文件系统哈希和资源包GUID组成的唯一键</returns>
        public string GetCombineKey()
        {
            return $"{_fileSystem.GetHashCode()}_{Bundle.BundleGuid}";
        }
    }
}