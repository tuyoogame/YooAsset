using System;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 验证销毁包裹时向外传递卸载进度，不依赖 Unity 实际执行资源卸载。
/// </summary>
public class TestDestroyPackageProgress
{
    public void RuntimeTester()
    {
        var options = new UnloadAllAssetsOptions(true, true, true);
        var unloadOperation = new UnloadAllAssetsOperation(null, options);
        unloadOperation.StartOperation();
        SetStep(unloadOperation, "None");

        var operation = new DestroyPackageOperation(null, null, options);
        operation.StartOperation();
        SetStep(operation, "UnloadAllAssets");
        RuntimeReflectionUtility.SetField(operation, "_unloadAllAssetsOp", unloadOperation);

        foreach (float progress in new[] { 0f, 0.25f, 0.6f, 0.9f })
        {
            RuntimeReflectionUtility.SetField(unloadOperation, "<Progress>k__BackingField", progress);
            operation.UpdateOperation();
            Assert.AreEqual(progress, operation.Progress);
            Assert.AreEqual(EOperationStatus.Processing, operation.Status);
            Assert.IsFalse(operation.IsDone);
        }

        const string error = "Injected unload failure.";
        RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(unloadOperation, "SetError", error);
        operation.UpdateOperation();
        Assert.AreEqual(EOperationStatus.Failed, operation.Status);
        Assert.AreEqual(error, operation.Error);
        Assert.AreEqual(1f, operation.Progress);
    }

    private static void SetStep(AsyncOperationBase operation, string name)
    {
        var step = RuntimeReflectionUtility.GetField(operation, "_steps");
        RuntimeReflectionUtility.SetField(operation, "_steps", Enum.Parse(step.GetType(), name));
    }
}
