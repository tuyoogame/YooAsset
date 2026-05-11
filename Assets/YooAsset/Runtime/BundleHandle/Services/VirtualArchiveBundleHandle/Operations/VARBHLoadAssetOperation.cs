using System;

namespace YooAsset
{
    /// <summary>
    /// 从虚拟归档资源包中加载单个资源对象
    /// </summary>
    internal sealed class VARBHLoadAssetOperation : BHLoadAssetOperation
    {
        private enum ESteps
        {
            None,
            LoadObject,
            CheckResult,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private readonly VirtualArchiveBundle _virtualArchiveBundle;
        private readonly AssetInfo _assetInfo;
        private ESteps _steps = ESteps.None;

        public VARBHLoadAssetOperation(PackageBundle packageBundle, VirtualArchiveBundle virtualArchiveBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _virtualArchiveBundle = virtualArchiveBundle;
            _assetInfo = assetInfo;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadObject;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadObject)
            {
                try
                {
                    Result = _virtualArchiveBundle.CreateRawFileObject(_assetInfo.AssetPath);
                    _steps = ESteps.CheckResult;
                }
                catch (Exception ex)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load raw file object : '{_assetInfo.AssetPath}', {ex.Message}");
                    YooLogger.LogError(Error);
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load raw file object : '{_assetInfo.AssetPath}'.");
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
