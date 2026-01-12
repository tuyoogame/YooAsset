using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件验证（线程版），验证缓存目录中的文件。
    /// </summary>
    internal sealed class VerifyCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly EFileVerifyLevel _verifyLevel;
        private readonly int _fileVerifyMaxConcurrency;
        private readonly List<SearchFileInfo> _pendingVerifyList;
        private List<SearchFileInfo> _activeVerifyList;
        private int _maxConcurrentVerifyCount;
        private int _verifyTotalCount;
        private double _verifyStartTime;
        private int _successCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建缓存文件验证操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="verifyLevel">文件校验等级</param>
        /// <param name="fileVerifyMaxConcurrency">文件校验最大并发数</param>
        /// <param name="elements">待验证的搜索文件信息列表</param>
        internal VerifyCacheFilesOperation(SandboxBundleCache fileCache, EFileVerifyLevel verifyLevel, int fileVerifyMaxConcurrency, List<SearchFileInfo> elements)
        {
            _fileCache = fileCache;
            _verifyLevel = verifyLevel;
            _fileVerifyMaxConcurrency = fileVerifyMaxConcurrency;
            _pendingVerifyList = elements;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.InitVerify;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitVerify)
            {
                // 设置同时验证的最大数
                int processorCount = Environment.ProcessorCount * 2 + 1;
                _maxConcurrentVerifyCount = Math.Min(processorCount, _fileVerifyMaxConcurrency);
                if (_maxConcurrentVerifyCount < 1)
                    _maxConcurrentVerifyCount = 1;

                YooLogger.Log($"Maximum verify concurrency is {_maxConcurrentVerifyCount}.");
                _activeVerifyList = new List<SearchFileInfo>(_maxConcurrentVerifyCount);
                _verifyStartTime = TimeUtility.RealtimeSinceStartup;
                _verifyTotalCount = _pendingVerifyList.Count;
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                // 检测校验结果
                for (int i = _activeVerifyList.Count - 1; i >= 0; i--)
                {
                    var verifyElement = _activeVerifyList[i];
                    int resultCode = verifyElement.VerifyResultCode; //注意: 一次命令取值
                    if (resultCode != 0)
                    {
                        _activeVerifyList.RemoveAt(i);
                        if (resultCode == (int)EFileVerifyResult.Succeed)
                        {
                            _successCount++;
                            var cacheEntry = new SandboxBundleCacheEntry(verifyElement.BundleGuid, verifyElement.InfoFilePath, verifyElement.DataFilePath);
                            _fileCache.AddEntry(verifyElement.BundleGuid, cacheEntry);
                        }
                        else
                        {
                            _failedCount++;
                            YooLogger.LogWarning($"File verification failed (code: {resultCode}). Deleting files: '{verifyElement.FolderPath}'.");
                            verifyElement.DeleteCacheFolder();
                        }
                    }
                }

                Progress = GetProgress();
                if (_pendingVerifyList.Count == 0 && _activeVerifyList.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    double costTime = TimeUtility.RealtimeSinceStartup - _verifyStartTime;
                    YooLogger.Log($"Cache file verification completed in {costTime:f1} seconds.");
                }

                for (int i = _pendingVerifyList.Count - 1; i >= 0; i--)
                {
                    if (IsBusy)
                        break;

                    if (_activeVerifyList.Count >= _maxConcurrentVerifyCount)
                        break;

                    var element = _pendingVerifyList[i];
                    bool succeed = ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyFileInThread), element);
                    if (succeed == false)
                        VerifyFileInThread(element);

                    _pendingVerifyList.RemoveAt(i);
                    _activeVerifyList.Add(element);
                }
            }
        }
        private float GetProgress()
        {
            if (_verifyTotalCount == 0)
                return 1f;
            return (float)(_successCount + _failedCount) / _verifyTotalCount;
        }

        // 验证缓存文件（子线程内操作）
        private void VerifyFileInThread(object obj)
        {
            SearchFileInfo element = (SearchFileInfo)obj;
            int verifyResultCode = (int)VerifyFile(element, _verifyLevel);
            element.VerifyResultCode = verifyResultCode; //注意: 一次命令赋值
        }
        private EFileVerifyResult VerifyFile(SearchFileInfo element, EFileVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(element.InfoFilePath) == false)
                    return EFileVerifyResult.InfoFileNotExisted;
                if (File.Exists(element.DataFilePath) == false)
                    return EFileVerifyResult.DataFileNotExisted;

                byte[] binaryData = FileUtility.ReadAllBytes(element.InfoFilePath);
                if (binaryData.Length < SandboxBundleCacheConsts.InfoFileExpectedSize)
                    return EFileVerifyResult.InfoFileInvalid;

                var reader = new BufferReader(binaryData);
                uint fileMagic = reader.ReadUInt32();
                if (fileMagic != SandboxBundleCacheConsts.InfoFileMagic)
                    return EFileVerifyResult.InfoFileMagicError;

                int fileVersion = reader.ReadInt32();
                if (fileVersion != SandboxBundleCacheConsts.InfoFileVersion)
                    return EFileVerifyResult.InfoFileVersionError;

                uint dataFileCRC = reader.ReadUInt32();
                long dataFileSize = reader.ReadInt64();
                long createdAtTicks = reader.ReadInt64();
                long lastAccessAtTicks = reader.ReadInt64();

                if (verifyLevel == EFileVerifyLevel.Low)
                    return EFileVerifyResult.Succeed;
                else if (verifyLevel == EFileVerifyLevel.Middle)
                    return FileVerifyHelper.VerifyFile(element.DataFilePath, dataFileSize, 0);
                else if (verifyLevel == EFileVerifyLevel.High)
                    return FileVerifyHelper.VerifyFile(element.DataFilePath, dataFileSize, dataFileCRC);
                else
                    throw new YooInternalException($"Unexpected verify level: {verifyLevel}.");
            }
            catch (Exception ex)
            {
                YooLogger.LogError($"File verification exception: {ex.Message}.");
                return EFileVerifyResult.Exception;
            }
        }
    }
}
