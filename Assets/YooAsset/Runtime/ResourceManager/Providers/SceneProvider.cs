using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 场景提供者，负责场景资源的加载。
    /// </summary>
    internal sealed class SceneProvider : ProviderBase
    {
        private readonly LoadSceneParameters _loadSceneParams;
        private bool _allowSceneActivation;
        private BHLoadSceneOperation _loadSceneOp;

        public SceneProvider(ResourceManager manager, string providerKey, AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation) : base(manager, providerKey, assetInfo)
        {
            _loadSceneParams = loadSceneParams;
            _allowSceneActivation = allowSceneActivation;
            LoadedSceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
        }
        protected override void InternalProcessBundleHandle()
        {
            if (_loadSceneOp == null)
            {
                _loadSceneOp = LoadedBundleHandle.LoadSceneAsync(MainAssetInfo, _loadSceneParams, _allowSceneActivation);
                _loadSceneOp.StartOperation();
                AddChildOperation(_loadSceneOp);
            }

            if (IsWaitForCompletion)
                _loadSceneOp.WaitForCompletion();

            if (_allowSceneActivation)
                _loadSceneOp.AllowSceneActivation();

            _loadSceneOp.UpdateOperation();
            Progress = _loadSceneOp.Progress;
            if (_loadSceneOp.IsDone == false)
                return;

            if (_loadSceneOp.Status != EOperationStatus.Succeeded)
            {
                SetFail(_loadSceneOp.Error);
            }
            else
            {
                SceneObject = _loadSceneOp.Result;
                SetSuccess();
            }
        }

        /// <summary>
        /// 允许场景激活
        /// </summary>
        public void AllowSceneActivation()
        {
            _allowSceneActivation = true;
        }
    }
}