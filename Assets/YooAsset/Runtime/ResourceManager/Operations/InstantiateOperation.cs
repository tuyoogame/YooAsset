using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 游戏对象实例化操作
    /// </summary>
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadObject,
            CloneSync,
            CloneAsync,
            Done,
        }

        private readonly AssetHandle _handle;
        private readonly InstantiateOptions _options;
        private ESteps _steps = ESteps.None;

#if UNITY_2023_3_OR_NEWER
        private AsyncInstantiateOperation _instantiateAsync;
#endif

        /// <summary>
        /// 实例化的游戏对象
        /// </summary>
        public GameObject Result { get; internal set; }


        internal InstantiateOperation(AssetHandle handle, InstantiateOptions options)
        {
            _handle = handle;
            _options = options;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.LoadObject;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadObject)
            {
                if (_handle.CheckValidWithWarning() == false)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(AssetHandle)} is invalid.");
                    return;
                }

                if (IsWaitForCompletion)
                    _handle.WaitForAsyncComplete();

                if (_handle.IsDone == false)
                    return;

                if (_handle.AssetObject == null)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(AssetHandle.AssetObject)} is null.");
                    return;
                }

#if UNITY_2023_3_OR_NEWER
                //TODO 官方BUG
                // BUG环境：Windows平台，Unity2022.3.41f1版本，编辑器模式。
                // BUG描述：异步实例化Prefab预制体，有概率丢失Mono脚本里序列化的数组里某个成员！
                //_steps = ESteps.CloneAsync;
                _steps = ESteps.CloneSync;
#else
                _steps = ESteps.CloneSync;
#endif
            }

            if (_steps == ESteps.CloneSync)
            {
                // 实例化游戏对象
                Result = InstantiateInternal(_handle.AssetObject, _options);
                if (Result == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to instantiate asset: '{_handle.GetAssetInfo().AssetPath}'.");
                    return;
                }

                if (_options.IsActive == false)
                    Result.SetActive(false);

                _steps = ESteps.Done;
                SetResult();
            }

#if UNITY_2023_3_OR_NEWER
            if (_steps == ESteps.CloneAsync)
            {
                if (_instantiateAsync == null)
                {
                    _instantiateAsync = InstantiateAsyncInternal(_handle.AssetObject, _options);
                    if (_instantiateAsync == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to instantiate asset: '{_handle.GetAssetInfo().AssetPath}'.");
                        return;
                    }
                }

                if (IsWaitForCompletion)
                    _instantiateAsync.WaitForCompletion();

                if (_instantiateAsync.isDone == false)
                    return;

                if (_instantiateAsync.Result != null && _instantiateAsync.Result.Length > 0)
                {
                    Result = _instantiateAsync.Result[0] as GameObject;
                    if (Result != null)
                    {
                        if (_options.IsActive == false)
                            Result.SetActive(false);

                        _steps = ESteps.Done;
                        SetResult();
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError($"Instantiate result could not be cast to GameObject.");
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Async instantiate operation returned null or empty results.");
                }
            }
#endif
        }
        /// <inheritdoc />
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
        /// <inheritdoc />
        protected override string InternalGetDescription()
        {
            var assetInfo = _handle.GetAssetInfo();
            return $"AssetPath: {assetInfo.AssetPath}";
        }

        /// <summary>
        /// 取消实例化对象操作
        /// </summary>
        public void Cancel()
        {
#if UNITY_2023_3_OR_NEWER
            if (_instantiateAsync != null && _instantiateAsync.isDone == false)
                _instantiateAsync.Cancel();
#endif

            AbortOperation();
        }

        /// <summary>
        /// 同步实例化
        /// </summary>
        internal static GameObject InstantiateInternal(UnityEngine.Object assetObject, InstantiateOptions options)
        {
            if (assetObject == null)
                return null;

            var go = assetObject as GameObject;
            if (go == null)
            {
                YooLogger.LogError($"Failed to instantiate. Asset is not a GameObject, actual type: '{assetObject.GetType().Name}'.");
                return null;
            }

            if (options.SetPositionAndRotation)
            {
                if (options.Parent != null)
                    return UnityEngine.Object.Instantiate(go, options.Position, options.Rotation, options.Parent);
                else
                    return UnityEngine.Object.Instantiate(go, options.Position, options.Rotation);
            }
            else
            {
                if (options.Parent != null)
                    return UnityEngine.Object.Instantiate(go, options.Parent, options.InWorldSpace);
                else
                    return UnityEngine.Object.Instantiate(go);
            }
        }

#if UNITY_2023_3_OR_NEWER
        /// <summary>
        /// 异步实例化游戏对象
        /// </summary>
        /// <remarks>
        /// <para>Unity2022.3.20f1 及以上版本生效。</para>
        /// <para>https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Object.InstantiateAsync.html</para>
        /// </remarks>
        internal static AsyncInstantiateOperation InstantiateAsyncInternal(UnityEngine.Object assetObject, InstantiateOptions options)
        {
            var go = assetObject as GameObject;
            if (go == null)
            {
                YooLogger.LogError($"Failed to instantiate async. Asset is not a GameObject, actual type: '{assetObject.GetType().Name}'.");
                return null;
            }

            if (options.SetPositionAndRotation)
            {
                if (options.Parent != null)
                    return UnityEngine.Object.InstantiateAsync(go, options.Parent, options.Position, options.Rotation);
                else
                    return UnityEngine.Object.InstantiateAsync(go, options.Position, options.Rotation);
            }
            else
            {
                if (options.Parent != null)
                    return UnityEngine.Object.InstantiateAsync(go, options.Parent);
                else
                    return UnityEngine.Object.InstantiateAsync(go);
            }
        }
#endif
    }
}