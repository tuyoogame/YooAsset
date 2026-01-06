using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 下载调度器
    /// </summary>
    /// <remarks>
    /// 管理所有活跃的下载任务，控制并发数量。
    /// </remarks>
    internal class DownloadSchedulerOperation : AsyncOperationBase, IDisposable
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly Dictionary<string, DownloadAndCacheFileOperation> _downloaders = new Dictionary<string, DownloadAndCacheFileOperation>(1000);
        private readonly List<string> _removeList = new List<string>(1000);

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool Paused { get; private set; } = false;

        /// <summary>
        /// 当前活跃的下载任务数
        /// </summary>
        public int ActiveDownloadCount { get; private set; }

        /// <summary>
        /// 当前等待中的下载任务数
        /// </summary>
        public int PendingDownloadCount
        {
            get
            {
                return _downloaders.Count - ActiveDownloadCount;
            }
        }


        /// <summary>
        /// 构造下载中心
        /// </summary>
        public DownloadSchedulerOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
        }
        internal override void InternalUpdate()
        {
            // 驱动下载后台
            _fileSystem.DownloadBackend.Update();

            // 获取可移除的下载器集合
            _removeList.Clear();
            foreach (var valuePair in _downloaders)
            {
                var downloader = valuePair.Value;
                downloader.UpdateOperation();
                if (downloader.IsDone)
                {
                    _removeList.Add(valuePair.Key);
                    continue;
                }

                // 注意：主动终止引用计数为零的下载任务
                if (downloader.RefCount <= 0)
                {
                    _removeList.Add(valuePair.Key);
                    downloader.AbortOperation();
                    continue;
                }
            }

            // 移除下载器
            foreach (var key in _removeList)
            {
                if (_downloaders.TryGetValue(key, out var downloader))
                {
                    RemoveChildOperation(downloader);
                    _downloaders.Remove(key);
                }
            }

            // 暂停时不启动新任务
            if (Paused)
                return;

            // 最大并发数检测
            ActiveDownloadCount = GetProcessingOperationCount();
            if (ActiveDownloadCount != _downloaders.Count)
            {
                int maxConcurrency = _fileSystem.DownloadMaxConcurrency;
                int maxRequestPerFrame = _fileSystem.DownloadMaxRequestPerFrame;
                if (ActiveDownloadCount < maxConcurrency)
                {
                    int startCount = maxConcurrency - ActiveDownloadCount;
                    if (startCount > maxRequestPerFrame)
                        startCount = maxRequestPerFrame;

                    foreach (var operationPair in _downloaders)
                    {
                        var operation = operationPair.Value;
                        if (operation.Status == EOperationStatus.None)
                        {
                            operation.StartOperation();
                            startCount--;
                            if (startCount <= 0)
                                break;
                        }
                    }
                }
            }
        }
        internal override string InternalGetDesc()
        {
            return $"{_fileSystem.GetType().FullName}";
        }

        /// <summary>
        /// 中止所有下载任务
        /// </summary>
        public void AbortAll()
        {
            foreach (var valuePair in _downloaders)
            {
                valuePair.Value.AbortOperation();
            }
            _downloaders.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            AbortAll();
        }

        /// <summary>
        /// 创建下载任务
        /// </summary>
        /// <param name="bundle">资源包信息</param>
        /// <param name="url">下载地址</param>
        /// <returns>下载操作</returns>
        public DownloadAndCacheFileOperation DownloadAndCacheFileAsync(PackageBundle bundle, string url)
        {
            // 查询旧的下载器
            if (_downloaders.TryGetValue(bundle.BundleGUID, out var oldDownloader))
            {
                oldDownloader.Reference();
                return oldDownloader;
            }

            // 创建新的下载器
            DownloadAndCacheFileOperation newDownloader;
            bool isRequestLocalFile = DownloadSystemHelper.IsRequestLocalFile(url);
            if (isRequestLocalFile)
            {
                newDownloader = new DownloadAndCacheLocalFileOperation(_fileSystem, bundle, url);
            }
            else
            {
                newDownloader = new DownloadAndCacheRemoteFileOperation(_fileSystem, bundle, url);
            }

            AddChildOperation(newDownloader);
            _downloaders.Add(bundle.BundleGUID, newDownloader);
            newDownloader.Reference();
            return newDownloader;
        }

        /// <summary>
        /// 获取正在进行中的下载器总数
        /// </summary>
        private int GetProcessingOperationCount()
        {
            int count = 0;
            foreach (var operationPair in _downloaders)
            {
                var operation = operationPair.Value;
                if (operation.Status != EOperationStatus.None)
                    count++;
            }
            return count;
        }
    }
}
