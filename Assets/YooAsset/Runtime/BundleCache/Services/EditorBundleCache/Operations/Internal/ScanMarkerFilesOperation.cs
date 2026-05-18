using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 扫描标记文件操作
    /// </summary>
    internal sealed class ScanMarkerFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Prepare,
            ScanFiles,
            Done,
        }

        private readonly EditorBundleCache _fileCache;
        private IEnumerator<string> _shardEnumerator = null;
        private double _scanStartTime;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 扫描到的标记件信息
        /// </summary>
        public readonly List<ScanFileInfo> Result = new List<ScanFileInfo>(5000);

        /// <summary>
        /// 创建操作实例
        /// </summary>
        /// <param name="fileCache">编辑器文件缓存系统</param>
        internal ScanMarkerFilesOperation(EditorBundleCache fileCache)
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
                    _shardEnumerator = directories.GetEnumerator();
                    _scanStartTime = TimeUtility.RealtimeSinceStartup;
                    _steps = ESteps.ScanFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }

            if (_steps == ESteps.ScanFiles)
            {
                if (ScanFiles())
                    return;

                _shardEnumerator.Dispose();
                _shardEnumerator = null;

                _steps = ESteps.Done;
                SetResult();
                double costTime = TimeUtility.RealtimeSinceStartup - _scanStartTime;
                YooLogger.Log($"Marker file scan completed in {costTime:f1} seconds. Found {Result.Count} marker files.");
            }
        }
        protected override void InternalDispose()
        {
            if (_shardEnumerator != null)
            {
                _shardEnumerator.Dispose();
                _shardEnumerator = null;
            }
        }

        private bool ScanFiles()
        {
            bool hasMore;
            while (true)
            {
                hasMore = _shardEnumerator.MoveNext();
                if (hasMore == false)
                    break;

                string shardFolder = _shardEnumerator.Current;
                var childDirectories = Directory.EnumerateDirectories(shardFolder);
                foreach (string childDirectory in childDirectories)
                {
                    string bundleGuid = Path.GetFileName(childDirectory);
                    string markerFilePath = PathUtility.Combine(childDirectory, EditorBundleCacheConsts.MarkerFileName);
                    if (File.Exists(markerFilePath))
                    {
                        var fileInfo = new ScanFileInfo(bundleGuid, markerFilePath);
                        Result.Add(fileInfo);
                    }
                }

                if (IsBusy)
                    break;
            }

            return hasMore;
        }
    }
}
