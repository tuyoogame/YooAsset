using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal sealed class DatabaseSceneProvider : ProviderBase
    {
        public readonly LoadSceneMode SceneMode;
        private bool _suspendLoadMode;
        private AsyncOperation _asyncOperation;

        public DatabaseSceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad) : base(manager, providerGUID, assetInfo)
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
#if UNITY_EDITOR
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
                    OwnerBundle.WaitForAsyncComplete();
                }

                if (OwnerBundle.IsDone() == false)
                    return;

                if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    string error = OwnerBundle.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete || IsForceDestroyComplete)
                {
                    LoadSceneParameters loadSceneParameters = new LoadSceneParameters(SceneMode);
                    SceneObject = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(MainAssetInfo.AssetPath, loadSceneParameters);
                    _steps = ESteps.Checking;
                }
                else
                {
                    LoadSceneParameters loadSceneParameters = new LoadSceneParameters(SceneMode);
                    _asyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(MainAssetInfo.AssetPath, loadSceneParameters);
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
#endif
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