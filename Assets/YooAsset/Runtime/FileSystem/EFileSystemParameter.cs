
namespace YooAsset
{
    /// <summary>
    /// 文件系统参数类型定义
    /// </summary>
    public enum EFileSystemParameter
    {
        /// <summary>
        /// 初始化的时候缓存文件校验级别 <see cref="EFileVerifyLevel"/>
        /// </summary>
        FileVerifyLevel,

        /// <summary>
        /// 初始化的时候缓存文件校验最大并发数 <see cref="int"/>
        /// </summary>
        FileVerifyMaxConcurrency,

        /// <summary>
        /// 覆盖安装缓存清理模式 <see cref="EInstallCleanupMode"/>
        /// </summary>
        InstallCleanupMode,

        /// <summary>
        /// 禁用Unity的网络缓存 <see cref="bool"/>
        /// </summary>
        DisableUnityWebCache,

        /// <summary>
        /// UnityWebRequest 创建委托 <see cref="UnityWebRequestCreator"/>
        /// </summary>
        UnityWebRequestCreator,

        /// <summary>
        /// 禁用边玩边下机制 <see cref="bool"/>
        /// </summary>
        DownloadDisableOndemand,

        /// <summary>
        /// 下载后台接口 <see cref="IDownloadBackend"/>
        /// </summary>
        DownloadBackend,

        /// <summary>
        /// 最大并发连接数 默认值：10（推荐范围 1-32） <see cref="int"/>
        /// </summary>
        DownloadMaxConcurrency,

        /// <summary>
        /// 每帧发起的最大请求数 默认值：5（推荐范围 1-10）<see cref="int"/>
        /// </summary>
        DownloadMaxRequestPerFrame,

        /// <summary>
        /// 下载任务的看门狗机制超时时间 <see cref="int"/>
        /// </summary>
        DownloadWatchdogTimeout,

        /// <summary>
        /// 启用断点续传的最小尺寸 <see cref="long"/>
        /// </summary>
        DownloadResumeMinimumSize,

        /// <summary>
        /// 模拟WebGL平台模式 <see cref="bool"/>
        /// </summary>
        VirtualWebglMode,

        /// <summary>
        /// 模拟虚拟下载模式 <see cref="bool"/>
        /// </summary>
        VirtualDownloadMode,

        /// <summary>
        /// 模拟虚拟下载的网速（单位：字节） <see cref="int"/>
        /// </summary>
        VirtualDownloadSpeed,

        /// <summary>
        /// 异步模拟加载最小帧数 <see cref="int"/>
        /// </summary>
        AsyncSimulateMinFrame,

        /// <summary>
        /// 异步模拟加载最大帧数 <see cref="int"/>
        /// </summary>
        AsyncSimulateMaxFrame,

        /// <summary>
        /// 拷贝内置清单 <see cref="bool"/>
        /// </summary>
        CopyBuiltinPackageManifest,

        /// <summary>
        /// 拷贝内置清单的目标目录 <see cref="string"/>
        /// </summary>
        CopyBuiltinPackageManifestDestRoot,

        /// <summary>
        /// 解压文件系统的根目录 <see cref="string"/>
        /// </summary>
        UnpackFileSystemRoot,

        /// <summary>
        /// 远端资源地址查询服务类 <see cref="IRemoteService"/>
        /// </summary>
        RemoteService,

        /// <summary>
        /// AssetBundle 解密器 <see cref="IBundleDecryptor"/>
        /// </summary>
        AssetBundleDecryptor,

        /// <summary>
        /// RawBundle 解密器 <see cref="IBundleDecryptor"/>
        /// </summary>
        RawBundleDecryptor,

        /// <summary>
        /// ArchiveBundle 解密器 <see cref="IBundleDecryptor"/>
        /// </summary>
        ArchiveBundleDecryptor,

        /// <summary>
        /// AssetBundle 备用解密器 <see cref="IBundleMemoryDecryptor"/>
        /// </summary>
        AssetBundleFallbackDecryptor,

        /// <summary>
        /// 资源清单解密器 <see cref="IManifestDecryptor"/>
        /// </summary>
        ManifestDecryptor,

        /// <summary>
        /// 下载重试判定策略 <see cref="IDownloadRetryPolicy"/>
        /// </summary>
        DownloadRetryPolicy,

        /// <summary>
        /// URL 选择策略 <see cref="IDownloadUrlPolicy"/>
        /// </summary>
        DownloadUrlPolicy,

        /// <summary>
        /// WebGL 平台策略 <see cref="IWebPlatformStrategy"/>
        /// </summary>
        WebPlatformStrategy,

        /// <summary>
        /// 内置资源包解包策略 <see cref="IBundleUnpackPolicy"/>
        /// </summary>
        BundleUnpackPolicy,

        /// <summary>
        /// 内置文件访问器 <see cref="IBuiltinFileAccessor"/>
        /// </summary>
        BuiltinFileAccessor,
    }
}