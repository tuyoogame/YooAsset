using System;
using System.IO;

namespace YooAsset
{
    internal class DBFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            LoadBuildinPackageVersion,
            CopyBuildinPackageHash,
            CopyBuildinPackageManifest,
            InitUnpackFileSystem,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private RequestBuildinPackageVersionOperation _requestBuildinPackageVersionOp;
        private CopyBuildinFileOperation _copyBuildinHashFileOp;
        private CopyBuildinFileOperation _copyBuildinManifestFileOp;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private LoadBuildinCatalogFileOperation _loadBuildinCatalogFileOp;
        private ESteps _steps = ESteps.None;

        internal DBFSInitializeOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
#if UNITY_WEBGL
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = $"{nameof(DefaultBuildinFileSystem)} is not support WEBGL platform !";
#else
            if (_fileSystem.CopyBuildinPackageManifest)
                _steps = ESteps.LoadBuildinPackageVersion;
            else
                _steps = ESteps.InitUnpackFileSystem;
#endif
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuildinPackageVersion)
            {
                if (_requestBuildinPackageVersionOp == null)
                {
                    _requestBuildinPackageVersionOp = new RequestBuildinPackageVersionOperation(_fileSystem);
                    _requestBuildinPackageVersionOp.StartOperation();
                    AddChildOperation(_requestBuildinPackageVersionOp);
                }

                _requestBuildinPackageVersionOp.UpdateOperation();
                if (_requestBuildinPackageVersionOp.IsDone == false)
                    return;

                if (_requestBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CopyBuildinPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuildinPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.CopyBuildinPackageHash)
            {
                if (_copyBuildinHashFileOp == null)
                {
                    string packageVersion = _requestBuildinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageHashDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuildinPackageHashFilePath(packageVersion);
                    _copyBuildinHashFileOp = new CopyBuildinFileOperation(sourceFilePath, destFilePath);
                    _copyBuildinHashFileOp.StartOperation();
                    AddChildOperation(_copyBuildinHashFileOp);
                }

                _copyBuildinHashFileOp.UpdateOperation();
                if (_copyBuildinHashFileOp.IsDone == false)
                    return;

                if (_copyBuildinHashFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CopyBuildinPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuildinHashFileOp.Error;
                }
            }

            if (_steps == ESteps.CopyBuildinPackageManifest)
            {
                if (_copyBuildinManifestFileOp == null)
                {
                    string packageVersion = _requestBuildinPackageVersionOp.PackageVersion;
                    string destFilePath = GetCopyPackageManifestDestPath(packageVersion);
                    string sourceFilePath = _fileSystem.GetBuildinPackageManifestFilePath(packageVersion);
                    _copyBuildinManifestFileOp = new CopyBuildinFileOperation(sourceFilePath, destFilePath);
                    _copyBuildinManifestFileOp.StartOperation();
                    AddChildOperation(_copyBuildinManifestFileOp);
                }

                _copyBuildinManifestFileOp.UpdateOperation();
                if (_copyBuildinManifestFileOp.IsDone == false)
                    return;

                if (_copyBuildinManifestFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.InitUnpackFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuildinManifestFileOp.Error;
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
                if (_loadBuildinCatalogFileOp == null)
                {
                    _loadBuildinCatalogFileOp = new LoadBuildinCatalogFileOperation(_fileSystem);
                    _loadBuildinCatalogFileOp.StartOperation();
                    AddChildOperation(_loadBuildinCatalogFileOp);
                }

                _loadBuildinCatalogFileOp.UpdateOperation();
                if (_loadBuildinCatalogFileOp.IsDone == false)
                    return;

                if (_loadBuildinCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    var catalog = _loadBuildinCatalogFileOp.Catalog;
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
                        var fileWrapper = new DefaultBuildinFileSystem.FileWrapper(wrapper.FileName);
                        _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
                    }

                    YooLogger.Log($"Package '{_fileSystem.PackageName}' buildin catalog files count : {catalog.Wrappers.Count}");
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBuildinCatalogFileOp.Error;
                }
            }
        }

        private string GetCopyManifestFileRoot()
        {
            string destRoot = _fileSystem.CopyBuildinPackageManifestDestRoot;
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