using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建上下文容器，管理构建流程中各阶段共享的上下文对象
    /// </summary>
    public class BuildContext
    {
        private readonly Dictionary<System.Type, object> _contextObjects = new Dictionary<System.Type, object>();

        /// <summary>
        /// 清空所有上下文对象
        /// </summary>
        public void ClearAllContext()
        {
            _contextObjects.Clear();
        }

        /// <summary>
        /// 注册上下文对象到容器
        /// </summary>
        /// <param name="contextObject">上下文对象实例</param>
        public void SetContextObject(object contextObject)
        {
            if (contextObject == null)
                throw new ArgumentNullException(nameof(contextObject));

            var type = contextObject.GetType();
            if (Attribute.GetCustomAttribute(type, typeof(ContextObjectAttribute)) == null)
                throw new ArgumentException($"Type '{type}' is not marked with [ContextObject] attribute.", nameof(contextObject));

            if (_contextObjects.ContainsKey(type))
                throw new InvalidOperationException($"Context object '{type}' already exists.");

            _contextObjects.Add(type, contextObject);
        }

        /// <summary>
        /// 获取指定类型的上下文对象
        /// </summary>
        /// <typeparam name="T">上下文对象的类型</typeparam>
        /// <returns>对应类型的上下文对象实例</returns>
        public T GetContextObject<T>() where T : class
        {
            var type = typeof(T);
            if (_contextObjects.TryGetValue(type, out object contextObject))
            {
                return (T)contextObject;
            }
            else
            {
                throw new InvalidOperationException($"Context object not found: '{type}'.");
            }
        }

        /// <summary>
        /// 尝试获取指定类型的上下文对象
        /// </summary>
        /// <typeparam name="T">上下文对象的类型</typeparam>
        /// <returns>对应类型的上下文对象实例，未找到时返回 null</returns>
        public T TryGetContextObject<T>() where T : class
        {
            var type = typeof(T);
            if (_contextObjects.TryGetValue(type, out object contextObject))
            {
                return (T)contextObject;
            }
            else
            {
                return default;
            }
        }
    }
}