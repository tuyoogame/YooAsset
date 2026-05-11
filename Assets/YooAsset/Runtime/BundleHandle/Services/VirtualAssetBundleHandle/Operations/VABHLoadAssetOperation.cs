
namespace YooAsset
{
    /// <summary>
    /// 通过 AssetDatabase 加载单个资源对象（仅编辑器模式）
    /// </summary>
    internal sealed class VABHLoadAssetOperation : BHLoadAssetOperation
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
        private readonly AssetInfo _assetInfo;

#if UNITY_EDITOR
        private ESteps _steps = ESteps.None;
#endif

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="assetInfo">待加载资源信息</param>
        public VABHLoadAssetOperation(PackageBundle packageBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _assetInfo = assetInfo;
        }
        protected override void InternalStart()
        {
#if UNITY_EDITOR
            _steps = ESteps.CheckBundle;
#else
            SetError($"{nameof(VABHLoadAssetOperation)} is only supported in the Unity Editor.");
#endif
        }
        protected override void InternalUpdate()
        {
#if UNITY_EDITOR
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckBundle)
            {
                // 检测资源文件是否存在
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(_assetInfo.AssetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    _steps = ESteps.Done;
                    SetError($"Asset not found: '{_assetInfo.AssetPath}'.");
                    YooLogger.LogError(Error);
                    return;
                }

                _steps = ESteps.LoadAsset;
            }

            if (_steps == ESteps.LoadAsset)
            {
                if (_assetInfo.AssetType == null)
                    Result = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_assetInfo.AssetPath);
                else
                    Result = UnityEditor.AssetDatabase.LoadAssetAtPath(_assetInfo.AssetPath, _assetInfo.AssetType);
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result == null)
                {
                    string error;
                    if (_assetInfo.AssetType == null)
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: null.";
                    else
                        error = $"Failed to load asset: '{_assetInfo.AssetPath}' AssetType: {_assetInfo.AssetType}.";

                    _steps = ESteps.Done;
                    SetError(error);
                    YooLogger.LogError(error);
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }
#endif
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}