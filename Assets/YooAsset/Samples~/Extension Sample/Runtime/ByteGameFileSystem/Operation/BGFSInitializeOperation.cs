#if UNITY_WEBGL
using YooAsset;

internal partial class BGFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly ByteGameFileSystem _fileSystem;

    public BGFSInitializeOperation(ByteGameFileSystem fileSystem)
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