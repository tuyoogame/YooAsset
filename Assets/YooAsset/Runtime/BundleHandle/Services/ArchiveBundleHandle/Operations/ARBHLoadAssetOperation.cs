using System;

namespace YooAsset
{
    /// <summary>
    /// 从归档资源包中加载单个资源对象
    /// </summary>
    internal sealed class ARBHLoadAssetOperation : BHLoadAssetOperation
    {
        private enum ESteps
        {
            None,
            LoadObject,
            CheckResult,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private readonly ArchiveBundle _archiveBundle;
        private readonly AssetInfo _assetInfo;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="archiveBundle">已解析的归档资源包数据对象</param>
        /// <param name="assetInfo">待加载资源信息</param>
        public ARBHLoadAssetOperation(PackageBundle packageBundle, ArchiveBundle archiveBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _archiveBundle = archiveBundle;
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
                    Result = _archiveBundle.CreateRawFileObject(_assetInfo.AssetPath);
                    _steps = ESteps.CheckResult;
                }
                catch (Exception ex)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load raw file object from archive: '{_assetInfo.AssetPath}', {ex.Message}");
                    YooLogger.LogError(Error);
                    return;
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load raw file object from archive: '{_assetInfo.AssetPath}'.");
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
