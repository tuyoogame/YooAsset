
namespace YooAsset
{
    internal sealed class CompletedProvider : ProviderOperation
    {
        public CompletedProvider(ResourceManager manager, AssetInfo assetInfo) : base(manager, string.Empty, assetInfo)
        {
        }

        internal override void InternalOnStart()
        {
        }
        internal override void InternalOnUpdate()
        {
        }

        public void SetCompleted(string error)
        {
            if (_steps == ESteps.None)
            {
                InvokeCompletion(error, EOperationStatus.Failed);
            }
        }
    }
}