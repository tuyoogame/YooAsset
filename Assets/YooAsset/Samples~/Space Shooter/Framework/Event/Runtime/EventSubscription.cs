using System;

namespace Framework.Event
{
    /// <summary>
    /// 订阅句柄
    /// </summary>
    /// <remarks>
    /// 句柄可能被外部长期持有，失效后不再保留通道和回调引用。
    /// </remarks>
    /// <typeparam name="TEvent">事件消息类型</typeparam>
    internal sealed class EventSubscription<TEvent> : IDisposable where TEvent : class, IEventMessage
    {
        private EventChannel<TEvent> _channel;

        /// <summary>
        /// 事件监听回调
        /// </summary>
        public Action<TEvent> Callback { get; private set; }

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed => _channel == null;

        /// <summary>
        /// 创建订阅句柄实例
        /// </summary>
        /// <param name="channel">持有该订阅的事件通道</param>
        /// <param name="callback">接收事件消息的回调</param>
        public EventSubscription(EventChannel<TEvent> channel, Action<TEvent> callback)
        {
            _channel = channel;
            Callback = callback;
        }

        /// <summary>
        /// 使订阅失效
        /// </summary>
        public void Invalidate()
        {
            _channel = null;
            Callback = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_channel == null)
                return;

            var channel = _channel;
            Invalidate();
            channel.Remove(this);
        }
    }

    /// <summary>
    /// 空订阅句柄
    /// </summary>
    /// <remarks>
    /// <para>注册失败时返回，释放它不会影响任何已有订阅。</para>
    /// </remarks>
    internal sealed class EmptySubscription : IDisposable
    {
        /// <summary>
        /// 共享的空订阅句柄实例
        /// </summary>
        public static readonly EmptySubscription Instance = new EmptySubscription();

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
