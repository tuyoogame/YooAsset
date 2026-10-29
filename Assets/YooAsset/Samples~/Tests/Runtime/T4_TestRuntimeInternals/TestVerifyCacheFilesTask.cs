using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 验证后台校验任务的结果、并发收尾、异常及释放行为。
/// </summary>
public class TestVerifyCacheFilesTask
{
    public void RuntimeTester()
    {
        TestEmptyTask();
        TestParallelCompletion(1);
        TestParallelCompletion(4);
        TestParallelCompletion(32);
        TestVerificationLevels(EFileVerifyLevel.Low);
        TestVerificationLevels(EFileVerifyLevel.Middle);
        TestVerificationLevels(EFileVerifyLevel.High);
        TestFileException();
        TestWorkerFailure();
        TestDisposeBeforeVerification();
        TestDisposeDuringVerification();
    }

    private static void TestEmptyTask()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var task = context.CreateTask(EFileVerifyLevel.High, 8);
            Assert.AreEqual(EVerifyTaskStatus.None, task.Status);
            Assert.AreEqual(0, task.TotalCount);
            Assert.AreEqual(0, task.CompletedCount);
            Assert.IsNull(task.Error);
            task.Start();
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status);
            Assert.AreEqual(0, CacheVerificationTestContext.GetRunningWorkers(task));
            Assert.Throws<InvalidOperationException>(task.Start);
            task.Dispose();
            task.Dispose();
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status);
        }
    }

    private static void TestParallelCompletion(int concurrency)
    {
        using (var context = new CacheVerificationTestContext())
        {
            var files = new SearchFileInfo[17];
            for (int i = 0; i < files.Length; i++)
                files[i] = context.CreateFile();
            var task = context.CreateTask(EFileVerifyLevel.High, concurrency, files);
            Assert.AreEqual(files.Length, task.TotalCount);
            task.Start();

            // 启动后不调用 Operation.Update，任务应自行完成整个数组的校验。
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status, task.Error);
            Assert.AreEqual(files.Length, task.CompletedCount);
            Assert.IsNull(task.Error);
            foreach (var file in files)
            {
                Assert.AreEqual((int)EFileVerifyResult.Succeed, file.VerifyResultCode, file.BundleGuid);
                Assert.IsTrue(Directory.Exists(file.FolderPath));
            }
            Assert.AreEqual(0, context.Logger.ErrorCount);
            Assert.AreEqual(0, context.Logger.WarningCount);
            Assert.Throws<InvalidOperationException>(task.Start);
        }
    }

    private static void TestVerificationLevels(EFileVerifyLevel level)
    {
        using (var context = new CacheVerificationTestContext())
        {
            var files = new SearchFileInfo[9];
            for (int i = 0; i < files.Length; i++)
                files[i] = context.CreateFile();

            File.Delete(files[1].InfoFilePath);
            File.Delete(files[2].DataFilePath);
            File.WriteAllBytes(files[3].InfoFilePath, new byte[SandboxBundleCacheConsts.InfoFileExpectedSize - 1]);
            context.WriteInfo(files[4], magic: 0);
            context.WriteInfo(files[5], version: SandboxBundleCacheConsts.InfoFileVersion + 1);
            context.WriteInfo(files[6], size: CacheVerificationTestContext.DataSize + 1);
            context.WriteInfo(files[7], size: CacheVerificationTestContext.DataSize - 1);
            // 保持文件大小不变，仅破坏内容，用于区分 Middle 与 High。
            File.WriteAllBytes(files[8].DataFilePath, new byte[CacheVerificationTestContext.DataSize]);

            var expected = new[]
            {
                EFileVerifyResult.Succeed,
                EFileVerifyResult.InfoFileNotExisted,
                EFileVerifyResult.DataFileNotExisted,
                EFileVerifyResult.InfoFileInvalid,
                EFileVerifyResult.InfoFileMagicError,
                EFileVerifyResult.InfoFileVersionError,
                level == EFileVerifyLevel.Low ? EFileVerifyResult.Succeed : EFileVerifyResult.DataFileNotComplete,
                level == EFileVerifyLevel.Low ? EFileVerifyResult.Succeed : EFileVerifyResult.DataFileOverflow,
                level == EFileVerifyLevel.High ? EFileVerifyResult.DataFileCrcError : EFileVerifyResult.Succeed,
            };

            var task = context.CreateTask(level, 4, files);
            task.Start();
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status, "文件校验未通过不代表后台任务失败。");
            Assert.AreEqual(files.Length, task.CompletedCount);
            Assert.IsNull(task.Error);
            for (int i = 0; i < files.Length; i++)
            {
                Assert.AreEqual((int)expected[i], files[i].VerifyResultCode, $"level={level}, file={i}");
                Assert.IsTrue(Directory.Exists(files[i].FolderPath), "Task 不负责删除无效文件。");
            }
            Assert.AreEqual(0, context.Logger.ErrorCount);
        }
    }

    private static void TestFileException()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var files = new[] { context.CreateFile(), context.CreateFile() };
            // 非法等级触发 VerifyFile 的异常分支，日志正常返回时应产生文件失败结果。
            var task = context.CreateTask((EFileVerifyLevel)99, 1, files);
            task.Start();
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status, task.Error);
            Assert.AreEqual(2, task.CompletedCount);
            Assert.IsNull(task.Error);
            Assert.AreEqual(2, context.Logger.ErrorCount);
            foreach (var file in files)
                Assert.AreEqual((int)EFileVerifyResult.Exception, file.VerifyResultCode);
        }
    }

    private static void TestWorkerFailure()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var firstEntered = context.CreateSignal();
            var secondEntered = context.CreateSignal();
            var releaseFirst = context.CreateSignal();
            var releaseSecond = context.CreateSignal();
            int errorIndex = 0;
            context.Logger.OnError = message =>
            {
                int index = Interlocked.Increment(ref errorIndex);
                var entered = index == 1 ? firstEntered : secondEntered;
                var release = index == 1 ? releaseFirst : releaseSecond;
                entered.Set();
                if (release.Wait(CacheVerificationTestContext.TimeoutMilliseconds) == false)
                    throw new TimeoutException("未收到测试工作项的释放信号。");
                throw new InvalidOperationException(index == 1 ? "First worker failure" : "Second worker failure");
            };
            var files = new[] { context.CreateFile(), context.CreateFile(), context.CreateFile() };
            var task = context.CreateTask((EFileVerifyLevel)99, 2, files);
            task.Start();
            Assert.IsTrue(firstEntered.Wait(CacheVerificationTestContext.TimeoutMilliseconds));
            Assert.IsTrue(secondEntered.Wait(CacheVerificationTestContext.TimeoutMilliseconds));
            Assert.AreEqual(EVerifyTaskStatus.Processing, task.Status);

            releaseFirst.Set();
            CacheVerificationTestContext.WaitUntil(() => CacheVerificationTestContext.GetRunningWorkers(task) == 1,
                "第一个失败的工作项未退出。");
            Assert.AreEqual(EVerifyTaskStatus.Processing, task.Status, "另一个工作项退出前不能发布 Failed。");

            releaseSecond.Set();
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Failed, task.Status);
            StringAssert.Contains("First worker failure", task.Error);
            Assert.AreEqual(0, task.CompletedCount);
            Assert.AreEqual(2, context.Logger.ErrorCount, "工作项异常收尾不能再次调用日志处理器。");
            foreach (var file in files)
                Assert.AreEqual((int)EFileVerifyResult.None, file.VerifyResultCode);
            Assert.Throws<InvalidOperationException>(task.Start);
            task.Dispose();
            Assert.AreEqual(EVerifyTaskStatus.Failed, task.Status);
        }
    }

    private static void TestDisposeBeforeVerification()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var file = context.CreateFile();
            var task = context.CreateTask(EFileVerifyLevel.High, 4, file);
            // 主线程重入启动锁，让已提交的工作项在取得文件前等待。
            lock (RuntimeReflectionUtility.GetField(task, "_startLock"))
            {
                task.Start();
                Assert.AreEqual(EVerifyTaskStatus.Processing, task.Status);
                Assert.AreEqual(0, task.CompletedCount);
                Assert.Throws<InvalidOperationException>(task.Start);
                task.Dispose();
                task.Dispose();
            }
            CacheVerificationTestContext.WaitUntil(() => CacheVerificationTestContext.GetRunningWorkers(task) == 0,
                "已释放的工作项未退出。");
            Assert.AreEqual(0, task.CompletedCount);
            Assert.AreEqual((int)EFileVerifyResult.None, file.VerifyResultCode);
            Assert.AreEqual(EVerifyTaskStatus.Processing, task.Status, "释放不会发布成功终态。");
            Assert.IsNull(task.Error);
        }
    }

    private static void TestDisposeDuringVerification()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var entered = context.CreateSignal();
            var release = context.CreateSignal();
            context.Logger.OnError = message =>
            {
                entered.Set();
                if (release.Wait(CacheVerificationTestContext.TimeoutMilliseconds) == false)
                    throw new TimeoutException("未收到测试工作项的释放信号。");
            };
            var files = new[] { context.CreateFile(), context.CreateFile(), context.CreateFile() };
            var task = context.CreateTask((EFileVerifyLevel)99, 1, files);
            task.Start();
            Assert.IsTrue(entered.Wait(CacheVerificationTestContext.TimeoutMilliseconds));
            task.Dispose();
            release.Set();
            CacheVerificationTestContext.WaitUntil(() => CacheVerificationTestContext.GetRunningWorkers(task) == 0,
                "正在校验的工作项未退出。");
            Assert.AreEqual(1, task.CompletedCount, "允许当前文件完成，但不能继续领取其他文件。");
            Assert.AreEqual((int)EFileVerifyResult.Exception, files[2].VerifyResultCode);
            Assert.AreEqual((int)EFileVerifyResult.None, files[1].VerifyResultCode);
            Assert.AreEqual((int)EFileVerifyResult.None, files[0].VerifyResultCode);
            Assert.AreEqual(EVerifyTaskStatus.Processing, task.Status);
            Assert.IsNull(task.Error);
            Assert.AreEqual(1, context.Logger.ErrorCount);
        }
    }
}
