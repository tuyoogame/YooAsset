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
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly PackageBundle _bundle;
        protected FSDownloadFileOperation _downloadFileOp;
        protected AssetBundleCreateRequest _createRequest;
        protected bool _isWaitForAsyncComplete = false;
        protected ESteps _steps = ESteps.None;


        internal DCFSLoadAssetBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckExist;
        }
        internal override void InternalOnUpdate()
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
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    DownloadParam downloadParam = new DownloadParam(int.MaxValue, 60);
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, downloadParam);
                }

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

                if (_isWaitForAsyncComplete)
                {
                    if (_bundle.Encrypted)
                    {
                        Result = _fileSystem.LoadEncryptedAssetBundle(_bundle);
                    }
                    else
                    {
                        string filePath = _fileSystem.GetCacheFileLoadPath(_bundle);
                        Result = AssetBundle.LoadFromFile(filePath);
                    }
                }
                else
                {
                    if (_bundle.Encrypted)
                    {
                        _createRequest = _fileSystem.LoadEncryptedAssetBundleAsync(_bundle);
                    }
                    else
                    {
                        string filePath = _fileSystem.GetCacheFileLoadPath(_bundle);
                        _createRequest = AssetBundle.LoadFromFileAsync(filePath);
                    }
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_createRequest != null)
                {
                    if (_isWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity bundle.");
                        Result = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        Result = _createRequest.assetBundle;
                    }
                }

                if (Result != null)
                {
                    _steps = ESteps.Done;
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
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed to load encrypted asset bundle file : {_bundle.BundleName}";
                        YooLogger.Error(Error);
                        return;
                    }

                    // 注意：在安卓移动平台，华为和三星真机上有极小概率加载资源包失败。
                    // 说明：大多数情况在首次安装下载资源到沙盒内，游戏过程中切换到后台再回到游戏内有很大概率触发！
                    string filePath = _fileSystem.GetCacheFileLoadPath(_bundle);
                    byte[] fileData = FileUtility.ReadAllBytes(filePath);
                    if (fileData != null && fileData.Length > 0)
                    {
                        Result = AssetBundle.LoadFromMemory(fileData);
                        if (Result == null)
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Failed to load asset bundle from memory : {_bundle.BundleName}";
                            YooLogger.Error(Error);
                        }
                        else
                        {
                            _steps = ESteps.Done;
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
                    _fileSystem.DeleteCacheFile(_bundle.BundleGUID);
                    Status = EOperationStatus.Failed;
                    Error = $"Find corrupted asset bundle file and delete : {_bundle.BundleName}";
                    YooLogger.Error(Error);
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            while (true)
            {
                if (_downloadFileOp != null)
                    _downloadFileOp.WaitForAsyncComplete();

                if (ExecuteWhileDone())
                {
                    if (_downloadFileOp != null && _downloadFileOp.Status == EOperationStatus.Failed)
                        YooLogger.Error($"Try load bundle {_bundle.BundleName} from remote !");

                    _steps = ESteps.Done;
                    break;
                }
            }
        }
        public override void AbortDownloadOperation()
        {
            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp != null)
                    _downloadFileOp.SetAbort();
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
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckExist;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                if (_fileSystem.Exists(_bundle))
                {
                    DownloadProgress = 1f;
                    DownloadedBytes = _bundle.FileSize;
                    _steps = ESteps.LoadCacheRawBundle;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    DownloadParam downloadParam = new DownloadParam(int.MaxValue, 60);
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, downloadParam);
                }

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

            if (_steps == ESteps.LoadCacheRawBundle)
            {
                string filePath = _fileSystem.GetCacheFileLoadPath(_bundle);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Result = new RawBundle(_fileSystem, _bundle, filePath);
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
                if (_downloadFileOp != null)
                    _downloadFileOp.WaitForAsyncComplete();

                if (ExecuteWhileDone())
                {
                    if (_downloadFileOp != null && _downloadFileOp.Status == EOperationStatus.Failed)
                        YooLogger.Error($"Try load bundle {_bundle.BundleName} from remote !");

                    _steps = ESteps.Done;
                    break;
                }
            }
        }
        public override void AbortDownloadOperation()
        {
            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp != null)
                    _downloadFileOp.SetAbort();
            }
        }
    }
}