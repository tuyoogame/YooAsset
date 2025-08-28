using System;
using UnityEngine;
using YooAsset;

public static class OperationMonitor
{
    public static void RegisterOperationCallback()
    {
        OperationSystem.RegisterStartCallback(OperationStartCallback);
        OperationSystem.RegisterFinishCallback(OperationFinishCallback);
    }

    private static void OperationStartCallback(string packageName, AsyncOperationBase operation)
    {
        Debug.Log($"Operation start : {operation.GetType().Name}");
    }
    private static void OperationFinishCallback(string packageName, AsyncOperationBase operation)
    {
        Debug.Log($"Operation finish : {operation.GetType().Name}");
    }
}