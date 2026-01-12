using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 从 AssetBundle 中加载单个资源对象
    /// </summary>
    internal sealed class ABHLoadAssetOperation : BHLoadAssetOperation
    {
        private enum ESteps
        {
            None,
            CheckBundle,
            LoadAsset,
            CheckResult,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private readonly AssetBundle _assetBundle;
        private readonly AssetInfo _assetInfo;
        private AssetBundleRequest _request;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="assetBundle">已加载的 Unity AssetBundle 对象</param>
        /// <param name="assetInfo">待加载资源信息</param>
        public ABHLoadAssetOperation(PackageBundle packageBundle, AssetBundle assetBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _assetBundle = assetBundle;
            _assetInfo = assetInfo;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckBundle;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckBundle)
            {
                if (_assetBundle == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Bundle '{_packageBundle.BundleName}' has been destroyed due to Unity engine bugs.");
                    return;
                }

                _steps = ESteps.LoadAsset;
            }

            if (_steps == ESteps.LoadAsset)
            {
                if (IsWaitForCompletion)
                {
                    if (_assetInfo.AssetType == null)
                        Result = _assetBundle.LoadAsset(_assetInfo.AssetPath);
                    else
                        Result = _assetBundle.LoadAsset(_assetInfo.AssetPath, _assetInfo.AssetType);
                }
                else
                {
                    if (_assetInfo.AssetType == null)
                        _request = _assetBundle.LoadAssetAsync(_assetInfo.AssetPath);
                    else
                        _request = _assetBundle.LoadAssetAsync(_assetInfo.AssetPath, _assetInfo.AssetType);
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
                        YooLogger.LogWarning("Blocking the main thread while loading a Unity asset.");
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
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: null AssetBundle: '{_packageBundle.BundleName}'.";
                    else
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: {_assetInfo.AssetType} AssetBundle: '{_packageBundle.BundleName}'.";

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