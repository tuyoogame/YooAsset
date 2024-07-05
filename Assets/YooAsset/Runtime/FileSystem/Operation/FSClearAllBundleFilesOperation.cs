
namespace YooAsset
{
    internal abstract class FSClearAllBundleFilesOperation : AsyncOperationBase
    {
    }

    internal sealed class FSClearAllBundleFilesCompleteOperation : FSClearAllBundleFilesOperation
    {
        internal FSClearAllBundleFilesCompleteOperation()
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