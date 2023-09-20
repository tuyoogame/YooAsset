using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class AssetOperationHandleExtension
{
    /// <summary>
    /// 等待异步执行完毕
    /// </summary>
    public static AssetHandle WaitForAsyncOperationComplete(this AssetHandle thisHandle)
    {
        thisHandle.WaitForAsyncComplete();
        return thisHandle;
    }
}