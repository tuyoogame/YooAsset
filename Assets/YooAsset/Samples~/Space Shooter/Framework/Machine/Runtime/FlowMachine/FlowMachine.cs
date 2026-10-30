using System;
using System.Collections.Generic;

namespace Framework.Machine
{
    /// <summary>
    /// 流程状态机
    /// </summary>
    /// <typeparam name="TStateId">状态标识</typeparam>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    /// <remarks>
    /// <para>面向单线程使用。</para>
    /// </remarks>
    public sealed class FlowMachine<TStateId, TContext>
        where TStateId : struct, Enum
        where TContext : class
    {
        /// <summary>
        /// 日志标签
        /// </summary>
        private const string LogTag = "Flow";

        /// <summary>
        /// 生命状态
        /// </summary>
        private enum MachineStatus
        {
            /// <summary>
            /// 尚未启动，或已经干净停止，可以再次 Start
            /// </summary>
            Idle,

            /// <summary>
            /// 已启动
            /// </summary>
            Running,

            /// <summary>
            /// 切换中途抛异常导致停机，该实例不可再用
            /// </summary>
            Faulted,
        }

        /// <summary>
        /// 调用栈当前的分发阶段
        /// </summary>
        private enum DispatchPhase
        {
            /// <summary>
            /// 不在任何回调里，可以安全地调用生命周期方法
            /// </summary>
            None,

            /// <summary>
            /// 正在处理请求队列或切换回调
            /// </summary>
            /// <remarks>此时新请求一律入队，交给外层的排空循环消化。</remarks>
            Firing,

            /// <summary>
            /// 正在跑 OnFixedUpdate 或 OnLateUpdate
            /// </summary>
            /// <remarks>此时请求切换属于编程错误，直接抛异常。</remarks>
            ReadOnly,
        }

        private readonly TContext _context;
        private readonly TStateId[] _stateIds;
        private readonly string[] _stateNames;
        private readonly StateEntry<TContext>[] _entries;
        private readonly RequestQueue _requestQueue;
        private readonly int _startIndex;
        private readonly int _maxTransitionsPerTick;
        private readonly int _queueCapacity;
        private readonly bool _enableTraceLog;

        private int _currentIndex = -1;
        private int _previousIndex = -1;
        private MachineStatus _status;
        private DispatchPhase _phase;
        private int _transitionsThisTick;

        /// <summary>
        /// 该实例绑定的上下文
        /// </summary>
        public TContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// 是否已启动且尚未停止
        /// </summary>
        public bool IsRunning
        {
            get { return _status == MachineStatus.Running; }
        }

        /// <summary>
        /// 是否因切换中途抛异常而停机
        /// </summary>
        /// <remarks>该实例不能再次 Start，只能丢弃重建。</remarks>
        public bool IsFaulted
        {
            get { return _status == MachineStatus.Faulted; }
        }

        /// <summary>
        /// 当前状态，未运行时为默认值
        /// </summary>
        public TStateId CurrentState
        {
            get { return _currentIndex >= 0 ? _stateIds[_currentIndex] : default(TStateId); }
        }

        /// <summary>
        /// 上一个状态，首次启动后不存在
        /// </summary>
        public TStateId PreviousState
        {
            get { return _previousIndex >= 0 ? _stateIds[_previousIndex] : default(TStateId); }
        }

        /// <summary>
        /// 是否存在上一个状态
        /// </summary>
        public bool HasPreviousState
        {
            get { return _previousIndex >= 0; }
        }

        /// <summary>
        /// 登记的状态数量
        /// </summary>
        public int StateCount
        {
            get { return _entries.Length; }
        }

        internal FlowMachine(
            TContext context,
            TStateId[] stateIds,
            string[] stateNames,
            StateEntry<TContext>[] entries,
            int startIndex,
            FlowMachineOptions options)
        {
            _context = context;
            _stateIds = stateIds;
            _stateNames = stateNames;
            _entries = entries;
            _startIndex = startIndex;

            _maxTransitionsPerTick = options.MaxTransitionsPerTick;
            _queueCapacity = options.QueueCapacity;
            _enableTraceLog = options.EnableTraceLog;
            _requestQueue = new RequestQueue(_queueCapacity);
        }

        /// <summary>
        /// 启动流程，对入口状态调用 OnEnter
        /// </summary>
        /// <remarks>
        /// <para>入口状态在 OnEnter 里请求的切换，会在 Start 返回前消化掉。</para>
        /// <para>已在运行或已故障停机时抛异常，后者只能丢弃实例重建。</para>
        /// </remarks>
        public void Start()
        {
            EnsureNotNested(nameof(Start));
            if (_status == MachineStatus.Running)
                throw new InvalidOperationException("Flow machine is already running. Start() cannot be called twice.");
            if (_status == MachineStatus.Faulted)
                throw new InvalidOperationException(
                    "Flow machine was halted by an exception thrown during a transition and cannot be restarted. " +
                    "Build a new instance instead.");

            _status = MachineStatus.Running;
            _currentIndex = _startIndex;
            _previousIndex = -1;
            _transitionsThisTick = 0;
            _requestQueue.Clear();

            _phase = DispatchPhase.Firing;
            try
            {
                EnterState(_currentIndex);

                // 说明：满足RTC设计，入口状态在 OnEnter 里发的请求在这里消化。
                RunToCompletion();
            }
            catch
            {
                Fault();
                throw;
            }
            finally
            {
                _phase = DispatchPhase.None;
            }
        }

        /// <summary>
        /// 停止流程，对当前状态调用 OnExit 并清空请求队列
        /// </summary>
        public void Stop()
        {
            EnsureNotNested(nameof(Stop));
            if (_status != MachineStatus.Running)
                return;

            _status = MachineStatus.Idle;
            _requestQueue.Clear();

            _phase = DispatchPhase.Firing;
            try
            {
                // 说明：退出期间仍可查询当前正在退出的状态
                ExitState(_currentIndex);
            }
            finally
            {
                _phase = DispatchPhase.None;
                _currentIndex = -1;
                _previousIndex = -1;
            }
        }

        /// <summary>
        /// 驱动一帧逻辑
        /// </summary>
        /// <remarks>先执行当前状态的 OnUpdate，再排空回调期间入队的请求。</remarks>
        public void Update()
        {
            EnsureRunning();
            EnsureNotNested(nameof(Update));

            _phase = DispatchPhase.Firing;
            try
            {
                _transitionsThisTick = 0;

                var callback = _entries[_currentIndex].Update;
                if (callback != null)
                    callback.OnUpdate(_context);

                RunToCompletion();
            }
            catch
            {
                // 注意：OnUpdate 抛异常时排空被跳过，这批请求必须丢掉，不能漏到下一帧！
                // 说明：索引没被破坏，状态是完整进入的，所以不必停机。
                _requestQueue.Clear();
                throw;
            }
            finally
            {
                _phase = DispatchPhase.None;
            }
        }

        /// <summary>
        /// 驱动一次物理帧回调
        /// </summary>
        /// <remarks>只读回调，在其中请求切换会抛异常，请把判断挪到 OnUpdate。</remarks>
        public void FixedUpdate()
        {
            EnsureRunning();
            EnsureNotNested(nameof(FixedUpdate));

            var callback = _entries[_currentIndex].FixedUpdate;
            if (callback == null)
                return;

            _phase = DispatchPhase.ReadOnly;
            try
            {
                callback.OnFixedUpdate(_context);
            }
            finally
            {
                _phase = DispatchPhase.None;
            }
        }

        /// <summary>
        /// 驱动一次帧末回调
        /// </summary>
        /// <remarks>只读回调，在其中请求切换会抛异常，请把判断挪到 OnUpdate。</remarks>
        public void LateUpdate()
        {
            EnsureRunning();
            EnsureNotNested(nameof(LateUpdate));

            var callback = _entries[_currentIndex].LateUpdate;
            if (callback == null)
                return;

            _phase = DispatchPhase.ReadOnly;
            try
            {
                callback.OnLateUpdate(_context);
            }
            finally
            {
                _phase = DispatchPhase.None;
            }
        }

        /// <summary>
        /// 判断当前是否处于指定状态
        /// </summary>
        /// <param name="state">要检查的状态标识</param>
        /// <returns>当前处于该状态时返回 true；未运行或不匹配时返回 false。</returns>
        public bool IsInState(TStateId state)
        {
            if (_currentIndex < 0)
                return false;

            return EqualityComparer<TStateId>.Default.Equals(_stateIds[_currentIndex], state);
        }

        /// <summary>
        /// 请求切换到指定状态
        /// </summary>
        /// <param name="state">目标状态标识</param>
        /// <returns>本次请求的处理结果</returns>
        /// <remarks>
        /// <para>分发期间调用会进入队列，在本次分发结束前消化。</para>
        /// <para>目标状态未登记，或调用发生在只读回调里，都属于编程错误，直接抛异常。</para>
        /// </remarks>
        public TransitionResult RequestChange(TStateId state)
        {
            int target = ToIndex(state);
            if (target < 0)
                throw new ArgumentException($"State '{state}' is not registered and cannot be entered.", nameof(state));
            if (_status != MachineStatus.Running)
                return TransitionResult.RejectedNotRunning;

            return InternalChangeQueued(target);
        }


        /// <summary>
        /// 逐个提交队列里的请求，直到队列排空或撞上迁移次数上限
        /// </summary>
        /// <returns>队列排空时返回 Success；撞上限时返回 RejectedLimitReached。</returns>
        /// <remarks>
        /// <para>撞上限时余下的请求整批丢弃，不会顺延到下一次分发。</para>
        /// <para>因此队列只在一次分发内存活，两次分发之间必定是空的。</para>
        /// </remarks>
        private TransitionResult RunToCompletion()
        {
            while (_requestQueue.TryDequeue(out int target))
            {
                if (ChargeBudget() == false)
                {
                    // 注意：撞上限说明大概率有环，与其带着一堆请求继续每帧空转，不如就地停住！
                    _requestQueue.Clear();
                    return TransitionResult.RejectedLimitReached;
                }

                InternalChangeOne(target);
            }

            return TransitionResult.Success;
        }
        private bool ChargeBudget()
        {
            // 占用一次本轮分发的迁移额度，额度耗尽时报错。
            if (_transitionsThisTick >= _maxTransitionsPerTick)
            {
                FLogger.ErrorTag(LogTag,
                    $"Transition limit {_maxTransitionsPerTick} was reached during a single dispatch. " +
                    $"Pending requests were discarded because a transition loop may exist. " +
                    $"Current state: '{_stateNames[_currentIndex]}'.");
                return false;
            }

            _transitionsThisTick++;
            return true;
        }

        /// <summary>
        /// 请求先入队，再决定由谁排空
        /// </summary>
        /// <param name="target">目标状态索引</param>
        /// <returns>本次请求的处理结果</returns>
        /// <remarks>已在处理中就交给外层循环，否则由本次调用独占排空到底。</remarks>
        private TransitionResult InternalChangeQueued(int target)
        {
            if (_phase == DispatchPhase.ReadOnly)
            {
                throw new InvalidOperationException(
                    "RequestChange() cannot be called from OnFixedUpdate() or OnLateUpdate(). " +
                    "These callbacks must not change the current state. Move the decision to OnUpdate().");
            }

            if (_requestQueue.TryEnqueue(target) == false)
            {
                FLogger.ErrorTag(LogTag,
                    $"Transition request queue is full (capacity: {_queueCapacity}). " +
                    $"Request was discarded because a transition loop may exist. " +
                    $"Current state: '{_stateNames[_currentIndex]}'.");
                return TransitionResult.RejectedQueueFull;
            }

            if (_phase == DispatchPhase.Firing)
                return TransitionResult.Queued;

            _phase = DispatchPhase.Firing;
            try
            {
                _transitionsThisTick = 0;
                return RunToCompletion();
            }
            finally
            {
                _phase = DispatchPhase.None;
            }
        }

        /// <summary>
        /// 完成一次切换：退出旧状态，改写索引，进入新状态
        /// </summary>
        /// <param name="target">目标状态索引</param>
        private void InternalChangeOne(int target)
        {
            int source = _currentIndex;
            try
            {
                ExitState(source);
                _previousIndex = source;
                _currentIndex = target;
                EnterState(target);
            }
            catch
            {
                // 注意：切换只完成了一半，内部索引已经不可信，只能停机！
                Fault();
                throw;
            }

            if (_enableTraceLog)
                FLogger.LogTag(LogTag, $"State '{_stateNames[source]}' --> '{_stateNames[target]}'.");
        }
        private void EnterState(int index)
        {
            _entries[index].State.OnEnter(_context);
        }
        private void ExitState(int index)
        {
            _entries[index].State.OnExit(_context);
        }

        /// <summary>
        /// 回调抛异常导致内部状态不可信时停机
        /// </summary>
        /// <remarks>
        /// <para>此时状态自身也处于半初始化，清理工作交给调用方。</para>
        /// <para>停机后不能再次启动，需要重新构建实例。</para>
        /// </remarks>
        private void Fault()
        {
            _status = MachineStatus.Faulted;
            _currentIndex = -1;
            _previousIndex = -1;
            _requestQueue.Clear();
        }

        private int ToIndex(TStateId state)
        {
            for (int i = 0; i < _stateIds.Length; i++)
            {
                if (EqualityComparer<TStateId>.Default.Equals(_stateIds[i], state))
                    return i;
            }

            return -1;
        }
        private void EnsureRunning()
        {
            if (_status != MachineStatus.Running)
                throw new InvalidOperationException("Flow machine is not running. Call Start() before updating it.");
        }
        private void EnsureNotNested(string operation)
        {
            if (_phase != DispatchPhase.None)
                throw new InvalidOperationException(
                    $"Lifecycle method '{operation}' cannot be called from a state callback. Nested lifecycle calls are not allowed.");
        }
    }
}
