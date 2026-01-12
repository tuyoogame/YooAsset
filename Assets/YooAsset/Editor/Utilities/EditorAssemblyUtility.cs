using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 程序集与属性工具类
    /// </summary>
    public static class EditorAssemblyUtility
    {
        /// <summary>
        /// 获取带继承关系的所有类的类型
        /// </summary>
        /// <param name="parentType">父类类型</param>
        /// <returns>所有派生类型的列表</returns>
        public static List<Type> GetAssignableTypes(System.Type parentType)
        {
            if (parentType == null)
                throw new ArgumentNullException(nameof(parentType));

            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(parentType);
            return collection.ToList();
        }

        /// <summary>
        /// 获取带有指定属性的所有类的类型
        /// </summary>
        /// <param name="attrType">属性类型</param>
        /// <returns>所有带有指定属性的类型列表</returns>
        public static List<Type> GetTypesWithAttribute(System.Type attrType)
        {
            if (attrType == null)
                throw new ArgumentNullException(nameof(attrType));

            TypeCache.TypeCollection collection = TypeCache.GetTypesWithAttribute(attrType);
            return collection.ToList();
        }

        /// <summary>
        /// 调用私有的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        /// <returns>方法的返回值</returns>
        public static object InvokeNonPublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentNullException(nameof(method));

            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
                throw new InvalidOperationException($"Method '{method}' not found in {type.FullName}.");

            return methodInfo.Invoke(null, parameters);
        }

        /// <summary>
        /// 调用公开的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        /// <returns>方法的返回值</returns>
        public static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentNullException(nameof(method));

            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
                throw new InvalidOperationException($"Method '{method}' not found in {type.FullName}.");

            return methodInfo.Invoke(null, parameters);
        }

        /// <summary>
        /// 获取指定类型上的自定义属性
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <returns>属性实例，不存在时为 null</returns>
        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T), false);
        }

        /// <summary>
        /// 获取指定方法上的自定义属性
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="methodInfo">目标方法</param>
        /// <returns>属性实例，不存在时为 null</returns>
        public static T GetAttribute<T>(MethodInfo methodInfo) where T : Attribute
        {
            return (T)methodInfo.GetCustomAttribute(typeof(T), false);
        }

        /// <summary>
        /// 获取指定字段上的自定义属性
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">目标字段</param>
        /// <returns>属性实例，不存在时为 null</returns>
        public static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            return (T)field.GetCustomAttribute(typeof(T), false);
        }
    }
}
