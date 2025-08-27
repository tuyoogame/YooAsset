using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件验证（线程版）
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

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly EFileVerifyLevel _fileVerifyLevel;
        private List<VerifyFileElement> _waitingList;
        private List<VerifyFileElement> _verifyingList;
        private int _verifyMaxNum;
        private int _verifyTotalCount;
        private float _verifyStartTime;
        private int _succeedCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;


        internal VerifyCacheFilesOperation(DefaultCacheFileSystem fileSystem, List<VerifyFileElement> elements)
        {
            _fileSystem = fileSystem;
            _waitingList = elements;
            _fileVerifyLevel = fileSystem.FileVerifyLevel;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.InitVerify;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitVerify)
            {
                int fileCount = _waitingList.Count;

                // 设置同时验证的最大数
                ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
                YooLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
                int threads = Math.Min(workerThreads, ioThreads);
                _verifyMaxNum = Math.Min(threads, _fileSystem.FileVerifyMaxConcurrency);
                _verifyTotalCount = fileCount;
                if (_verifyMaxNum < 1)
                    _verifyMaxNum = 1;

                YooLogger.Log($"Verify max concurrency : {_verifyMaxNum}");
                _verifyingList = new List<VerifyFileElement>(_verifyMaxNum);
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                // 检测校验结果
                for (int i = _verifyingList.Count - 1; i >= 0; i--)
                {
                    var verifyElement = _verifyingList[i];
                    int result = verifyElement.Result;
                    if (result != 0)
                    {
                        _verifyingList.RemoveAt(i);
                        RecordVerifyFile(verifyElement);
                    }
                }

                Progress = GetProgress();
                if (_waitingList.Count == 0 && _verifyingList.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
                    YooLogger.Log($"Verify cache files elapsed time {costTime:f1} seconds");
                }

                for (int i = _waitingList.Count - 1; i >= 0; i--)
                {
                    if (OperationSystem.IsBusy)
                        break;

                    if (_verifyingList.Count >= _verifyMaxNum)
                        break;

                    var element = _waitingList[i];
                    bool succeed = ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), element);
                    if (succeed)
                    {
                        _waitingList.RemoveAt(i);
                        _verifyingList.Add(element);
                    }
                    else
                    {
                        YooLogger.Warning("The thread pool is failed queued.");
                        break;
                    }
                }
            }
        }

        private float GetProgress()
        {
            if (_verifyTotalCount == 0)
                return 1f;
            return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
        }
        private void VerifyInThread(object obj)
        {
            VerifyFileElement element = (VerifyFileElement)obj;
            int verifyResult = (int)VerifyingCacheFile(element, _fileVerifyLevel);
            element.Result = verifyResult;
        }
        private void RecordVerifyFile(VerifyFileElement element)
        {
            if (element.Result == (int)EFileVerifyResult.Succeed)
            {
                _succeedCount++;
                var recordFileElement = new RecordFileElement(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                _fileSystem.RecordBundleFile(element.BundleGUID, recordFileElement);
            }
            else
            {
                _failedCount++;
                YooLogger.Warning($"Failed to verify file {element.Result} and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }

        /// <summary>
        /// 验证缓存文件（子线程内操作）
        /// </summary>
        private EFileVerifyResult VerifyingCacheFile(VerifyFileElement element, EFileVerifyLevel verifyLevel)
        {
            try
            {
                if (verifyLevel == EFileVerifyLevel.Low)
                {
                    if (File.Exists(element.InfoFilePath) == false)
                        return EFileVerifyResult.InfoFileNotExisted;
                    if (File.Exists(element.DataFilePath) == false)
                        return EFileVerifyResult.DataFileNotExisted;
                    return EFileVerifyResult.Succeed;
                }
                else
                {
                    if (File.Exists(element.InfoFilePath) == false)
                        return EFileVerifyResult.InfoFileNotExisted;

                    // 解析信息文件获取验证数据
                    _fileSystem.ReadBundleInfoFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
                }
            }
            catch (Exception)
            {
                return EFileVerifyResult.Exception;
            }

            return FileVerifyHelper.FileVerify(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
        }
    }
}