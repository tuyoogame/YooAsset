using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存条目
    /// </summary>
    internal class SandboxBundleCacheEntry : ICacheEntry
    {
        private long _fileSize = -1;

        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; private set; }

        /// <summary>
        /// 信息文件路径
        /// </summary>
        public string InfoFilePath { get; private set; }

        /// <summary>
        /// 数据文件路径
        /// </summary>
        public string DataFilePath { get; private set; }


        /// <summary>
        /// 创建沙盒文件缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="infoFilePath">信息文件路径</param>
        /// <param name="dataFilePath">数据文件路径</param>
        public SandboxBundleCacheEntry(string bundleGuid, string infoFilePath, string dataFilePath)
        {
            BundleGuid = bundleGuid;
            InfoFilePath = infoFilePath;
            DataFilePath = dataFilePath;
        }

        /// <summary>
        /// 删除缓存文件夹及其所有内容
        /// </summary>
        /// <returns>删除是否成功</returns>
        public bool Delete()
        {
            try
            {
                string directory = Path.GetDirectoryName(InfoFilePath);
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                YooLogger.LogError($"Failed to delete sandbox file: {ex.Message}.");
                return false;
            }
        }

        /// <summary>
        /// 获取缓存文件总大小
        /// </summary>
        /// <returns>文件总大小（字节）</returns>
        public long GetFileSize()
        {
            if (_fileSize < 0)
                _fileSize = FileUtility.GetFileSize(DataFilePath);
            return _fileSize;
        }
    }
}