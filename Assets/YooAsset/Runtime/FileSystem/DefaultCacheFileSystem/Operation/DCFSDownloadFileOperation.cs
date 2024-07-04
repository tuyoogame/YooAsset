using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal sealed class DCFSDownloadNormalFileOperation : DefaultDownloadFileOperation
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        private VerifyTempFileOperation _verifyOperation;
        private string _fileSavePath;
        private ESteps _steps = ESteps.None;

        internal DCFSDownloadNormalFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string mainURL, string fallbackURL, int failedTryAgain, int timeout)
            : base(bundle, mainURL, fallbackURL, failedTryAgain, timeout)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _fileSavePath = _fileSystem.GetTempFilePath(Bundle);

            // 注意：检测文件是否存在
            if (_fileSystem.Exists(Bundle))
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.CreateRequest;
            }
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                FileUtility.CreateFileDirectory(_fileSavePath);

                // 获取请求地址
                _requestURL = GetRequestURL();

                // 重置变量
                _isAbort = false;
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = Time.realtimeSinceStartup;
                DownloadProgress = 0f;
                DownloadedBytes = 0;

                // 重置计时器
                if (_tryAgainTimer > 0f)
                    YooLogger.Warning($"Try again download : {_requestURL}");
                _tryAgainTimer = 0f;

                // 删除临时文件
                if (File.Exists(_fileSavePath))
                    File.Delete(_fileSavePath);

                // 创建下载器
                CreateWebRequest();

                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = DownloadProgress;
                if (_webRequest.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                // 检查网络错误
                if (CheckRequestResult())
                    _steps = ESteps.VerifyTempFile;
                else
                    _steps = ESteps.TryAgain;

                // 注意：最终释放请求器
                DisposeWebRequest();
            }

            // 验证下载文件
            if (_steps == ESteps.VerifyTempFile)
            {
                var element = new TempFileElement(_fileSavePath, Bundle.FileCRC, Bundle.FileSize);
                _verifyOperation = new VerifyTempFileOperation(element);
                OperationSystem.StartOperation(_fileSystem.PackageName, _verifyOperation);
                _steps = ESteps.CheckVerifyTempFile;
            }

            // 等待验证完成
            if (_steps == ESteps.CheckVerifyTempFile)
            {
                // 注意：同步解压文件更新
                _verifyOperation.InternalOnUpdate();
                if (_verifyOperation.IsDone == false)
                    return;

                if (_verifyOperation.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.WriteFile(Bundle, _fileSavePath))
                    {
                        Status = EOperationStatus.Succeed;
                        _steps = ESteps.Done;
                    }
                    else
                    {
                        Error = $"{_fileSystem.GetType().FullName} write file failed !";
                        Status = EOperationStatus.Failed;
                        _steps = ESteps.Done;
                        YooLogger.Error(Error);
                    }
                }
                else
                {
                    Error = _verifyOperation.Error;
                    _steps = ESteps.TryAgain;
                }

                // 注意：验证完成后直接删除文件
                if (File.Exists(_fileSavePath))
                    File.Delete(_fileSavePath);
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                if (FailedTryAgain <= 0)
                {
                    Status = EOperationStatus.Failed;
                    _steps = ESteps.Done;
                    YooLogger.Error(Error);
                    return;
                }

                _tryAgainTimer += Time.unscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    FailedTryAgain--;
                    _steps = ESteps.CreateRequest;
                    YooLogger.Warning(Error);
                }
            }
        }
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeWebRequest();
        }
        public override void WaitForAsyncComplete()
        {
            while (true)
            {
                // 文件验证
                if (_verifyOperation != null)
                {
                    if (_verifyOperation.IsDone == false)
                        _verifyOperation.WaitForAsyncComplete();
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }

        private void CreateWebRequest()
        {
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
            DownloadHandlerFile handler = new DownloadHandlerFile(_fileSavePath);
            handler.removeFileOnAbort = true;
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _webRequest.SendWebRequest();
        }
        private void DisposeWebRequest()
        {
            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
    }

    internal sealed class DCFSDownloadResumeFileOperation : DefaultDownloadFileOperation
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        private DownloadHandlerFileRange _downloadHandle;
        private VerifyTempFileOperation _verifyOperation;
        private long _fileOriginLength = 0;
        private string _fileSavePath;
        private ESteps _steps = ESteps.None;


        internal DCFSDownloadResumeFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string mainURL, string fallbackURL, int failedTryAgain, int timeout)
                 : base(bundle, mainURL, fallbackURL, failedTryAgain, timeout)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _fileSavePath = _fileSystem.GetTempFilePath(Bundle);

            // 注意：检测文件是否存在
            if (_fileSystem.Exists(Bundle))
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.CreateRequest;
            }
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                FileUtility.CreateFileDirectory(_fileSavePath);

                // 获取请求地址
                _requestURL = GetRequestURL();

                // 重置变量
                _isAbort = false;
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = Time.realtimeSinceStartup;
                _fileOriginLength = 0;
                DownloadProgress = 0f;
                DownloadedBytes = 0;

                // 重置计时器
                if (_tryAgainTimer > 0f)
                    YooLogger.Warning($"Try again download : {_requestURL}");
                _tryAgainTimer = 0f;

                // 获取下载起始位置
                long fileBeginLength = -1;
                if (File.Exists(_fileSavePath))
                {
                    FileInfo fileInfo = new FileInfo(_fileSavePath);
                    fileBeginLength = fileInfo.Length;
                    _fileOriginLength = fileBeginLength;
                    DownloadedBytes = _fileOriginLength;
                }

                // 检测下载起始位置
                if (fileBeginLength >= Bundle.FileSize)
                {
                    // 删除临时文件
                    if (File.Exists(_fileSavePath))
                        File.Delete(_fileSavePath);
                }

                // 创建下载器
                CreateWebRequest(fileBeginLength);

                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = _fileOriginLength + (long)_webRequest.downloadedBytes;
                if (_webRequest.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                // 检查网络错误
                if (CheckRequestResult())
                    _steps = ESteps.VerifyTempFile;
                else
                    _steps = ESteps.TryAgain;

                // 在遇到特殊错误的时候删除文件
                ClearTempFileWhenError();

                // 注意：最终释放请求器
                DisposeWebRequest();
            }

            // 验证下载文件
            if (_steps == ESteps.VerifyTempFile)
            {
                var element = new TempFileElement(_fileSavePath, Bundle.FileCRC, Bundle.FileSize);
                _verifyOperation = new VerifyTempFileOperation(element);
                OperationSystem.StartOperation(_fileSystem.PackageName, _verifyOperation);
                _steps = ESteps.CheckVerifyTempFile;
            }

            // 等待验证完成
            if (_steps == ESteps.CheckVerifyTempFile)
            {
                if (_verifyOperation.IsDone == false)
                    return;

                if (_verifyOperation.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.WriteFile(Bundle, _fileSavePath))
                    {
                        Status = EOperationStatus.Succeed;
                        _steps = ESteps.Done;
                    }
                    else
                    {
                        Error = $"{_fileSystem.GetType().FullName} write file failed : {_fileSavePath}";
                        Status = EOperationStatus.Failed;
                        _steps = ESteps.Done;
                    }
                }
                else
                {
                    Error = _verifyOperation.Error;
                    _steps = ESteps.TryAgain;
                }

                // 注意：验证完成后直接删除文件
                if (File.Exists(_fileSavePath))
                    File.Delete(_fileSavePath);
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                if (FailedTryAgain <= 0)
                {
                    Status = EOperationStatus.Failed;
                    _steps = ESteps.Done;
                    YooLogger.Error(Error);
                    return;
                }

                _tryAgainTimer += Time.unscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    FailedTryAgain--;
                    _steps = ESteps.CreateRequest;
                    YooLogger.Warning(Error);
                }
            }
        }
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeWebRequest();
        }
        public override void WaitForAsyncComplete()
        {
            while (true)
            {
                // 文件验证
                if (_verifyOperation != null)
                {
                    if (_verifyOperation.IsDone == false)
                        _verifyOperation.WaitForAsyncComplete();
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }

        private void CreateWebRequest(long beginLength)
        {
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
#if UNITY_2019_4_OR_NEWER
            var handler = new DownloadHandlerFile(_fileSavePath, true);
            handler.removeFileOnAbort = false;
#else
            var handler = new DownloadHandlerFileRange(FileSavePath, Bundle.FileSize, _webRequest);
            _downloadHandle = handler;
#endif
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            if (beginLength > 0)
                _webRequest.SetRequestHeader("Range", $"bytes={beginLength}-");
            _webRequest.SendWebRequest();
        }
        private void DisposeWebRequest()
        {
            if (_downloadHandle != null)
            {
                _downloadHandle.Cleanup();
                _downloadHandle = null;
            }

            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
        private void ClearTempFileWhenError()
        {
            if (_fileSystem.ResumeDownloadResponseCodes == null)
                return;

            //说明：如果遇到以下错误返回码，验证失败直接删除文件
            if (_fileSystem.ResumeDownloadResponseCodes.Contains(HttpCode))
            {
                if (File.Exists(_fileSavePath))
                    File.Delete(_fileSavePath);
            }
        }
    }
}