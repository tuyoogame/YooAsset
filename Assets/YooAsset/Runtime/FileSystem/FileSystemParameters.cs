using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 提供文件系统的创建参数与工厂方法
    /// </summary>
    public partial class FileSystemParameters
    {
        internal readonly Dictionary<string, object> _createParameters = new Dictionary<string, object>(100);

        /// <summary>
        /// 文件系统的完整类型名称
        /// </summary>
        /// <remarks>
        /// 格式: "命名空间.类型名,程序集" (例如 "namespace.class,assembly")
        /// </remarks>
        public string FileSystemTypeName { get; private set; }

        /// <summary>
        /// 文件系统的根目录
        /// </summary>
        public string PackageRoot { get; private set; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="fileSystemTypeName">文件系统的完整类型名称</param>
        /// <param name="packageRoot">文件系统的根目录</param>
        public FileSystemParameters(string fileSystemTypeName, string packageRoot)
        {
            FileSystemTypeName = fileSystemTypeName;
            PackageRoot = packageRoot;
        }

        /// <summary>
        /// 添加自定义参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">参数值</param>
        public void AddParameter(string paramName, object value)
        {
            _createParameters.Add(paramName, value);
        }

        /// <summary>
        /// 添加自定义参数
        /// </summary>
        /// <param name="paramType">参数类型</param>
        /// <param name="value">参数值</param>
        public void AddParameter(Enum paramType, object value)
        {
            _createParameters.Add(paramType.ToString(), value);
        }

        /// <summary>
        /// 根据配置创建文件系统实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>创建的文件系统实例</returns>
        internal IFileSystem CreateFileSystem(string packageName)
        {
            YooLogger.Log($"Package '{packageName}' created file system: '{FileSystemTypeName}'.");

            Type classType = Type.GetType(FileSystemTypeName);
            if (classType == null)
            {
                throw new InvalidOperationException($"Could not find file system class type: '{FileSystemTypeName}'.");
            }

            var instance = Activator.CreateInstance(classType, true) as IFileSystem;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create file system instance: '{FileSystemTypeName}'. Type must implement {nameof(IFileSystem)}.");
            }

            foreach (var param in _createParameters)
            {
                instance.SetParameter(param.Key, param.Value);
            }
            instance.OnCreate(packageName, PackageRoot);
            return instance;
        }

        #region 创建默认的文件系统类
        /// <summary>
        /// 创建默认的编辑器文件系统参数
        /// </summary>
        /// <param name="packageRoot">文件系统的根目录</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultEditorFileSystemParameters(string packageRoot)
        {
            string fileSystemClass = typeof(EditorFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的内置文件系统参数
        /// </summary>
        /// <param name="packageRoot">文件系统的根目录</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultBuiltinFileSystemParameters(string packageRoot)
        {
            string fileSystemClass = typeof(BuiltinFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的内置文件系统参数
        /// </summary>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultBuiltinFileSystemParameters()
        {
            string fileSystemClass = typeof(BuiltinFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的沙盒文件系统参数
        /// </summary>
        /// <param name="remoteService">远端资源地址查询服务类</param>
        /// <param name="packageRoot">文件系统的根目录</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultSandboxFileSystemParameters(IRemoteService remoteService, string packageRoot)
        {
            string fileSystemClass = typeof(SandboxFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的沙盒文件系统参数
        /// </summary>
        /// <param name="remoteService">远端资源地址查询服务类</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultSandboxFileSystemParameters(IRemoteService remoteService)
        {
            string fileSystemClass = typeof(SandboxFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的WebGL 服务器文件系统参数
        /// </summary>
        /// <param name="disableUnityWebCache">禁用Unity的网络缓存</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultWebServerFileSystemParameters(bool disableUnityWebCache)
        {
            string fileSystemClass = typeof(WebServerFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(EFileSystemParameter.DisableUnityWebCache, disableUnityWebCache);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的WebGL 服务器文件系统参数
        /// </summary>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultWebServerFileSystemParameters()
        {
            string fileSystemClass = typeof(WebServerFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(EFileSystemParameter.DisableUnityWebCache, false);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的 WebGL 网络文件系统参数
        /// </summary>
        /// <param name="remoteService">远端资源地址查询服务类</param>
        /// <param name="disableUnityWebCache">禁用Unity的网络缓存</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultWebNetworkFileSystemParameters(IRemoteService remoteService, bool disableUnityWebCache)
        {
            string fileSystemClass = typeof(WebNetworkFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
            fileSystemParams.AddParameter(EFileSystemParameter.DisableUnityWebCache, disableUnityWebCache);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的 WebGL 网络文件系统参数
        /// </summary>
        /// <param name="remoteService">远端资源地址查询服务类</param>
        /// <returns>配置好的文件系统参数实例</returns>
        public static FileSystemParameters CreateDefaultWebNetworkFileSystemParameters(IRemoteService remoteService)
        {
            string fileSystemClass = typeof(WebNetworkFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
            fileSystemParams.AddParameter(EFileSystemParameter.DisableUnityWebCache, false);
            return fileSystemParams;
        }
        #endregion
    }
}
