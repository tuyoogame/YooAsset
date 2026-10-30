namespace Framework.Machine
{
    /// <summary>
    /// 状态的必需契约
    /// </summary>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    /// <remarks>按需实现 IStateUpdate、IStateFixedUpdate 与 IStateLateUpdate。</remarks>
    public interface IState<in TContext> where TContext : class
    {
        /// <summary>
        /// 进入该状态时调用
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        void OnEnter(TContext context);

        /// <summary>
        /// 退出该状态时调用
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        void OnExit(TContext context);
    }
}
