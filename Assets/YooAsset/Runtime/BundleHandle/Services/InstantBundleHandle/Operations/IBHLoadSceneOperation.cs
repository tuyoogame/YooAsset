#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 场景加载操作（InstantBundle）
    /// </summary>
    internal sealed class IBHLoadSceneOperation : BHLoadSceneOperation
    {
        private enum ESteps
        {
            None,
            CheckSceneTable,
            LoadScene,
            CheckResult,
            Done,
        }

        private readonly InstantAssetTable _sceneTable;
        private readonly AssetInfo _assetInfo;
        private readonly LoadSceneParameters _loadSceneParams;
        private bool _allowSceneActivation;
        private AsyncOperation _asyncOperation;
        private ESteps _steps = ESteps.None;

        public IBHLoadSceneOperation(InstantAssetTable sceneTable, AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            _sceneTable = sceneTable;
            _assetInfo = assetInfo;
            _loadSceneParams = loadSceneParams;
            _allowSceneActivation = allowSceneActivation;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckSceneTable;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckSceneTable)
            {
                if (_sceneTable == null)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(IBHLoadSceneOperation)} scene table is null, cannot load scene: '{_assetInfo.AssetPath}'.");
                    YooLogger.LogError(Error);
                    return;
                }

                _steps = ESteps.LoadScene;
            }

            if (_steps == ESteps.LoadScene)
            {
                if (IsWaitForCompletion)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(IBHLoadSceneOperation)} does not support synchronous scene loading.");
                    YooLogger.LogError(Error);
                    return;
                }
                else
                {
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
                        YooLogger.LogError("The scene is already loading asynchronously.");
                    }
                    else
                    {
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
            ExecuteOnce();
        }
        protected override void InternalAllowSceneActivation()
        {
            _allowSceneActivation = true;
        }
    }
}
#endif
