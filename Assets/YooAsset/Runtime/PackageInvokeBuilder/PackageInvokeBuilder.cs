#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
    public static class PackageInvokeBuilder
    {
        /// <summary>
        /// 调用Editro类来执行构建资源包任务
        /// </summary>
        public static PackageInvokeBuildResult InvokeBuilder(PackageInvokeBuildParam buildParam)
        {
            var assemblyName = buildParam.InvokeAssmeblyName;
            var className = buildParam.InvokeClassFullName;
            var methodName = buildParam.InvokeMethodName;
            var classType = Assembly.Load(assemblyName).GetType(className);
            return (PackageInvokeBuildResult)InvokePublicStaticMethod(classType, methodName, buildParam);
        }

        private static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }
    }
}
#else
namespace YooAsset
{ 
    public static class PackageInvokeBuilder
    {
        public static PackageInvokeBuildResult InvokeBuilder(PackageInvokeBuildParam buildParam)
        {
            throw new YooPlatformNotSupportedException("This feature is only supported in Unity Editor platform.");
        }
    }
}
#endif