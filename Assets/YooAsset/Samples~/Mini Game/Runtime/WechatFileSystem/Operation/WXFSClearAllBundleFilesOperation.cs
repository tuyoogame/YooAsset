#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using WeChatWASM;

internal class WXFSClearAllBundleFilesOperation : FSClearCacheFilesOperation
{
    private enum ESteps
    {
        None,
        ClearAllCacheFiles,
        WaitResult,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private ESteps _steps = ESteps.None;

    internal WXFSClearAllBundleFilesOperation(WechatFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.ClearAllCacheFiles;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.ClearAllCacheFiles)
        {
            _steps = ESteps.WaitResult;

            WX.CleanAllFileCache((bool isOk) =>
            {
                if (isOk)
                {
                    YooLogger.Log("微信缓存清理成功！");
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    YooLogger.Log("微信缓存清理失败！");
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "微信缓存清理失败！";
                }
            });
        }
    }
}
#endif
