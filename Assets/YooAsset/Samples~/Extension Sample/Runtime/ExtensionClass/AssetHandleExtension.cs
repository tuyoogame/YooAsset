using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// 提供资源句柄的扩展方法
/// </summary>
public static class AssetHandleExtension
{
    /// <summary>
    /// 等待资源加载操作完成
    /// </summary>
    /// <param name="thisHandle">等待完成的资源句柄</param>
    /// <returns>已完成等待的资源句柄</returns>
    public static AssetHandle WaitForAsyncOperationComplete(this AssetHandle thisHandle)
    {
        thisHandle.WaitForAsyncComplete();
        return thisHandle;
    }
}