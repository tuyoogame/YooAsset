using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class DatabaseAllAssetsProvider : ProviderOperation
    {
        public DatabaseAllAssetsProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
#if UNITY_EDITOR
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                // 检测资源文件是否存在
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(MainAssetInfo.AssetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    string error = $"Not found asset : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.CheckBundle;

                // 注意：模拟异步加载效果提前返回
                if (IsWaitForAsyncComplete == false)
                    return;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                    return;

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (MainAssetInfo.AssetType == null)
                {
                    List<UnityEngine.Object> result = new List<Object>();
                    foreach (var assetPath in LoadBundleFileOp.BundleFileInfo.IncludeAssetsInEditor)
                    {
                        UnityEngine.Object mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                        if (mainAsset != null)
                            result.Add(mainAsset);
                    }
                    AllAssetObjects = result.ToArray();
                }
                else
                {
                    List<UnityEngine.Object> result = new List<Object>();
                    foreach (var assetPath in LoadBundleFileOp.BundleFileInfo.IncludeAssetsInEditor)
                    {
                        UnityEngine.Object mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, MainAssetInfo.AssetType);
                        if (mainAsset != null)
                            result.Add(mainAsset);
                    }
                    AllAssetObjects = result.ToArray();
                }
                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (AllAssetObjects == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                        error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : null";
                    else
                        error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
            }
#endif
        }
    }
}