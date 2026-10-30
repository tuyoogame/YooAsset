using System;
using System.Collections.Generic;

namespace Framework.Event
{
    /// <summary>
    /// 强类型事件通道
    /// </summary>
    /// <typeparam name="TEvent">事件消息类型</typeparam>
    internal sealed class EventChannel<TEvent> : IEventChannel where TEvent : class, IEventMessage
    {
        private readonly List<EventSubscription<TEvent>> _subscribers = new List<EventSubscription<TEvent>>();
        private bool _isPublishing = false;
        private bool _isDirty = false;

        /// <inheritdoc/>
        public Type EventType
        {
            get { return typeof(TEvent); }
        }

        /// <inheritdoc/>
        public int ListenerCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _subscribers.Count; i++)
                {
                    if (_subscribers[i].IsDisposed == false)
                        count++;
                }
                return count;
            }
        }

        /// <inheritdoc/>
        public bool IsPublishing
        {
            get { return _isPublishing; }
        }

        /// <summary>
        /// 添加监听器
        /// </summary>
        /// <param name="callback">接收事件消息的回调</param>
        /// <returns>通道内的订阅记录</returns>
        public IDisposable AddListener(Action<TEvent> callback)
        {
            var subscription = new EventSubscription<TEvent>(this, callback);
            _subscribers.Add(subscription);
            return subscription;
        }

        /// <summary>
        /// 移除已失效的订阅记录
        /// </summary>
        /// <param name="subscription">已失效处理的订阅记录</param>
        public void Remove(EventSubscription<TEvent> subscription)
        {
            if (_isPublishing)
            {
                // 广播期间延迟移除，避免修改正在遍历的订阅列表。
                _isDirty = true;
            }
            else
            {
                _subscribers.Remove(subscription);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (_isPublishing)
                throw new InvalidOperationException("Cannot clear the event bus during event publication.");

            // 先清空所有订阅的通道和回调引用，再统一清空列表。
            for (int i = 0; i < _subscribers.Count; i++)
            {
                _subscribers[i].Invalidate();
            }

            _subscribers.Clear();
            _isPublishing = false;
            _isDirty = false;
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="message">要广播的事件消息</param>
        public void Publish(TEvent message)
        {
            // 检测递归广播行为
            if (_isPublishing)
            {
                FLogger.Error($"Event type '{typeof(TEvent).FullName}' is already being published; recursive publication is not allowed.");
                return;
            }

            _isPublishing = true;
            try
            {
                // 本轮新增监听从下一次分发开始生效
                int count = _subscribers.Count;
                for (int i = 0; i < count && i < _subscribers.Count; i++)
                {
                    var subscription = _subscribers[i];
                    if (subscription.IsDisposed)
                        continue;

                    try
                    {
                        subscription.Callback.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
            }
            finally
            {
                _isPublishing = false;

                // 移除广播过程中已释放的订阅
                if (_isDirty)
                {
                    _isDirty = false;
                    RemoveDisposedSubscribers();
                }
            }
        }
        private void RemoveDisposedSubscribers()
        {
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                if (_subscribers[i].IsDisposed)
                    _subscribers.RemoveAt(i);
            }
        }
    }
}
