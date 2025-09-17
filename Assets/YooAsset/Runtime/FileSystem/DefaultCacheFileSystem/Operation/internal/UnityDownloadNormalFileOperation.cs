using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal sealed class UnityDownloadNormalFileOperation : UnityDownloadFileOperation
    {
        private VerifyTempFileOperation _verifyOperation;
        private ESteps _steps = ESteps.None;

        internal UnityDownloadNormalFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url)
            : base(fileSystem, bundle, url)
        {
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                FileUtility.CreateFileDirectory(_tempFilePath);
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                CreateWebRequest();
                _steps = ESteps.Download;
            }

            // 检测下载结果
            if (_steps == ESteps.Download)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = DownloadProgress;

                UpdateWatchDog();
                if (_webRequest.isDone == false)
                    return;

                // 检查网络错误
                if (CheckRequestResult())
                {
                    _steps = ESteps.VerifyFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                }

                // 注意：最终释放请求器
                DisposeRequest();
            }

            // 验证下载文件
            if (_steps == ESteps.VerifyFile)
            {
                if (_verifyOperation == null)
                {
                    var element = new TempFileElement(_tempFilePath, _bundle.FileCRC, _bundle.FileSize);
                    _verifyOperation = new VerifyTempFileOperation(element);
                    _verifyOperation.StartOperation();
                    AddChildOperation(_verifyOperation);
                }

                if (IsWaitForAsyncComplete)
                    _verifyOperation.WaitForAsyncComplete();

                _verifyOperation.UpdateOperation();
                if (_verifyOperation.IsDone == false)
                    return;

                if (_verifyOperation.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.WriteCacheBundleFile(_bundle, _tempFilePath))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"{_fileSystem.GetType().FullName} failed to write file ! {_tempFilePath}";
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyOperation.Error;
                }

                // 注意：验证完成后直接删除文件
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            if (_steps != ESteps.Done)
            {
                YooLogger.Error($"Try load bundle {_bundle.BundleName} from remote : {_requestURL} !");
            }
        }

        private void CreateWebRequest()
        {
            DownloadHandlerFile handler = new DownloadHandlerFile(_tempFilePath);
            handler.removeFileOnAbort = true;
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _webRequest.SendWebRequest();
        }
    }
}