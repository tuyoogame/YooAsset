using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class HandleBaseExtension
{
    public static bool IsSucceed(this HandleBase thisHandle)
    {
        return thisHandle.IsDone && thisHandle.Status == EOperationStatus.Succeeded;
    }
}