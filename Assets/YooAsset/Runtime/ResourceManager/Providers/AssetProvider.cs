
namespace YooAsset
{
    /// <summary>
    /// 资源提供者，负责加载单个资源对象。
    /// </summary>
    internal sealed class AssetProvider : ProviderBase
    {
        private BHLoadAssetOperation _loadAssetOp;

        public AssetProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo) : base(manager, providerKey, assetInfo)
        {
        }
        protected override void InternalProcessBundleHandle()
        {
            if (_loadAssetOp == null)
            {
                _loadAssetOp = LoadedBundleHandle.LoadAssetAsync(MainAssetInfo);
                _loadAssetOp.StartOperation();
                AddChildOperation(_loadAssetOp);

#if UNITY_WEBGL
                if (_resourceManager.WebGLForceSyncLoadAsset)
                    _loadAssetOp.WaitForCompletion();
#endif
            }

            if (IsWaitForCompletion)
                _loadAssetOp.WaitForCompletion();

            _loadAssetOp.UpdateOperation();
            Progress = _loadAssetOp.Progress;
            if (_loadAssetOp.IsDone == false)
                return;

            if (_loadAssetOp.Status != EOperationStatus.Succeeded)
            {
                SetFail(_loadAssetOp.Error);
            }
            else
            {
                AssetObject = _loadAssetOp.Result;
                SetSuccess();
            }
        }
    }
}