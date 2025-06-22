using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YooAsset
{
    public abstract class AsyncOperationBase : IEnumerator, IComparable<AsyncOperationBase>
    {
        private Action<AsyncOperationBase> _callback;
        private string _packageName = null;
        private int _whileFrame = 1000;

        /// <summary>
        /// 所有子任务
        /// </summary>
        internal readonly List<AsyncOperationBase> Childs = new List<AsyncOperationBase>(10);

        /// <summary>
        /// 等待异步执行完成
        /// </summary>
        internal bool IsWaitForAsyncComplete { private set; get; } = false;

        /// <summary>
        /// 是否已经完成
        /// </summary>
        internal bool IsFinish { private set; get; } = false;

        /// <summary>
        /// 任务优先级
        /// </summary>
        public uint Priority { set; get; } = 0;

        /// <summary>
        /// 任务状态
        /// </summary>
        public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName
        {
            get
            {
                return _packageName;
            }
        }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Status == EOperationStatus.Failed || Status == EOperationStatus.Succeed;
            }
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                if (IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                _callback -= value;
            }
        }

        /// <summary>
        /// 异步操作任务
        /// </summary>
        public Task Task
        {
            get
            {
                if (_taskCompletionSource == null)
                {
                    _taskCompletionSource = new TaskCompletionSource<object>();
                    if (IsDone)
                        _taskCompletionSource.SetResult(null);
                }
                return _taskCompletionSource.Task;
            }
        }

        internal abstract void InternalStart();
        internal abstract void InternalUpdate();
        internal virtual void InternalAbort()
        {
        }
        internal virtual void InternalWaitForAsyncComplete()
        {
            throw new System.NotImplementedException(this.GetType().Name);
        }
        internal virtual string InternalGetDesc()
        {
            return string.Empty;
        }

        /// <summary>
        /// 设置包裹名称
        /// </summary>
        internal void SetPackageName(string packageName)
        {
            _packageName = packageName;
        }

        /// <summary>
        /// 添加子任务
        /// </summary>
        internal void AddChildOperation(AsyncOperationBase child)
        {
#if UNITY_EDITOR
            if (Childs.Contains(child))
                throw new Exception($"The child node {child.GetType().Name} already exists !");
#endif

            Childs.Add(child);
        }

        /// <summary>
        /// 获取异步操作说明
        /// </summary>
        internal string GetOperationDesc()
        {
            return InternalGetDesc();
        }

        /// <summary>
        /// 开始异步操作
        /// </summary>
        internal void StartOperation()
        {
            if (Status == EOperationStatus.None)
            {
                Status = EOperationStatus.Processing;

                // 开始记录
                DebugBeginRecording();

                // 开始任务
                InternalStart();
            }
        }

        /// <summary>
        /// 更新异步操作
        /// </summary>
        internal void UpdateOperation()
        {
            if (IsDone == false)
            {
                // 更新记录
                DebugUpdateRecording();

                // 更新任务
                InternalUpdate();
            }

            if (IsDone && IsFinish == false)
            {
                IsFinish = true;

                // 进度百分百完成
                Progress = 1f;

                // 结束记录
                DebugEndRecording();

                //注意：如果完成回调内发生异常，会导致Task无限期等待
                _callback?.Invoke(this);

                if (_taskCompletionSource != null)
                    _taskCompletionSource.TrySetResult(null);
            }
        }

        /// <summary>
        /// 终止异步任务
        /// </summary>
        internal void AbortOperation()
        {
            foreach (var child in Childs)
            {
                child.AbortOperation();
            }

            if (IsDone == false)
            {
                Status = EOperationStatus.Failed;
                Error = "user abort";
                YooLogger.Warning($"Async operaiton {this.GetType().Name} has been abort !");
                InternalAbort();
            }
        }

        /// <summary>
        /// 执行While循环
        /// </summary>
        protected bool ExecuteWhileDone()
        {
            if (IsDone == false)
            {
                // 执行更新逻辑
                InternalUpdate();

                // 当执行次数用完时
                _whileFrame--;
                if (_whileFrame <= 0)
                {
                    Status = EOperationStatus.Failed;
                    Error = $"Operation {this.GetType().Name} failed to wait for async complete !";
                    YooLogger.Error(Error);
                }
            }
            return IsDone;
        }

        /// <summary>
        /// 清空完成回调
        /// </summary>
        protected void ClearCompletedCallback()
        {
            _callback = null;
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsDone)
                return;

            //TODO 防止异步操作被挂起陷入无限死循环！
            // 例如：文件解压任务或者文件导入任务！
            if (Status == EOperationStatus.None)
            {
                StartOperation();
            }

            if (IsWaitForAsyncComplete == false)
            {
                IsWaitForAsyncComplete = true;
                InternalWaitForAsyncComplete();
            }
        }

        #region 调试信息
        /// <summary>
        /// 开始的时间
        /// </summary>
        public string BeginTime = string.Empty;

        /// <summary>
        /// 处理耗时（单位：毫秒）
        /// </summary>
        public long ProcessTime { protected set; get; }

        // 加载耗时统计
        private Stopwatch _watch = null;

        [Conditional("DEBUG")]
        private void DebugBeginRecording()
        {
            if (_watch == null)
            {
                BeginTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
                _watch = Stopwatch.StartNew();
            }
        }

        [Conditional("DEBUG")]
        private void DebugUpdateRecording()
        {
            if (_watch != null)
            {
                ProcessTime = _watch.ElapsedMilliseconds;
            }
        }

        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (_watch != null)
            {
                ProcessTime = _watch.ElapsedMilliseconds;
                _watch = null;
            }
        }

        private string SpawnTimeToString(float spawnTime)
        {
            float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
            float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
            float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }
        #endregion

        #region 排序接口实现
        public int CompareTo(AsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }
        #endregion

        #region 异步编程相关
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        void IEnumerator.Reset()
        {
        }
        object IEnumerator.Current => null;

        private TaskCompletionSource<object> _taskCompletionSource;
        #endregion
    }
}