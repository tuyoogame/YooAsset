using System;

namespace Framework.Machine
{
    /// <summary>
    /// 流程状态机配置
    /// </summary>
    public sealed class FlowMachineOptions
    {
        /// <summary>
        /// MaxTransitionsPerTick 的默认值，同时是下限
        /// </summary>
        public const int DefaultMaxTransitionsPerTick = 32;

        /// <summary>
        /// QueueCapacity 的默认值，同时是下限
        /// </summary>
        public const int DefaultQueueCapacity = 8;


        /// <summary>
        /// 单次分发内允许的最大连续迁移次数
        /// </summary>
        /// <remarks>
        /// <para>超过该次数说明可能存在状态切换环，状态机会丢弃余下的请求并输出错误日志。</para>
        /// <para>默认值放得比任何合理链条都宽，撞上基本就是环，因此只能往大调。</para>
        /// </remarks>
        public int MaxTransitionsPerTick { get; }

        /// <summary>
        /// 切换请求队列容量
        /// </summary>
        /// <remarks>
        /// <para>队列不扩容，写满说明存在切换环，请求会被拒绝并输出错误日志。</para>
        /// <para>默认值放得比任何合理批次都宽，写满基本就是环，因此只能往大调。</para>
        /// </remarks>
        public int QueueCapacity { get; }

        /// <summary>
        /// 是否输出迁移轨迹日志
        /// </summary>
        /// <remarks>
        /// <para>开启后每次迁移都会拼接日志字符串，只在排查问题时打开。</para>
        /// <para>轨迹日志走 FLogger.LogTag，在正式包中会被条件编译剥离。</para>
        /// </remarks>
        public bool EnableTraceLog { get; }

        /// <summary>
        /// 创建流程状态机配置
        /// </summary>
        /// <param name="maxTransitionsPerTick">单次分发内允许的最大连续迁移次数，不得小于默认值</param>
        /// <param name="queueCapacity">切换请求队列容量，不得小于默认值</param>
        /// <param name="enableTraceLog">是否输出迁移轨迹日志</param>
        public FlowMachineOptions(int maxTransitionsPerTick, int queueCapacity, bool enableTraceLog)
        {
            if (maxTransitionsPerTick < DefaultMaxTransitionsPerTick)
                throw new ArgumentOutOfRangeException(
                    nameof(maxTransitionsPerTick), maxTransitionsPerTick, $"MaxTransitionsPerTick must not be less than {DefaultMaxTransitionsPerTick}.");
            if (queueCapacity < DefaultQueueCapacity)
                throw new ArgumentOutOfRangeException(
                    nameof(queueCapacity), queueCapacity, $"QueueCapacity must not be less than {DefaultQueueCapacity}.");

            MaxTransitionsPerTick = maxTransitionsPerTick;
            QueueCapacity = queueCapacity;
            EnableTraceLog = enableTraceLog;
        }

        /// <summary>
        /// 使用公开下限并开启轨迹日志的默认配置
        /// </summary>
        public static readonly FlowMachineOptions Default = new FlowMachineOptions(
            DefaultMaxTransitionsPerTick, DefaultQueueCapacity, true);

        /// <summary>
        /// 创建测试用配置
        /// </summary>
        internal FlowMachineOptions(int maxTransitionsPerTick, int queueCapacity)
        {
            MaxTransitionsPerTick = maxTransitionsPerTick;
            QueueCapacity = queueCapacity;
        }
    }
}
