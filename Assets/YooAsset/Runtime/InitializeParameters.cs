using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum EPlayMode
    {
        /// <summary>
        /// 编辑器下的模拟模式
        /// </summary>
        EditorSimulateMode,

        /// <summary>
        /// 离线运行模式
        /// </summary>
        OfflinePlayMode,

        /// <summary>
        /// 联机运行模式
        /// </summary>
        HostPlayMode,

        /// <summary>
        /// WebGL运行模式
        /// </summary>
        WebPlayMode,

        /// <summary>
        /// 自定义运行模式
        /// </summary>
        CustomPlayMode,
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    public abstract class InitializeParameters
    {
        /// <summary>
        /// 同时加载Bundle文件的最大并发数
        /// </summary>
        public int BundleLoadingMaxConcurrency = int.MaxValue;

        /// <summary>
        /// 当资源引用计数为零的时候自动释放资源包
        /// </summary>
        public bool AutoUnloadBundleWhenUnused = false;

        /// <summary>
        /// WebGL平台强制同步加载资源对象
        /// </summary>
        public bool WebGLForceSyncLoadAsset = false;

#if YOOASSET_EXPERIMENTAL
        /// <summary>
        /// 启用弱引用资源句柄
        /// </summary>
        public bool UseWeakReferenceHandle = false;
#else
        internal bool UseWeakReferenceHandle = false;
#endif
    }

    /// <summary>
    /// 编辑器下模拟运行模式的初始化参数
    /// </summary>
    public class EditorSimulateModeParameters : InitializeParameters
    {
        public FileSystemParameters EditorFileSystemParameters;
    }

    /// <summary>
    /// 离线运行模式的初始化参数
    /// </summary>
    public class OfflinePlayModeParameters : InitializeParameters
    {
        public FileSystemParameters BuildinFileSystemParameters;
    }

    /// <summary>
    /// 联机运行模式的初始化参数
    /// </summary>
    public class HostPlayModeParameters : InitializeParameters
    {
        public FileSystemParameters BuildinFileSystemParameters;
        public FileSystemParameters CacheFileSystemParameters;
    }

    /// <summary>
    /// WebGL运行模式的初始化参数
    /// </summary>
    public class WebPlayModeParameters : InitializeParameters
    {
        public FileSystemParameters WebServerFileSystemParameters;
        public FileSystemParameters WebRemoteFileSystemParameters;
    }

    /// <summary>
    /// 自定义运行模式的初始化参数
    /// </summary>
    public class CustomPlayModeParameters : InitializeParameters
    {
        /// <summary>
        /// 文件系统初始化参数列表
        /// 注意：列表最后一个元素作为主文件系统！
        /// </summary>
        public readonly List<FileSystemParameters> FileSystemParameterList = new List<FileSystemParameters>();
    }
}