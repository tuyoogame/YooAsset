using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 搜索缓存文件操作，扫描缓存目录中的文件。
    /// </summary>
    internal sealed class SearchCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Prepare,
            SearchFiles,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private IEnumerator<string> _bundleEnumerator;
        private string[] _shardFolders;
        private int _shardIndex;
        private double _startTime;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 需要验证的元素
        /// </summary>
        public readonly List<SearchFileInfo> Result = new List<SearchFileInfo>(5000);

        /// <summary>
        /// 创建搜索缓存文件操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        internal SearchCacheFilesOperation(SandboxBundleCache fileCache)
        {
            _fileCache = fileCache;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.Prepare;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Prepare)
            {
                if (Directory.Exists(_fileCache.RootPath))
                {
                    _startTime = TimeUtility.RealtimeSinceStartup;
                    _shardFolders = Directory.GetDirectories(_fileCache.RootPath);
                    _shardIndex = 0;
                    _steps = ESteps.SearchFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }

            if (_steps == ESteps.SearchFiles)
            {
                if (SearchFiles())
                    return;

                _steps = ESteps.Done;
                SetResult();
                double costTime = TimeUtility.RealtimeSinceStartup - _startTime;
                YooLogger.Log($"Cache file search completed in {costTime:f1} seconds. Found {Result.Count} cache files.");
            }
        }
        protected override void InternalDispose()
        {
            if (_bundleEnumerator != null)
            {
                _bundleEnumerator.Dispose();
                _bundleEnumerator = null;
            }
        }

        private bool SearchFiles()
        {
            // 说明：按哈希分片逐项扫描 Bundle 目录，保留枚举器以便跨帧继续。
            while (_shardIndex < _shardFolders.Length)
            {
                if (_bundleEnumerator == null)
                {
                    var shardFolder = _shardFolders[_shardIndex];
                    _bundleEnumerator = Directory.EnumerateDirectories(shardFolder).GetEnumerator();
                }

                if (_bundleEnumerator.MoveNext())
                {
                    var childDirectory = _bundleEnumerator.Current;
                    string bundleGuid = Path.GetFileName(childDirectory);
                    if (_fileCache.IsCached(bundleGuid) == false)
                    {
                        // 创建验证元素类
                        string fileRootPath = childDirectory;
                        string dataFilePath = PathUtility.Combine(fileRootPath, SandboxBundleCacheConsts.BundleDataFileName);
                        string infoFilePath = PathUtility.Combine(fileRootPath, SandboxBundleCacheConsts.BundleInfoFileName);
                        var element = new SearchFileInfo(bundleGuid, fileRootPath, dataFilePath, infoFilePath);
                        Result.Add(element);
                    }
                }
                else
                {
                    // 当前哈希分片目录扫描完成，继续下一个哈希分片目录。
                    _bundleEnumerator.Dispose();
                    _bundleEnumerator = null;
                    _shardIndex++;
                    Progress = (float)_shardIndex / _shardFolders.Length;
                }

                if (IsBusy)
                    break;
            }

            return _shardIndex < _shardFolders.Length;
        }
    }
}
