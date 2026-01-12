using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 搜索的文件信息，用于缓存文件校验流程。
    /// </summary>
    internal class SearchFileInfo
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; private set; }

        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        public string FolderPath { get; private set; }

        /// <summary>
        /// 数据文件路径
        /// </summary>
        public string DataFilePath { get; private set; }

        /// <summary>
        /// 信息文件路径
        /// </summary>
        public string InfoFilePath { get; private set; }

        /// <summary>
        /// 验证结果码（原子操作对象，用于线程安全）
        /// </summary>
        public volatile int VerifyResultCode = 0;

        /// <summary>
        /// 创建验证文件信息实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="folderPath">缓存文件夹路径</param>
        /// <param name="dataFilePath">数据文件路径</param>
        /// <param name="infoFilePath">信息文件路径</param>
        public SearchFileInfo(string bundleGuid, string folderPath, string dataFilePath, string infoFilePath)
        {
            BundleGuid = bundleGuid;
            FolderPath = folderPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
        }

        /// <summary>
        /// 删除缓存文件夹及其内容
        /// </summary>
        public void DeleteCacheFolder()
        {
            try
            {
                Directory.Delete(FolderPath, true);
            }
            catch (System.Exception ex)
            {
                YooLogger.LogWarning($"Failed to delete cache bundle folder: {ex}.");
            }
        }
    }
}
