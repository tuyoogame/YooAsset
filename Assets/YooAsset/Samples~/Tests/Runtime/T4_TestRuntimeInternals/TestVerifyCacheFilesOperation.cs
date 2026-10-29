using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 验证并行校验操作的阶段衔接、结果处理、进度及中止行为。
/// </summary>
public class TestVerifyCacheFilesOperation
{
    public void RuntimeTester()
    {
        TestEmptyOperation();
        TestConcurrencyClamp(-1);
        TestConcurrencyClamp(0);
        TestConcurrencyClamp(int.MaxValue);
        TestResultsAndSnapshot();
        TestProgressAndBudget();
        TestTaskFailure();
        TestAbortBeforeVerification();
        TestAbortDuringVerification();
        TestResultProcessingFailure();
    }

    private static void TestEmptyOperation()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(true);
            var cache = context.CreateCache();
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, 8, new List<SearchFileInfo>());
            int completedCount = 0;
            operation.Completed += result => completedCount++;
            Assert.IsNull(CacheVerificationTestContext.GetTask(operation));
            operation.StartOperation();
            Assert.IsNull(CacheVerificationTestContext.GetTask(operation), "Task 应在 StartVerify 阶段创建。");
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
            Assert.IsTrue(operation.IsCompleted);
            Assert.AreEqual(1f, operation.Progress);
            Assert.AreEqual(0, cache.FileCount);
            Assert.AreEqual(0L, cache.SpaceOccupied);
            operation.UpdateOperation();
            operation.AbortOperation();
            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status);
        }
    }

    private static void TestConcurrencyClamp(int concurrency)
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(true);
            var cache = context.CreateCache();
            var files = new List<SearchFileInfo> { context.CreateFile(), context.CreateFile() };
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, concurrency, files);
            operation.StartOperation();
            operation.UpdateOperation();
            var task = CacheVerificationTestContext.GetTask(operation);
            CacheVerificationTestContext.WaitForTask(task);
            int expected = Math.Max(Math.Min(Environment.ProcessorCount * 2 + 1, concurrency), 1);
            Assert.AreEqual(expected, RuntimeReflectionUtility.GetField(task, "_maxConcurrency"));
            context.SetBusy(false);
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
            Assert.AreEqual(2, cache.FileCount);
            Assert.AreEqual(CacheVerificationTestContext.DataSize * 2L, cache.SpaceOccupied);
        }
    }

    private static void TestResultsAndSnapshot()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(true);
            var cache = context.CreateCache();
            var valid = context.CreateFile();
            var anotherValid = context.CreateFile();
            var corrupt = context.CreateFile();
            var missingInfo = context.CreateFile();
            File.WriteAllBytes(corrupt.DataFilePath, new byte[CacheVerificationTestContext.DataSize]);
            File.Delete(missingInfo.InfoFilePath);
            var files = new List<SearchFileInfo> { valid, corrupt, anotherValid, missingInfo };
            var original = files.ToArray();
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, 4, files);
            int completedCount = 0;
            int completedThread = 0;
            operation.Completed += result =>
            {
                completedCount++;
                completedThread = Thread.CurrentThread.ManagedThreadId;
            };
            operation.StartOperation();
            operation.UpdateOperation();
            var task = CacheVerificationTestContext.GetTask(operation);
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Succeeded, task.Status, task.Error);
            CollectionAssert.AreEqual(original, files, "校验不能清空或移除调用方列表中的元素。");

            var addedLater = context.CreateFile();
            files.Add(addedLater);
            operation.UpdateOperation();
            Assert.AreEqual(0.5f, operation.Progress);
            Assert.IsFalse(operation.IsDone);
            Assert.AreEqual(0, completedCount);
            Assert.AreEqual(0, cache.FileCount);
            foreach (var file in original)
                Assert.IsTrue(Directory.Exists(file.FolderPath), "预算耗尽时不能处理结果。");

            context.SetBusy(false);
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
            Assert.IsTrue(operation.IsCompleted);
            Assert.AreEqual(1f, operation.Progress);
            Assert.AreEqual(2, cache.FileCount);
            Assert.AreEqual(CacheVerificationTestContext.DataSize * 2L, cache.SpaceOccupied);
            Assert.AreEqual(valid.DataFilePath, cache.GetCacheFilePath(valid.BundleGuid));
            Assert.AreEqual(anotherValid.DataFilePath, cache.GetCacheFilePath(anotherValid.BundleGuid));
            Assert.IsFalse(cache.IsCached(corrupt.BundleGuid));
            Assert.IsFalse(cache.IsCached(missingInfo.BundleGuid));
            Assert.IsFalse(Directory.Exists(corrupt.FolderPath));
            Assert.IsFalse(Directory.Exists(missingInfo.FolderPath));
            Assert.IsTrue(File.Exists(valid.DataFilePath));
            Assert.IsTrue(File.Exists(anotherValid.DataFilePath));
            Assert.AreEqual((int)EFileVerifyResult.None, addedLater.VerifyResultCode, "启动后新增元素不进入本次快照。");
            Assert.IsFalse(cache.IsCached(addedLater.BundleGuid));
            Assert.IsTrue(Directory.Exists(addedLater.FolderPath));
            Assert.AreEqual(5, files.Count);
            Assert.AreEqual(2, context.Logger.WarningCount);
            Assert.AreEqual(0, context.Logger.ErrorCount);
            operation.UpdateOperation();
            operation.AbortOperation();
            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, completedThread);
            Assert.AreEqual(2, cache.FileCount, "完成后再次更新不能重复登记结果。");
        }
    }

    private static void TestProgressAndBudget()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(false);
            var entered = context.CreateSignal();
            var release = context.CreateSignal();
            context.Logger.OnError = message =>
            {
                entered.Set();
                if (release.Wait(CacheVerificationTestContext.TimeoutMilliseconds) == false)
                    throw new TimeoutException("未收到测试工作项的释放信号。");
            };
            var cache = context.CreateCache();
            var files = new List<SearchFileInfo> { context.CreateFile(), context.CreateFile(), context.CreateFile() };
            // 任务倒序领取：最后一个文件立即完成，第二个文件在异常日志处等待。
            File.Delete(files[2].InfoFilePath);
            var operation = context.CreateOperation(cache, (EFileVerifyLevel)99, 1, files);
            operation.StartOperation();
            operation.UpdateOperation();
            Assert.IsTrue(entered.Wait(CacheVerificationTestContext.TimeoutMilliseconds));
            operation.UpdateOperation();
            Assert.AreEqual(1f / 6f, operation.Progress, 0.00001f);
            Assert.IsFalse(operation.IsDone);
            foreach (var file in files)
                Assert.IsTrue(Directory.Exists(file.FolderPath), "任务未完成时不能提前清理已产生的失败结果。");

            release.Set();
            CacheVerificationTestContext.WaitForTask(CacheVerificationTestContext.GetTask(operation));
            context.SetBusy(true);
            operation.UpdateOperation();
            Assert.AreEqual(0.5f, operation.Progress);

            // 每处理一个失败结果就耗尽预算，下一次 Update 才允许继续。
            context.Logger.OnWarning = message => context.SetBusy(true);
            for (int i = 0; i < files.Count; i++)
            {
                context.SetBusy(false);
                operation.UpdateOperation();
                Assert.AreEqual(0.5f + (i + 1f) / (files.Count * 2f), operation.Progress, 0.00001f);
                for (int j = 0; j < files.Count; j++)
                    Assert.AreEqual(j > i, Directory.Exists(files[j].FolderPath), $"处理位置={i}, 文件={j}");
                Assert.AreEqual(i == files.Count - 1, operation.IsCompleted);
            }
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
            Assert.AreEqual(0, cache.FileCount);
            Assert.AreEqual(3, context.Logger.WarningCount);
            Assert.AreEqual(2, context.Logger.ErrorCount);
        }
    }

    private static void TestTaskFailure()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(true);
            var cache = context.CreateCache();
            var valid = context.CreateFile();
            var invalid = context.CreateFile();
            File.Delete(invalid.InfoFilePath);
            // 单工作项先产生成功及失败结果，再由空元素触发任务级异常。
            var files = new List<SearchFileInfo> { null, invalid, valid };
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, 1, files);
            int completedCount = 0;
            operation.Completed += result => completedCount++;
            operation.StartOperation();
            operation.UpdateOperation();
            var task = CacheVerificationTestContext.GetTask(operation);
            CacheVerificationTestContext.WaitForTask(task);
            Assert.AreEqual(EVerifyTaskStatus.Failed, task.Status);
            Assert.AreEqual(2, task.CompletedCount);
            Assert.AreEqual((int)EFileVerifyResult.Succeed, valid.VerifyResultCode);
            Assert.AreEqual((int)EFileVerifyResult.InfoFileNotExisted, invalid.VerifyResultCode);
            context.SetBusy(false);
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Failed, operation.Status);
            Assert.IsNotEmpty(operation.Error);
            Assert.AreEqual(task.Error, operation.Error);
            Assert.IsTrue(operation.IsCompleted);
            Assert.AreEqual(1f, operation.Progress);
            Assert.AreEqual(0, cache.FileCount, "任务失败时不登记已校验成功的文件。");
            Assert.IsTrue(Directory.Exists(invalid.FolderPath), "任务失败时不删除已校验失败的文件。");
            Assert.IsTrue(File.Exists(valid.DataFilePath));
            Assert.AreEqual(1, context.Logger.ErrorCount);
            operation.UpdateOperation();
            Assert.AreEqual(1, completedCount);
        }
    }

    private static void TestAbortBeforeVerification()
    {
        using (var context = new CacheVerificationTestContext())
        {
            var file = context.CreateFile();
            var cache = context.CreateCache();
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, 1, new List<SearchFileInfo> { file });
            operation.StartOperation();
            operation.AbortOperation();
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Failed, operation.Status);
            Assert.IsTrue(operation.IsCompleted);
            Assert.IsNull(CacheVerificationTestContext.GetTask(operation));
            Assert.AreEqual((int)EFileVerifyResult.None, file.VerifyResultCode);
            Assert.IsTrue(File.Exists(file.DataFilePath));
            Assert.AreEqual(0, cache.FileCount);
        }
    }

    private static void TestAbortDuringVerification()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(false);
            var entered = context.CreateSignal();
            var release = context.CreateSignal();
            context.Logger.OnError = message =>
            {
                entered.Set();
                if (release.Wait(CacheVerificationTestContext.TimeoutMilliseconds) == false)
                    throw new TimeoutException("未收到测试工作项的释放信号。");
            };
            var cache = context.CreateCache();
            var files = new List<SearchFileInfo> { context.CreateFile(), context.CreateFile() };
            var operation = context.CreateOperation(cache, (EFileVerifyLevel)99, 1, files);
            int completedCount = 0;
            operation.Completed += result => completedCount++;
            operation.StartOperation();
            operation.UpdateOperation();
            Assert.IsTrue(entered.Wait(CacheVerificationTestContext.TimeoutMilliseconds));
            operation.AbortOperation();
            Assert.IsTrue(operation.IsCompleted, "中止操作无需等待正在校验的文件结束。");
            release.Set();
            var task = CacheVerificationTestContext.GetTask(operation);
            CacheVerificationTestContext.WaitUntil(() => CacheVerificationTestContext.GetRunningWorkers(task) == 0,
                "中止操作后，后台工作项未退出。");
            Assert.AreEqual(1, task.CompletedCount);
            Assert.AreEqual((int)EFileVerifyResult.None, files[0].VerifyResultCode);
            operation.UpdateOperation();
            operation.AbortOperation();
            Assert.AreEqual(EOperationStatus.Failed, operation.Status);
            Assert.AreEqual("Operation was aborted.", operation.Error);
            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(0, cache.FileCount);
            foreach (var file in files)
                Assert.IsTrue(Directory.Exists(file.FolderPath));
        }
    }

    private static void TestResultProcessingFailure()
    {
        using (var context = new CacheVerificationTestContext())
        {
            context.SetBusy(true);
            var cache = context.CreateCache();
            var duplicate = context.CreateFile();
            var invalid = context.CreateFile();
            File.Delete(invalid.InfoFilePath);
            cache.AddEntry(duplicate.BundleGuid, new SandboxBundleCacheEntry(duplicate.BundleGuid,
                duplicate.InfoFilePath, duplicate.DataFilePath));
            var files = new List<SearchFileInfo> { duplicate, invalid };
            var operation = context.CreateOperation(cache, EFileVerifyLevel.High, 1, files);
            operation.StartOperation();
            operation.UpdateOperation();
            CacheVerificationTestContext.WaitForTask(CacheVerificationTestContext.GetTask(operation));
            context.SetBusy(false);
            operation.UpdateOperation();
            Assert.AreEqual(EOperationStatus.Failed, operation.Status);
            StringAssert.Contains("Cache entry already exists", operation.Error);
            Assert.IsTrue(operation.IsCompleted);
            Assert.AreEqual(1, cache.FileCount);
            Assert.AreEqual(CacheVerificationTestContext.DataSize, cache.SpaceOccupied);
            operation.UpdateOperation();
            Assert.IsTrue(Directory.Exists(invalid.FolderPath), "结果处理异常后不能继续删除后续文件。");
            Assert.AreEqual(1, context.Logger.ErrorCount);
        }
    }
}
