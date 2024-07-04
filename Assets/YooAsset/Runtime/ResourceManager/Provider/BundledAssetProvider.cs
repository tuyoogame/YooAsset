using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class BundledAssetProvider : ProviderBase
    {
        private AssetBundle _assetBundle;
        private AssetBundleRequest _cacheRequest;

        public BundledAssetProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (IsWaitForAsyncComplete)
                {
                    DependLoaders.WaitForAsyncComplete();
                    FileLoader.WaitForAsyncComplete();
                }

                if (DependLoaders.IsDone() == false)
                    return;
                if (FileLoader.IsDone() == false)
                    return;

                if (DependLoaders.IsSucceed() == false)
                {
                    string error = DependLoaders.GetLastError();
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (FileLoader.Status != BundleFileLoader.EStatus.Succeed)
                {
                    string error = FileLoader.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (FileLoader.Result == null)
                {
                    ProcessFatalEvent();
                    return;
                }

                if (FileLoader.Result is AssetBundle == false)
                {
                    string error = "Try load raw file using load assetbundle method !";
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _assetBundle = FileLoader.Result as AssetBundle;
                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete)
                {
                    if (MainAssetInfo.AssetType == null)
                        AssetObject = _assetBundle.LoadAsset(MainAssetInfo.AssetPath);
                    else
                        AssetObject = _assetBundle.LoadAsset(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
                }
                else
                {
                    if (MainAssetInfo.AssetType == null)
                        _cacheRequest = _assetBundle.LoadAssetAsync(MainAssetInfo.AssetPath);
                    else
                        _cacheRequest = _assetBundle.LoadAssetAsync(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
                }
                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (_cacheRequest != null)
                {
                    if (IsWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity asset.");
                        AssetObject = _cacheRequest.asset;
                    }
                    else
                    {
                        Progress = _cacheRequest.progress;
                        if (_cacheRequest.isDone == false)
                            return;
                        AssetObject = _cacheRequest.asset;
                    }
                }

                if (AssetObject == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                        error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {FileLoader.MainBundleInfo.Bundle.BundleName}";
                    else
                        error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {FileLoader.MainBundleInfo.Bundle.BundleName}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
            }
        }
    }
}