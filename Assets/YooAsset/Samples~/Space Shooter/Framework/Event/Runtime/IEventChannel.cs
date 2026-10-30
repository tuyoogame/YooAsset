using System;

namespace Framework.Event
{
    /// <summary>
    /// 事件通道接口
    /// </summary>
    internal interface IEventChannel
    {
        /// <summary>
        /// 通道消息类型
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// 有效监听器数量
        /// </summary>
        int ListenerCount { get; }

        /// <summary>
        /// 是否正在广播
        /// </summary>
        bool IsPublishing { get; }

        /// <summary>
        /// 清空通道监听器
        /// </summary>
        void Clear();
    }
}
