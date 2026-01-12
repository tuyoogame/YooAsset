
namespace YooAsset
{
    public class FileSystemParametersDefine
    {
        /// <summary>
        /// 初始化的时候缓存文件校验级别 <see cref=EFileVerifyLevel>
        /// </summary>
        public const string FILE_VERIFY_LEVEL = "FILE_VERIFY_LEVEL";
        /// <summary>
        /// 初始化的时候缓存文件校验最大并发数 <see cref=int>
        /// </summary>
        public const string FILE_VERIFY_MAX_CONCURRENCY = "FILE_VERIFY_MAX_CONCURRENCY";
        /// <summary>
        /// 覆盖安装缓存清理模式 <see cref=EOverwriteInstallClearMode>
        /// </summary>
        public const string INSTALL_CLEAR_MODE = "INSTALL_CLEAR_MODE";
        /// <summary>
        /// 远端资源地址查询服务类 <see cref=IRemoteServices>
        /// </summary>
        public const string REMOTE_SERVICES = "REMOTE_SERVICES";
        /// <summary> 
        /// 解密服务接口的实例类 <see cref=IDecryptionServices>
        /// </summary>
        public const string DECRYPTION_SERVICES = "DECRYPTION_SERVICES";
        /// <summary>
        /// 资源清单服务类 <see cref=IManifestRestoreServices>
        /// </summary>
        public const string MANIFEST_SERVICES = "MANIFEST_SERVICES";
        /// <summary>
        /// 数据文件追加文件格式 <see cref=bool>
        /// </summary>
        public const string APPEND_FILE_EXTENSION = "APPEND_FILE_EXTENSION";
        /// <summary>
        /// 禁用Catalog目录查询文件 <see cref=bool>
        /// </summary>
        public const string DISABLE_CATALOG_FILE = "DISABLE_CATALOG_FILE";
        /// <summary>
        /// 禁用Unity的网络缓存 <see cref=bool>
        /// </summary>
        public const string DISABLE_UNITY_WEB_CACHE = "DISABLE_UNITY_WEB_CACHE";
        /// <summary>
        /// 禁用边玩边下机制 <see cref=bool>
        /// </summary>
        public const string DISABLE_ONDEMAND_DOWNLOAD = "DISABLE_ONDEMAND_DOWNLOAD";
        /// <summary>
        /// UnityWebRequest 创建委托 <see cref=UnityWebRequestCreator>
        /// </summary>
        public const string UNITY_WEB_REQUEST_CREATOR = "UNITY_WEB_REQUEST_CREATOR";
        /// <summary>
        /// 下载后台接口 <see cref=IDownloadBackend>
        /// </summary>
        public const string DOWNLOAD_BACKEND = "DOWNLOAD_BACKEND";
        /// <summary>
        /// 最大并发连接数 默认值：10（推荐范围 1-32） <see cref=int>
        /// </summary>
        public const string DOWNLOAD_MAX_CONCURRENCY = "DOWNLOAD_MAX_CONCURRENCY";
        /// <summary>
        /// 每帧发起的最大请求数 默认值：5（推荐范围 1-10）<see cref=int>
        /// </summary>
        public const string DOWNLOAD_MAX_REQUEST_PER_FRAME = "DOWNLOAD_MAX_REQUEST_PER_FRAME";
        /// <summary>
        /// 下载任务的看门狗机制监控时间 <see cref=int>
        /// </summary>
        public const string DOWNLOAD_WATCH_DOG_TIME = "DOWNLOAD_WATCH_DOG_TIME";
        /// <summary>
        /// 启用断点续传的最小尺寸 <see cref=long>
        /// </summary>
        public const string RESUME_DOWNLOAD_MINMUM_SIZE = "RESUME_DOWNLOAD_MINMUM_SIZE";
        /// <summary>
        /// 断点续传下载器关注的错误码 <see cref=List<long>>
        /// </summary>
        public const string RESUME_DOWNLOAD_RESPONSE_CODES = "RESUME_DOWNLOAD_RESPONSE_CODES";
        /// <summary>
        /// 模拟WebGL平台模式 <see cref=bool>
        /// </summary>
        public const string VIRTUAL_WEBGL_MODE = "VIRTUAL_WEBGL_MODE";
        /// <summary>
        /// 模拟虚拟下载模式 <see cref=bool>
        /// </summary>
        public const string VIRTUAL_DOWNLOAD_MODE = "VIRTUAL_DOWNLOAD_MODE";
        /// <summary>
        /// 模拟虚拟下载的网速（单位：字节） <see cref=int>
        /// </summary>
        public const string VIRTUAL_DOWNLOAD_SPEED = "VIRTUAL_DOWNLOAD_SPEED";
        /// <summary>
        /// 异步模拟加载最小帧数 <see cref=int>
        /// </summary>
        public const string ASYNC_SIMULATE_MIN_FRAME = "ASYNC_SIMULATE_MIN_FRAME";
        /// <summary>
        /// 异步模拟加载最大帧数 <see cref=int>
        /// </summary>
        public const string ASYNC_SIMULATE_MAX_FRAME = "ASYNC_SIMULATE_MAX_FRAME";
        /// <summary>
        /// 拷贝内置清单 <see cref=bool>
        /// </summary>
        public const string COPY_BUILDIN_PACKAGE_MANIFEST = "COPY_BUILDIN_PACKAGE_MANIFEST";
        /// <summary>
        /// 拷贝内置清单的目标目录 <see cref=string>
        /// </summary>
        public const string COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT = "COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT";
        /// <summary>
        /// 拷贝内置文件接口的实例类 <see cref=ICopyLocalFileServices>
        /// </summary>
        public const string COPY_LOCAL_FILE_SERVICES = "COPY_LOCAL_FILE_SERVICES";
        /// <summary>
        /// 解压文件系统的根目录 <see cref=string>
        /// </summary>
        public const string UNPACK_FILE_SYSTEM_ROOT = "UNPACK_FILE_SYSTEM_ROOT";
    }
}
