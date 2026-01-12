
namespace YooAsset
{
    /// <summary>
    /// 拷贝内置包裹清单到沙盒操作
    /// </summary>
    internal sealed class CopyBuiltinPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadBuiltinPackageVersion,
            CopyBuiltinPackageHash,
            CopyBuiltinPackageManifest,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private RequestBuiltinPackageVersionOperation _requestBuiltinPackageVersionOp;
        private CopyBuiltinFileOperation _copyBuiltinHashFileOp;
        private CopyBuiltinFileOperation _copyBuiltinManifestFileOp;
        private ESteps _steps = ESteps.None;

        public CopyBuiltinPackageManifestOperation(BuiltinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadBuiltinPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuiltinPackageVersion)
            {
                if (_requestBuiltinPackageVersionOp == null)
                {
                    _requestBuiltinPackageVersionOp = new RequestBuiltinPackageVersionOperation(_fileSystem);
                    _requestBuiltinPackageVersionOp.StartOperation();
                    AddChildOperation(_requestBuiltinPackageVersionOp);
                }

                _requestBuiltinPackageVersionOp.UpdateOperation();
                if (_requestBuiltinPackageVersionOp.IsDone == false)
                    return;

                if (_requestBuiltinPackageVersionOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CopyBuiltinPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestBuiltinPackageVersionOp.Error);
                }
            }

            if (_steps == ESteps.CopyBuiltinPackageHash)
            {
                if (_copyBuiltinHashFileOp == null)
                {
                    // 注意：只负责拷贝文件，不负责校验文件。
                    string packageVersion = _requestBuiltinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageHashDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuiltinPackageHashFilePath(packageVersion);
                    _copyBuiltinHashFileOp = new CopyBuiltinFileOperation(_fileSystem, sourceFilePath, destFilePath);
                    _copyBuiltinHashFileOp.StartOperation();
                    AddChildOperation(_copyBuiltinHashFileOp);
                }

                _copyBuiltinHashFileOp.UpdateOperation();
                if (_copyBuiltinHashFileOp.IsDone == false)
                    return;

                if (_copyBuiltinHashFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CopyBuiltinPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_copyBuiltinHashFileOp.Error);
                }
            }

            if (_steps == ESteps.CopyBuiltinPackageManifest)
            {
                if (_copyBuiltinManifestFileOp == null)
                {
                    // 注意：只负责拷贝文件，不负责校验文件。
                    string packageVersion = _requestBuiltinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageManifestDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuiltinPackageManifestFilePath(packageVersion);
                    _copyBuiltinManifestFileOp = new CopyBuiltinFileOperation(_fileSystem, sourceFilePath, destFilePath);
                    _copyBuiltinManifestFileOp.StartOperation();
                    AddChildOperation(_copyBuiltinManifestFileOp);
                }

                _copyBuiltinManifestFileOp.UpdateOperation();
                if (_copyBuiltinManifestFileOp.IsDone == false)
                    return;

                if (_copyBuiltinManifestFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_copyBuiltinManifestFileOp.Error);
                }
            }
        }

        private string GetCopyManifestFileRoot()
        {
            string destRoot = _fileSystem.CopyBuiltinPackageManifestDestRoot;
            if (string.IsNullOrEmpty(destRoot))
            {
                string defaultCacheRoot = YooAssetConfiguration.GetDefaultCacheRoot();
                destRoot = PathUtility.Combine(defaultCacheRoot, _fileSystem.PackageName, SandboxFileSystemConsts.ManifestFilesFolderName);
            }
            return destRoot;
        }
        private string GetCopyPackageHashDestPath(string packageVersion)
        {
            string fileRoot = GetCopyManifestFileRoot();
            string fileName = YooAssetConfiguration.GetPackageHashFileName(_fileSystem.PackageName, packageVersion);
            return PathUtility.Combine(fileRoot, fileName);
        }
        private string GetCopyPackageManifestDestPath(string packageVersion)
        {
            string fileRoot = GetCopyManifestFileRoot();
            string fileName = YooAssetConfiguration.GetManifestBinaryFileName(_fileSystem.PackageName, packageVersion);
            return PathUtility.Combine(fileRoot, fileName);
        }
    }
}