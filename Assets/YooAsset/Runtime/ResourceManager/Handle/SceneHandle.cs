using UnityEngine.SceneManagement;

namespace YooAsset
{
    public class SceneHandle : HandleBase
    {
        private System.Action<SceneHandle> _callback;
        internal string PackageName { set; get; }

        internal SceneHandle(ProviderOperation provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<SceneHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(SceneHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(SceneHandle)} is invalid. It may have been released or the provider was destroyed.");
                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        internal void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName
        {
            get
            {
                if (IsValidWithWarning == false)
                    return string.Empty;
                return Provider.SceneName;
            }
        }

        /// <summary>
        /// 场景对象
        /// </summary>
        public Scene SceneObject
        {
            get
            {
                if (IsValidWithWarning == false)
                    return new Scene();
                return Provider.SceneObject;
            }
        }

        /// <summary>
        /// 激活场景（当同时存在多个场景时用于切换激活场景）
        /// </summary>
        public bool ActivateScene()
        {
            if (IsValidWithWarning == false)
                return false;

            if (SceneObject.IsValid() && SceneObject.isLoaded)
            {
                return SceneManager.SetActiveScene(SceneObject);
            }
            else
            {
                YooLogger.Warning($"Scene is invalid or not loaded : {SceneObject.name}");
                return false;
            }
        }

        /// <summary>
        /// 解除场景加载挂起操作
        /// </summary>
        public bool UnSuspend()
        {
            if (IsValidWithWarning == false)
                return false;

            if (Provider is SceneProvider)
            {
                var provider = Provider as SceneProvider;
                provider.UnSuspendLoad();
            }
            else
            {
                throw new System.NotImplementedException();
            }
            return true;
        }

        /// <summary>
        /// 异步卸载场景对象
        /// 注意：场景卸载成功后，会自动释放该handle的引用计数！
        /// </summary>
        public UnloadSceneOperation UnloadAsync()
        {
            string packageName = GetAssetInfo().PackageName;

            // 如果句柄无效
            if (IsValidWithWarning == false)
            {
                string error = $"{nameof(SceneHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 注意：如果场景正在加载过程，必须等待加载完成后才可以卸载该场景。
            {
                var operation = new UnloadSceneOperation(Provider);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }
        }
    }
}