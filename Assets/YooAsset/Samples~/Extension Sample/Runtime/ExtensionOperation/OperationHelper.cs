using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YooAsset;

/// <summary>
/// 提供业务自定义操作的启动入口
/// </summary>
public static class OperationHelper
{
    /// <summary>
    /// 启动业务自定义操作
    /// </summary>
    /// <param name="packageName">调度器或资源包裹名称</param>
    /// <param name="operation">待启动的操作实例</param>
    public static void StartOperation(string packageName, AsyncOperationBase operation)
    {
        AsyncOperationSystem.StartOperation(packageName, operation);
    }
}
