using System;
using System.IO;
using System.Threading;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件校验任务状态
    /// </summary>
    internal enum EVerifyTaskStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None,

        /// <summary>
        /// 正在提交或执行工作项
        /// </summary>
        Processing,

        /// <summary>
        /// 工作项全部校验完毕
        /// </summary>
        Succeeded,

        /// <summary>
        /// 任务失败，所有已提交的工作项均已退出。
        /// </summary>
        Failed,
    }

    /// <summary>
    /// 缓存文件并行校验任务
    /// 约定：所有工作项退出后发布终态。
    /// </summary>
    internal sealed class VerifyCacheFilesTask
    {
        private readonly EFileVerifyLevel _verifyLevel;
        private readonly SearchFileInfo[] _verifyFiles;
        private readonly int _maxConcurrency;

        private readonly object _startLock = new object();
        private volatile EVerifyTaskStatus _status = EVerifyTaskStatus.None;
        private volatile bool _stopVerification = false;
        private int _nextVerifyIndex;
        private int _runningWorkers;
        private int _completedCount;
        private string _error;

        /// <summary>
        /// 待校验的文件总数
        /// </summary>
        public int TotalCount
        {
            get { return _verifyFiles.Length; }
        }

        /// <summary>
        /// 已完成校验的文件数量，包含校验未通过的文件。
        /// </summary>
        public int CompletedCount
        {
            get { return Volatile.Read(ref _completedCount); }
        }

        /// <summary>
        /// 当前任务状态
        /// </summary>
        public EVerifyTaskStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// 任务失败的错误信息
        /// </summary>
        public string Error
        {
            get { return _error; }
        }


        /// <summary>
        /// 创建缓存文件并行校验任务
        /// </summary>
        /// <param name="verifyLevel">文件校验等级</param>
        /// <param name="maxConcurrency">文件校验最大并发数</param>
        /// <param name="verifyFiles">待验证的搜索文件信息数组</param>
        public VerifyCacheFilesTask(EFileVerifyLevel verifyLevel, int maxConcurrency, SearchFileInfo[] verifyFiles)
        {
            _verifyLevel = verifyLevel;
            _verifyFiles = verifyFiles;
            _maxConcurrency = maxConcurrency;
        }

        /// <summary>
        /// 启动并行校验
        /// </summary>
        public void Start()
        {
            if (Status != EVerifyTaskStatus.None)
                throw new InvalidOperationException("Cache verification task cannot be restarted.");

            if (TotalCount == 0)
            {
                _status = EVerifyTaskStatus.Succeeded;
                return;
            }

            _status = EVerifyTaskStatus.Processing;
            _nextVerifyIndex = TotalCount;
            WaitCallback callback = VerifyFilesWorker;

            // 注意：工作项等待此锁释放后才开始校验，确保成功提交数量已登记完成。
            lock (_startLock)
            {
                int workerCount = Math.Min(_maxConcurrency, TotalCount);
                for (int i = 0; i < workerCount; i++)
                {
                    if (ThreadPool.QueueUserWorkItem(callback))
                        _runningWorkers++;
                }

                // 没有成功提交的工作项，直接发布失败状态。
                if (_runningWorkers == 0 && _stopVerification == false)
                {
                    string error = "Failed to queue any cache verification worker.";
                    Interlocked.CompareExchange(ref _error, error, null);
                    _stopVerification = true;
                    _status = EVerifyTaskStatus.Failed;
                }
            }
        }

        /// <summary>
        /// 释放任务
        /// </summary>
        public void Dispose()
        {
            _stopVerification = true;
        }

        /// <summary>
        /// 验证缓存文件（子线程内操作）
        /// </summary>
        private void VerifyFilesWorker(object state)
        {
            try
            {
                lock (_startLock)
                {
                    // 等待提交和计数设置完成，通过后立即释放锁，文件校验在锁外并行执行。
                }

                while (_stopVerification == false)
                {
                    int index = Interlocked.Decrement(ref _nextVerifyIndex);
                    if (index < 0)
                        return;

                    var element = _verifyFiles[index];
                    int verifyResultCode = (int)VerifyFile(element, _verifyLevel);
                    element.VerifyResultCode = verifyResultCode;
                    Interlocked.Increment(ref _completedCount);
                }
            }
            catch (Exception ex)
            {
                // 注意：此处不调用日志处理器，防止自定义日志再次抛出异常。
                string error = $"Cache verification worker failed: {ex.Message}.";
                Interlocked.CompareExchange(ref _error, error, null);
                _stopVerification = true;
            }
            finally
            {
                CompleteWorker();
            }
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
        private void CompleteWorker()
        {
            if (Interlocked.Decrement(ref _runningWorkers) != 0)
                return;

            // 最后一个名额释放后，再根据已记录的错误发布终态。
            if (_error != null)
            {
                _status = EVerifyTaskStatus.Failed;
            }
            else if (_stopVerification == false)
            {
                _stopVerification = true;
                _status = EVerifyTaskStatus.Succeeded;
            }
        }
    }
}
