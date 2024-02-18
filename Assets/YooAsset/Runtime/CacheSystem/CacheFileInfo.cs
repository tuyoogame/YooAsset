
namespace YooAsset
{
    public class CacheFileInfo
    {
        public string RemoteFileName { private set; get; }
        public string FilePath { private set; get; }
        public string FileCRC { private set; get; }
        public long FileSize { private set; get; }

        public CacheFileInfo(string remoteFileName, string filePath, string fileCRC, long fileSize)
        {
            RemoteFileName = remoteFileName;
            FilePath = filePath;
            FileCRC = fileCRC;
            FileSize = fileSize;
        }
    }
}