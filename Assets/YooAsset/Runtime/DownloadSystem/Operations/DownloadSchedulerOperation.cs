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
    internal class DownloadSchedulerOperation : AsyncOperationBase
    {
        /// <summary>
        /// 调度器配置
        /// </summary>
        public readonly struct Configuration
        {
            /// <summary>
            /// 调度器名称
            /// </summary>
            /// <remarks>
            /// 主要用于调试信息和运行时描述输出
            /// </remarks>
            public string SchedulerName { get; }

            /// <summary>
            /// 下载后端实现
            /// </summary>
            public IDownloadBackend DownloadBackend { get; }

            /// <summary>
            /// 最大并发下载数量
            /// </summary>
            public int MaxConcurrency { get; }

            /// <summary>
            /// 每帧最多启动的请求数量
            /// </summary>
            /// <remarks>
            /// 用于限制单帧内集中创建过多请求，降低瞬时开销。
            /// </remarks>
            public int MaxRequestsPerFrame { get; }

            /// <summary>
            /// 构造调度器配置
            /// </summary>
            /// <param name="schedulerName">调度器名称</param>
            /// <param name="downloadBackend">下载后端实现</param>
            /// <param name="maxConcurrency">最大并发下载数量</param>
            /// <param name="maxRequestsPerFrame">每帧最多启动的请求数量</param>
            public Configuration(string schedulerName, IDownloadBackend downloadBackend, int maxConcurrency, int maxRequestsPerFrame)
            {
                SchedulerName = schedulerName;
                DownloadBackend = downloadBackend;
                MaxConcurrency = maxConcurrency;
                MaxRequestsPerFrame = maxRequestsPerFrame;
            }
        }

        private readonly Dictionary<string, DownloadFileBaseOperation> _operationMap = new Dictionary<string, DownloadFileBaseOperation>(1000);
        private readonly List<DownloadFileBaseOperation> _operationList = new List<DownloadFileBaseOperation>(1000);
        private readonly List<DownloadFileBaseOperation> _updateSnapshot = new List<DownloadFileBaseOperation>(1000);
        private readonly Configuration _config;

        /// <summary>
        /// 是否暂停调度
        /// </summary>
        /// <remarks>
        /// 暂停时不启动新任务，但已启动的任务继续执行。
        /// </remarks>
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
                return _operationList.Count - ActiveDownloadCount;
            }
        }

        /// <summary>
        /// 构造下载调度器
        /// </summary>
        /// <param name="config">调度器配置</param>
        public DownloadSchedulerOperation(Configuration config)
        {
            _config = config;
        }
        protected override void InternalStart()
        {
        }
        protected override void InternalUpdate()
        {
            // 驱动下载后台
            _config.DownloadBackend.Update();

            // 快照遍历（避免在遍历中修改原列表）
            _updateSnapshot.Clear();
            _updateSnapshot.AddRange(_operationList);
            _operationList.Clear();

            foreach (var downloader in _updateSnapshot)
            {
                downloader.UpdateOperation();
                if (downloader.IsDone)
                {
                    RemoveChildOperation(downloader);
                    _operationMap.Remove(downloader.Bundle.BundleGuid);
                    continue;
                }

                // 注意：主动终止引用计数为零的下载任务
                if (downloader.ReferenceCount <= 0)
                {
                    downloader.AbortOperation();
                    RemoveChildOperation(downloader);
                    _operationMap.Remove(downloader.Bundle.BundleGuid);
                    continue;
                }

                _operationList.Add(downloader);
            }

            // 注意：统计始终保持最新
            ActiveDownloadCount = GetActiveOperationCount();

            // 暂停时不启动新任务
            if (Paused)
                return;

            // 最大并发数检测
            if (ActiveDownloadCount != _operationList.Count)
            {
                int maxConcurrency = _config.MaxConcurrency;
                int maxRequestPerFrame = _config.MaxRequestsPerFrame;
                if (ActiveDownloadCount < maxConcurrency)
                {
                    int startCount = maxConcurrency - ActiveDownloadCount;
                    if (startCount > maxRequestPerFrame)
                        startCount = maxRequestPerFrame;

                    foreach (var operation in _operationList)
                    {
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
        protected override string InternalGetDescription()
        {
            return _config.SchedulerName;
        }
        private int GetActiveOperationCount()
        {
            int count = 0;
            foreach (var operation in _operationList)
            {
                if (operation.Status != EOperationStatus.None)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 暂停调度器
        /// </summary>
        public void PauseScheduler()
        {
            Paused = true;
        }

        /// <summary>
        /// 恢复调度器
        /// </summary>
        public void ResumeScheduler()
        {
            Paused = false;
        }

        /// <summary>
        /// 尝试获取指定资源包对应的下载任务
        /// </summary>
        /// <param name="bundle">目标资源包</param>
        /// <returns>已存在的下载任务（自动增加引用），不存在返回 null。</returns>
        public DownloadFileBaseOperation TryGetDownloadOperation(PackageBundle bundle)
        {
            if (_operationMap.TryGetValue(bundle.BundleGuid, out var oldDownloader))
            {
                oldDownloader.Reference();
                return oldDownloader;
            }
            return null;
        }

        /// <summary>
        /// 注册一个新的下载任务
        /// </summary>
        /// <param name="downloadFileOp">要注册的下载任务</param>
        /// <remarks>
        /// 注册后该任务会被纳入调度器统一管理，并自动增加一次引用。
        /// </remarks>
        public void RegisterDownloadOperation(DownloadFileBaseOperation downloadFileOp)
        {
            string bundleGuid = downloadFileOp.Bundle.BundleGuid;
            if (_operationMap.ContainsKey(bundleGuid))
                throw new YooInternalException($"Download operation is already registered: '{bundleGuid}'.");

            AddChildOperation(downloadFileOp);
            _operationMap.Add(bundleGuid, downloadFileOp);
            _operationList.Add(downloadFileOp);
            downloadFileOp.Reference();
        }
    }
}
