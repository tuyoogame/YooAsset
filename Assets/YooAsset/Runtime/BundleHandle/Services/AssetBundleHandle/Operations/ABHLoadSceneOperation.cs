using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 从 AssetBundle 中加载场景资源
    /// </summary>
    internal sealed class ABHLoadSceneOperation : BHLoadSceneOperation
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
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="assetInfo">待加载场景信息</param>
        /// <param name="loadSceneParams">控制场景加载的选项</param>
        /// <param name="allowSceneActivation">是否允许场景在加载完成后立即激活</param>
        public ABHLoadSceneOperation(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            _assetInfo = assetInfo;
            _loadSceneParams = loadSceneParams;
            _allowSceneActivation = allowSceneActivation;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadScene;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadScene)
            {
                if (IsWaitForCompletion)
                {
                    // 注意：场景同步加载方法不会立即加载场景，而是在下一帧加载。
                    Result = SceneManager.LoadScene(_assetInfo.AssetPath, _loadSceneParams);
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    // 注意：如果场景不存在异步加载方法返回NULL
                    // 注意：即使是异步加载也要在当帧获取到场景对象
                    _asyncOperation = SceneManager.LoadSceneAsync(_assetInfo.AssetPath, _loadSceneParams);
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