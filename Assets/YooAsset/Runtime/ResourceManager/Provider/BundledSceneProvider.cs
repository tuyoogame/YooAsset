using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal sealed class BundledSceneProvider : ProviderBase
    {
        public readonly LoadSceneMode SceneMode;
        private AsyncOperation _asyncOperation;
        private bool _suspendLoadMode;

        public BundledSceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad) : base(manager, providerGUID, assetInfo)
        {
            SceneMode = sceneMode;
            SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
            _suspendLoadMode = suspendLoad;
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
                    DependBundles.WaitForAsyncComplete();
                    OwnerBundle.WaitForAsyncComplete();
                }

                if (DependBundles.IsDone() == false)
                    return;
                if (OwnerBundle.IsDone() == false)
                    return;

                if (DependBundles.IsSucceed() == false)
                {
                    string error = DependBundles.GetLastError();
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    string error = OwnerBundle.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载场景
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete || IsForceDestroyComplete)
                {
                    // 注意：场景同步加载方法不会立即加载场景，而是在下一帧加载。
                    LoadSceneParameters parameters = new LoadSceneParameters(SceneMode);
                    SceneObject = SceneManager.LoadScene(MainAssetInfo.AssetPath, parameters);
                    _steps = ESteps.Checking;
                }
                else
                {
                    // 注意：如果场景不存在异步加载方法返回NULL
                    // 注意：即使是异步加载也要在当帧获取到场景对象
                    _asyncOperation = SceneManager.LoadSceneAsync(MainAssetInfo.AssetPath, SceneMode);
                    if (_asyncOperation != null)
                    {
                        _asyncOperation.allowSceneActivation = !_suspendLoadMode;
                        _asyncOperation.priority = 100;
                        SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        _steps = ESteps.Checking;
                    }
                    else
                    {
                        string error = $"Failed to load scene : {MainAssetInfo.AssetPath}";
                        YooLogger.Error(error);
                        InvokeCompletion(error, EOperationStatus.Failed);
                    }
                }
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (_asyncOperation != null)
                {
                    if (IsWaitForAsyncComplete || IsForceDestroyComplete)
                    {
                        // 场景加载无法强制异步转同步
                        YooLogger.Error("The scene is loading asyn !");
                    }
                    else
                    {
                        // 注意：在业务层中途可以取消挂起
                        if (_asyncOperation.allowSceneActivation == false)
                        {
                            if (_suspendLoadMode == false)
                                _asyncOperation.allowSceneActivation = true;
                        }

                        Progress = _asyncOperation.progress;
                        if (_asyncOperation.isDone == false)
                            return;
                    }
                }

                if (SceneObject.IsValid())
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
                else
                {
                    string error = $"The loaded scene is invalid : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
            }
        }

        /// <summary>
        /// 解除场景加载挂起操作
        /// </summary>
        public void UnSuspendLoad()
        {
            if (IsDone == false)
            {
                _suspendLoadMode = false;
            }
        }
    }
}