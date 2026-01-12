using System;

namespace YooAsset
{
    /// <summary>
    /// 资源包裹构建的调用参数
    /// </summary>
    public class PackageBuildParameters
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipelineName { get; set; }

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType { get; set; }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        /// <remarks>
        /// 此属性为通用扩展点，调用方应自行确保存取时的类型一致性。
        /// </remarks>
        public object UserData { get; set; }

        /// <summary>
        /// 构建类所属的程序集名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 构建执行的类型全称
        /// </summary>
        /// <remarks>
        /// 类型名称必须包含命名空间
        /// </remarks>
        public string TypeFullName { get; set; }

        /// <summary>
        /// 构建执行的方法名称
        /// </summary>
        /// <remarks>
        /// 目标方法必须为公开静态方法（BindingFlags.Public | BindingFlags.Static）
        /// </remarks>
        public string MethodName { get; set; }

        /// <summary>
        /// 创建资源包裹构建参数实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public PackageBuildParameters(string packageName)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            PackageName = packageName;
        }
    }
}