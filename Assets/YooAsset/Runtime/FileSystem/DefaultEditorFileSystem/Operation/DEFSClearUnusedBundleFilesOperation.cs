
namespace YooAsset
{
    internal sealed class DEFSClearUnusedBundleFilesOperation : FSClearUnusedBundleFilesOperation
    {
        private readonly DefaultEditorFileSystem _fileSystem;

        internal DEFSClearUnusedBundleFilesOperation(DefaultEditorFileSystem fileSystem)
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
}