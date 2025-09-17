using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal sealed class UnityDownloadLocalFileOperation : UnityDownloadFileOperation
    {
        private VerifyTempFileOperation _verifyOperation;
        private ESteps _steps = ESteps.None;

        internal UnityDownloadLocalFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url)
            : base(fileSystem, bundle, url)
        {
        }
        internal override void InternalStart()
        {
            if (_fileSystem.CopyLocalFileServices != null)
                _steps = ESteps.CopyLocalFile;
            else
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

            // 拷贝内置文件
            if (_steps == ESteps.CopyLocalFile)
            {
                FileUtility.CreateFileDirectory(_tempFilePath);
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                try
                {
                    //TODO 团结引擎，在某些机型（红米），拷贝包内文件会小概率失败！需要借助其它方式来拷贝包内文件。
                    var localFileInfo = new LocalFileInfo();
                    localFileInfo.PackageName = _fileSystem.PackageName;
                    localFileInfo.BundleName = _bundle.BundleName;
                    localFileInfo.SourceFileURL = _requestURL;
                    _fileSystem.CopyLocalFileServices.CopyFile(localFileInfo, _tempFilePath);
                    if (File.Exists(_tempFilePath))
                    {
                        DownloadProgress = 1f;
                        DownloadedBytes = _bundle.FileSize;
                        Progress = DownloadProgress;
                        _steps = ESteps.VerifyFile;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed copy local file : {_requestURL}";
                    }
                }
                catch (System.Exception ex)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed copy local file : {ex.Message}";
                }
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
                        Error = $"{_fileSystem.GetType().FullName} failed to write file !";
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
            while (true)
            {
                //TODO 等待导入或解压本地文件完毕，该操作会挂起主线程！
                InternalUpdate();
                if (IsDone)
                    break;

                // 短暂休眠避免完全卡死
                System.Threading.Thread.Sleep(1);
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