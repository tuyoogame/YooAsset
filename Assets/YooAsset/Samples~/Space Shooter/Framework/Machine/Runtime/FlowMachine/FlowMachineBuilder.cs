using System;
using System.Collections.Generic;

namespace Framework.Machine
{
    /// <summary>
    /// 流程状态机构建器
    /// </summary>
    /// <typeparam name="TStateId">状态标识</typeparam>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    public sealed class FlowMachineBuilder<TStateId, TContext>
        where TStateId : struct, Enum
        where TContext : class
    {
        private readonly List<TStateId> _stateIds = new List<TStateId>();
        private readonly List<IState<TContext>> _states = new List<IState<TContext>>();

        private FlowMachineOptions _options;
        private TStateId _startState;
        private bool _hasStartState;
        private bool _isBuilt;

        /// <summary>
        /// 按类型登记一个状态
        /// </summary>
        /// <typeparam name="TState">状态类型，需要有公开的无参构造函数</typeparam>
        /// <param name="id">状态标识</param>
        /// <returns>当前构建器</returns>
        /// <remarks>由构建器负责创建实例。</remarks>
        public FlowMachineBuilder<TStateId, TContext> AddState<TState>(TStateId id)
            where TState : IState<TContext>, new()
        {
            ThrowIfBuilt();
            RegisterState(id, new TState());
            return this;
        }

        /// <summary>
        /// 登记一个已创建好的状态
        /// </summary>
        /// <param name="id">状态标识</param>
        /// <param name="state">已创建的状态实例</param>
        /// <returns>当前构建器</returns>
        public FlowMachineBuilder<TStateId, TContext> AddState(TStateId id, IState<TContext> state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            ThrowIfBuilt();
            RegisterState(id, state);
            return this;
        }

        /// <summary>
        /// 指定入口状态
        /// </summary>
        /// <param name="id">入口状态标识</param>
        /// <returns>当前构建器</returns>
        /// <remarks>必须调用，否则 Build 会抛异常。</remarks>
        public FlowMachineBuilder<TStateId, TContext> SetStartState(TStateId id)
        {
            ThrowIfBuilt();
            _startState = id;
            _hasStartState = true;
            return this;
        }

        /// <summary>
        /// 设置运行选项
        /// </summary>
        /// <param name="options">运行选项</param>
        /// <returns>当前构建器</returns>
        /// <remarks>不调用则使用默认值。</remarks>
        public FlowMachineBuilder<TStateId, TContext> SetOptions(FlowMachineOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            ThrowIfBuilt();
            _options = options;
            return this;
        }

        /// <summary>
        /// 校验并产出一台绑定该上下文的流程状态机
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        /// <returns>已绑定该上下文的流程状态机</returns>
        /// <remarks>只能调用一次，构建后这个构建器不能再修改或再次构建。</remarks>
        public FlowMachine<TStateId, TContext> Build(TContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            ThrowIfBuilt();

            if (_states.Count == 0)
                throw new InvalidOperationException("Flow machine does not contain any registered states.");
            if (_hasStartState == false)
                throw new InvalidOperationException($"No start state has been specified. Call {nameof(SetStartState)} before Build().");

            FlowMachineOptions options = _options;
            if (options == null)
                options = FlowMachineOptions.Default;

            int startIndex = IndexOf(_startState);
            if (startIndex < 0)
                throw new ArgumentException($"Start state '{_startState}' is not registered.");

            // 提前解析可选能力接口，运行期分发时不再需要类型判断
            var entries = new StateEntry<TContext>[_states.Count];
            var names = new string[_states.Count];
            for (int i = 0; i < _states.Count; i++)
            {
                IState<TContext> state = _states[i];
                entries[i] = new StateEntry<TContext>
                {
                    State = state,
                    Update = state as IStateUpdate<TContext>,
                    FixedUpdate = state as IStateFixedUpdate<TContext>,
                    LateUpdate = state as IStateLateUpdate<TContext>,
                };
                names[i] = _stateIds[i].ToString();
            }

            // 创建状态机
            var stateIds = _stateIds.ToArray();
            var machine = new FlowMachine<TStateId, TContext>(
                context, stateIds, names, entries, startIndex, options);
            _isBuilt = true;
            return machine;
        }

        private void RegisterState(TStateId id, IState<TContext> state)
        {
            if (IndexOf(id) >= 0)
                throw new ArgumentException($"State '{id}' is already registered.", nameof(id));

            _stateIds.Add(id);
            _states.Add(state);
        }
        private int IndexOf(TStateId id)
        {
            for (int i = 0; i < _stateIds.Count; i++)
            {
                if (EqualityComparer<TStateId>.Default.Equals(_stateIds[i], id))
                    return i;
            }

            return -1;
        }
        private void ThrowIfBuilt()
        {
            if (_isBuilt)
                throw new InvalidOperationException("Build() has already been called on this builder. It cannot be modified or built again.");
        }
    }
}
