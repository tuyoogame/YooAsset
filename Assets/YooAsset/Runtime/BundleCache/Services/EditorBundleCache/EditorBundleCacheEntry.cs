using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存条目
    /// </summary>
    internal class EditorBundleCacheEntry : ICacheEntry
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
        /// 创建编辑器文件缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="markerFilePath">标记文件路径</param>
        public EditorBundleCacheEntry(string bundleGuid, string markerFilePath)
        {
            BundleGuid = bundleGuid;
            MarkerFilePath = markerFilePath;
        }

        /// <summary>
        /// 删除缓存文件夹及其所有内容
        /// </summary>
        /// <returns>删除是否成功</returns>
        public bool Delete()
        {
            try
            {
                string directory = Path.GetDirectoryName(MarkerFilePath);
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
                YooLogger.LogError($"Failed to delete editor cache file: {ex.Message}.");
                return false;
            }
        }
    }
}
