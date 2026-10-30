using System;
using System.Collections.Generic;

namespace Framework.Event
{
    /// <summary>
    /// 轻量级事件总线
    /// </summary>
    /// <remarks>
    /// <para>按事件类型精确路由，监听器按订阅先后顺序执行。</para>
    /// </remarks>
    public sealed class EventBus : IDisposable
    {
        private readonly Dictionary<Type, IEventChannel> _channels;
        private bool _isDisposed = false;
        private bool _isClearing = false;

        /// <summary>
        /// 创建事件总线实例
        /// </summary>
        /// <param name="options">事件总线配置</param>
        public EventBus(EventBusOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _channels = new Dictionary<Type, IEventChannel>(options.Capacity);
        }

        /// <summary>
        /// 已创建的事件通道数量
        /// </summary>
        /// <remarks>
        /// <para>没有监听器的空通道也包含在内</para>
        /// </remarks>
        public int ChannelCount
        {
            get { return _channels.Count; }
        }

        /// <summary>
        /// 所有事件通道的有效监听器总数
        /// </summary>
        public int ListenerCount
        {
            get
            {
                int count = 0;
                foreach (var channel in _channels.Values)
                {
                    count += channel.ListenerCount;
                }
                return count;
            }
        }

        /// <summary>
        /// 创建事件订阅作用域
        /// </summary>
        /// <returns>用于统一管理订阅生命周期的事件订阅作用域</returns>
        public EventSubscriptionScope CreateScope()
        {
            return new EventSubscriptionScope(this);
        }

        /// <summary>
        /// 清空所有监听
        /// </summary>
        /// <remarks>
        /// <para>清理期间新增的监听和广播会被忽略</para>
        /// </remarks>
        public void ClearAll()
        {
            if (_isClearing)
                return;

            _isClearing = true;
            try
            {
                foreach (var channel in _channels.Values)
                {
                    channel.Clear();
                }
                _channels.Clear();
            }
            finally
            {
                _isClearing = false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            ClearAll();
            _isDisposed = true;
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        /// <typeparam name="TEvent">事件消息类型</typeparam>
        /// <param name="listener">接收事件消息的回调</param>
        /// <returns>订阅句柄，释放它即可取消该次订阅。</returns>
        /// <remarks>
        /// <para>总线已释放或正在清理时，注册会被忽略，并返回可安全释放的空订阅句柄。</para>
        /// </remarks>
        public IDisposable AddListener<TEvent>(Action<TEvent> listener) where TEvent : class, IEventMessage
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            if (_isDisposed)
            {
                FLogger.Warning($"Listener registration is ignored because the event bus is disposed: '{typeof(TEvent).FullName}'.");
                return EmptySubscription.Instance;
            }

            if (_isClearing)
            {
                FLogger.Warning($"Listener registration is ignored while the event bus is clearing: '{typeof(TEvent).FullName}'.");
                return EmptySubscription.Instance;
            }

            Type eventType = typeof(TEvent);
            if (_channels.TryGetValue(eventType, out var channel))
            {
                var eventChannel = (EventChannel<TEvent>)channel;
                return eventChannel.AddListener(listener);
            }
            else
            {
                var newChannel = new EventChannel<TEvent>();
                _channels.Add(eventType, newChannel);
                return newChannel.AddListener(listener);
            }
        }

        /// <summary>
        /// 实时广播事件
        /// </summary>
        /// <typeparam name="TEvent">事件消息类型</typeparam>
        /// <param name="message">要广播的事件消息</param>
        /// <remarks>
        /// <para>总线已释放或正在清理时，广播会被忽略。</para>
        /// </remarks>
        public void SendMessage<TEvent>(TEvent message) where TEvent : class, IEventMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (_isDisposed)
            {
                FLogger.Warning($"Event message is ignored because the event bus is disposed: '{message.GetType().FullName}'.");
                return;
            }

            if (_isClearing)
            {
                FLogger.Warning($"Event message is ignored while the event bus is clearing: '{message.GetType().FullName}'.");
                return;
            }

            // 按消息的静态类型精确路由，不向基类监听者广播。
            if (_channels.TryGetValue(typeof(TEvent), out var channel))
            {
                ((EventChannel<TEvent>)channel).Publish(message);
            }
        }

        /// <summary>
        /// 获取所有事件通道的统计信息
        /// </summary>
        /// <returns>所有事件通道当前状态的无序快照</returns>
        public IReadOnlyList<EventChannelInfo> GetAllChannelInfos()
        {
            var results = new List<EventChannelInfo>(_channels.Count);
            foreach (var channel in _channels.Values)
            {
                results.Add(new EventChannelInfo(
                    eventType: channel.EventType,
                    listenerCount: channel.ListenerCount,
                    isPublishing: channel.IsPublishing));
            }
            return results;
        }
    }
}
