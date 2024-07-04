
namespace YooAsset
{
    internal sealed class DWFSClearUnusedBundleFilesOperation : FSClearUnusedBundleFilesOperation
    {
        private readonly DefaultWebFileSystem _fileSystem;

        internal DWFSClearUnusedBundleFilesOperation(DefaultWebFileSystem fileSystem)
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