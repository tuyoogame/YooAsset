﻿#if UNITY_WEBGL && BYTEMINIGAME
using YooAsset;

internal class BGFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
{
    private enum ESteps
    {
        None,
        RequestRemotePackageHash,
        LoadRemotePackageManifest,
        Done,
    }

    private readonly ByteGameFileSystem _fileSystem;
    private readonly string _packageVersion;
    private readonly int _timeout;
    private RequestByteGamePackageHashOperation _requestRemotePackageHashOp;
    private LoadByteGamePackageManifestOperation _loadRemotePackageManifestOp;
    private ESteps _steps = ESteps.None;

    
    public BGFSLoadPackageManifestOperation(ByteGameFileSystem fileSystem, string packageVersion, int timeout)
    {
        _fileSystem = fileSystem;
        _packageVersion = packageVersion;
        _timeout = timeout;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.RequestRemotePackageHash;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestRemotePackageHash)
        {
            if (_requestRemotePackageHashOp == null)
            {
                _requestRemotePackageHashOp = new RequestByteGamePackageHashOperation(_fileSystem, _packageVersion, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _requestRemotePackageHashOp);
            }

            if (_requestRemotePackageHashOp.IsDone == false)
                return;

            if (_requestRemotePackageHashOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.LoadRemotePackageManifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _requestRemotePackageHashOp.Error;
            }
        }

        if (_steps == ESteps.LoadRemotePackageManifest)
        {
            if (_loadRemotePackageManifestOp == null)
            {
                string packageHash = _requestRemotePackageHashOp.PackageHash;
                _loadRemotePackageManifestOp = new LoadByteGamePackageManifestOperation(_fileSystem, _packageVersion, packageHash, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _loadRemotePackageManifestOp);
            }

            Progress = _loadRemotePackageManifestOp.Progress;
            if (_loadRemotePackageManifestOp.IsDone == false)
                return;

            if (_loadRemotePackageManifestOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                Manifest = _loadRemotePackageManifestOp.Manifest;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _loadRemotePackageManifestOp.Error;
            }
        }
    }
}
#endif