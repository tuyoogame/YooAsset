
namespace YooAsset
{
    internal interface IFileSystem
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        string FileRoot { get; }

        /// <summary>
        /// 文件数量
        /// </summary>
        int FileCount { get; }


        /// <summary>
        /// 初始化缓存系统
        /// </summary>
        FSInitializeFileSystemOperation InitializeFileSystemAsync();

        /// <summary>
        /// 加载包裹清单
        /// </summary>
        FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 查询最新的版本
        /// </summary>
        FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 清空所有的文件
        /// </summary>
        FSClearAllBundleFilesOperation ClearAllBundleFilesAsync();

        /// <summary>
        /// 清空未使用的文件
        /// </summary>
        FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest);

        /// <summary>
        /// 下载远端文件
        /// </summary>
        FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param);

        /// <summary>
        /// 加载Bundle文件
        /// </summary>
        FSLoadBundleOperation LoadBundleFile(PackageBundle bundle);

        /// <summary>
        /// 卸载Bundle文件
        /// </summary>
        void UnloadBundleFile(PackageBundle bundle, object result);


        /// <summary>
        /// 设置自定义参数
        /// </summary>
        void SetParameter(string name, object value);

        /// <summary>
        /// 创建缓存系统
        /// </summary>
        void OnCreate(string packageName, string rootDirectory);

        /// <summary>
        /// 更新文件系统
        /// </summary>
        void OnUpdate();


        /// <summary>
        /// 查询文件归属
        /// </summary>
        bool Belong(PackageBundle bundle);

        /// <summary>
        /// 查询文件是否存在
        /// </summary>
        bool Exists(PackageBundle bundle);

        /// <summary>
        /// 是否需要下载
        /// </summary>
        bool NeedDownload(PackageBundle bundle);

        /// <summary>
        /// 是否需要解压
        /// </summary>
        bool NeedUnpack(PackageBundle bundle);

        /// <summary>
        /// 是否需要导入
        /// </summary>
        bool NeedImport(PackageBundle bundle);


        /// <summary>
        /// 读取文件二进制数据
        /// </summary>
        byte[] ReadFileData(PackageBundle bundle);

        /// <summary>
        /// 读取文件文本数据
        /// </summary>
        string ReadFileText(PackageBundle bundle);
    }
}