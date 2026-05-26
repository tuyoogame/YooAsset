using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 初始化资源包的操作选项
    /// </summary>
    public abstract class InitializePackageOptions
    {
        /// <summary>
        /// 同时加载Bundle文件的最大并发数
        /// </summary>
        public int BundleLoadingMaxConcurrency { get; set; } = int.MaxValue;

        /// <summary>
        /// 是否在资源引用计数为零时自动释放资源包
        /// </summary>
        public bool AutoUnloadBundleWhenUnused { get; set; } = false;

        /// <summary>
        /// 是否在WebGL平台强制同步加载资源对象
        /// </summary>
        public bool WebGLForceSyncLoadAsset { get; set; } = false;
    }

    /// <summary>
    /// 编辑器下模拟运行模式的操作选项
    /// </summary>
    public class EditorSimulateModeOptions : InitializePackageOptions
    {
        /// <summary>
        /// 编辑器文件系统初始化参数
        /// </summary>
        public FileSystemParameters EditorFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 离线运行模式的操作选项
    /// </summary>
    public class OfflinePlayModeOptions : InitializePackageOptions
    {
        /// <summary>
        /// 内置文件系统初始化参数
        /// </summary>
        public FileSystemParameters BuiltinFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 联机运行模式的操作选项
    /// </summary>
    public class HostPlayModeOptions : InitializePackageOptions
    {
        /// <summary>
        /// 内置文件系统初始化参数
        /// </summary>
        public FileSystemParameters BuiltinFileSystemParameters { get; set; }

        /// <summary>
        /// 缓存文件系统初始化参数
        /// </summary>
        public FileSystemParameters CacheFileSystemParameters { get; set; }
    }

    /// <summary>
    /// WebGL运行模式的操作选项
    /// </summary>
    public class WebPlayModeOptions : InitializePackageOptions
    {
        /// <summary>
        /// Web 服务器文件系统初始化参数
        /// </summary>
        public FileSystemParameters WebServerFileSystemParameters { get; set; }

        /// <summary>
        /// Web 网络文件系统初始化参数
        /// </summary>
        public FileSystemParameters WebNetworkFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 自定义运行模式的操作选项
    /// </summary>
    public class CustomPlayModeOptions : InitializePackageOptions
    {
        /// <summary>
        /// 文件系统初始化参数列表
        /// </summary>
        /// <remarks>列表中最后一个元素将作为主文件系统</remarks>
        public readonly List<FileSystemParameters> FileSystemParameterList = new List<FileSystemParameters>();
    }
}