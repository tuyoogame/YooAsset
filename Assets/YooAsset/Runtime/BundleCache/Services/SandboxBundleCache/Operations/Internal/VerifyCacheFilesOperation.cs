using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件验证操作
    /// </summary>
    /// <remarks>
    /// 后台任务负责文件校验，成功后由主线程分帧登记缓存和清理无效文件。
    /// </remarks>
    internal sealed class VerifyCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            StartVerify,
            UpdateVerify,
            ProcessResults,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly EFileVerifyLevel _verifyLevel;
        private readonly int _verifyMaxConcurrency;
        private readonly List<SearchFileInfo> _elements;

        private VerifyCacheFilesTask _verifyTask;
        private SearchFileInfo[] _verifyFiles;
        private double _verifyStartTime;
        private int _processIndex;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建缓存文件验证操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="verifyLevel">文件校验等级</param>
        /// <param name="verifyMaxConcurrency">文件校验最大并发数</param>
        /// <param name="elements">待验证的搜索文件信息列表</param>
        internal VerifyCacheFilesOperation(SandboxBundleCache fileCache, EFileVerifyLevel verifyLevel, int verifyMaxConcurrency, List<SearchFileInfo> elements)
        {
            _fileCache = fileCache;
            _verifyLevel = verifyLevel;
            _verifyMaxConcurrency = verifyMaxConcurrency;
            _elements = elements;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.StartVerify;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.StartVerify)
            {
                // 计算同时验证的最大数
                int processorCount = Environment.ProcessorCount * 2 + 1;
                int maxConcurrency = Math.Min(processorCount, _verifyMaxConcurrency);
                maxConcurrency = Math.Max(maxConcurrency, 1);
                YooLogger.Log($"Maximum verify concurrency is {maxConcurrency}.");

                _verifyStartTime = TimeUtility.RealtimeSinceStartup;
                _verifyFiles = _elements.ToArray();
                _verifyTask = new VerifyCacheFilesTask(_verifyLevel, maxConcurrency, _verifyFiles);
                _verifyTask.Start();
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                int progressTotal = _verifyTask.TotalCount;
                int progressRemain = _verifyTask.TotalCount - _verifyTask.CompletedCount;
                Progress = CalculateMultiStageProgress(0, 2, progressRemain, progressTotal);

                var taskStatus = _verifyTask.Status;
                if (taskStatus == EVerifyTaskStatus.Processing)
                    return;

                if (taskStatus == EVerifyTaskStatus.Failed)
                {
                    _steps = ESteps.Done;
                    SetError(_verifyTask.Error);
                    return;
                }
                else if (taskStatus == EVerifyTaskStatus.Succeeded)
                {
                    _steps = ESteps.ProcessResults;
                }
            }

            if (_steps == ESteps.ProcessResults)
            {
                // 校验任务成功后，在主线程分帧处理结果。
                while (_processIndex < _verifyFiles.Length && IsBusy == false)
                {
                    var verifyElement = _verifyFiles[_processIndex];
                    int resultCode = verifyElement.VerifyResultCode;
                    if (resultCode == (int)EFileVerifyResult.Succeed)
                    {
                        var cacheEntry = new SandboxBundleCacheEntry(verifyElement.BundleGuid, verifyElement.InfoFilePath, verifyElement.DataFilePath);
                        _fileCache.AddEntry(verifyElement.BundleGuid, cacheEntry);
                    }
                    else
                    {
                        YooLogger.LogWarning($"File verification failed (code: {resultCode}). Deleting files: '{verifyElement.FolderPath}'.");
                        verifyElement.DeleteCacheFolder();
                    }
                    _processIndex++;
                }

                int progressTotal = _verifyFiles.Length;
                int progressRemain = _verifyFiles.Length - _processIndex;
                Progress = CalculateMultiStageProgress(1, 2, progressRemain, progressTotal);
                if (_processIndex < progressTotal)
                    return;

                _steps = ESteps.Done;
                SetResult();
                double costTime = TimeUtility.RealtimeSinceStartup - _verifyStartTime;
                YooLogger.Log($"Cache file verification completed in {costTime:f1} seconds.");
            }
        }
        protected override void InternalDispose()
        {
            if (_verifyTask != null)
                _verifyTask.Dispose();
        }
    }
}
