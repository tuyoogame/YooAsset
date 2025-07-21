using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
    internal class CopyBuildinPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            CheckHashFile,
            UnpackHashFile,
            CheckManifestFile,
            UnpackManifestFile,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private RequestBuildinPackageVersionOperation _requestBuildinPackageVersionOp;
        private UnityWebFileRequestOperation _hashWebFileRequestOp;
        private UnityWebFileRequestOperation _manifestWebFileRequestOp;
        private string _buildinPackageVersion;
        private ESteps _steps = ESteps.None;

        public CopyBuildinPackageManifestOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
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
                    _steps = ESteps.CheckHashFile;
                    _buildinPackageVersion = _requestBuildinPackageVersionOp.PackageVersion;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuildinPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.CheckHashFile)
            {
                string hashFilePath = GetCopyPackageHashDestPath(_buildinPackageVersion);
                if (File.Exists(hashFilePath))
                {
                    _steps = ESteps.CheckManifestFile;
                    return;
                }

                _steps = ESteps.UnpackHashFile;
            }

            if (_steps == ESteps.UnpackHashFile)
            {
                if (_hashWebFileRequestOp == null)
                {
                    string sourcePath = _fileSystem.GetBuildinPackageHashFilePath(_buildinPackageVersion);
                    string destPath = GetCopyPackageHashDestPath(_buildinPackageVersion);
                    string url = DownloadSystemHelper.ConvertToWWWPath(sourcePath);
                    _hashWebFileRequestOp = new UnityWebFileRequestOperation(url, destPath, 60);
                    _hashWebFileRequestOp.StartOperation();
                    AddChildOperation(_hashWebFileRequestOp);
                }

                _hashWebFileRequestOp.UpdateOperation();
                if (_hashWebFileRequestOp.IsDone == false)
                    return;

                if (_hashWebFileRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CheckManifestFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _hashWebFileRequestOp.Error;
                }
            }

            if (_steps == ESteps.CheckManifestFile)
            {
                string manifestFilePath = GetCopyPackageManifestDestPath(_buildinPackageVersion);
                if (File.Exists(manifestFilePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    return;
                }

                _steps = ESteps.UnpackManifestFile;
            }

            if (_steps == ESteps.UnpackManifestFile)
            {
                if (_manifestWebFileRequestOp == null)
                {
                    string sourcePath = _fileSystem.GetBuildinPackageManifestFilePath(_buildinPackageVersion);
                    string destPath = GetCopyPackageManifestDestPath(_buildinPackageVersion);
                    string url = DownloadSystemHelper.ConvertToWWWPath(sourcePath);
                    _manifestWebFileRequestOp = new UnityWebFileRequestOperation(url, destPath, 60);
                    _manifestWebFileRequestOp.StartOperation();
                    AddChildOperation(_manifestWebFileRequestOp);
                }

                _manifestWebFileRequestOp.UpdateOperation();
                if (_manifestWebFileRequestOp.IsDone == false)
                    return;

                if (_manifestWebFileRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _manifestWebFileRequestOp.Error;
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