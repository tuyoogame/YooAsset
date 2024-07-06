using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
    internal class CacheFileElement
    {
        public string PackageName { private set; get; }
        public string BundleGUID { private set; get; }
        public string FileRootPath { private set; get; }
        public string DataFilePath { private set; get; }
        public string InfoFilePath { private set; get; }

        public EFileVerifyResult Result;
        public string DataFileCRC;
        public long DataFileSize;

        public CacheFileElement(string packageName, string bundleGUID, string fileRootPath, string dataFilePath, string infoFilePath)
        {
            PackageName = packageName;
            BundleGUID = bundleGUID;
            FileRootPath = fileRootPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
        }

        public void DeleteFiles()
        {
            try
            {
                Directory.Delete(FileRootPath, true);
            }
            catch (System.Exception e)
            {
                YooLogger.Warning($"Failed to delete cache bundle folder : {e}");
            }
        }
    }

    /// <summary>
    /// 缓存文件验证（线程版）
    /// </summary>
    internal class VerifyCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        private readonly ThreadSyncContext _syncContext = new ThreadSyncContext();
        private readonly DefaultCacheFileSystem _fileSystem;
        private List<CacheFileElement> _waitingList;
        private List<CacheFileElement> _verifyingList;
        private EFileVerifyLevel _verifyLevel = EFileVerifyLevel.Middle;
        private int _verifyMaxNum;
        private int _verifyTotalCount;
        private float _verifyStartTime;
        private int _succeedCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;


        internal VerifyCacheFilesOperation(DefaultCacheFileSystem fileSystem, List<CacheFileElement> elements)
        {
            _fileSystem = fileSystem;
            _waitingList = elements;
            _verifyLevel = _fileSystem.FileVerifyLevel;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.InitVerify;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitVerify)
            {
                int fileCount = _waitingList.Count;

                // 设置同时验证的最大数
                ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
                YooLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
                _verifyMaxNum = Math.Min(workerThreads, ioThreads);
                _verifyTotalCount = fileCount;
                if (_verifyMaxNum < 1)
                    _verifyMaxNum = 1;

                _verifyingList = new List<CacheFileElement>(_verifyMaxNum);
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                _syncContext.Update();

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
                    if (BeginVerifyFileWithThread(element))
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
        private bool BeginVerifyFileWithThread(CacheFileElement element)
        {
            return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), element);
        }
        private void VerifyInThread(object obj)
        {
            CacheFileElement element = (CacheFileElement)obj;
            element.Result = VerifyingCacheFile(element, _verifyLevel);
            _syncContext.Post(VerifyCallback, element);
        }
        private void VerifyCallback(object obj)
        {
            CacheFileElement element = (CacheFileElement)obj;
            _verifyingList.Remove(element);

            if (element.Result == EFileVerifyResult.Succeed)
            {
                _succeedCount++;
                var fileWrapper = new DefaultCacheFileSystem.FileWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                _fileSystem.RecordFile(element.BundleGUID, fileWrapper);
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
        private EFileVerifyResult VerifyingCacheFile(CacheFileElement element, EFileVerifyLevel verifyLevel)
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
                    _fileSystem.ReadInfoFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
                }
            }
            catch (Exception)
            {
                return EFileVerifyResult.Exception;
            }

            return FileSystemHelper.FileVerify(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
        }
    }
}