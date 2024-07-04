
namespace YooAsset
{
    internal sealed class DWFSClearAllBundleFilesOperation : FSClearAllBundleFilesOperation
    {
        private readonly DefaultWebFileSystem _fileSystem;

        internal DWFSClearAllBundleFilesOperation(DefaultWebFileSystem fileSystem)
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