namespace Framework.Machine
{
    /// <summary>
    /// 切换请求的处理结果
    /// </summary>
    public enum TransitionResult
    {
        /// <summary>
        /// 未赋值，不表示任何一次切换结果
        /// </summary>
        None,

        /// <summary>
        /// 已完成切换
        /// </summary>
        Success,

        /// <summary>
        /// 调用发生在分发期间，已入队等待处理。
        /// </summary>
        Queued,

        /// <summary>
        /// 被拒绝：尚未启动或已停止
        /// </summary>
        RejectedNotRunning,

        /// <summary>
        /// 被拒绝：本次分发已达迁移次数上限
        /// </summary>
        RejectedLimitReached,

        /// <summary>
        /// 被拒绝：请求队列已满
        /// </summary>
        RejectedQueueFull,
    }
}
