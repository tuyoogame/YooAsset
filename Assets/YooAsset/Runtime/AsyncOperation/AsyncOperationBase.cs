using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 异步操作基类
    /// </summary>
    public abstract partial class AsyncOperationBase : IEnumerator, IComparable<AsyncOperationBase>
    {
        private List<AsyncOperationBase> _children;
        private Action<AsyncOperationBase> _completedCallback;
        private List<Action<AsyncOperationBase>> _completedCallbackList;
        private EOperationStatus _status = EOperationStatus.None;
        private string _error;
        private uint _priority;

        /// <summary>
        /// 标记脏（用于调度器检测并重排）
        /// </summary>
        internal bool IsDirty { get; set; }

        /// <summary>
        /// 任务是否已结束（已触发回调和Task完成）
        /// </summary>
        internal bool IsCompleted { get; private set; }

        /// <summary>
        /// 是否正处于同步等待状态
        /// </summary>
        internal bool IsWaitForCompletion { get; private set; }

        /// <summary>
        /// 当前帧时间切片是否已用完
        /// </summary>
        /// <remarks>
        /// 同步等待时始终返回 false，以确保操作能持续执行直到完成。
        /// </remarks>
        protected bool IsBusy
        {
            get
            {
                if (IsWaitForCompletion)
                    return false;
                return AsyncOperationSystem.IsBusy;
            }
        }


        /// <summary>
        /// 任务优先级（值越大越优先执行）
        /// </summary>
        public uint Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (_priority == value)
                    return;
                _priority = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// 异步操作的处理进度（0f - 1f）
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 异步操作是否已结束
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Status == EOperationStatus.Succeeded || Status == EOperationStatus.Failed;
            }
        }

        /// <summary>
        /// 操作失败时的错误描述
        /// </summary>
        public string Error
        {
            get { return _error; }
        }

        /// <summary>
        /// 异步操作的当前状态
        /// </summary>
        public EOperationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// 异步操作的完成事件
        /// </summary>
        /// <remarks>
        /// 若注册时操作已完成，回调将立即执行。
        /// </remarks>
        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                if (IsDone)
                {
                    try
                    {
                        // 注意：任务已完成，立即调用回调
                        if (value != null)
                            value.Invoke(this);
                    }
                    catch (Exception ex)
                    {
                        YooLogger.LogError($"Exception in completion callback: {ex}.");
                    }
                }
                else
                {
                    if (_completedCallback == null)
                    {
                        _completedCallback = value;
                    }
                    else
                    {
                        if (_completedCallbackList == null)
                            _completedCallbackList = new List<Action<AsyncOperationBase>>(4);
                        _completedCallbackList.Add(value);
                    }
                }
            }
            remove
            {
                if (value == null)
                    return;

                if (_completedCallback == value)
                {
                    _completedCallback = null;
                }
                else if (_completedCallbackList != null)
                {
                    _completedCallbackList.Remove(value);
                }
            }
        }


        /// <summary>
        /// 同步等待异步执行完毕
        /// </summary>
        /// <remarks>
        /// 会在当前帧循环执行直到操作完成
        /// </remarks>
        public void WaitForCompletion()
        {
            // 注意：防止异步操作被挂起陷入无限死循环
            if (_status == EOperationStatus.None)
            {
                StartOperation();
            }

            if (IsWaitForCompletion == false)
            {
                IsWaitForCompletion = true;

                if (IsDone == false)
                    InternalWaitForCompletion();

                if (IsDone == false)
                {
                    _error = $"Operation '{GetType().Name}' did not complete during synchronous wait.";
                    _status = EOperationStatus.Failed;
                    YooLogger.LogError(_error);
                }

                // 注意：强制收尾，确保Task能完成
                CompleteOperation();
            }
        }

        /// <summary>
        /// 开始异步操作
        /// </summary>
        internal void StartOperation()
        {
            if (_status == EOperationStatus.None)
            {
                _status = EOperationStatus.Processing;

                // 开始记录
                DebugBeginRecording();

                // 开始任务
                try
                {
                    InternalStart();
                }
                catch (Exception ex)
                {
                    // 注意：无论子类是否已调用 SetResult/SetError，
                    // 内部逻辑抛出异常一律视为该异步任务失败。
                    _error = ex.ToString();
                    _status = EOperationStatus.Failed;
                    YooLogger.LogError($"Exception in {GetType().Name}.InternalStart: {ex}.");
                }

                // 注意：同步完成的操作立即收尾
                if (IsDone)
                {
                    CompleteOperation();
                }
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
                // 注意：兜底隔离机制
                // 说明：检测的异常源包含：I/O（解压/读写权限/磁盘满），平台差异等
                try
                {
                    InternalUpdate();
                }
                catch (Exception ex)
                {
                    // 注意：无论子类是否已调用 SetResult/SetError，
                    // 内部逻辑抛出异常一律视为该异步任务失败。
                    _error = ex.ToString();
                    _status = EOperationStatus.Failed;
                    YooLogger.LogError($"Exception in {GetType().Name}.InternalUpdate: {ex}.");
                }
            }

            if (IsDone && IsCompleted == false)
            {
                CompleteOperation();
            }
        }

        /// <summary>
        /// 终止异步任务（递归中止所有子任务）
        /// </summary>
        internal void AbortOperation()
        {
            if (_children != null)
            {
                for (int i = _children.Count - 1; i >= 0; i--)
                {
                    _children[i].AbortOperation();
                }
            }

            if (IsDone == false)
            {
                InternalAbort();
                _error = "Operation was aborted.";
                _status = EOperationStatus.Failed;
                YooLogger.LogWarning($"Async operation '{GetType().Name}' has been aborted.");
            }

            // 注意：强制收尾，确保Task能完成
            CompleteOperation();
        }


        /// <summary>
        /// 内部启动方法（子类必须实现）
        /// </summary>
        protected abstract void InternalStart();

        /// <summary>
        /// 内部更新方法（子类必须实现）
        /// </summary>
        protected abstract void InternalUpdate();

        /// <summary>
        /// 内部中止方法（子类可选实现）
        /// </summary>
        protected virtual void InternalAbort()
        {
        }

        /// <summary>
        /// 内部释放方法（子类可选实现）
        /// </summary>
        protected virtual void InternalDispose()
        {
        }

        /// <summary>
        /// 获取操作的描述信息（子类可选实现）
        /// </summary>
        /// <returns>操作的描述字符串，默认返回空字符串。</returns>
        protected virtual string InternalGetDescription()
        {
            return string.Empty;
        }

        /// <summary>
        /// 内部同步等待方法（子类可选实现）
        /// </summary>
        /// <remarks>
        /// 默认抛出异常，子类应重写以支持同步等待。
        /// </remarks>
        protected virtual void InternalWaitForCompletion()
        {
            throw new YooInternalException($"{GetType().Name} does not override InternalWaitForCompletion.");
        }


        /// <summary>
        /// 将操作标记为成功完成
        /// </summary>
        protected void SetResult()
        {
            if (IsDone)
                throw new InvalidOperationException(
                    $"Operation '{GetType().Name}' has already completed and cannot transition to another final state.");
            _status = EOperationStatus.Succeeded;
        }

        /// <summary>
        /// 将操作标记为失败
        /// </summary>
        /// <param name="error">错误描述</param>
        protected void SetError(string error)
        {
            if (IsDone)
                throw new InvalidOperationException(
                    $"Operation '{GetType().Name}' has already completed and cannot transition to another final state.");
            _error = error;
            _status = EOperationStatus.Failed;
        }

        /// <summary>
        /// 计算多阶段操作的整体进度
        /// </summary>
        /// <param name="stageIndex">当前阶段索引（从0开始）</param>
        /// <param name="stageCount">阶段总数</param>
        /// <param name="remaining">当前阶段剩余工作量</param>
        /// <param name="total">当前阶段总工作量</param>
        /// <returns>返回归一化的整体进度值（0-1）</returns>
        protected float CalculateMultiStageProgress(int stageIndex, int stageCount, int remaining, int total)
        {
            if (total <= 0)
                return (stageIndex + 1f) / stageCount;
            float stageProgress = 1f - remaining / (float)total;
            return (stageIndex + stageProgress) / stageCount;
        }

        /// <summary>
        /// 添加子任务
        /// </summary>
        /// <param name="child">要添加的子任务</param>
        protected void AddChildOperation(AsyncOperationBase child)
        {
            if (_children == null)
                _children = new List<AsyncOperationBase>(10);

#if UNITY_EDITOR || DEBUG
            if (child == null)
                throw new YooInternalException("Child operation is null.");

            if (ReferenceEquals(child, this))
                throw new YooInternalException("Cannot add operation as its own child.");

            if (_children.Contains(child))
                throw new YooInternalException($"Child operation '{child.GetType().Name}' already exists.");

            // 禁止形成环依赖
            if (WouldCreateCycle(child))
                throw new YooInternalException($"Adding '{child.GetType().Name}' would create a circular dependency with '{GetType().Name}'.");
#endif

            _children.Add(child);
        }

        /// <summary>
        /// 移除子任务
        /// </summary>
        /// <param name="child">要移除的子任务</param>
        protected void RemoveChildOperation(AsyncOperationBase child)
        {
            if (_children == null)
                return;

#if UNITY_EDITOR || DEBUG
            if (child == null)
                throw new YooInternalException("Child operation is null.");

            if (_children.Contains(child) == false)
                throw new YooInternalException($"Child operation '{child.GetType().Name}' not found.");
#endif

            _children.Remove(child);
        }

        /// <summary>
        /// 执行一次更新逻辑
        /// </summary>
        protected void ExecuteOnce()
        {
            if (IsDone)
                return;

            UpdateOperation();
        }

        /// <summary>
        /// 批量执行一定次数的更新逻辑
        /// </summary>
        /// <param name="count">最大执行次数，默认1000次。</param>
        /// <remarks>
        /// 用于需要快速完成但又不想完全阻塞主线程的场景
        /// </remarks>
        protected void ExecuteBatch(int count = 1000)
        {
            if (IsDone)
                return;

            int runCount = count;
            while (true)
            {
                UpdateOperation();
                if (IsDone)
                    break;

                runCount--;
                if (runCount <= 0)
                    break;
            }
        }

        /// <summary>
        /// 循环执行更新逻辑直到操作完成
        /// </summary>
        /// <param name="sleepMilliseconds">每次循环后的休眠时长（毫秒）</param>
        /// <remarks>
        /// 该方法会阻塞调用线程，每次更新之间会短暂休眠以避免占满 CPU。
        /// </remarks>
        protected void ExecuteUntilComplete(int sleepMilliseconds = 1)
        {
            if (IsDone)
                return;

            while (true)
            {
                UpdateOperation();
                if (IsDone)
                    break;

                // 注意：短暂休眠避免完全占用CPU资源
                System.Threading.Thread.Sleep(sleepMilliseconds);
            }
        }

        /// <summary>
        /// 完成异步任务（触发回调和Task完成）
        /// </summary>
        private void CompleteOperation()
        {
            if (IsCompleted == false)
            {
                IsCompleted = true;
                Progress = 1f;

                // 结束记录
                DebugEndRecording();

                try
                {
                    InternalDispose();
                }
                catch (Exception ex)
                {
                    YooLogger.LogError($"Exception in {GetType().Name}.InternalDispose: {ex}.");
                }

                InvokeCompletedCallbacks();
            }
        }

        /// <summary>
        /// 触发所有已注册的完成回调并清空
        /// </summary>
        private void InvokeCompletedCallbacks()
        {
            if (_completedCallback != null)
            {
                try
                {
                    _completedCallback.Invoke(this);
                }
                catch (Exception ex)
                {
                    YooLogger.LogError($"Exception in completion callback: {ex}.");
                }
                _completedCallback = null;
            }

            if (_completedCallbackList != null)
            {
                for (int i = 0; i < _completedCallbackList.Count; i++)
                {
                    try
                    {
                        _completedCallbackList[i].Invoke(this);
                    }
                    catch (Exception ex)
                    {
                        YooLogger.LogError($"Exception in completion callback: {ex}.");
                    }
                }
                _completedCallbackList = null;
            }
        }

        #region 调试信息
        /// <summary>
        /// 异步操作的开始时间，格式为 HH:MM:SS
        /// </summary>
        internal string StartTime { get; private set; }

        /// <summary>
        /// 处理耗时（单位：毫秒）
        /// </summary>
        internal long ElapsedMilliseconds { get; private set; }

        /// <summary>
        /// 任务耗时计时器
        /// </summary>
        private Stopwatch _stopwatch = null;

        [Conditional("DEBUG")]
        private void DebugBeginRecording()
        {
            if (_stopwatch == null)
            {
                StartTime = FormatElapsedTime(TimeUtility.RealtimeSinceStartup);
                _stopwatch = Stopwatch.StartNew();
            }
        }

        [Conditional("DEBUG")]
        private void DebugUpdateRecording()
        {
            if (_stopwatch != null)
            {
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            }
        }

        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (_stopwatch != null)
            {
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
                _stopwatch = null;
            }
        }

        /// <summary>
        /// 将游戏运行时间格式化为 HH:MM:SS 格式
        /// </summary>
        /// <param name="time">运行时间（秒）</param>
        private string FormatElapsedTime(double time)
        {
            double h = System.Math.Floor(time / 3600);
            double m = System.Math.Floor(time / 60 - h * 60);
            double s = System.Math.Floor(time - m * 60 - h * 3600);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        /// <summary>
        /// 检测添加子任务是否会形成循环依赖
        /// </summary>
        /// <remarks>
        /// 使用深度优先搜索遍历子任务图
        /// </remarks>
        private bool WouldCreateCycle(AsyncOperationBase child)
        {
            const int MaxCycleCheckDepth = 4096; // 循环检测最大深度
            var stack = new Stack<AsyncOperationBase>();
            var visited = new HashSet<AsyncOperationBase>();
            stack.Push(child);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node == null)
                    continue;

                // 防止重复访问
                if (visited.Add(node) == false)
                    continue;

                // 防止无限循环（图过大）
                if (visited.Count > MaxCycleCheckDepth)
                    throw new YooInternalException("Child operation graph is too large, cycle check aborted.");

                // 检测循环：如果遍历到自己，说明形成循环
                if (ReferenceEquals(node, this))
                    return true;

                if (node._children == null)
                    continue;

                // 将子节点加入栈
                for (int i = 0; i < node._children.Count; i++)
                {
                    stack.Push(node._children[i]);
                }
            }

            return false;
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        /// <remarks>
        /// 递归构建子树存在深度风险
        /// </remarks>
        internal DiagnosticOperationInfo GetDiagnosticInfo()
        {
            var operationInfo = new DiagnosticOperationInfo();
            operationInfo.OperationName = this.GetType().Name;
            operationInfo.OperationDescription = InternalGetDescription();
            operationInfo.Priority = Priority;
            operationInfo.Progress = Progress;
            operationInfo.StartTime = StartTime;
            operationInfo.ElapsedMilliseconds = ElapsedMilliseconds;
            operationInfo.Status = Status.ToString();

            if (_children == null)
            {
                operationInfo.Children = new List<DiagnosticOperationInfo>();
            }
            else
            {
                operationInfo.Children = new List<DiagnosticOperationInfo>(_children.Count);
                foreach (var child in _children)
                {
                    var childInfo = child.GetDiagnosticInfo();
                    operationInfo.Children.Add(childInfo);
                }
            }

            return operationInfo;
        }
        #endregion

        #region 排序接口实现
        /// <inheritdoc />
        public int CompareTo(AsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }
        #endregion

        #region 异步编程相关
        /// <summary>
        /// 获取用于 async/await 的等待器
        /// </summary>
        /// <returns>当前操作的等待器</returns>
        public OperationAwaiter GetAwaiter()
        {
            return new OperationAwaiter(this);
        }

        /// <inheritdoc />
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        /// <inheritdoc />
        void IEnumerator.Reset()
        {
        }
        /// <inheritdoc />
        object IEnumerator.Current => null;
        #endregion
    }
}
