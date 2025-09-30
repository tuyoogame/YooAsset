using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 文件系统参数
    /// </summary>
    public class FileSystemParameters
    {
        internal readonly Dictionary<string, object> CreateParameters = new Dictionary<string, object>(100);

        /// <summary>
        /// 文件系统类
        /// 格式: "namespace.class,assembly"
        /// 格式: "命名空间.类型名,程序集"
        /// </summary>
        public string FileSystemClass { private set; get; }

        /// <summary>
        /// 文件系统的根目录
        /// </summary>
        public string PackageRoot { private set; get; }


        public FileSystemParameters(string fileSystemClass, string packageRoot)
        {
            FileSystemClass = fileSystemClass;
            PackageRoot = packageRoot;
        }

        /// <summary>
        /// 添加自定义参数
        /// </summary>
        public void AddParameter(string name, object value)
        {
            CreateParameters.Add(name, value);
        }

        /// <summary>
        /// 创建文件系统
        /// </summary>
        internal IFileSystem CreateFileSystem(string packageName)
        {
            YooLogger.Log($"The package {packageName} create file system : {FileSystemClass}");

            Type classType = Type.GetType(FileSystemClass);
            if (classType == null)
            {
                YooLogger.Error($"Can not found file system class type {FileSystemClass}");
                return null;
            }

            var instance = (IFileSystem)System.Activator.CreateInstance(classType, true);
            if (instance == null)
            {
                YooLogger.Error($"Failed to create file system instance {FileSystemClass}");
                return null;
            }

            foreach (var param in CreateParameters)
            {
                instance.SetParameter(param.Key, param.Value);
            }
            instance.OnCreate(packageName, PackageRoot);
            return instance;
        }

        #region 创建默认的文件系统类
        /// <summary>
        /// 创建默认的编辑器文件系统参数
        /// <param name="packageRoot">文件系统的根目录</param>
        /// </summary>
        public static FileSystemParameters CreateDefaultEditorFileSystemParameters(string packageRoot)
        {
            string fileSystemClass = typeof(DefaultEditorFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的内置文件系统参数
        /// </summary>
        /// <param name="decryptionServices">加密文件解密服务类</param>
        /// <param name="packageRoot">文件系统的根目录</param>
        public static FileSystemParameters CreateDefaultBuiltinFileSystemParameters(IDecryptionServices decryptionServices = null, string packageRoot = null)
        {
            string fileSystemClass = typeof(DefaultBuiltinFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的缓存文件系统参数
        /// </summary>
        /// <param name="remoteServices">远端资源地址查询服务类</param>
        /// <param name="decryptionServices">加密文件解密服务类</param>
        /// <param name="packageRoot">文件系统的根目录</param>
        public static FileSystemParameters CreateDefaultCacheFileSystemParameters(IRemoteServices remoteServices, IDecryptionServices decryptionServices = null, string packageRoot = null)
        {
            string fileSystemClass = typeof(DefaultCacheFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
            fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的WebServer文件系统参数
        /// </summary>
        /// <param name="decryptionServices">加密文件解密服务类</param>
        /// <param name="disableUnityWebCache">禁用Unity的网络缓存</param>
        public static FileSystemParameters CreateDefaultWebServerFileSystemParameters(IWebDecryptionServices decryptionServices = null, bool disableUnityWebCache = false)
        {
            string fileSystemClass = typeof(DefaultWebServerFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE, disableUnityWebCache);
            return fileSystemParams;
        }

        /// <summary>
        /// 创建默认的WebRemote文件系统参数
        /// </summary>
        /// <param name="remoteServices">远端资源地址查询服务类</param>
        /// <param name="decryptionServices">加密文件解密服务类</param>
        /// <param name="disableUnityWebCache">禁用Unity的网络缓存</param>
        public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteServices remoteServices, IWebDecryptionServices decryptionServices = null, bool disableUnityWebCache = false)
        {
            string fileSystemClass = typeof(DefaultWebRemoteFileSystem).FullName;
            var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
            fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
            fileSystemParams.AddParameter(FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE, disableUnityWebCache);
            return fileSystemParams;
        }
        #endregion
    }
}