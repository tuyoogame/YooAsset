#if YOOASSET_UNITASK_SUPPORT
using System;
using System.Threading;
using YooAsset;

namespace Cysharp.Threading.Tasks
{
    public static class HandleBaseExtensions
    {
        public static UniTask.Awaiter GetAwaiter(this HandleBase handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        public static UniTask WithCancellation(this HandleBase handle, CancellationToken cancellationToken, bool cancelImmediately = false)
        {
            return ToUniTask(handle, cancellationToken: cancellationToken, cancelImmediately: cancelImmediately);
        }

        public static UniTask ToUniTask(this HandleBase handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default, bool cancelImmediately = false)
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);
            if (!handle.IsValid || handle.IsDone) return UniTask.CompletedTask;

            return new UniTask(HandleBaseConfiguredSource.Create(handle, timing, progress, cancellationToken, cancelImmediately, out var token), token);
        }

        sealed class HandleBaseConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<HandleBaseConfiguredSource>
        {
            static TaskPool<HandleBaseConfiguredSource> pool;
            HandleBaseConfiguredSource nextNode;
            public ref HandleBaseConfiguredSource NextNode => ref nextNode;

            static HandleBaseConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(HandleBaseConfiguredSource), () => pool.Size);
            }

            HandleBase handle;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;
            IProgress<float> progress;
            bool cancelImmediately;
            bool completed;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            HandleBaseConfiguredSource()
            {
            }

            public static IUniTaskSource Create(HandleBase handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new HandleBaseConfiguredSource();
                }

                result.handle = handle;
                result.progress = progress;
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;
                result.completed = false;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
                    {
                        var source = (HandleBaseConfiguredSource)state;
                        source.core.TrySetCanceled(source.cancellationToken);
                    }, result);
                }

                TaskTracker.TrackActiveTask(result, 3);
                PlayerLoopHelper.AddAction(timing, result);

                // 注意：统一用强类型回调订阅 Handle.Completed，修复 IL2CPP 逆变委托崩溃
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
                    case BundleFileHandle bundle_file_handle:
                        bundle_file_handle.Completed += result.BundleFileContinuation;
                        break;
                    case AllAssetsHandle all_assets_handle:
                        all_assets_handle.Completed += result.AllAssetsContinuation;
                        break;
                }

                token = result.core.Version;
                return result;
            }

            void AssetContinuation(AssetHandle _) => HandleCompleted(null);
            void SceneContinuation(SceneHandle _) => HandleCompleted(null);
            void SubContinuation(SubAssetsHandle _) => HandleCompleted(null);
            void BundleFileContinuation(BundleFileHandle _) => HandleCompleted(null);
            void AllAssetsContinuation(AllAssetsHandle _) => HandleCompleted(null);

            void HandleCompleted(HandleBase _)
            {
                RemoveCompleted();

                if (completed) return;

                completed = true;
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                }
                else
                {
                    core.TrySetResult(AsyncUnit.Default);
                }
            }

            void RemoveCompleted()
            {
                if (handle == null || !handle.IsValid) return;

                switch (handle)
                {
                    case AssetHandle asset_handle:
                        asset_handle.Completed -= AssetContinuation;
                        break;
                    case SceneHandle scene_handle:
                        scene_handle.Completed -= SceneContinuation;
                        break;
                    case SubAssetsHandle sub_asset_handle:
                        sub_asset_handle.Completed -= SubContinuation;
                        break;
                    case BundleFileHandle bundle_file_handle:
                        bundle_file_handle.Completed -= BundleFileContinuation;
                        break;
                    case AllAssetsHandle all_assets_handle:
                        all_assets_handle.Completed -= AllAssetsContinuation;
                        break;
                }
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }
            
            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (completed)
                {
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completed = true;
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (handle == null || !handle.IsValid || handle.IsDone)
                {
                    completed = true;
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                if (progress != null && handle.IsValid)
                {
                    progress.Report(handle.Progress);
                }

                return true;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                return pool.TryPush(this);
            }
        }
    }
}
#endif
