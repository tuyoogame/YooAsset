using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 通过 AssetDatabase 加载指定资源的全部子资源对象（仅编辑器模式）
    /// </summary>
    internal sealed class VABHLoadSubAssetsOperation : BHLoadSubAssetsOperation
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
        public VABHLoadSubAssetsOperation(PackageBundle packageBundle, AssetInfo assetInfo)
        {
            _packageBundle = packageBundle;
            _assetInfo = assetInfo;
        }
        protected override void InternalStart()
        {
#if UNITY_EDITOR
            _steps = ESteps.CheckBundle;
#else
            SetError($"{nameof(VABHLoadSubAssetsOperation)} is only supported in the Unity Editor.");
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
                {
                    Result = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(_assetInfo.AssetPath);
                }
                else
                {
                    UnityEngine.Object[] findAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(_assetInfo.AssetPath);
                    List<UnityEngine.Object> result = new List<UnityEngine.Object>(findAssets.Length);
                    foreach (var findAsset in findAssets)
                    {
                        if (_assetInfo.AssetType.IsAssignableFrom(findAsset.GetType()))
                            result.Add(findAsset);
                    }
                    Result = result.ToArray();
                }
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result == null)
                {
                    string error;
                    if (_assetInfo.AssetType == null)
                        error = $"Failed to load sub-assets: '{_assetInfo.AssetPath}' AssetType: null.";
                    else
                        error = $"Failed to load sub-assets: '{_assetInfo.AssetPath}' AssetType: {_assetInfo.AssetType}.";

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