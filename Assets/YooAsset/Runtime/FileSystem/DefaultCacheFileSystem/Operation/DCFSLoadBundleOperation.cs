using System.IO;
using UnityEngine;

namespace YooAsset
{
    internal class DCFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            DownloadFile,
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private FSDownloadFileOperation _downloadFileOp;
        private AssetBundleCreateRequest _createRequest;
        private bool _isWaitForAsyncComplete = false;
        private ESteps _steps = ESteps.None;


        internal DCFSLoadAssetBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            if (_fileSystem.CheckNeedDownload(_bundle))
            {                
                _steps = ESteps.DownloadFile;
            }
            else
            {
                DownloadProgress = 1f;
                DownloadedBytes = _bundle.FileSize;
                _steps = ESteps.LoadAssetBundle;
            }
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    int failedTryAgain = int.MaxValue;
                    int timeout = 60;
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, null, failedTryAgain, timeout);
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
                string filePath = _fileSystem.GetFileLoadPath(_bundle);
                if (_isWaitForAsyncComplete)
                {
                    Result = AssetBundle.LoadFromFile(filePath);
                }
                else
                {
                    _createRequest = AssetBundle.LoadFromFileAsync(filePath);
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
                }
                else
                {
                    // 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
                    // 说明：在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
                    EFileVerifyResult result = _fileSystem.VerifyFile(_bundle);
                    if (result == EFileVerifyResult.Succeed)
                    {
                        // 注意：在安卓移动平台，华为和三星真机上有极小概率加载资源包失败。
                        // 说明：大多数情况在首次安装下载资源到沙盒内，游戏过程中切换到后台再回到游戏内有很大概率触发！
                        string filePath = _fileSystem.GetFileLoadPath(_bundle);
                        byte[] fileData = FileUtility.ReadAllBytes(filePath);
                        if (fileData != null && fileData.Length > 0)
                        {
                            Result = AssetBundle.LoadFromMemory(fileData);
                            if (Result == null)
                            {
                                _steps = ESteps.Done;
                                Status = EOperationStatus.Failed;
                                Error = $"Failed to load assetBundle from memory : {_bundle.BundleName}";                          
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
                            Error = $"Failed to read assetBundle file bytes : {_bundle.BundleName}";                   
                            YooLogger.Error(Error);
                        }
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        _fileSystem.DeleteFile(_bundle);
                        Status = EOperationStatus.Failed;
                        Error = $"Find corrupted file and delete the file : {_bundle.BundleName}";                  
                        YooLogger.Error(Error);
                    }
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            int frame = 1000;
            while (true)
            {
                // 保险机制
                // 注意：如果需要从远端下载资源，可能会触发保险机制！
                frame--;
                if (frame == 0)
                {
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(WaitForAsyncComplete)} failed ! Try load bundle {_bundle.BundleName} from remote with sync load method !";
                    _steps = ESteps.Done;
                    YooLogger.Error(Error);
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
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
        private enum ESteps
        {
            None,
            DownloadFile,
            LoadRawBundle,
            CheckResult,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private FSDownloadFileOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;


        internal DCFSLoadRawBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            if (_fileSystem.CheckNeedDownload(_bundle))
            {
                _steps = ESteps.DownloadFile;
            }
            else
            {
                DownloadProgress = 1f;
                DownloadedBytes = _bundle.FileSize;
                _steps = ESteps.LoadRawBundle;
            }
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    int failedTryAgain = int.MaxValue;
                    int timeout = 60;
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, null, failedTryAgain, timeout);
                }

                DownloadProgress = _downloadFileOp.DownloadProgress;
                DownloadedBytes = _downloadFileOp.DownloadedBytes;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadRawBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadFileOp.Error;              
                }
            }

            if (_steps == ESteps.LoadRawBundle)
            {
                string filePath = _fileSystem.GetFileLoadPath(_bundle);
                Result = filePath;
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Result != null)
                {
                    string filePath = Result as string;
                    if (File.Exists(filePath))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;                      
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Can not found raw bundle file : {filePath}";                       
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed load raw bundle file : {_bundle.BundleName}";                
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            int frame = 1000;
            while (true)
            {
                // 保险机制
                // 注意：如果需要从远端下载资源，可能会触发保险机制！
                frame--;
                if (frame == 0)
                {
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(WaitForAsyncComplete)} failed ! Try load bundle {_bundle.BundleName} from remote with sync load method !";
                    _steps = ESteps.Done;
                    YooLogger.Error(Error);
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
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