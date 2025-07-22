using System;
using System.Collections.Generic;

namespace YooAsset
{
    internal class DownloadCenterOperation : AsyncOperationBase
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        protected readonly Dictionary<string, UnityDownloadFileOperation> _downloaders = new Dictionary<string, UnityDownloadFileOperation>(1000);
        protected readonly List<string> _removeList = new List<string>(1000);

        public DownloadCenterOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
        }
        internal override void InternalUpdate()
        {
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
                    Childs.Remove(downloader);
                    _downloaders.Remove(key);
                }
            }

            // 最大并发数检测
            int processCount = GetProcessingOperationCount();
            if (processCount != _downloaders.Count)
            {
                if (processCount < _fileSystem.DownloadMaxConcurrency)
                {
                    int startCount = _fileSystem.DownloadMaxConcurrency - processCount;
                    if (startCount > _fileSystem.DownloadMaxRequestPerFrame)
                        startCount = _fileSystem.DownloadMaxRequestPerFrame;

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

        /// <summary>
        /// 创建下载任务
        /// </summary>
        public UnityDownloadFileOperation DownloadFileAsync(PackageBundle bundle, string url)
        {
            // 查询旧的下载器
            if (_downloaders.TryGetValue(bundle.BundleGUID, out var oldDownloader))
            {
                oldDownloader.Reference();
                return oldDownloader;
            }

            // 创建新的下载器
            UnityDownloadFileOperation newDownloader;
            bool isRequestLocalFile = DownloadSystemHelper.IsRequestLocalFile(url);
            if (isRequestLocalFile)
            {
                newDownloader = new UnityDownloadLocalFileOperation(_fileSystem, bundle, url);
                AddChildOperation(newDownloader);
                _downloaders.Add(bundle.BundleGUID, newDownloader);
            }
            else
            {
                if (bundle.FileSize >= _fileSystem.ResumeDownloadMinimumSize)
                {
                    newDownloader = new UnityDownloadResumeFileOperation(_fileSystem, bundle, url);
                    AddChildOperation(newDownloader);
                    _downloaders.Add(bundle.BundleGUID, newDownloader);
                }
                else
                {
                    newDownloader = new UnityDownloadNormalFileOperation(_fileSystem, bundle, url);
                    AddChildOperation(newDownloader);
                    _downloaders.Add(bundle.BundleGUID, newDownloader);
                }
            }

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