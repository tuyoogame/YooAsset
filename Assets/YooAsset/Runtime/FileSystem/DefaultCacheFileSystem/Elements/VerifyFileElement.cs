using System.IO;

namespace YooAsset
{
    internal class VerifyFileElement
    {
        public string PackageName { private set; get; }
        public string BundleGUID { private set; get; }
        public string FileRootPath { private set; get; }
        public string DataFilePath { private set; get; }
        public string InfoFilePath { private set; get; }

        public uint DataFileCRC;
        public long DataFileSize;

        /// <summary>
        /// 注意：原子操作对象
        /// </summary>
        public volatile int Result = 0;

        public VerifyFileElement(string packageName, string bundleGUID, string fileRootPath, string dataFilePath, string infoFilePath)
        {
            PackageName = packageName;
            BundleGUID = bundleGUID;
            FileRootPath = fileRootPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
        }

        public void DeleteFiles()
        {
            try
            {
                Directory.Delete(FileRootPath, true);
            }
            catch (System.Exception e)
            {
                YooLogger.Warning($"Failed to delete cache bundle folder : {e}");
            }
        }
    }
}