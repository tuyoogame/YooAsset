using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 通过 EditorSceneManager 加载场景资源（仅编辑器模式）
    /// </summary>
    internal sealed class VABHLoadSceneOperation : BHLoadSceneOperation
    {
        private enum ESteps
        {
            None,
            LoadScene,
            CheckResult,
            Done,
        }

        private readonly AssetInfo _assetInfo;
        private readonly LoadSceneParameters _loadSceneParams;
        private bool _allowSceneActivation;
        private AsyncOperation _asyncOperation;

#if UNITY_EDITOR
        private ESteps _steps = ESteps.None;
#endif

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="assetInfo">待加载场景信息</param>
        /// <param name="loadSceneParams">控制场景加载的选项</param>
        /// <param name="allowSceneActivation">是否允许场景在加载完成后立即激活</param>
        public VABHLoadSceneOperation(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            _assetInfo = assetInfo;
            _loadSceneParams = loadSceneParams;
            _allowSceneActivation = allowSceneActivation;
        }
        protected override void InternalStart()
        {
#if UNITY_EDITOR
            _steps = ESteps.LoadScene;
#else
            SetError($"{nameof(VABHLoadSceneOperation)} is only supported in the Unity Editor.");
#endif
        }
        protected override void InternalUpdate()
        {
#if UNITY_EDITOR
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadScene)
            {
                if (IsWaitForCompletion)
                {
                    Result = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(_assetInfo.AssetPath, _loadSceneParams);
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _asyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(_assetInfo.AssetPath, _loadSceneParams);
                    if (_asyncOperation != null)
                    {
                        _asyncOperation.allowSceneActivation = _allowSceneActivation;
                        _asyncOperation.priority = 100;
                        Result = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        _steps = ESteps.CheckResult;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to load scene: '{_assetInfo.AssetPath}'.");
                        YooLogger.LogError(Error);
                    }
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_asyncOperation != null)
                {
                    if (IsWaitForCompletion)
                    {
                        // 注意：场景加载无法强制异步转同步
                        YooLogger.LogError("The scene is already loading asynchronously.");
                    }
                    else
                    {
                        // 注意：在业务层中途可以取消挂起
                        if (_asyncOperation.allowSceneActivation == false)
                        {
                            if (_allowSceneActivation)
                                _asyncOperation.allowSceneActivation = true;
                        }

                        Progress = _asyncOperation.progress;
                        if (_asyncOperation.isDone == false)
                            return;
                    }
                }

                if (Result.IsValid())
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Loaded scene is invalid: '{_assetInfo.AssetPath}'.");
                    YooLogger.LogError(Error);
                }
            }
#endif
        }
        protected override void InternalWaitForCompletion()
        {
            //注意：场景加载不支持异步转同步，为了支持同步加载方法需要实现该方法！
            ExecuteOnce();
        }
        protected override void InternalAllowSceneActivation()
        {
            _allowSceneActivation = true;
        }
    }
}