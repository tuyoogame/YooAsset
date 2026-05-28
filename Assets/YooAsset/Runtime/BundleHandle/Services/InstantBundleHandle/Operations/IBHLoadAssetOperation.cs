#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 单个资源加载操作（InstantBundle）
    /// </summary>
    internal sealed class IBHLoadAssetOperation : BHLoadAssetOperation
    {
        private enum ESteps
        {
            None,
            CheckAssetTable,
            LoadAsset,
            CheckResult,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private readonly InstantAssetTable _assetTable;
        private readonly AssetInfo _assetInfo;
        private InstantAssetRequest _request;
        private ESteps _steps = ESteps.None;

        public IBHLoadAssetOperation(PackageBundle packageBundle, InstantAssetTable assetTable, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _assetTable = assetTable;
            _assetInfo = assetInfo;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckAssetTable;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckAssetTable)
            {
                if (_assetTable == null)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(IBHLoadAssetOperation)} asset table is null, cannot load asset: '{_assetInfo.AssetPath}'.");
                    YooLogger.LogError(Error);
                    return;
                }

                _steps = ESteps.LoadAsset;
            }

            if (_steps == ESteps.LoadAsset)
            {
                if (IsWaitForCompletion)
                {
                    if (_assetInfo.AssetType == null)
                        Result = _assetTable.LoadAsset(_assetInfo.AssetPath);
                    else
                        Result = _assetTable.LoadAsset(_assetInfo.AssetPath, _assetInfo.AssetType);
                }
                else
                {
                    if (_assetInfo.AssetType == null)
                        _request = _assetTable.LoadAssetAsync(_assetInfo.AssetPath);
                    else
                        _request = _assetTable.LoadAssetAsync(_assetInfo.AssetPath, _assetInfo.AssetType);
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_request != null)
                {
                    // 注意: 异步加载过程中，业务逻辑可能会强制转换为同步加载
                    if (IsWaitForCompletion)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.LogWarning("Blocking the main thread while loading an InstantAsset.");
                        Result = _request.asset;
                    }
                    else
                    {
                        Progress = _request.progress;
                        if (_request.isDone == false)
                            return;
                        Result = _request.asset;
                    }
                }

                if (Result == null)
                {
                    string error;
                    if (_assetInfo.AssetType == null)
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: null InstantBundle: '{_packageBundle.BundleName}'.";
                    else
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: {_assetInfo.AssetType} InstantBundle: '{_packageBundle.BundleName}'.";

                    _steps = ESteps.Done;
                    SetError(error);
                    YooLogger.LogError(Error);
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
#endif
