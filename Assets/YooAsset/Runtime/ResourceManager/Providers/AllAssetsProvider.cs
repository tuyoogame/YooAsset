
namespace YooAsset
{
    /// <summary>
    /// 全资源提供者，负责加载资源包内所有资源。
    /// </summary>
    internal sealed class AllAssetsProvider : ProviderBase
    {
        private BHLoadAllAssetsOperation _loadAllAssetsOp;

        public AllAssetsProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo) : base(manager, providerKey, assetInfo)
        {
        }
        protected override void InternalProcessBundleHandle()
        {
            if (_loadAllAssetsOp == null)
            {
                _loadAllAssetsOp = LoadedBundleHandle.LoadAllAssetsAsync(MainAssetInfo);
                _loadAllAssetsOp.StartOperation();
                AddChildOperation(_loadAllAssetsOp);

#if UNITY_WEBGL
                if (_resourceManager.WebGLForceSyncLoadAsset)
                    _loadAllAssetsOp.WaitForCompletion();
#endif
            }

            if (IsWaitForCompletion)
                _loadAllAssetsOp.WaitForCompletion();

            _loadAllAssetsOp.UpdateOperation();
            Progress = _loadAllAssetsOp.Progress;
            if (_loadAllAssetsOp.IsDone == false)
                return;

            if (_loadAllAssetsOp.Status != EOperationStatus.Succeeded)
            {
                SetFail(_loadAllAssetsOp.Error);
            }
            else
            {
                AllAssetObjects = _loadAllAssetsOp.Result;
                SetSuccess();
            }
        }
    }
}