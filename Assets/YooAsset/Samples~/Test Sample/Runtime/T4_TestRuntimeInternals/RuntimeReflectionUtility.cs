using NUnit.Framework;
using System;
using System.Reflection;
using YooAsset;

/// <summary>
/// 运行时反射相关的工具类
/// </summary>
internal static class RuntimeReflectionUtility
{
    /// <summary>
    /// 调用实例内部的方法
    /// </summary>
    /// <remarks>
    /// 方法名必须唯一，不自动选择同名重载。
    /// </remarks>
    public static void InvokeAsyncOperationBaseMethod(AsyncOperationBase target, string methodName, params object[] args)
    {
        var method = GetMethod(typeof(AsyncOperationBase), methodName);
        method.Invoke(target, args);
    }

    /// <summary>
    /// 获取类内部的方法
    /// </summary>
    public static MethodInfo GetMethod(System.Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        Assert.IsNotNull(method, $"Not found method '{methodName}' in type '{type.Name}'");
        return method;
    }

    /// <summary>
    /// 读取实例字段，支持基类中的私有字段。
    /// </summary>
    public static object GetField(object target, string fieldName)
    {
        return FindField(target, fieldName).GetValue(target);
    }

    /// <summary>
    /// 设置实例字段，支持基类中的私有字段。
    /// </summary>
    public static void SetField(object target, string fieldName, object value)
    {
        FindField(target, fieldName).SetValue(target, value);
    }

    private static FieldInfo FindField(object target, string fieldName)
    {
        // 私有字段不随继承链自动返回，需要从实际类型逐级向上查找。
        Type type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null)
                return field;
            type = type.BaseType;
        }

        throw new MissingFieldException(target.GetType().FullName, fieldName);
    }
}
