#if UNITY_WEBGL && TAPMINIGAME
using YooAsset;

internal partial class TPFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly TaptapFileSystem _fileSystem;

    public TPFSInitializeOperation(TaptapFileSystem fileSystem)
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