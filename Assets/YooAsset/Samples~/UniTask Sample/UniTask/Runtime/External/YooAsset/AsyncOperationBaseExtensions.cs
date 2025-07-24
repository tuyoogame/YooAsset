#if UNITASK_YOOASSET_SUPPORT
using System;
using YooAsset;
using static Cysharp.Threading.Tasks.Internal.Error;

namespace Cysharp.Threading.Tasks
{
    public static class AsyncOperationBaseExtensions
    {
        public static UniTask.Awaiter GetAwaiter(this AsyncOperationBase handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }
        public static UniTask ToUniTask(this AsyncOperationBase handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            ThrowArgumentNullException(handle, nameof(handle));

            if (handle.IsDone)
            {
                return UniTask.CompletedTask;
            }

            return new UniTask(
                AsyncOperationBaserConfiguredSource.Create(handle, timing, progress, out var token),
                token
            );
        }

        sealed class AsyncOperationBaserConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationBaserConfiguredSource>
        {
            private static TaskPool<AsyncOperationBaserConfiguredSource> _pool;
            private AsyncOperationBaserConfiguredSource _nextNode;
            private readonly Action<AsyncOperationBase> _continuationAction;
            private AsyncOperationBase _handle;
            private IProgress<float> _progress;
            private bool _completed;
            private UniTaskCompletionSourceCore<AsyncUnit> _core;

            public ref AsyncOperationBaserConfiguredSource NextNode => ref _nextNode;

            static AsyncOperationBaserConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationBaserConfiguredSource), () => _pool.Size);
            }

            AsyncOperationBaserConfiguredSource() { _continuationAction = Continuation; }

            public static IUniTaskSource Create(AsyncOperationBase handle, PlayerLoopTiming timing, IProgress<float> progress, out short token)
            {
                if (!_pool.TryPop(out var result))
                {
                    result = new AsyncOperationBaserConfiguredSource();
                }

                result._handle = handle;
                result._progress = progress;
                result._completed = false;
                TaskTracker.TrackActiveTask(result, 3);

                if (progress != null)
                {
                    PlayerLoopHelper.AddAction(timing, result);
                }

                handle.Completed += result._continuationAction;
                token = result._core.Version;
                return result;
            }
            
            private void Continuation(AsyncOperationBase _)
            {
                _handle.Completed -= _continuationAction;

                if (_completed)
                {
                    TryReturn();
                }
                else
                {
                    _completed = true;
                    if (_handle.Status == EOperationStatus.Failed)
                    {
                        _core.TrySetException(new Exception(_handle.Error));
                    }
                    else
                    {
                        _core.TrySetResult(AsyncUnit.Default);
                    }
                }
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

                if (!_handle.IsDone)
                {
                    _progress?.Report(_handle.Progress);
                }

                return true;
            }
        }
    }
}
#endif
