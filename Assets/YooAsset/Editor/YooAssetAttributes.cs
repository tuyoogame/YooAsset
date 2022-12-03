using System;
using System.Reflection;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器显示名字
    /// </summary>
    internal sealed class EditorShowAttribute : Attribute 
    {
        public string Name;
        public EditorShowAttribute(string name)
        {
            this.Name = name;
        }
    }

    internal static class YooAssetAttributes
    {
        /// <summary>
        /// 获取 Type 属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns>可能为空</returns>
        internal static T GetAttribute<T>(Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T), false);
        }

        /// <summary>
        /// 获取 MethodInfo 属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInfo"></param>
        /// <returns>可能为空</returns>
        internal static T GetAttribute<T>(MethodInfo methodInfo) where T : Attribute
        {
            return (T)methodInfo.GetCustomAttribute(typeof(T), false);
        }

        /// <summary>
        /// 获取 FieldInfo 属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns>可能为空</returns>
        internal static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            return (T)field.GetCustomAttribute(typeof(T), false);
        }


    }
}
