using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统的加载包裹清单操作
    /// </summary>
    internal sealed class SFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            DownloadPackageHash,
            DownloadPackageManifest,
            LoadPackageHash,
            LoadPackageManifest,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private DownloadPackageHashOperation _downloadPackageHashOp;
        private DownloadPackageManifestOperation _downloadPackageManifestOp;
        private LoadCachePackageHashOperation _loadCachePackageHashOp;
        private LoadCachePackageManifestOperation _loadCachePackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal SFSLoadPackageManifestOperation(SandboxFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.DownloadPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadPackageHash)
            {
                if (_downloadPackageHashOp == null)
                {
                    _downloadPackageHashOp = new DownloadPackageHashOperation(_fileSystem, _packageVersion, _timeout);
                    _downloadPackageHashOp.StartOperation();
                    AddChildOperation(_downloadPackageHashOp);
                }

                _downloadPackageHashOp.UpdateOperation();
                if (_downloadPackageHashOp.IsDone == false)
                    return;

                if (_downloadPackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.DownloadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadPackageHashOp.Error);
                }
            }

            if (_steps == ESteps.DownloadPackageManifest)
            {
                if (_downloadPackageManifestOp == null)
                {
                    _downloadPackageManifestOp = new DownloadPackageManifestOperation(_fileSystem, _packageVersion, _timeout);
                    _downloadPackageManifestOp.StartOperation();
                    AddChildOperation(_downloadPackageManifestOp);
                }

                _downloadPackageManifestOp.UpdateOperation();
                if (_downloadPackageManifestOp.IsDone == false)
                    return;

                if (_downloadPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadPackageManifestOp.Error);
                }
            }

            if (_steps == ESteps.LoadPackageHash)
            {
                if (_loadCachePackageHashOp == null)
                {
                    _loadCachePackageHashOp = new LoadCachePackageHashOperation(_fileSystem, _packageVersion);
                    _loadCachePackageHashOp.StartOperation();
                    AddChildOperation(_loadCachePackageHashOp);
                }

                _loadCachePackageHashOp.UpdateOperation();
                if (_loadCachePackageHashOp.IsDone == false)
                    return;

                if (_loadCachePackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadCachePackageHashOp.Error);
                    ClearCacheFatalFile();
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadCachePackageManifestOp == null)
                {
                    string packageHash = _loadCachePackageHashOp.PackageHash;
                    _loadCachePackageManifestOp = new LoadCachePackageManifestOperation(_fileSystem, _packageVersion, packageHash);
                    _loadCachePackageManifestOp.StartOperation();
                    AddChildOperation(_loadCachePackageManifestOp);
                }

                _loadCachePackageManifestOp.UpdateOperation();
                Progress = _loadCachePackageManifestOp.Progress;
                if (_loadCachePackageManifestOp.IsDone == false)
                    return;

                if (_loadCachePackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    Manifest = _loadCachePackageManifestOp.Manifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadCachePackageManifestOp.Error);
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
                YooLogger.LogWarning($"Invalid package manifest file has been removed: '{manifestFilePath}'.");
                File.Delete(manifestFilePath);
            }

            string hashFilePath = _fileSystem.GetCachePackageHashFilePath(_packageVersion);
            if (File.Exists(hashFilePath))
            {
                YooLogger.LogWarning($"Invalid package hash file has been removed: '{hashFilePath}'.");
                File.Delete(hashFilePath);
            }
        }
    }
}