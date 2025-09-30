using System;
using System.IO;

namespace YooAsset
{
    internal class DBFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            LoadBuiltinPackageVersion,
            CopyBuiltinPackageHash,
            CopyBuiltinPackageManifest,
            InitUnpackFileSystem,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultBuiltinFileSystem _fileSystem;
        private RequestBuiltinPackageVersionOperation _requestBuiltinPackageVersionOp;
        private CopyBuiltinFileOperation _copyBuiltinHashFileOp;
        private CopyBuiltinFileOperation _copyBuiltinManifestFileOp;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private LoadBuiltinCatalogFileOperation _loadBuiltinCatalogFileOp;
        private ESteps _steps = ESteps.None;

        internal DBFSInitializeOperation(DefaultBuiltinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
#if UNITY_WEBGL
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = $"{nameof(DefaultBuiltinFileSystem)} is not support WEBGL platform !";
#else
            if (_fileSystem.CopyBuiltinPackageManifest)
                _steps = ESteps.LoadBuiltinPackageVersion;
            else
                _steps = ESteps.InitUnpackFileSystem;
#endif
        }
        internal override void InternalUpdate()
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

                if (_requestBuiltinPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CopyBuiltinPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuiltinPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.CopyBuiltinPackageHash)
            {
                if (_copyBuiltinHashFileOp == null)
                {
                    string packageVersion = _requestBuiltinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageHashDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuiltinPackageHashFilePath(packageVersion);
                    _copyBuiltinHashFileOp = new CopyBuiltinFileOperation(sourceFilePath, destFilePath);
                    _copyBuiltinHashFileOp.StartOperation();
                    AddChildOperation(_copyBuiltinHashFileOp);
                }

                _copyBuiltinHashFileOp.UpdateOperation();
                if (_copyBuiltinHashFileOp.IsDone == false)
                    return;

                if (_copyBuiltinHashFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CopyBuiltinPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuiltinHashFileOp.Error;
                }
            }

            if (_steps == ESteps.CopyBuiltinPackageManifest)
            {
                if (_copyBuiltinManifestFileOp == null)
                {
                    string packageVersion = _requestBuiltinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageManifestDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuiltinPackageManifestFilePath(packageVersion);
                    _copyBuiltinManifestFileOp = new CopyBuiltinFileOperation(sourceFilePath, destFilePath);
                    _copyBuiltinManifestFileOp.StartOperation();
                    AddChildOperation(_copyBuiltinManifestFileOp);
                }

                _copyBuiltinManifestFileOp.UpdateOperation();
                if (_copyBuiltinManifestFileOp.IsDone == false)
                    return;

                if (_copyBuiltinManifestFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.InitUnpackFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuiltinManifestFileOp.Error;
                }
            }

            if (_steps == ESteps.InitUnpackFileSystem)
            {
                if (_initUnpackFIleSystemOp == null)
                {
                    _initUnpackFIleSystemOp = _fileSystem.InitializeUpackFileSystem();
                    _initUnpackFIleSystemOp.StartOperation();
                    AddChildOperation(_initUnpackFIleSystemOp);
                }

                _initUnpackFIleSystemOp.UpdateOperation();
                Progress = _initUnpackFIleSystemOp.Progress;
                if (_initUnpackFIleSystemOp.IsDone == false)
                    return;

                if (_initUnpackFIleSystemOp.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.DisableCatalogFile)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        _steps = ESteps.LoadCatalogFile;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initUnpackFIleSystemOp.Error;
                }
            }

            if (_steps == ESteps.LoadCatalogFile)
            {
                if (_loadBuiltinCatalogFileOp == null)
                {
                    _loadBuiltinCatalogFileOp = new LoadBuiltinCatalogFileOperation(_fileSystem);
                    _loadBuiltinCatalogFileOp.StartOperation();
                    AddChildOperation(_loadBuiltinCatalogFileOp);
                }

                _loadBuiltinCatalogFileOp.UpdateOperation();
                if (_loadBuiltinCatalogFileOp.IsDone == false)
                    return;

                if (_loadBuiltinCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    var catalog = _loadBuiltinCatalogFileOp.Catalog;
                    if (catalog == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Fatal error : catalog is null !";
                        return;
                    }

                    if (catalog.PackageName != _fileSystem.PackageName)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Catalog file package name {catalog.PackageName} cannot match the file system package name {_fileSystem.PackageName}";
                        return;
                    }

                    foreach (var wrapper in catalog.Wrappers)
                    {
                        var fileWrapper = new DefaultBuiltinFileSystem.FileWrapper(wrapper.FileName);
                        _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
                    }

                    YooLogger.Log($"Package '{_fileSystem.PackageName}' builtin catalog files count : {catalog.Wrappers.Count}");
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBuiltinCatalogFileOp.Error;
                }
            }
        }

        private string GetCopyManifestFileRoot()
        {
            string destRoot = _fileSystem.CopyBuiltinPackageManifestDestRoot;
            if (string.IsNullOrEmpty(destRoot))
            {
                string defaultCacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
                destRoot = PathUtility.Combine(defaultCacheRoot, _fileSystem.PackageName, DefaultCacheFileSystemDefine.ManifestFilesFolderName);
            }
            return destRoot;
        }
        private string GetCopyPackageHashDestPath(string packageVersion)
        {
            string fileRoot = GetCopyManifestFileRoot();
            string fileName = YooAssetSettingsData.GetPackageHashFileName(_fileSystem.PackageName, packageVersion);
            return PathUtility.Combine(fileRoot, fileName);
        }
        private string GetCopyPackageManifestDestPath(string packageVersion)
        {
            string fileRoot = GetCopyManifestFileRoot();
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, packageVersion);
            return PathUtility.Combine(fileRoot, fileName);
        }
    }
}