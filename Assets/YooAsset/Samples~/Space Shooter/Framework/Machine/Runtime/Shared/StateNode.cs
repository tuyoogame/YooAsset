namespace Framework.Machine
{
    /// <summary>
    /// 状态节点基类
    /// </summary>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    public abstract class StateNode<TContext> : IState<TContext> where TContext : class
    {
        /// <inheritdoc/>
        public abstract void OnEnter(TContext context);

        /// <inheritdoc/>
        public abstract void OnExit(TContext context);
    }
}
