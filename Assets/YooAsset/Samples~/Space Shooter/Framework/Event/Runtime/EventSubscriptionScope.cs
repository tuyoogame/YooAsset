using System;
using System.Collections.Generic;

namespace Framework.Event
{
    /// <summary>
    /// 事件订阅组
    /// </summary>
    /// <remarks>
    /// <para>用于集中保存一批订阅，调用方应在宿主销毁时释放订阅组以统一退订。</para>
    /// </remarks>
    public class EventSubscriptionScope : IDisposable
    {
        private readonly EventBus _bus;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _isDisposed;

        /// <summary>
        /// 当前订阅组是否已释放
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }


        /// <summary>
        /// 创建事件订阅组实例
        /// </summary>
        /// <param name="bus">负责注册监听的事件总线</param>
        internal EventSubscriptionScope(EventBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// 添加一个事件监听
        /// </summary>
        /// <typeparam name="TEvent">事件消息类型</typeparam>
        /// <param name="listener">接收事件消息的回调</param>
        /// <returns>订阅句柄，释放它即可取消该次订阅。</returns>
        public IDisposable AddListener<TEvent>(Action<TEvent> listener) where TEvent : class, IEventMessage
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EventSubscriptionScope));

            var subscription = _bus.AddListener(listener);
            _subscriptions.Add(subscription);
            return subscription;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            // 句柄可重复释放，因此调用方提前退订不会影响订阅组的释放。
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
