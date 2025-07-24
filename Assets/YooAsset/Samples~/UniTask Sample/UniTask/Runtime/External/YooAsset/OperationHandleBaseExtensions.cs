#if UNITASK_YOOASSET_SUPPORT
#if UNITY_2020_1_OR_NEWER && ! UNITY_2021
#define UNITY_2020_BUG
#endif

using System;
using System.Runtime.CompilerServices;
using YooAsset;
using static Cysharp.Threading.Tasks.Internal.Error;

namespace Cysharp.Threading.Tasks
{
    public static class HandleBaseExtensions
    {
        public static UniTask.Awaiter GetAwaiter(this HandleBase handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }
        public static UniTask ToUniTask(this HandleBase handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            ThrowArgumentNullException(handle, nameof(handle));

            if (!handle.IsValid)
            {
                return UniTask.CompletedTask;
            }

            return new UniTask(
                HandleBaserConfiguredSource.Create(handle, timing, progress, out var token),
                token
            );
        }

        sealed class HandleBaserConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<HandleBaserConfiguredSource>
        {
            private static TaskPool<HandleBaserConfiguredSource> _pool;
            private HandleBaserConfiguredSource _nextNode;
            private readonly Action<HandleBase> _continuationAction;
            private HandleBase _handle;
            private IProgress<float> _progress;
            private bool _completed;
            private UniTaskCompletionSourceCore<AsyncUnit> _core;

            public ref HandleBaserConfiguredSource NextNode => ref _nextNode;

            static HandleBaserConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(HandleBaserConfiguredSource), () => _pool.Size);
            }

            HandleBaserConfiguredSource() { _continuationAction = Continuation; }

            public static IUniTaskSource Create(HandleBase handle, PlayerLoopTiming timing, IProgress<float> progress, out short token)
            {
                if (!_pool.TryPop(out var result))
                {
                    result = new HandleBaserConfiguredSource();
                }

                result._handle = handle;
                result._progress = progress;
                result._completed = false;
                TaskTracker.TrackActiveTask(result, 3);

                if (progress != null)
                {
                    PlayerLoopHelper.AddAction(timing, result);
                }

                // BUG 在 Unity 2020.3.36 版本测试中, IL2Cpp 会报 如下错误
                // BUG ArgumentException: Incompatible Delegate Types. First is System.Action`1[[YooAsset.AssetHandle, YooAsset, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] second is System.Action`1[[YooAsset.OperationHandleBase, YooAsset, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
                // BUG 也可能报的是 Action '1' Action '1' 的 InvalidCastException
                // BUG 此处不得不这么修改, 如果后续 Unity 修复了这个问题, 可以恢复之前的写法 
#if UNITY_2020_BUG
                switch (handle)
                {
                    case AssetHandle asset_handle:
                        asset_handle.Completed += result.AssetContinuation;
                        break;
                    case SceneHandle scene_handle:
                        scene_handle.Completed += result.SceneContinuation;
                        break;
                    case SubAssetsHandle sub_asset_handle:
                        sub_asset_handle.Completed += result.SubContinuation;
                        break;
                    case RawFileHandle raw_file_handle:
                        raw_file_handle.Completed += result.RawFileContinuation;
                        break;
                    case AllAssetsHandle all_assets_handle:
                        all_assets_handle.Completed += result.AllAssetsContinuation;
                        break;
                }
#else
                switch (handle)
                {
                    case AssetHandle asset_handle:
                        asset_handle.Completed += result.continuationAction;
                        break;
                    case SceneHandle scene_handle:
                        scene_handle.Completed += result.continuationAction;
                        break;
                    case SubAssetsHandle sub_asset_handle:
                        sub_asset_handle.Completed += result.continuationAction;
                        break;
                    case RawFileHandle raw_file_handle:
                        raw_file_handle.Completed += result.continuationAction;
                        break;
                    case AllAssetsHandle all_assets_handle:
                        all_assets_handle.Completed += result.continuationAction;
                        break;
                }
#endif
                token = result._core.Version;
                return result;
            }

#if UNITY_2020_BUG
            private void AssetContinuation(AssetHandle handle)
            {
                handle.Completed -= AssetContinuation;
                BaseContinuation();
            }
            private void SceneContinuation(SceneHandle handle)
            {
                handle.Completed -= SceneContinuation;
                BaseContinuation();
            }
            private void SubContinuation(SubAssetsHandle handle)
            {
                handle.Completed -= SubContinuation;
                BaseContinuation();
            }
            private void RawFileContinuation(RawFileHandle handle)
            {
                handle.Completed -= RawFileContinuation;
                BaseContinuation();
            }
            private void AllAssetsContinuation(AllAssetsHandle handle)
            {
                handle.Completed -= AllAssetsContinuation;
                BaseContinuation();
            }
#endif
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void BaseContinuation()
            {
                if (_completed)
                {
                    TryReturn();
                }
                else
                {
                    _completed = true;
                    if (_handle.Status == EOperationStatus.Failed)
                    {
                        _core.TrySetException(new Exception(_handle.LastError));
                    }
                    else
                    {
                        _core.TrySetResult(AsyncUnit.Default);
                    }
                }
            }
            private void Continuation(HandleBase _)
            {
                switch (_handle)
                {
                    case AssetHandle asset_handle:
                        asset_handle.Completed -= _continuationAction;
                        break;
                    case SceneHandle scene_handle:
                        scene_handle.Completed -= _continuationAction;
                        break;
                    case SubAssetsHandle sub_asset_handle:
                        sub_asset_handle.Completed -= _continuationAction;
                        break;
                    case RawFileHandle raw_file_handle:
                        raw_file_handle.Completed -= _continuationAction;
                        break;
                    case AllAssetsHandle all_assets_handle:
                        all_assets_handle.Completed -= _continuationAction;
                        break;
                }

                BaseContinuation();
            }
            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                _core.Reset();
                _handle = default;
                _progress = default;
                return _pool.TryPush(this);
            }

            public UniTaskStatus GetStatus(short token) => _core.GetStatus(token);
            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                _core.OnCompleted(continuation, state, token);
            }
            public void GetResult(short token) { _core.GetResult(token); }
            public UniTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();
            public bool MoveNext()
            {
                if (_completed)
                {
                    TryReturn();
                    return false;
                }

                if (_handle.IsValid)
                {
                    _progress?.Report(_handle.Progress);
                }

                return true;
            }
        }
    }
}
#endif