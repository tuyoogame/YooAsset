#if YOOASSET_UNITASK_SUPPORT
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;
using Cysharp.Threading.Tasks;

/// <summary>
/// 测试 UniTask 扩展加载模式
/// </summary>
/// <remarks>
/// 覆盖 API: HandleBase.ToUniTask / HandleBase.WithCancellation / AsyncOperationBase.ToUniTask
/// 测试内容:
/// 1. 使用 ToUniTask 异步加载资源，验证成功路径
/// 2. 使用 ToUniTask 等待实例化操作，验证 AsyncOperationBase 扩展
/// 3. 验证失败句柄正常完成（不抛异常），开发者自行检查 Status
/// 4. 验证已释放句柄正常完成（不抛异常，不挂起）
/// 5. 验证已取消的 CancellationToken 立即抛出 OperationCanceledException
/// 6. 验证等待过程中取消 CancellationToken 正确抛出异常且 handle 仍有效
/// </remarks>
public class TestUniTask
{
    public IEnumerator RuntimeTester()
    {
        var task = new AsyncTaskRunner(TestBody);
        yield return task.Run();
    }

    private async void TestBody(Action onDone, Action<Exception> onError)
    {
        try
        {
            ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
            Assert.IsNotNull(package);

            await TestHandleToUniTaskSuccess(package);
            await TestOperationToUniTaskSuccess(package);
            await TestFailedHandleCompletes(package);
            await TestAlreadyFailedHandleCompletes(package);
            await TestReleasedHandleCompletes(package);
            await TestAlreadyCanceled(package);
            await TestCancelDuringAwait(package);

            onDone();
        }
        catch (Exception ex)
        {
            onError(ex);
        }
    }
    private async UniTask TestHandleToUniTaskSuccess(ResourcePackage package)
    {
        float? progressValue = null;
        var progress = new Progress<float>(value => progressValue = value);
        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_a");

        await assetHandle.ToUniTask(progress);

        Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);
        Assert.IsNotNull(assetHandle.AssetObject);
        if (progressValue.HasValue)
        {
            Assert.GreaterOrEqual(progressValue.Value, 0f);
            Assert.LessOrEqual(progressValue.Value, 1f);
        }

        assetHandle.Release();
    }
    private async UniTask TestOperationToUniTaskSuccess(ResourcePackage package)
    {
        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_b");
        await assetHandle.ToUniTask();

        var instantiateOp = assetHandle.InstantiateAsync();
        await instantiateOp.ToUniTask();

        Assert.AreEqual(EOperationStatus.Succeeded, instantiateOp.Status);
        Assert.IsNotNull(instantiateOp.Result);

        GameObject.Destroy(instantiateOp.Result);
        assetHandle.Release();
    }
    private async UniTask TestFailedHandleCompletes(ResourcePackage package)
    {
        LogAssert.ignoreFailingMessages = true;
        var assetHandle = package.LoadAssetAsync<GameObject>("__unitask_not_exist__");
        await assetHandle.ToUniTask();
        LogAssert.ignoreFailingMessages = false;

        Assert.AreEqual(EOperationStatus.Failed, assetHandle.Status);
        Assert.IsFalse(string.IsNullOrEmpty(assetHandle.Error));
        assetHandle.Release();
    }
    private async UniTask TestAlreadyFailedHandleCompletes(ResourcePackage package)
    {
        LogAssert.ignoreFailingMessages = true;
        var assetHandle = package.LoadAssetAsync<GameObject>("__unitask_already_failed__");
        await assetHandle;
        LogAssert.ignoreFailingMessages = false;

        Assert.AreEqual(EOperationStatus.Failed, assetHandle.Status);

        await assetHandle.ToUniTask();
        Assert.AreEqual(EOperationStatus.Failed, assetHandle.Status);
        assetHandle.Release();
    }
    private async UniTask TestReleasedHandleCompletes(ResourcePackage package)
    {
        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_a");
        await assetHandle.ToUniTask();

        assetHandle.Release();

        await assetHandle.ToUniTask();
        Assert.IsFalse(assetHandle.IsValid);
    }
    private async UniTask TestAlreadyCanceled(ResourcePackage package)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_a");

        bool wasCanceled = false;
        try
        {
            await assetHandle.ToUniTask(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            wasCanceled = true;
        }

        Assert.IsTrue(wasCanceled);

        await assetHandle;
        assetHandle.Release();
    }
    private async UniTask TestCancelDuringAwait(ResourcePackage package)
    {
        using var cts = new CancellationTokenSource();

        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_a");

        cts.CancelAfter(0);
        await UniTask.Yield();

        bool wasCanceled = false;
        try
        {
            await assetHandle.WithCancellation(cts.Token);
        }
        catch (OperationCanceledException)
        {
            wasCanceled = true;
        }

        Assert.IsTrue(wasCanceled);
        Assert.IsTrue(assetHandle.IsValid);

        await assetHandle;
        assetHandle.Release();
    }

    private class AsyncTaskRunner
    {
        private const int MaxWaitFrames = 600;

        private readonly Action<Action, Action<Exception>> _asyncMethod;
        private bool _finished;
        private Exception _error;

        public AsyncTaskRunner(Action<Action, Action<Exception>> asyncMethod)
        {
            _asyncMethod = asyncMethod;
        }

        public IEnumerator Run()
        {
            _finished = false;
            _error = null;

            _asyncMethod(() => _finished = true, ex => _error = ex);

            int frameCount = 0;
            while (!_finished && _error == null)
            {
                frameCount++;
                if (frameCount > MaxWaitFrames)
                    throw new TimeoutException("UniTask test did not complete within the expected frame count.");

                yield return null;
            }

            if (_error != null)
                throw _error;
        }
    }
}
#else
using System.Collections;
using UnityEngine;

public class TestUniTask
{
    public IEnumerator RuntimeTester()
    {
        Debug.LogWarning("UniTask tests skipped: YOOASSET_UNITASK_SUPPORT is not defined.");
        yield break;
    }
}
#endif
