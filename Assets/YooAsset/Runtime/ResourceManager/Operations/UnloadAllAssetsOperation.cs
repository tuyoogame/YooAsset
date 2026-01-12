using System;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 卸载所有资源的异步操作
    /// </summary>
    public sealed class UnloadAllAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckOptions,
            ReleaseAll,
            TryAbortLoader,
            CheckLoading,
            DestroyAll,
            Done,
        }

        private readonly ResourceManager _resourceManager;
        private readonly UnloadAllAssetsOptions _options;
        private ESteps _steps = ESteps.None;

        internal UnloadAllAssetsOperation(ResourceManager resourceManager, UnloadAllAssetsOptions options)
        {
            _resourceManager = resourceManager;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckOptions;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckOptions)
            {
                // 设置锁定状态
                if (_options.ShouldLockLoading)
                    _resourceManager.IsLoadingLocked = true;

                _steps = ESteps.ReleaseAll;
            }

            if (_steps == ESteps.ReleaseAll)
            {
                // 清空所有场景句柄
                _resourceManager.ClearSceneHandles();

                // 释放所有资源句柄
                if (_options.ShouldReleaseHandles)
                    _resourceManager.ReleaseAllHandles();

                _steps = ESteps.TryAbortLoader;
            }

            if (_steps == ESteps.TryAbortLoader)
            {
                // 尝试终止所有加载任务
                // 注意：正在加载AssetBundle的任务无法终止
                _resourceManager.TryAbortAllBundleLoaders();
                _steps = ESteps.CheckLoading;
            }

            if (_steps == ESteps.CheckLoading)
            {
                // 注意：等待所有任务完成
                if (_resourceManager.AreAllProvidersDone() == false)
                    return;
                _steps = ESteps.DestroyAll;
            }

            if (_steps == ESteps.DestroyAll)
            {
                // 强制销毁资源提供者
                _resourceManager.DestroyAllProviders();

                // 强制销毁文件加载器
                _resourceManager.DestroyAllBundleLoaders();
                _resourceManager.IsLoadingLocked = false;

                // 注意：调用底层接口释放所有资源
                Resources.UnloadUnusedAssets();

                _steps = ESteps.Done;
                SetResult();
            }
        }
    }
}