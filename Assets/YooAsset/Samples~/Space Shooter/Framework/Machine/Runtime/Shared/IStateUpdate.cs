namespace Framework.Machine
{
    /// <summary>
    /// 可选能力：接收引擎 Update
    /// </summary>
    /// <typeparam name="TContext">状态回调共享的上下文，必须是引用类型</typeparam>
    public interface IStateUpdate<in TContext> where TContext : class
    {
        /// <summary>
        /// 驱动一帧逻辑时调用
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        void OnUpdate(TContext context);
    }
}
