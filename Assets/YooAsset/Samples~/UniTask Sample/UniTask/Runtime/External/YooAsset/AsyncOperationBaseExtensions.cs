#if YOOASSET_UNITASK_SUPPORT
using System;
using System.Threading;
using YooAsset;

namespace Cysharp.Threading.Tasks
{
    public static class AsyncOperationBaseExtensions
    {
        public static UniTask.Awaiter GetAwaiter(this AsyncOperationBase handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        public static UniTask WithCancellation(this AsyncOperationBase handle, CancellationToken cancellationToken, bool cancelImmediately = false)
        {
            return ToUniTask(handle, cancellationToken: cancellationToken, cancelImmediately: cancelImmediately);
        }

        public static UniTask ToUniTask(this AsyncOperationBase handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default, bool cancelImmediately = false)
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);
            if (handle.IsDone) return UniTask.CompletedTask;

            return new UniTask(AsyncOperationBaseConfiguredSource.Create(handle, timing, progress, cancellationToken, cancelImmediately, out var token), token);
        }

        sealed class AsyncOperationBaseConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationBaseConfiguredSource>
        {
            static TaskPool<AsyncOperationBaseConfiguredSource> pool;
            AsyncOperationBaseConfiguredSource nextNode;
            public ref AsyncOperationBaseConfiguredSource NextNode => ref nextNode;

            static AsyncOperationBaseConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationBaseConfiguredSource), () => pool.Size);
            }

            readonly Action<AsyncOperationBase> completedCallback;
            AsyncOperationBase handle;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;
            IProgress<float> progress;
            bool cancelImmediately;
            bool completed;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            AsyncOperationBaseConfiguredSource()
            {
                completedCallback = HandleCompleted;
            }

            public static IUniTaskSource Create(AsyncOperationBase handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new AsyncOperationBaseConfiguredSource();
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
                        var source = (AsyncOperationBaseConfiguredSource)state;
                        source.core.TrySetCanceled(source.cancellationToken);
                    }, result);
                }

                TaskTracker.TrackActiveTask(result, 3);
                PlayerLoopHelper.AddAction(timing, result);

                handle.Completed += result.completedCallback;

                token = result.core.Version;
                return result;
            }

            void HandleCompleted(AsyncOperationBase _)
            {
                if (handle != null)
                {
                    handle.Completed -= completedCallback;
                }

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

                if (handle == null || handle.IsDone)
                {
                    completed = true;
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                if (progress != null)
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
