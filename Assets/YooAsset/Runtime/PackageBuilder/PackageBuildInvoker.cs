#if UNITY_EDITOR
using System;
using System.Reflection;

namespace YooAsset
{
    /// <summary>
    /// 通过反射调用编辑器构建方法的静态工具类
    /// </summary>
    public static class PackageBuildInvoker
    {
        /// <summary>
        /// 调用指定的编辑器构建方法，执行资源包裹构建任务。
        /// </summary>
        /// <param name="parameters">构建任务的配置参数</param>
        /// <returns>构建结果</returns>
        public static PackageBuildResult InvokeBuild(PackageBuildParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var assemblyName = parameters.AssemblyName;
            var className = parameters.TypeFullName;
            var methodName = parameters.MethodName;

            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentException("AssemblyName must not be null or empty.", nameof(parameters));
            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("TypeFullName must not be null or empty.", nameof(parameters));
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("MethodName must not be null or empty.", nameof(parameters));

            var assembly = Assembly.Load(assemblyName);
            var classType = assembly.GetType(className);
            if (classType == null)
                throw new InvalidOperationException($"Could not find type '{className}' in assembly '{assemblyName}'.");

            var result = InvokeStaticMethod(classType, methodName, parameters);
            if (result == null)
                throw new InvalidOperationException("Build method returned null.");
            if (!(result is PackageBuildResult buildResult))
                throw new InvalidOperationException($"Return type '{result.GetType().FullName}' does not match expected type {nameof(PackageBuildResult)}.");
            return buildResult;
        }

        private static object InvokeStaticMethod(Type targetType, string methodName, params object[] parameters)
        {
            var methodInfo = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
                throw new MissingMethodException($"Could not find method '{methodName}' on type '{targetType.FullName}'.");

            return methodInfo.Invoke(null, parameters);
        }
    }
}
#endif