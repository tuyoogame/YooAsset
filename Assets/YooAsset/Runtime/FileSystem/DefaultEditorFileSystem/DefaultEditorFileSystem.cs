
namespace YooAsset
{
    /// <summary>
    /// 模拟文件系统
    /// </summary>
    internal class DefaultEditorFileSystem : IFileSystem
    {
        protected string _packageRoot;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return _packageRoot;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return 0;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：模拟构建结果
        /// </summary>
        public SimulateBuildResult BuildResult { private set; get; } = null;
        #endregion


        public DefaultEditorFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DEFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DEFSLoadPackageManifestOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DEFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            var operation = new FSClearAllBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
        {
            var operation = new FSClearUnusedBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            throw new System.NotImplementedException();
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            var operation = new DEFSLoadBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == "SIMULATE_BUILD_RESULT")
            {
                BuildResult = (SimulateBuildResult)value;
            }
            else
            {
                YooLogger.Warning($"Invalid parameter : {name}");
            }
        }
        public virtual void OnCreate(string packageName, string rootDirectory)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(rootDirectory))
                rootDirectory = GetDefaultRoot();

            _packageRoot = PathUtility.Combine(rootDirectory, packageName);
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual byte[] ReadFileData(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual string ReadFileText(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }

        #region 内部方法
        protected string GetDefaultRoot()
        {
            return "Assets/";
        }
        #endregion
    }
}