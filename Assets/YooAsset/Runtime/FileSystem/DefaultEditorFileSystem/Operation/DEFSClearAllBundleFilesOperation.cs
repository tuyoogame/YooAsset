
namespace YooAsset
{
    internal sealed class DEFSClearAllBundleFilesOperation : FSClearAllBundleFilesOperation
    {
        private readonly DefaultEditorFileSystem _fileSystem;

        internal DEFSClearAllBundleFilesOperation(DefaultEditorFileSystem fileSystem)
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