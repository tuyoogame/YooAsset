
namespace YooAsset
{
    internal class DEFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private readonly DefaultEditorFileSystem _fileSytem;

        internal DEFSInitializeOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSytem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }
}