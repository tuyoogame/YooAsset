#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
using YooAsset;
using WeChatWASM;

internal sealed class WXFSClearAllBundleFilesOperation : FSClearCacheOperation
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
    protected override void InternalStart()
    {
        _steps = ESteps.ClearAllCacheFiles;
    }
    protected override void InternalUpdate()
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
                    SetResult();
                }
                else
                {
                    YooLogger.Log("微信缓存清理失败！");
                    _steps = ESteps.Done;
                    SetError("微信缓存清理失败！");
                }
            });
        }
    }
}
#endif
