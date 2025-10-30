using System;
using UnityEngine;

namespace YooAsset
{
    public sealed class UnloadAllAssetsOptions
    {
        /// <summary>
        /// 释放所有资源句柄，防止卸载过程中触发完成回调！
        /// </summary>
        public bool ReleaseAllHandles = false;

        /// <summary>
        /// 卸载过程中锁定加载操作，防止新的任务请求！
        /// </summary>
        public bool LockLoadOperation = false;
    }

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

        private readonly ResourceManager _resManager;
        private readonly UnloadAllAssetsOptions _options;
        private ESteps _steps = ESteps.None;

        internal UnloadAllAssetsOperation(ResourceManager resourceManager, UnloadAllAssetsOptions options)
        {
            _resManager = resourceManager;
            _options = options;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckOptions;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckOptions)
            {
                if (_options == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(UnloadAllAssetsOptions)} is null.";
                    return;
                }

                // 设置锁定状态
                if (_options.LockLoadOperation)
                    _resManager.LockLoadOperation = true;

                _steps = ESteps.ReleaseAll;
            }

            if (_steps == ESteps.ReleaseAll)
            {
                // 清空所有场景句柄
                _resManager.SceneHandles.Clear();

                // 释放所有资源句柄
                if (_options.ReleaseAllHandles)
                {
                    foreach (var provider in _resManager.ProviderDic.Values)
                    {
                        provider.ReleaseAllHandles();
                    }
                }

                _steps = ESteps.TryAbortLoader;
            }

            if (_steps == ESteps.TryAbortLoader)
            {
                // 尝试终止所有加载任务
                // 注意：正在加载AssetBundle的任务无法终止
                foreach (var loader in _resManager.LoaderDic.Values)
                {
                    loader.TryAbortLoader();
                }
                _steps = ESteps.CheckLoading;
            }

            if (_steps == ESteps.CheckLoading)
            {
                // 注意：等待所有任务完成
                foreach (var provider in _resManager.ProviderDic.Values)
                {
                    if (provider.IsDone == false)
                        return;
                }
                _steps = ESteps.DestroyAll;
            }

            if (_steps == ESteps.DestroyAll)
            {
                // 强制销毁资源提供者
                foreach (var provider in _resManager.ProviderDic.Values)
                {
                    provider.DestroyProvider();
                }

                // 强制销毁文件加载器
                foreach (var loader in _resManager.LoaderDic.Values)
                {
                    loader.DestroyLoader();
                }

                // 清空数据
                _resManager.ProviderDic.Clear();
                _resManager.LoaderDic.Clear();
                _resManager.LockLoadOperation = false;

                // 注意：调用底层接口释放所有资源
                Resources.UnloadUnusedAssets();

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}