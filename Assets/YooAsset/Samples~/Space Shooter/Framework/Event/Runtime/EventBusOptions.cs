using System;

namespace Framework.Event
{
    /// <summary>
    /// 事件总线创建配置
    /// </summary>
    public class EventBusOptions
    {
        /// <summary>
        /// 事件通道字典的初始容量
        /// 说明：不会预先创建事件通道。
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// 创建事件总线配置
        /// </summary>
        /// <param name="capacity">事件通道字典的初始容量</param>
        public EventBusOptions(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
        }
    }
}
