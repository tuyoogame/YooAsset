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
        private IEnumerator<string> _filesEnumerator = null;
        private double _verifyStartTime;
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
                    var directories = Directory.EnumerateDirectories(_fileCache.RootPath);
                    _filesEnumerator = directories.GetEnumerator();
                    _verifyStartTime = TimeUtility.RealtimeSinceStartup;
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

                _filesEnumerator.Dispose();
                _filesEnumerator = null;

                _steps = ESteps.Done;
                SetResult();
                double costTime = TimeUtility.RealtimeSinceStartup - _verifyStartTime;
                YooLogger.Log($"Cache file search completed in {costTime:f1} seconds.");
            }
        }
        protected override void InternalDispose()
        {
            if (_filesEnumerator != null)
            {
                _filesEnumerator.Dispose();
                _filesEnumerator = null;
            }
        }

        private bool SearchFiles()
        {
            bool isFindItem;
            while (true)
            {
                isFindItem = _filesEnumerator.MoveNext();
                if (isFindItem == false)
                    break;

                var rootFolder = _filesEnumerator.Current;
                var childDirectories = Directory.EnumerateDirectories(rootFolder);
                foreach (var childDirectory in childDirectories)
                {
                    string bundleGuid = Path.GetFileName(childDirectory);
                    if (_fileCache.IsCached(bundleGuid))
                        continue;

                    // 创建验证元素类
                    string fileRootPath = childDirectory;
                    string dataFilePath = PathUtility.Combine(fileRootPath, SandboxBundleCacheConsts.BundleDataFileName);
                    string infoFilePath = PathUtility.Combine(fileRootPath, SandboxBundleCacheConsts.BundleInfoFileName);
                    var element = new SearchFileInfo(bundleGuid, fileRootPath, dataFilePath, infoFilePath);
                    Result.Add(element);
                }

                if (IsBusy)
                    break;
            }

            return isFindItem;
        }
    }
}
