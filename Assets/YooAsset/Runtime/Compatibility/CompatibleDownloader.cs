#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - DownloaderOperation 兼容
// 通过 partial class 为 DownloaderOperation 补充 v2.3 的旧式回调属性和 BeginDownload 方法。

using System;

namespace YooAsset
{
    #region v2.3 委托和数据类型
    /// <summary>
    /// v2.3 下载器结束回调委托
    /// </summary>
    [Obsolete("Use DownloadCompleted event instead.")]
    public delegate void DownloaderFinish(DownloaderFinishData data);

    /// <summary>
    /// v2.3 下载进度更新回调委托
    /// </summary>
    [Obsolete("Use DownloadProgressChanged event instead.")]
    public delegate void DownloadUpdateDelegate(DownloadUpdateData data);

    /// <summary>
    /// v2.3 下载错误回调委托（重命名以避免与 v3 event DownloadError 冲突）
    /// </summary>
    [Obsolete("Use DownloadError event instead.")]
    public delegate void DownloadErrorDelegate(DownloadErrorData data);

    /// <summary>
    /// v2.3 开始下载文件回调委托
    /// </summary>
    [Obsolete("Use DownloadFileStarted event instead.")]
    public delegate void DownloadFileBeginDelegate(DownloadFileData data);

    /// <summary>
    /// v2.3 下载器结束数据
    /// </summary>
    [Obsolete("Use DownloadCompletedEventArgs instead.")]
    public struct DownloaderFinishData
    {
        public string PackageName;
        public bool Succeed;
    }

    /// <summary>
    /// v2.3 下载进度更新数据
    /// </summary>
    [Obsolete("Use DownloadProgressChangedEventArgs instead.")]
    public struct DownloadUpdateData
    {
        public string PackageName;
        public float Progress;
        public int TotalDownloadCount;
        public int CurrentDownloadCount;
        public long TotalDownloadBytes;
        public long CurrentDownloadBytes;
    }

    /// <summary>
    /// v2.3 下载错误数据
    /// </summary>
    [Obsolete("Use DownloadErrorEventArgs instead.")]
    public struct DownloadErrorData
    {
        public string PackageName;
        public string FileName;
        public string ErrorInfo;
    }

    /// <summary>
    /// v2.3 下载文件数据
    /// </summary>
    [Obsolete("Use DownloadFileStartedEventArgs instead.")]
    public struct DownloadFileData
    {
        public string PackageName;
        public string FileName;
        public long FileSize;
    }
    #endregion

    public abstract partial class DownloaderOperation
    {
        private DownloaderFinish _downloadFinishCallback;
        private DownloadUpdateDelegate _downloadUpdateCallback;
        private DownloadErrorDelegate _downloadErrorCallback;
        private DownloadFileBeginDelegate _downloadFileBeginCallback;

        private bool _finishBridged;
        private bool _updateBridged;
        private bool _errorBridged;
        private bool _fileBeginBridged;

        [Obsolete("Use DownloadCompleted event instead.")]
        public DownloaderFinish DownloadFinishCallback
        {
            get => _downloadFinishCallback;
            set
            {
                _downloadFinishCallback = value;
                if (!_finishBridged && value != null)
                {
                    _finishBridged = true;
                    DownloadCompleted += args =>
                    {
                        _downloadFinishCallback?.Invoke(new DownloaderFinishData
                        {
                            PackageName = args.PackageName,
                            Succeed = args.Succeeded
                        });
                    };
                }
            }
        }

        [Obsolete("Use DownloadProgressChanged event instead.")]
        public DownloadUpdateDelegate DownloadUpdateCallback
        {
            get => _downloadUpdateCallback;
            set
            {
                _downloadUpdateCallback = value;
                if (!_updateBridged && value != null)
                {
                    _updateBridged = true;
                    DownloadProgressChanged += args =>
                    {
                        _downloadUpdateCallback?.Invoke(new DownloadUpdateData
                        {
                            PackageName = args.PackageName,
                            Progress = args.Progress,
                            TotalDownloadCount = args.TotalDownloadCount,
                            CurrentDownloadCount = args.CurrentDownloadCount,
                            TotalDownloadBytes = args.TotalDownloadBytes,
                            CurrentDownloadBytes = args.CurrentDownloadBytes
                        });
                    };
                }
            }
        }

        [Obsolete("Use DownloadError event instead.")]
        public DownloadErrorDelegate DownloadErrorCallback
        {
            get => _downloadErrorCallback;
            set
            {
                _downloadErrorCallback = value;
                if (!_errorBridged && value != null)
                {
                    _errorBridged = true;
                    DownloadError += args =>
                    {
                        _downloadErrorCallback?.Invoke(new DownloadErrorData
                        {
                            PackageName = args.PackageName,
                            FileName = args.FileName,
                            ErrorInfo = args.ErrorInfo
                        });
                    };
                }
            }
        }

        [Obsolete("Use DownloadFileStarted event instead.")]
        public DownloadFileBeginDelegate DownloadFileBeginCallback
        {
            get => _downloadFileBeginCallback;
            set
            {
                _downloadFileBeginCallback = value;
                if (!_fileBeginBridged && value != null)
                {
                    _fileBeginBridged = true;
                    DownloadFileStarted += args =>
                    {
                        _downloadFileBeginCallback?.Invoke(new DownloadFileData
                        {
                            PackageName = args.PackageName,
                            FileName = args.FileName,
                            FileSize = args.FileSize
                        });
                    };
                }
            }
        }

        [Obsolete("Use StartDownload() instead.")]
        public void BeginDownload() => StartDownload();
    }
}
#endif
