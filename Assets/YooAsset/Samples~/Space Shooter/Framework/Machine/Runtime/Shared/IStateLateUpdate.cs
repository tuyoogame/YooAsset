namespace Framework.Machine
{
    /// <summary>
    /// 可选能力：接收引擎 LateUpdate
    /// </summary>
    /// <typeparam name="TContext">状态回调共享的上下文</typeparam>
    public interface IStateLateUpdate<in TContext> where TContext : class
    {
        /// <summary>
        /// 驱动一次帧末回调时调用
        /// </summary>
        /// <param name="context">状态机绑定的上下文</param>
        void OnLateUpdate(TContext context);
    }
}
