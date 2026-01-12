
namespace YooAsset
{
    /// <summary>
    /// 下载完成事件参数
    /// </summary>
    public readonly struct DownloadCompletedEventArgs
    {
        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// 下载失败时的错误信息
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// 创建下载完成事件参数实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="succeeded">是否下载成功</param>
        /// <param name="error">下载失败时的错误信息</param>
        public DownloadCompletedEventArgs(string packageName, bool succeeded, string error)
        {
            PackageName = packageName;
            Succeeded = succeeded;
            Error = error;
        }

        /// <summary>
        /// 创建表示成功下载的事件参数
        /// </summary>
        internal static DownloadCompletedEventArgs CreateSucceeded(string packageName)
        {
            return new DownloadCompletedEventArgs(
                packageName: packageName,
                succeeded: true,
                error: null);
        }

        /// <summary>
        /// 创建表示失败下载的事件参数
        /// </summary>
        internal static DownloadCompletedEventArgs CreateFailed(string packageName, string error)
        {
            return new DownloadCompletedEventArgs(
                packageName: packageName,
                succeeded: false,
                error: error);
        }
    }

    /// <summary>
    /// 下载进度更新事件参数
    /// </summary>
    public readonly struct DownloadProgressChangedEventArgs
    {
        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 下载进度 (0-1f)
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// 下载文件总数
        /// </summary>
        public int TotalDownloadCount { get; }

        /// <summary>
        /// 下载数据总大小（单位：字节）
        /// </summary>
        public long TotalDownloadBytes { get; }

        /// <summary>
        /// 当前完成的下载文件数量
        /// </summary>
        public int CurrentDownloadCount { get; }

        /// <summary>
        /// 当前完成的下载数据大小（单位：字节）
        /// </summary>
        public long CurrentDownloadBytes { get; }

        /// <summary>
        /// 创建下载进度更新事件参数实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="progress">下载进度（0-1f）</param>
        /// <param name="totalDownloadCount">下载文件总数</param>
        /// <param name="totalDownloadBytes">下载数据总大小（单位：字节）</param>
        /// <param name="currentDownloadCount">当前完成的下载文件数量</param>
        /// <param name="currentDownloadBytes">当前完成的下载数据大小（单位：字节）</param>
        public DownloadProgressChangedEventArgs(string packageName, float progress,
            int totalDownloadCount, long totalDownloadBytes,
            int currentDownloadCount, long currentDownloadBytes)
        {
            PackageName = packageName;
            Progress = progress;
            TotalDownloadCount = totalDownloadCount;
            TotalDownloadBytes = totalDownloadBytes;
            CurrentDownloadCount = currentDownloadCount;
            CurrentDownloadBytes = currentDownloadBytes;
        }
    }

    /// <summary>
    /// 下载错误事件参数
    /// </summary>
    public readonly struct DownloadErrorEventArgs
    {
        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 下载失败的文件名称
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo { get; }

        /// <summary>
        /// 创建下载错误事件参数实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="fileName">下载失败的文件名称</param>
        /// <param name="errorInfo">错误信息</param>
        public DownloadErrorEventArgs(string packageName, string fileName, string errorInfo)
        {
            PackageName = packageName;
            FileName = fileName;
            ErrorInfo = errorInfo;
        }
    }

    /// <summary>
    /// 开始下载单个文件事件参数
    /// </summary>
    public readonly struct DownloadFileStartedEventArgs
    {
        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// 创建开始下载单个文件事件参数实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="fileSize">文件大小（单位：字节）</param>
        public DownloadFileStartedEventArgs(string packageName, string bundleName, string fileName, long fileSize)
        {
            PackageName = packageName;
            BundleName = bundleName;
            FileName = fileName;
            FileSize = fileSize;
        }
    }

}
