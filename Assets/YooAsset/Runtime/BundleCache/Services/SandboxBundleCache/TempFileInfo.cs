
namespace YooAsset
{
    /// <summary>
    /// 临时的文件信息，用于存储待验证的下载文件。
    /// </summary>
    internal class TempFileInfo
    {
        /// <summary>
        /// 临时文件路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 文件CRC校验值
        /// </summary>
        public uint FileCrc { get; private set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// 验证结果码（原子操作对象，用于线程安全）。
        /// </summary>
        public volatile int VerifyResultCode = 0;

        /// <summary>
        /// 创建临时文件信息实例
        /// </summary>
        /// <param name="filePath">临时文件路径</param>
        /// <param name="fileCrc">文件CRC校验值</param>
        /// <param name="fileSize">文件大小（字节）</param>
        public TempFileInfo(string filePath, uint fileCrc, long fileSize)
        {
            FilePath = filePath;
            FileCrc = fileCrc;
            FileSize = fileSize;
        }
    }
}
