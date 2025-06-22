using System;
using System.IO;

namespace YooAsset
{
    internal class DBFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            CopyBuildinManifest,
            InitUnpackFileSystem,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private CopyBuildinPackageManifestOperation _copyBuildinPackageManifestOp;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private LoadBuildinCatalogFileOperation _loadCatalogFileOp;
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
                _steps = ESteps.CopyBuildinManifest;
            else
                _steps = ESteps.InitUnpackFileSystem;
#endif
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CopyBuildinManifest)
            {
                if (_copyBuildinPackageManifestOp == null)
                {
                    _copyBuildinPackageManifestOp = new CopyBuildinPackageManifestOperation(_fileSystem);
                    _copyBuildinPackageManifestOp.StartOperation();
                    AddChildOperation(_copyBuildinPackageManifestOp);
                }

                _copyBuildinPackageManifestOp.UpdateOperation();
                if (_copyBuildinPackageManifestOp.IsDone == false)
                    return;

                if (_copyBuildinPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.InitUnpackFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuildinPackageManifestOp.Error;
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
                if (_loadCatalogFileOp == null)
                {
#if UNITY_EDITOR
                    /*
                    // 兼容性初始化
                    // 说明：内置文件系统在编辑器下运行时需要动态生成
                    string packageRoot = _fileSystem.FileRoot;
                    bool result = DefaultBuildinFileSystemBuild.CreateBuildinCatalogFile(_fileSystem.ManifestServices, _fileSystem.PackageName, packageRoot);
                    if (result == false)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Create package catalog file failed ! See the detail error in console !";
                        return;
                    }
                    */
#endif

                    _loadCatalogFileOp = new LoadBuildinCatalogFileOperation(_fileSystem);
                    _loadCatalogFileOp.StartOperation();
                    AddChildOperation(_loadCatalogFileOp);
                }

                _loadCatalogFileOp.UpdateOperation();
                if (_loadCatalogFileOp.IsDone == false)
                    return;

                if (_loadCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCatalogFileOp.Error;
                }
            }
        }
    }
}