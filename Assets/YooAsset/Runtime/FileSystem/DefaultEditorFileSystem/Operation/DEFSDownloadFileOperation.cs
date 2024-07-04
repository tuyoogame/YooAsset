
namespace YooAsset
{
    internal class DEFSDownloadFileOperation : FSDownloadFileOperation
    {
        internal DEFSDownloadFileOperation(PackageBundle bundle) : base(bundle)
        {
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