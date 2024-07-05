
namespace YooAsset
{
    internal abstract class FSClearUnusedBundleFilesOperation : AsyncOperationBase
    {
    }

    internal sealed class FSClearUnusedBundleFilesCompleteOperation : FSClearUnusedBundleFilesOperation
    {
        internal FSClearUnusedBundleFilesCompleteOperation()
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