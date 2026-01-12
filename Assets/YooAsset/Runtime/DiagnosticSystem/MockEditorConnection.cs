using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 模拟的 Editor 连接
    /// </summary>
    /// <remarks>
    /// 在 Editor 模式下模拟 EditorConnection 的行为，用于本地调试。
    /// </remarks>
    internal sealed class MockEditorConnection
    {
        private MockEditorConnection() { }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            s_instance = null;
        }
#endif

        private static MockEditorConnection s_instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static MockEditorConnection Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new MockEditorConnection();
                return s_instance;
            }
        }

        private readonly Dictionary<Guid, UnityAction<MessageEventArgs>> _messageHandlers = new Dictionary<Guid, UnityAction<MessageEventArgs>>();

        /// <summary>
        /// 初始化连接
        /// </summary>
        public void Initialize()
        {
            _messageHandlers.Clear();
        }

        /// <summary>
        /// 注册消息处理回调
        /// </summary>
        /// <param name="messageId">消息标识符</param>
        /// <param name="callback">收到消息时的回调函数</param>
        public void Register(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (_messageHandlers.ContainsKey(messageId))
                _messageHandlers[messageId] += callback;
            else
                _messageHandlers[messageId] = callback;
        }

        /// <summary>
        /// 注销消息处理回调
        /// </summary>
        /// <param name="messageId">消息标识符</param>
        /// <param name="callback">要注销的回调函数</param>
        public void Unregister(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (_messageHandlers.ContainsKey(messageId))
            {
                _messageHandlers[messageId] -= callback;
                if (_messageHandlers[messageId] == null)
                    _messageHandlers.Remove(messageId);
            }
        }

        /// <summary>
        /// 向 Player 端发送消息
        /// </summary>
        /// <param name="messageId">消息标识符</param>
        /// <param name="data">消息数据</param>
        public void Send(Guid messageId, byte[] data)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            MockPlayerConnection.Instance.HandleEditorMessage(messageId, data);
        }

        /// <summary>
        /// 处理来自 Player 端的消息
        /// </summary>
        /// <param name="messageId">消息标识符</param>
        /// <param name="data">消息数据</param>
        internal void HandlePlayerMessage(Guid messageId, byte[] data)
        {
            if (_messageHandlers.TryGetValue(messageId, out UnityAction<MessageEventArgs> value))
            {
                var args = new MessageEventArgs();
                args.playerId = 0;
                args.data = data;
                value?.Invoke(args);
            }
        }
    }
}