
namespace YooAsset
{
    /// <summary>
    /// 子资源提供者，负责加载资源包内的子资源。
    /// </summary>
    internal sealed class SubAssetsProvider : ProviderBase
    {
        private BHLoadSubAssetsOperation _loadSubAssetsOp;

        public SubAssetsProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo) : base(manager, providerKey, assetInfo)
        {
        }
        protected override void InternalProcessBundleHandle()
        {
            if (_loadSubAssetsOp == null)
            {
                _loadSubAssetsOp = LoadedBundleHandle.LoadSubAssetsAsync(MainAssetInfo);
                _loadSubAssetsOp.StartOperation();
                AddChildOperation(_loadSubAssetsOp);

#if UNITY_WEBGL
                if (_resourceManager.WebGLForceSyncLoadAsset)
                    _loadSubAssetsOp.WaitForCompletion();
#endif
            }

            if (IsWaitForCompletion)
                _loadSubAssetsOp.WaitForCompletion();

            _loadSubAssetsOp.UpdateOperation();
            Progress = _loadSubAssetsOp.Progress;
            if (_loadSubAssetsOp.IsDone == false)
                return;

            if (_loadSubAssetsOp.Status != EOperationStatus.Succeeded)
            {
                SetFail(_loadSubAssetsOp.Error);
            }
            else
            {
                SubAssetObjects = _loadSubAssetsOp.Result;
                SetSuccess();
            }
        }
    }
}