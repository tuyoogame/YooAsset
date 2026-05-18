
namespace YooAsset
{
    /// <summary>
    /// 扫描到的标记文件信息
    /// </summary>
    internal class ScanFileInfo
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; private set; }

        /// <summary>
        /// 标记文件路径
        /// </summary>
        public string MarkerFilePath { get; private set; }

        /// <summary>
        /// 创建扫描文件信息实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="markerFilePath">标记文件路径</param>
        public ScanFileInfo(string bundleGuid, string markerFilePath)
        {
            BundleGuid = bundleGuid;
            MarkerFilePath = markerFilePath;
        }
    }
}
