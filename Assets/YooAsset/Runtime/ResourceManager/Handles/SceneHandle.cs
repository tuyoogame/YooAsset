using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 场景句柄，用于管理场景的加载、激活和卸载。
    /// </summary>
    public sealed partial class SceneHandle : HandleBase
    {
        private System.Action<SceneHandle> _callback;

        /// <summary>
        /// 所属包裹名称
        /// </summary>
        internal string PackageName { set; get; }

        internal SceneHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 当加载完成时触发
        /// </summary>
        public event System.Action<SceneHandle> Completed
        {
            add
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(SceneHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(SceneHandle)} is invalid. It may have been released or the provider was destroyed.");
                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// 注意：场景加载不支持异步转同步，因此此方法有意设为 internal，防止外部误用。
        /// </summary>
        internal void WaitForAsyncComplete()
        {
            if (CheckValidWithWarning() == false)
                return;
            Provider.WaitForCompletion();
        }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return string.Empty;
                return Provider.LoadedSceneName;
            }
        }

        /// <summary>
        /// 场景对象
        /// </summary>
        public Scene SceneObject
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return new Scene();
                return Provider.SceneObject;
            }
        }

        /// <summary>
        /// 激活场景（当同时存在多个场景时用于切换激活场景）
        /// </summary>
        /// <returns>是否成功激活场景</returns>
        public bool ActivateScene()
        {
            if (CheckValidWithWarning() == false)
                return false;

            if (SceneObject.IsValid() && SceneObject.isLoaded)
            {
                return SceneManager.SetActiveScene(SceneObject);
            }
            else
            {
                YooLogger.LogWarning($"Scene is invalid or not loaded: '{SceneObject.name}'.");
                return false;
            }
        }

        /// <summary>
        /// 允许场景激活
        /// </summary>
        /// <returns>是否成功执行</returns>
        public bool AllowSceneActivation()
        {
            if (CheckValidWithWarning() == false)
                return false;

            if (Provider is SceneProvider)
            {
                var provider = Provider as SceneProvider;
                provider.AllowSceneActivation();
            }
            else
            {
                throw new YooInternalException($"Unexpected provider type: '{Provider.GetType().Name}'.");
            }
            return true;
        }

        /// <summary>
        /// 卸载场景对象
        /// </summary>
        /// <remarks>
        /// 场景卸载成功后，会自动释放该 handle 的引用计数。
        /// </remarks>
        /// <returns>卸载场景操作</returns>
        public UnloadSceneOperation UnloadSceneAsync()
        {
            string packageName = GetAssetInfo().PackageName;

            // 如果句柄无效
            if (CheckValidWithWarning() == false)
            {
                string error = $"{nameof(SceneHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                AsyncOperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 注意：如果场景正在加载过程，必须等待加载完成后才可以卸载该场景。
            {
                var operation = new UnloadSceneOperation(Provider);
                AsyncOperationSystem.StartOperation(packageName, operation);
                return operation;
            }
        }
    }
}