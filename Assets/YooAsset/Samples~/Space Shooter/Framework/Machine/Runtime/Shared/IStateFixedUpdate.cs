namespace Framework.Machine
{
    /// <summary>
    /// 可选能力：接收引擎 FixedUpdate
    /// </summary>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    public interface IStateFixedUpdate<in TContext> where TContext : class
    {
        /// <summary>
        /// 驱动一次物理帧回调时调用
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        void OnFixedUpdate(TContext context);
    }
}
