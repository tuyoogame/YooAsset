using System;

namespace Framework.Event
{
    /// <summary>
    /// 事件通道的统计信息
    /// </summary>
    public readonly struct EventChannelInfo
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public Type EventType { get; }

        /// <summary>
        /// 有效监听器数量
        /// </summary>
        public int ListenerCount { get; }

        /// <summary>
        /// 是否正在广播
        /// </summary>
        public bool IsPublishing { get; }

        internal EventChannelInfo(Type eventType, int listenerCount, bool isPublishing)
        {
            EventType = eventType;
            ListenerCount = listenerCount;
            IsPublishing = isPublishing;
        }
    }
}
