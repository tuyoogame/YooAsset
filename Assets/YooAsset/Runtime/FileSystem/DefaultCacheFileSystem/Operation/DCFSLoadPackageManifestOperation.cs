using System.IO;

namespace YooAsset
{
    internal class DCFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            DownloadPackageHash,
            DownloadPackageManifest,
            LoadCachePackageHash,
            LoadCachePackageManifest,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private DownloadPackageHashOperation _downloadPackageHashOp;
        private DownloadPackageManifestOperation _downloadPackageManifestOp;
        private LoadCachePackageHashOperation _loadCachePackageHashOp;
        private LoadCachePackageManifestOperation _loadCachePackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal DCFSLoadPackageManifestOperation(DefaultCacheFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.DownloadPackageHash;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadPackageHash)
            {
                if (_downloadPackageHashOp == null)
                {
                    _downloadPackageHashOp = new DownloadPackageHashOperation(_fileSystem, _packageVersion, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _downloadPackageHashOp);
                }

                if (_downloadPackageHashOp.IsDone == false)
                    return;

                if (_downloadPackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.DownloadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadPackageHashOp.Error;
                }
            }

            if (_steps == ESteps.DownloadPackageManifest)
            {
                if (_downloadPackageManifestOp == null)
                {
                    _downloadPackageManifestOp = new DownloadPackageManifestOperation(_fileSystem, _packageVersion, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _downloadPackageManifestOp);
                }

                if (_downloadPackageManifestOp.IsDone == false)
                    return;

                if (_downloadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCachePackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadPackageManifestOp.Error;
                }
            }

            if (_steps == ESteps.LoadCachePackageHash)
            {
                if (_loadCachePackageHashOp == null)
                {
                    _loadCachePackageHashOp = new LoadCachePackageHashOperation(_fileSystem, _packageVersion);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCachePackageHashOp);
                }

                if (_loadCachePackageHashOp.IsDone == false)
                    return;

                if (_loadCachePackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCachePackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCachePackageHashOp.Error;
                    ClearCacheFatalFile();
                }
            }

            if (_steps == ESteps.LoadCachePackageManifest)
            {
                if (_loadCachePackageManifestOp == null)
                {
                    string packageHash = _loadCachePackageHashOp.PackageHash;
                    _loadCachePackageManifestOp = new LoadCachePackageManifestOperation(_fileSystem, _packageVersion, packageHash);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCachePackageManifestOp);
                }

                Progress = _loadCachePackageManifestOp.Progress;
                if (_loadCachePackageManifestOp.IsDone == false)
                    return;

                if (_loadCachePackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadCachePackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCachePackageManifestOp.Error;
                    ClearCacheFatalFile();
                }
            }
        }

        private void ClearCacheFatalFile()
        {
            // 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，主动把损坏的文件删除。
            string manifestFilePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
            if (File.Exists(manifestFilePath))
            {
                YooLogger.Warning($"Invalid cache manifest file have been removed : {manifestFilePath}");
                File.Delete(manifestFilePath);
            }

            string hashFilePath = _fileSystem.GetCachePackageHashFilePath(_packageVersion);
            if (File.Exists(hashFilePath))
            {
                File.Delete(hashFilePath);
            }
        }
    }
}