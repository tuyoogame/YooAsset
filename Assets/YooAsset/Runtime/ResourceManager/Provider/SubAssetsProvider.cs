
namespace YooAsset
{
    internal sealed class SubAssetsProvider : ProviderOperation
    {
        private FSLoadSubAssetsOperation _loadSubAssetsOp;
        
        public SubAssetsProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        protected override void ProcessBundleResult()
        {
            if (_loadSubAssetsOp == null)
            {
                _loadSubAssetsOp = BundleResultObject.LoadSubAssetsAsync(MainAssetInfo);
                _loadSubAssetsOp.StartOperation();
                AddChildOperation(_loadSubAssetsOp);

#if UNITY_WEBGL
                if (_resManager.WebGLForceSyncLoadAsset)
                    _loadSubAssetsOp.WaitForAsyncComplete();
#endif
            }

            if (IsWaitForAsyncComplete)
                _loadSubAssetsOp.WaitForAsyncComplete();

            _loadSubAssetsOp.UpdateOperation();
            Progress = _loadSubAssetsOp.Progress;
            if (_loadSubAssetsOp.IsDone == false)
                return;

            if (_loadSubAssetsOp.Status != EOperationStatus.Succeed)
            {
                InvokeCompletion(_loadSubAssetsOp.Error, EOperationStatus.Failed);
            }
            else
            {
                SubAssetObjects = _loadSubAssetsOp.Result;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }
    }
}