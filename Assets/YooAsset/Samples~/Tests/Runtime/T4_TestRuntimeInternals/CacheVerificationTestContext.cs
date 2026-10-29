using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 缓存校验逻辑测试的临时文件、日志及线程收尾环境。
/// </summary>
internal sealed class CacheVerificationTestContext : IDisposable
{
    // 标准 CRC32 测试向量 "123456789"，避免使用被测校验器生成期望结果。
    public const int DataSize = 9;
    public const uint DataCrc = 0xCBF43926;
    public const int TimeoutMilliseconds = 10000;

    private readonly ILogger _previousLogger;
    private readonly List<VerifyCacheFilesTask> _tasks = new List<VerifyCacheFilesTask>();
    private readonly List<VerifyCacheFilesOperation> _operations = new List<VerifyCacheFilesOperation>();
    private readonly List<ManualResetEventSlim> _signals = new List<ManualResetEventSlim>();
    private readonly List<SandboxBundleCache> _caches = new List<SandboxBundleCache>();
    private int _fileIndex;
    private bool _clockReplaced;
    private object _previousStopwatch;
    private object _previousFrameStartTime;
    private long _previousTimeSlice;

    public string RootPath { get; private set; }
    public VerificationLogger Logger { get; private set; }

    public CacheVerificationTestContext()
    {
        RootPath = Path.Combine(Path.GetTempPath(), "YooAssetVerifyLogic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(RootPath);
        _previousLogger = YooLogger.Current;
        Logger = new VerificationLogger();
        YooLogger.Current = Logger;
    }

    public SearchFileInfo CreateFile()
    {
        string guid = (++_fileIndex).ToString("x32");
        string folder = Path.Combine(RootPath, guid.Substring(0, 2), guid);
        Directory.CreateDirectory(folder);
        var file = new SearchFileInfo(guid, folder,
            Path.Combine(folder, SandboxBundleCacheConsts.BundleDataFileName),
            Path.Combine(folder, SandboxBundleCacheConsts.BundleInfoFileName));
        File.WriteAllBytes(file.DataFilePath, Encoding.ASCII.GetBytes("123456789"));
        WriteInfo(file);
        return file;
    }

    public void WriteInfo(SearchFileInfo file, long size = DataSize, uint crc = DataCrc,
        uint magic = SandboxBundleCacheConsts.InfoFileMagic, int version = SandboxBundleCacheConsts.InfoFileVersion)
    {
        using (var writer = new BinaryWriter(File.Create(file.InfoFilePath)))
        {
            writer.Write(magic);
            writer.Write(version);
            writer.Write(crc);
            writer.Write(size);
            writer.Write(0L);
            writer.Write(0L);
        }
    }

    public VerifyCacheFilesTask CreateTask(EFileVerifyLevel level, int concurrency, params SearchFileInfo[] files)
    {
        var task = new VerifyCacheFilesTask(level, concurrency, files);
        _tasks.Add(task);
        return task;
    }

    public SandboxBundleCache CreateCache()
    {
        var cache = new SandboxBundleCache("VerifyLogicTest", RootPath, default(SandboxBundleCache.Configuration));
        _caches.Add(cache);
        return cache;
    }

    public VerifyCacheFilesOperation CreateOperation(SandboxBundleCache cache, EFileVerifyLevel level,
        int concurrency, List<SearchFileInfo> files)
    {
        var operation = new VerifyCacheFilesOperation(cache, level, concurrency, files);
        _operations.Add(operation);
        return operation;
    }

    public ManualResetEventSlim CreateSignal()
    {
        var signal = new ManualResetEventSlim(false);
        _signals.Add(signal);
        return signal;
    }

    public static VerifyCacheFilesTask GetTask(VerifyCacheFilesOperation operation)
    {
        return (VerifyCacheFilesTask)RuntimeReflectionUtility.GetField(operation, "_verifyTask");
    }

    public static int GetRunningWorkers(VerifyCacheFilesTask task)
    {
        Thread.MemoryBarrier();
        return (int)RuntimeReflectionUtility.GetField(task, "_runningWorkers");
    }

    public static void WaitUntil(Func<bool> condition, string message)
    {
        // 仅作为线程未退出的保护上限，不断言运行耗时或吞吐量。
        var timer = Stopwatch.StartNew();
        while (condition() == false)
        {
            Assert.Less(timer.ElapsedMilliseconds, TimeoutMilliseconds, message);
            Thread.Sleep(1);
        }
    }

    public static void WaitForTask(VerifyCacheFilesTask task)
    {
        int previous = task.CompletedCount;
        WaitUntil(() =>
        {
            var status = task.Status;
            int completed = task.CompletedCount;
            Assert.That(completed, Is.InRange(previous, task.TotalCount), "完成计数必须单调递增且不超过总数。");
            previous = completed;
            return status == EVerifyTaskStatus.Succeeded || status == EVerifyTaskStatus.Failed;
        }, "校验任务未发布终态。");
        Assert.AreEqual(0, GetRunningWorkers(task), "发布终态前，所有工作项必须退出。");
    }

    /// <summary>
    /// 使用停止的计时器控制预算，避免依赖机器速度或实际帧间隔。
    /// </summary>
    public void SetBusy(bool busy)
    {
        if (_clockReplaced == false)
        {
            _previousStopwatch = GetSchedulerField("_stopwatch").GetValue(null);
            _previousFrameStartTime = GetSchedulerField("_frameStartTime").GetValue(null);
            _previousTimeSlice = AsyncOperationSystem.MaxTimeSlice;
            _clockReplaced = true;
            GetSchedulerField("_stopwatch").SetValue(null, new Stopwatch());
            AsyncOperationSystem.MaxTimeSlice = 10;
        }
        GetSchedulerField("_frameStartTime").SetValue(null, busy ? -10L : 0L);
    }

    public void Dispose()
    {
        bool drained = false;
        try
        {
            foreach (var operation in _operations)
            {
                operation.AbortOperation();
                var task = GetTask(operation);
                if (task != null)
                    _tasks.Add(task);
            }
            foreach (var task in _tasks)
                task.Dispose();
            foreach (var signal in _signals)
                signal.Set();
            foreach (var task in _tasks)
                WaitUntil(() => GetRunningWorkers(task) == 0, "清理测试环境时，校验工作项未退出。");
            drained = true;
        }
        finally
        {
            if (_clockReplaced)
            {
                GetSchedulerField("_stopwatch").SetValue(null, _previousStopwatch);
                GetSchedulerField("_frameStartTime").SetValue(null, _previousFrameStartTime);
                AsyncOperationSystem.MaxTimeSlice = _previousTimeSlice;
            }
            YooLogger.Current = _previousLogger;
            foreach (var cache in _caches)
                cache.Dispose();

            // 工作项全部退出后才释放信号和删除临时文件。
            if (drained)
            {
                foreach (var signal in _signals)
                    signal.Dispose();
                Directory.Delete(RootPath, true);
            }
        }
    }

    private static FieldInfo GetSchedulerField(string name)
    {
        var field = typeof(AsyncOperationSystem).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
        if (field == null)
            throw new MissingFieldException(typeof(AsyncOperationSystem).FullName, name);
        return field;
    }

    internal sealed class VerificationLogger : ILogger
    {
        private int _errorCount;
        private int _warningCount;
        public Action<string> OnError;
        public Action<string> OnWarning;
        public int ErrorCount { get { return Volatile.Read(ref _errorCount); } }
        public int WarningCount { get { return Volatile.Read(ref _warningCount); } }

        public void Log(string message) { }
        public void LogWarning(string message)
        {
            Interlocked.Increment(ref _warningCount);
            if (OnWarning != null)
                OnWarning(message);
        }
        public void LogError(string message)
        {
            Interlocked.Increment(ref _errorCount);
            if (OnError != null)
                OnError(message);
        }
        public void LogException(Exception exception) { LogError(exception.ToString()); }
    }
}
