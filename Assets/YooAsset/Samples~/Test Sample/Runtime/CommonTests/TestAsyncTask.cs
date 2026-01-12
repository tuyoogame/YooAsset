using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 async/await 异步加载模式
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync (await) / InstantiateAsync (await)
/// 测试内容:
/// 1. 使用 await 模式异步加载 GameObject，验证 HandleBase.GetAwaiter() 正常工作
/// 2. 使用 await 模式异步实例化，验证 AsyncOperationBase.GetAwaiter() 正常工作
/// 3. 验证实例化结果非空，销毁实例化对象
/// </remarks>
public class TestAsyncTask
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

            // 验证 HandleBase.GetAwaiter()
            var assetHandle = package.LoadAssetAsync<GameObject>("canvas_a");
            await assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            // 验证 AsyncOperationBase.GetAwaiter()
            var instantiateOp = assetHandle.InstantiateAsync();
            await instantiateOp;
            Assert.AreEqual(EOperationStatus.Succeeded, instantiateOp.Status);
            Assert.IsNotNull(instantiateOp.Result);

            TestLogger.Log(instantiateOp, instantiateOp.Result.name);
            GameObject.Destroy(instantiateOp.Result);
            assetHandle.Release();

            onDone();
        }
        catch (Exception ex)
        {
            onError(ex);
        }
    }

    private class AsyncTaskRunner
    {
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

            while (!_finished && _error == null)
                yield return null;

            if (_error != null)
                throw _error;
        }
    }
}
