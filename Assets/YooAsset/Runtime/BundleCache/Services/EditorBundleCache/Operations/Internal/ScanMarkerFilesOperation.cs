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
        private IEnumerator<string> _bundleEnumerator;
        private string[] _shardFolders;
        private int _shardIndex;
        private double _startTime;
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
                    _startTime = TimeUtility.RealtimeSinceStartup;
                    _shardFolders = Directory.GetDirectories(_fileCache.RootPath);
                    _shardIndex = 0;
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

                _steps = ESteps.Done;
                SetResult();
                double costTime = TimeUtility.RealtimeSinceStartup - _startTime;
                YooLogger.Log($"Marker file scan completed in {costTime:f1} seconds. Found {Result.Count} marker files.");
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

        private bool ScanFiles()
        {
            while (_shardIndex < _shardFolders.Length)
            {
                if (_bundleEnumerator == null)
                {
                    string shardFolder = _shardFolders[_shardIndex];
                    _bundleEnumerator = Directory.EnumerateDirectories(shardFolder).GetEnumerator();
                }

                if (_bundleEnumerator.MoveNext())
                {
                    string childDirectory = _bundleEnumerator.Current;
                    string bundleGuid = Path.GetFileName(childDirectory);
                    string markerFilePath = PathUtility.Combine(childDirectory, EditorBundleCacheConsts.MarkerFileName);
                    if (File.Exists(markerFilePath))
                    {
                        var fileInfo = new ScanFileInfo(bundleGuid, markerFilePath);
                        Result.Add(fileInfo);
                    }
                }
                else
                {
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
