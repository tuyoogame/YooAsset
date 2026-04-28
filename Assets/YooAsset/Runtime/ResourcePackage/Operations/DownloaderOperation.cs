using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 下载操作基类，提供资源下载、暂停、恢复和取消功能。
    /// </summary>
    public abstract partial class DownloaderOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Check,
            Downloading,
            Finish,
            Done,
        }

        private const int MaxLoaderCount = 32;
        private readonly string _packageName;
        private readonly int _maxConcurrency;
        private readonly int _retryCount;
        private readonly List<BundleInfo> _bundleInfoList;
        private readonly List<FSDownloadBundleOperation> _downloaders = new List<FSDownloadBundleOperation>(MaxLoaderCount);
        private readonly List<FSDownloadBundleOperation> _removeList = new List<FSDownloadBundleOperation>(MaxLoaderCount);
        private readonly List<FSDownloadBundleOperation> _failedList = new List<FSDownloadBundleOperation>(MaxLoaderCount);

        // 数据相关
        private bool _isPause = false;
        private long _lastDownloadBytes = 0;
        private int _lastDownloadCount = 0;
        private long _completedDownloadBytes = 0;
        private int _completedDownloadCount = 0;
        private ESteps _steps = ESteps.None;


        /// <summary>
        /// 统计的下载文件总数量
        /// </summary>
        public int TotalDownloadCount { get; private set; }

        /// <summary>
        /// 统计的下载文件的总大小
        /// </summary>
        public long TotalDownloadBytes { get; private set; }

        /// <summary>
        /// 当前已经完成的下载总数量
        /// </summary>
        public int CurrentDownloadCount
        {
            get { return _lastDownloadCount; }
        }

        /// <summary>
        /// 当前已经完成的下载总大小
        /// </summary>
        public long CurrentDownloadBytes
        {
            get { return _lastDownloadBytes; }
        }

        /// <summary>
        /// 当下载完成时触发（无论成功或失败）。
        /// </summary>
        public event System.Action<DownloadCompletedEventArgs> DownloadCompleted;

        /// <summary>
        /// 当下载进度更新时触发
        /// </summary>
        public event System.Action<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// 当发生下载错误时触发
        /// </summary>
        public event System.Action<DownloadErrorEventArgs> DownloadError;

        /// <summary>
        /// 当开始下载单个文件时触发
        /// </summary>
        public event System.Action<DownloadFileStartedEventArgs> DownloadFileStarted;


        /// <summary>
        /// 创建下载操作实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="downloadList">下载列表</param>
        /// <param name="maximumConcurrency">最大并发数量</param>
        /// <param name="retryCount">失败重试次数</param>
        internal DownloaderOperation(string packageName, List<BundleInfo> downloadList, int maximumConcurrency, int retryCount)
        {
            _packageName = packageName;
            _bundleInfoList = downloadList;
            _maxConcurrency = UnityEngine.Mathf.Clamp(maximumConcurrency, 1, MaxLoaderCount);
            _retryCount = retryCount;

            // 统计下载信息
            CalculateStatistics();
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            YooLogger.Log($"Beginning download of {TotalDownloadCount} files ({TotalDownloadBytes} bytes).");
            _steps = ESteps.Check;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Check)
            {
                if (_bundleInfoList == null)
                {
                    _steps = ESteps.Done;
                    SetError("Download bundle list is null.");

                    if (DownloadCompleted != null)
                    {
                        var args = DownloadCompletedEventArgs.CreateFailed(_packageName, Error);
                        DownloadCompleted.Invoke(args);
                    }
                }
                else if (_bundleInfoList.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    Progress = 1f;

                    if (DownloadCompleted != null)
                    {
                        var args = DownloadCompletedEventArgs.CreateSucceeded(_packageName);
                        DownloadCompleted.Invoke(args);
                    }
                }
                else
                {
                    _steps = ESteps.Downloading;
                }
            }

            if (_steps == ESteps.Downloading)
            {
                // 检测下载器结果
                _removeList.Clear();
                long downloadBytes = _completedDownloadBytes;
                foreach (var downloader in _downloaders)
                {
                    downloader.UpdateOperation();
                    downloadBytes += downloader.Report.DownloadedBytes;
                    if (downloader.IsDone == false)
                        continue;

                    // 检测是否下载失败
                    if (downloader.Status != EOperationStatus.Succeeded)
                    {
                        _removeList.Add(downloader);
                        _failedList.Add(downloader);
                        continue;
                    }

                    // 下载成功
                    _removeList.Add(downloader);
                    _completedDownloadCount++;
                    _completedDownloadBytes += downloader.Report.DownloadedBytes;
                }

                // 移除已经完成的下载器（无论成功或失败）
                foreach (var downloader in _removeList)
                {
                    _downloaders.Remove(downloader);
                }

                // 如果下载进度发生变化
                if (_lastDownloadBytes != downloadBytes || _lastDownloadCount != _completedDownloadCount)
                {
                    _lastDownloadBytes = downloadBytes;
                    _lastDownloadCount = _completedDownloadCount;
                    Progress = CalculateProgress();

                    if (DownloadProgressChanged != null)
                    {
                        var data = new DownloadProgressChangedEventArgs(
                            packageName: _packageName,
                            progress: Progress,
                            totalDownloadCount: TotalDownloadCount,
                            totalDownloadBytes: TotalDownloadBytes,
                            currentDownloadCount: _lastDownloadCount,
                            currentDownloadBytes: _lastDownloadBytes);
                        DownloadProgressChanged.Invoke(data);
                    }
                }

                // 动态创建新的下载器到最大数量限制
                // 注意：每帧仅创建一个下载器，将初始化开销分摊到多帧，避免移动端单帧尖峰。
                // 注意：如果期间有下载失败的文件，暂停动态创建下载器
                if (_bundleInfoList.Count > 0 && _failedList.Count == 0)
                {
                    if (_isPause)
                        return;

                    if (_downloaders.Count < _maxConcurrency)
                    {
                        int index = _bundleInfoList.Count - 1;
                        var bundleInfo = _bundleInfoList[index];
                        var downloader = bundleInfo.CreateBundleDownloader(_retryCount);
                        downloader.StartOperation();
                        this.AddChildOperation(downloader);

                        _downloaders.Add(downloader);
                        _bundleInfoList.RemoveAt(index);

                        if (DownloadFileStarted != null)
                        {
                            var data = new DownloadFileStartedEventArgs(
                                packageName: _packageName,
                                bundleName: bundleInfo.Bundle.BundleName,
                                fileName: bundleInfo.Bundle.GetFileName(),
                                fileSize: bundleInfo.Bundle.FileSize);
                            DownloadFileStarted.Invoke(data);
                        }
                    }
                }

                // 下载结束
                if (_downloaders.Count == 0)
                {
                    _steps = ESteps.Finish;
                }
            }

            if (_steps == ESteps.Finish)
            {
                if (_failedList.Count > 0)
                {
                    var failedDownloader = _failedList[0];
                    string bundleName = failedDownloader.Bundle.BundleName;
                    _steps = ESteps.Done;
                    SetError($"Failed to download file: '{bundleName}'.");

                    if (DownloadError != null)
                    {
                        var data = new DownloadErrorEventArgs(
                            packageName: _packageName,
                            fileName: bundleName,
                            errorInfo: failedDownloader.Error);
                        DownloadError.Invoke(data);
                    }

                    if (DownloadCompleted != null)
                    {
                        var args = DownloadCompletedEventArgs.CreateFailed(_packageName, failedDownloader.Error);
                        DownloadCompleted.Invoke(args);
                    }
                }
                else
                {
                    // 结算成功
                    _steps = ESteps.Done;
                    SetResult();

                    if (DownloadCompleted != null)
                    {
                        var args = DownloadCompletedEventArgs.CreateSucceeded(_packageName);
                        DownloadCompleted.Invoke(args);
                    }
                }
            }
        }

        /// <summary>
        /// 计算下载统计信息
        /// </summary>
        private void CalculateStatistics()
        {
            if (_bundleInfoList != null)
            {
                TotalDownloadBytes = 0;
                TotalDownloadCount = _bundleInfoList.Count;
                foreach (var packageBundle in _bundleInfoList)
                {
                    TotalDownloadBytes += packageBundle.Bundle.FileSize;
                }
            }
            else
            {
                TotalDownloadBytes = 0;
                TotalDownloadCount = 0;
            }
        }

        /// <summary>
        /// 计算下载进度
        /// </summary>
        /// <returns>返回下载进度值（0-1）</returns>
        private float CalculateProgress()
        {
            if (TotalDownloadBytes == 0)
            {
                if (TotalDownloadCount == 0)
                    return 1f;
                return (float)_lastDownloadCount / TotalDownloadCount;
            }
            else
            {
                return (float)_lastDownloadBytes / TotalDownloadBytes;
            }
        }

        /// <summary>
        /// 合并其它下载器
        /// </summary>
        /// <param name="downloader">合并的下载器</param>
        public void Combine(DownloaderOperation downloader)
        {
            if (_packageName != downloader._packageName)
            {
                YooLogger.LogError("Downloaders have different resource packages.");
                return;
            }

            if (Status != EOperationStatus.None)
            {
                YooLogger.LogError("Cannot combine downloaders while a download is in progress.");
                return;
            }

            HashSet<string> combineKeySet = new HashSet<string>();
            foreach (var bundleInfo in _bundleInfoList)
            {
                string combineKey = bundleInfo.GetCombineKey();
                if (combineKeySet.Contains(combineKey) == false)
                {
                    combineKeySet.Add(combineKey);
                }
            }

            // 合并下载列表
            foreach (var bundleInfo in downloader._bundleInfoList)
            {
                string combineKey = bundleInfo.GetCombineKey();
                if (combineKeySet.Contains(combineKey) == false)
                {
                    _bundleInfoList.Add(bundleInfo);
                }
            }

            // 重新统计下载信息
            CalculateStatistics();
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            if (_steps == ESteps.None)
            {
                AsyncOperationSystem.StartOperation(_packageName, this);
            }
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public void PauseDownload()
        {
            _isPause = true;
        }

        /// <summary>
        /// 恢复下载
        /// </summary>
        public void ResumeDownload()
        {
            _isPause = false;
        }

        /// <summary>
        /// 取消下载
        /// </summary>
        public void CancelDownload()
        {
            if (_steps != ESteps.Done)
            {
                _steps = ESteps.Done;
                SetError("Cancelled by user.");

                foreach (var downloader in _downloaders)
                {
                    downloader.AbortOperation();
                }

                if (DownloadCompleted != null)
                {
                    var args = DownloadCompletedEventArgs.CreateFailed(_packageName, Error);
                    DownloadCompleted.Invoke(args);
                }
            }
        }
    }

    /// <summary>
    /// 资源下载操作类
    /// </summary>
    public sealed class ResourceDownloaderOperation : DownloaderOperation
    {
        internal ResourceDownloaderOperation(string packageName, List<BundleInfo> downloadList, int maximumConcurrency, int retryCount)
            : base(packageName, downloadList, maximumConcurrency, retryCount)
        {
        }

        /// <summary>
        /// 创建空的下载器
        /// </summary>
        internal static ResourceDownloaderOperation CreateEmptyDownloader(string packageName)
        {
            List<BundleInfo> downloadList = new List<BundleInfo>();
            var operation = new ResourceDownloaderOperation(packageName, downloadList, 1, 1);
            return operation;
        }
    }
    /// <summary>
    /// 资源解压操作类
    /// </summary>
    public sealed class ResourceUnpackerOperation : DownloaderOperation
    {
        internal ResourceUnpackerOperation(string packageName, List<BundleInfo> downloadList, int maximumConcurrency, int retryCount)
            : base(packageName, downloadList, maximumConcurrency, retryCount)
        {
        }

        /// <summary>
        /// 创建空的解压器
        /// </summary>
        internal static ResourceUnpackerOperation CreateEmptyUnpacker(string packageName)
        {
            List<BundleInfo> downloadList = new List<BundleInfo>();
            var operation = new ResourceUnpackerOperation(packageName, downloadList, 1, 1);
            return operation;
        }
    }
    /// <summary>
    /// 资源导入操作类
    /// </summary>
    public sealed class ResourceImporterOperation : DownloaderOperation
    {
        internal ResourceImporterOperation(string packageName, List<BundleInfo> downloadList, int maximumConcurrency, int retryCount)
            : base(packageName, downloadList, maximumConcurrency, retryCount)
        {
        }

        /// <summary>
        /// 创建空的导入器
        /// </summary>
        internal static ResourceImporterOperation CreateEmptyImporter(string packageName)
        {
            List<BundleInfo> downloadList = new List<BundleInfo>();
            var operation = new ResourceImporterOperation(packageName, downloadList, 1, 1);
            return operation;
        }
    }
}