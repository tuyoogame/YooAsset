using System;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    internal class DCFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        protected enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            AbortDownload,
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly PackageBundle _bundle;
        protected FSDownloadFileOperation _downloadFileOp;
        protected AssetBundleCreateRequest _createRequest;
        private AssetBundle _assetBundle;
        private Stream _managedStream;
        protected ESteps _steps = ESteps.None;


        internal DCFSLoadAssetBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckExist;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                if (_fileSystem.Exists(_bundle))
                {
                    DownloadProgress = 1f;
                    DownloadedBytes = _bundle.FileSize;
                    _steps = ESteps.LoadAssetBundle;
                }
                else
                {
                    if (_fileSystem.DisableOnDemandDownload)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The bundle not cached : {_bundle.BundleName}";
                        YooLogger.Warning(Error);
                    }
                    else
                    {
                        _steps = ESteps.DownloadFile;
                    }
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                // 中断下载
                if (AbortDownloadFile)
                {
                    if (_downloadFileOp != null)
                        _downloadFileOp.AbortOperation();
                    _steps = ESteps.AbortDownload;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, options);
                    _downloadFileOp.StartOperation();
                    AddChildOperation(_downloadFileOp);
                }

                if (IsWaitForAsyncComplete)
                    _downloadFileOp.WaitForAsyncComplete();

                _downloadFileOp.UpdateOperation();
                DownloadProgress = _downloadFileOp.DownloadProgress;
                DownloadedBytes = _downloadFileOp.DownloadedBytes;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadAssetBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadFileOp.Error;
                }
            }

            if (_steps == ESteps.AbortDownload)
            {
                if (_downloadFileOp != null)
                {
                    if (IsWaitForAsyncComplete)
                        _downloadFileOp.WaitForAsyncComplete();

                    _downloadFileOp.UpdateOperation();
                    if (_downloadFileOp.IsDone == false)
                        return;
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Abort download file !";
            }

            if (_steps == ESteps.LoadAssetBundle)
            {
                if (_bundle.Encrypted)
                {
                    if (_fileSystem.DecryptionServices == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The {nameof(IDecryptionServices)} is null !";
                        YooLogger.Error(Error);
                        return;
                    }
                }

                if (IsWaitForAsyncComplete)
                {
                    if (_bundle.Encrypted)
                    {
                        var decryptResult = _fileSystem.LoadEncryptedAssetBundle(_bundle);
                        _assetBundle = decryptResult.Result;
                        _managedStream = decryptResult.ManagedStream;
                    }
                    else
                    {
                        string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
                        _assetBundle = AssetBundle.LoadFromFile(filePath);
                    }
                }
                else
                {
                    if (_bundle.Encrypted)
                    {
                        var decryptResult = _fileSystem.LoadEncryptedAssetBundleAsync(_bundle);
                        _createRequest = decryptResult.CreateRequest;
                        _managedStream = decryptResult.ManagedStream;
                    }
                    else
                    {
                        string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
                        _createRequest = AssetBundle.LoadFromFileAsync(filePath);
                    }
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_createRequest != null)
                {
                    if (IsWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity bundle.");
                        _assetBundle = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        _assetBundle = _createRequest.assetBundle;
                    }
                }

                if (_assetBundle != null)
                {
                    _steps = ESteps.Done;
                    Result = new AssetBundleResult(_fileSystem, _bundle, _assetBundle, _managedStream);
                    Status = EOperationStatus.Succeed;
                    return;
                }

                // 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
                // 说明：在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
                EFileVerifyResult verifyResult = _fileSystem.VerifyCacheFile(_bundle);
                if (verifyResult == EFileVerifyResult.Succeed)
                {
                    if (_bundle.Encrypted)
                    {
                        var decryptResult = _fileSystem.LoadEncryptedAssetBundleFallback(_bundle);
                        _assetBundle = decryptResult.Result;
                        if (_assetBundle != null)
                        {
                            _steps = ESteps.Done;
                            Result = new AssetBundleResult(_fileSystem, _bundle, _assetBundle, _managedStream);
                            Status = EOperationStatus.Succeed;
                            return;
                        }
                        else
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Failed to load encrypted asset bundle file : {_bundle.BundleName}";
                            YooLogger.Error(Error);
                            return;
                        }
                    }

                    // 注意：在安卓移动平台，华为和三星真机上有极小概率加载资源包失败。
                    // 说明：大多数情况在首次安装下载资源到沙盒内，游戏过程中切换到后台再回到游戏内有很大概率触发！
                    string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
                    byte[] fileData = FileUtility.ReadAllBytes(filePath);
                    if (fileData != null && fileData.Length > 0)
                    {
                        _assetBundle = AssetBundle.LoadFromMemory(fileData);
                        if (_assetBundle == null)
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Failed to load asset bundle from memory : {_bundle.BundleName}";
                            YooLogger.Error(Error);
                        }
                        else
                        {
                            _steps = ESteps.Done;
                            Result = new AssetBundleResult(_fileSystem, _bundle, _assetBundle, null);
                            Status = EOperationStatus.Succeed;
                        }
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed to read asset bundle file bytes : {_bundle.BundleName}";
                        YooLogger.Error(Error);
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    _fileSystem.DeleteCacheBundleFile(_bundle.BundleGUID);
                    Status = EOperationStatus.Failed;
                    Error = $"Find corrupted asset bundle file and delete : {_bundle.BundleName}";
                    YooLogger.Error(Error);
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
    }

    internal class DCFSLoadRawBundleOperation : FSLoadBundleOperation
    {
        protected enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            AbortDownload,
            LoadCacheRawBundle,
            Done,
        }

        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly PackageBundle _bundle;
        protected FSDownloadFileOperation _downloadFileOp;
        protected ESteps _steps = ESteps.None;


        internal DCFSLoadRawBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckExist;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                if (_fileSystem.Exists(_bundle))
                {
                    // 注意：缓存的原生文件的格式，可能会在业务端根据需求发生变动！
                    // 注意：这里需要校验文件格式，如果不一致对本地文件进行修正！
                    string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
                    if (File.Exists(filePath) == false)
                    {
                        try
                        {
                            var recordFileElement = _fileSystem.GetRecordFileElement(_bundle);
                            File.Move(recordFileElement.DataFilePath, filePath);
                            _steps = ESteps.LoadCacheRawBundle;
                        }
                        catch (Exception e)
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Faild rename raw data file : {e.Message}";
                        }
                    }
                    else
                    {
                        DownloadProgress = 1f;
                        DownloadedBytes = _bundle.FileSize;
                        _steps = ESteps.LoadCacheRawBundle;
                    }
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                // 中断下载
                if (AbortDownloadFile)
                {
                    if (_downloadFileOp != null)
                        _downloadFileOp.AbortOperation();
                    _steps = ESteps.AbortDownload;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, options);
                    _downloadFileOp.StartOperation();
                    AddChildOperation(_downloadFileOp);
                }

                if (IsWaitForAsyncComplete)
                    _downloadFileOp.WaitForAsyncComplete();

                _downloadFileOp.UpdateOperation();
                DownloadProgress = _downloadFileOp.DownloadProgress;
                DownloadedBytes = _downloadFileOp.DownloadedBytes;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCacheRawBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadFileOp.Error;
                }
            }

            if (_steps == ESteps.AbortDownload)
            {
                if (_downloadFileOp != null)
                {
                    if (IsWaitForAsyncComplete)
                        _downloadFileOp.WaitForAsyncComplete();

                    _downloadFileOp.UpdateOperation();
                    if (_downloadFileOp.IsDone == false)
                        return;
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Abort download file !";
            }

            if (_steps == ESteps.LoadCacheRawBundle)
            {
                string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Result = new RawBundleResult(_fileSystem, _bundle);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found cache raw bundle file : {filePath}";
                    YooLogger.Error(Error);
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
    }
}