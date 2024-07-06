using System;
using System.IO;

namespace YooAsset
{
    internal class DBFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            InitUnpackFileSystem,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private LoadBuildinCatalogFileOperation _loadCatalogFileOp;
        private ESteps _steps = ESteps.None;

        internal DBFSInitializeOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.InitUnpackFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitUnpackFileSystem)
            {
                if (_initUnpackFIleSystemOp == null)
                    _initUnpackFIleSystemOp = _fileSystem.InitializeUpackFileSystem();

                Progress = _initUnpackFIleSystemOp.Progress;
                if (_initUnpackFIleSystemOp.IsDone == false)
                    return;

                if (_initUnpackFIleSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCatalogFile;
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
                    _loadCatalogFileOp = new LoadBuildinCatalogFileOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCatalogFileOp);
                }

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

    /// <summary>
    /// 在编辑器下离线模式的兼容性初始化
    /// </summary>
    internal sealed class DBFSInitializeInEditorPlayModeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            InitUnpackFileSystem,
            LoadPackageManifest,
            RecordFiles,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private DBFSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;

        internal DBFSInitializeInEditorPlayModeOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.InitUnpackFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitUnpackFileSystem)
            {
                if (_initUnpackFIleSystemOp == null)
                    _initUnpackFIleSystemOp = _fileSystem.InitializeUpackFileSystem();

                Progress = _initUnpackFIleSystemOp.Progress;
                if (_initUnpackFIleSystemOp.IsDone == false)
                    return;

                if (_initUnpackFIleSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initUnpackFIleSystemOp.Error;
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                {
                    _loadPackageManifestOp = new DBFSLoadPackageManifestOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadPackageManifestOp);
                }

                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.RecordFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadPackageManifestOp.Error;
                }
            }

            if (_steps == ESteps.RecordFiles)
            {
                PackageManifest manifest = _loadPackageManifestOp.Manifest;
                string pacakgeDirectory = _fileSystem.FileRoot;
                DirectoryInfo rootDirectory = new DirectoryInfo(pacakgeDirectory);
                FileInfo[] fileInfos = rootDirectory.GetFiles();
                foreach (var fileInfo in fileInfos)
                {
                    if (fileInfo.Extension == ".meta" || fileInfo.Extension == ".version" ||
                        fileInfo.Extension == ".hash" || fileInfo.Extension == ".bytes")
                        continue;

                    string fileName = fileInfo.Name;
                    if (manifest.TryGetPackageBundleByFileName(fileName, out PackageBundle value))
                    {
                        var fileWrapper = new DefaultBuildinFileSystem.FileWrapper(fileName);
                        _fileSystem.RecordFile(value.BundleGUID, fileWrapper);
                    }
                    else
                    {
                        YooLogger.Warning($"Failed to mapping buildin bundle file : {fileName}");
                    }
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}