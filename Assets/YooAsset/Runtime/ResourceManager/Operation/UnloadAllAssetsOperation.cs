using System;
using UnityEngine;

namespace YooAsset
{
    public sealed class UnloadAllAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            AbortDownload,
            CheckLoading,
            UnloadAll,
            Done,
        }

        private readonly ResourceManager _resManager;
        private ESteps _steps = ESteps.None;

        internal UnloadAllAssetsOperation(ResourceManager resourceManager)
        {
            _resManager = resourceManager;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.AbortDownload;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.AbortDownload)
            {
                // 注意：终止所有下载任务
                var loaderDic = _resManager._loaderDic;
                foreach (var loader in loaderDic.Values)
                {
                    loader.AbortDownloadOperation();
                }
                _steps = ESteps.CheckLoading;
            }

            if (_steps == ESteps.CheckLoading)
            {
                // 注意：等待所有任务完成
                var providerDic = _resManager._providerDic;
                foreach (var provider in providerDic.Values)
                {
                    if (provider.IsDone == false)
                        return;
                }
                _steps = ESteps.UnloadAll;
            }

            if (_steps == ESteps.UnloadAll)
            {
                var loaderDic = _resManager._loaderDic;
                var providerDic = _resManager._providerDic;

                // 释放所有资源句柄
                foreach (var provider in providerDic.Values)
                {
                    provider.ReleaseAllHandles();
                }

                // 强制销毁资源提供者
                foreach (var provider in providerDic.Values)
                {
                    provider.DestroyProvider();
                }

                // 强制销毁文件加载器
                foreach (var loader in loaderDic.Values)
                {
                    loader.DestroyLoader();
                }

                // 清空数据
                providerDic.Clear();
                loaderDic.Clear();
                _resManager.ClearSceneHandle();

                // 注意：调用底层接口释放所有资源
                Resources.UnloadUnusedAssets();

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}