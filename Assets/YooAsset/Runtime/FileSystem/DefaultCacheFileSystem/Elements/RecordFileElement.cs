using System;
using System.IO;

namespace YooAsset
{
    internal class RecordFileElement
    {
        public string InfoFilePath { private set; get; }
        public string DataFilePath { private set; get; }
        public uint DataFileCRC { private set; get; }
        public long DataFileSize { private set; get; }

        public RecordFileElement(string infoFilePath, string dataFilePath, uint dataFileCRC, long dataFileSize)
        {
            InfoFilePath = infoFilePath;
            DataFilePath = dataFilePath;
            DataFileCRC = dataFileCRC;
            DataFileSize = dataFileSize;
        }

        /// <summary>
        /// 删除记录文件
        /// </summary>
        public bool DeleteFolder()
        {
            try
            {
                string directory = Path.GetDirectoryName(InfoFilePath);
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
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
            catch (Exception e)
            {
                YooLogger.Error($"Failed to delete cache file ! {e.Message}");
                return false;
            }
        }
    }
}