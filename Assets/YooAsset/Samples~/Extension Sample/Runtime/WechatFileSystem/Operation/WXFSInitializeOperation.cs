#if UNITY_WECHAT_GAME
using YooAsset;

internal partial class WXFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly WechatFileSystem _fileSystem;

    public WXFSInitializeOperation(WechatFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    internal override void InternalOnStart()
    {
        Status = EOperationStatus.Succeed;
    }
    internal override void InternalOnUpdate()
    {
    }
}
#endif