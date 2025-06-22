#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal partial class TTFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly TiktokFileSystem _fileSystem;

    public TTFSInitializeOperation(TiktokFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    internal override void InternalStart()
    {
        Status = EOperationStatus.Succeed;
    }
    internal override void InternalUpdate()
    {
    }
}
#endif