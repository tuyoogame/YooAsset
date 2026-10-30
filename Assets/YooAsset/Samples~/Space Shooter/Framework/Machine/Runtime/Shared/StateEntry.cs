
namespace Framework.Machine
{
    /// <summary>
    /// 解析完成的状态节点
    /// </summary>
    internal struct StateEntry<TContext> where TContext : class
    {
        public IState<TContext> State;
        public IStateUpdate<TContext> Update;
        public IStateFixedUpdate<TContext> FixedUpdate;
        public IStateLateUpdate<TContext> LateUpdate;
    }
}
