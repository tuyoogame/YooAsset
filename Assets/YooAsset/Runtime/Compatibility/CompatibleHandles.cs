#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - Handle 类兼容
// 通过 partial class 为各 Handle 类补充 v2.3 的旧属性和方法。

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// v2.3 下载状态结构体（v3 已移除）
    /// </summary>
    [Obsolete("DownloadStatus has been removed in v3.")]
    public struct DownloadStatus
    {
        public bool IsDone;
        public float Progress;
        public long TotalBytes;
        public long DownloadedBytes;

        public static DownloadStatus CreateDefaultStatus()
        {
            return new DownloadStatus();
        }
    }

    #region HandleBase -- LastError / Task / GetDownloadStatus
    public abstract partial class HandleBase
    {
        private TaskCompletionSource<object> _taskCompletionSource;

        /// <summary>
        /// v2.3: handle.LastError
        /// </summary>
        [Obsolete("Use Error instead.")]
        public string LastError => Error;

        /// <summary>
        /// v2.3: handle.Task (System.Threading.Tasks.Task)
        /// </summary>
        [Obsolete("Use 'await handle' directly instead.")]
        public Task Task
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return null;

                if (_taskCompletionSource == null)
                {
                    _taskCompletionSource = new TaskCompletionSource<object>();
                    if (Provider.IsDone)
                    {
                        _taskCompletionSource.TrySetResult(null);
                    }
                    else
                    {
                        Provider.Completed += delegate (AsyncOperationBase op)
                        {
                            _taskCompletionSource.TrySetResult(null);
                        };
                    }
                }
                return _taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// v2.3: handle.GetDownloadStatus()
        /// </summary>
        [Obsolete("GetDownloadStatus is no longer supported in v3.")]
        public DownloadStatus GetDownloadStatus()
        {
            return DownloadStatus.CreateDefaultStatus();
        }
    }
    #endregion

    #region SceneHandle -- UnSuspend / UnloadAsync
    public sealed partial class SceneHandle
    {
        /// <summary>
        /// v2.3: sceneHandle.UnSuspend() -> v3: AllowSceneActivation()
        /// </summary>
        [Obsolete("Use AllowSceneActivation() instead.")]
        public bool UnSuspend() => AllowSceneActivation();

        /// <summary>
        /// v2.3: sceneHandle.UnloadAsync() -> v3: UnloadSceneAsync()
        /// </summary>
        [Obsolete("Use UnloadSceneAsync() instead.")]
        public UnloadSceneOperation UnloadAsync() => UnloadSceneAsync();
    }
    #endregion

    #region BundleFileHandle -- GetBundleFilePath / GetRawFileData / GetRawFileText
    public sealed partial class BundleFileHandle
    {
        /// <summary>
        /// v2.3: rawFileHandle.GetRawFilePath()
        /// </summary>
        [Obsolete("Use EnsureBundleFileAsync() to get bundle file path via EnsureBundleFileOperation.Detail.BundleFilePath.")]
        public string GetBundleFilePath()
        {
            throw new NotSupportedException("GetBundleFilePath() is no longer available. Use EnsureBundleFileAsync() to get bundle file path via EnsureBundleFileOperation.Detail.BundleFilePath.");
        }

        /// <summary>
        /// v2.3: rawFileHandle.GetRawFileData()
        /// </summary>
        [Obsolete("Use LoadAssetAsync<RawFileObject>() to read binary data via RawFileObject.GetBytes().")]
        public byte[] GetRawFileData()
        {
            throw new NotSupportedException("GetRawFileData() is no longer available. Use LoadAssetAsync<RawFileObject>() to read binary data via RawFileObject.GetBytes().");
        }

        /// <summary>
        /// v2.3: rawFileHandle.GetRawFileText()
        /// </summary>
        [Obsolete("Use LoadAssetAsync<RawFileObject>() to read text data via RawFileObject.GetText().")]
        public string GetRawFileText()
        {
            throw new NotSupportedException("GetRawFileText() is no longer available. Use LoadAssetAsync<RawFileObject>() to read text data via RawFileObject.GetText().");
        }
    }
    #endregion

    #region AssetHandle -- Instantiate 多重载
    public sealed partial class AssetHandle
    {
        /// <summary>
        /// v2.3: handle.InstantiateSync(parent)
        /// </summary>
        [Obsolete("Use InstantiateSync(InstantiateOptions) instead.")]
        public GameObject InstantiateSync(Transform parent)
        {
            var options = new InstantiateOptions(true, parent, false);
            return InstantiateSync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateSync(parent, worldPositionStays)
        /// </summary>
        [Obsolete("Use InstantiateSync(InstantiateOptions) instead.")]
        public GameObject InstantiateSync(Transform parent, bool worldPositionStays)
        {
            var options = new InstantiateOptions(true, parent, worldPositionStays);
            return InstantiateSync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateSync(position, rotation)
        /// </summary>
        [Obsolete("Use InstantiateSync(InstantiateOptions) instead.")]
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation)
        {
            var options = new InstantiateOptions(true, position, rotation);
            return InstantiateSync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateSync(position, rotation, parent)
        /// </summary>
        [Obsolete("Use InstantiateSync(InstantiateOptions) instead.")]
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent)
        {
            var options = new InstantiateOptions(true, parent, position, rotation);
            return InstantiateSync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateAsync(parent, actived)
        /// </summary>
        [Obsolete("Use InstantiateAsync(InstantiateOptions) instead.")]
        public InstantiateOperation InstantiateAsync(Transform parent, bool actived = true)
        {
            var options = new InstantiateOptions(actived, parent, false);
            return InstantiateAsync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateAsync(parent, worldPositionStays, actived)
        /// </summary>
        [Obsolete("Use InstantiateAsync(InstantiateOptions) instead.")]
        public InstantiateOperation InstantiateAsync(Transform parent, bool worldPositionStays, bool actived = true)
        {
            var options = new InstantiateOptions(actived, parent, worldPositionStays);
            return InstantiateAsync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateAsync(position, rotation, actived)
        /// </summary>
        [Obsolete("Use InstantiateAsync(InstantiateOptions) instead.")]
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, bool actived = true)
        {
            var options = new InstantiateOptions(actived, position, rotation);
            return InstantiateAsync(options);
        }

        /// <summary>
        /// v2.3: handle.InstantiateAsync(position, rotation, parent, actived)
        /// </summary>
        [Obsolete("Use InstantiateAsync(InstantiateOptions) instead.")]
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent, bool actived = true)
        {
            var options = new InstantiateOptions(actived, parent, position, rotation);
            return InstantiateAsync(options);
        }
    }
    #endregion
}
#endif
