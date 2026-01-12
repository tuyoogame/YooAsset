
namespace YooAsset
{
    /// <summary>
    /// 从原生资源包中加载单个资源对象
    /// </summary>
    internal sealed class RBHLoadAssetOperation : BHLoadAssetOperation
    {
        private enum ESteps
        {
            None,
            LoadObject,
            CheckResult,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private readonly RawBundle _rawBundle;
        private readonly AssetInfo _assetInfo;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="rawBundle">已加载的原生资源包数据对象</param>
        /// <param name="assetInfo">待加载资源信息</param>
        public RBHLoadAssetOperation(PackageBundle packageBundle, RawBundle rawBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _rawBundle = rawBundle;
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
                Result = _rawBundle.CreateRawFileObject();
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load raw file object: '{_assetInfo.AssetPath}'.");
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