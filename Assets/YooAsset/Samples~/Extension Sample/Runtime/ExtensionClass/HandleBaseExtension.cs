using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// 提供资源句柄基类的扩展方法
/// </summary>
public static class HandleBaseExtension
{
    /// <summary>
    /// 检查句柄是否已成功完成
    /// </summary>
    /// <param name="thisHandle">待检查的句柄</param>
    /// <returns>句柄已完成且状态为成功时返回 true。</returns>
    public static bool IsSucceed(this HandleBase thisHandle)
    {
        return thisHandle.IsDone && thisHandle.Status == EOperationStatus.Succeeded;
    }
}