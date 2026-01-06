using System.IO;

namespace YooAsset
{
    internal sealed class DownloadAndCacheLocalFileOperation : DownloadAndCacheFileOperation
    {
        private enum ESteps
        {
            None,
            CheckCopy,
            CopyLocalFile,
            CreateRequest,
            CheckRequest,
            VerifyBundleFile,
            CacheBundleFile,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private readonly string _tempFilePath;
        private IDownloadRequest _request;
        private VerifyTempFileOperation _verifyOperation;
        private ESteps _steps = ESteps.None;

        internal DownloadAndCacheLocalFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url) : base(url)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
            _tempFilePath = _fileSystem.GetTempFilePath(_bundle);
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckCopy;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 检测文件拷贝
            if (_steps == ESteps.CheckCopy)
            {
                // 删除历史缓存文件
                FileUtility.CreateFileDirectory(_tempFilePath);
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                if (_fileSystem.CopyLocalFileServices != null)
                    _steps = ESteps.CopyLocalFile;
                else
                    _steps = ESteps.CreateRequest;
            }

            // 拷贝本地文件
            if (_steps == ESteps.CopyLocalFile)
            {
                try
                {
                    //TODO 团结引擎，在某些机型（红米），拷贝包内文件会小概率失败！需要借助其它方式来拷贝包内文件。
                    var localFileInfo = new LocalFileInfo();
                    localFileInfo.PackageName = _fileSystem.PackageName;
                    localFileInfo.BundleName = _bundle.BundleName;
                    localFileInfo.SourceFileURL = URL;
                    _fileSystem.CopyLocalFileServices.CopyFile(localFileInfo, _tempFilePath);
                    if (File.Exists(_tempFilePath))
                    {
                        DownloadProgress = 1f;
                        DownloadedBytes = _bundle.FileSize;
                        _steps = ESteps.VerifyBundleFile;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed copy local file : {URL}";
                    }
                }
                catch (System.Exception ex)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed copy local file : {ex.Message}";
                }
            }

            // 创建下载请求
            if (_steps == ESteps.CreateRequest)
            {
                int watchdogTime = _fileSystem.DownloadWatchDogTime;
                int timeout = 0; //注意：文件下载不做超时检测
                var args = new DownloadFileRequestArgs(URL, _tempFilePath, timeout, watchdogTime);
                _request = _fileSystem.DownloadBackend.CreateFileRequest(args);
                _request.SendRequest();
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _request.DownloadProgress;
                DownloadedBytes = _request.DownloadedBytes;
                Progress = DownloadProgress;
                if (_request.IsDone == false)
                    return;

                // 检查网络错误
                if (_request.Status == EDownloadRequestStatus.Succeed)
                {
                    _steps = ESteps.VerifyBundleFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _request.Error;
                }

                // 最终释放请求器
                _request.Dispose();
            }

            // 验证下载结果
            if (_steps == ESteps.VerifyBundleFile)
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
                    _steps = ESteps.CacheBundleFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyOperation.Error;

                    // 注意：验证失败后直接删除文件
                    if (File.Exists(_tempFilePath))
                        File.Delete(_tempFilePath);
                }
            }

            // 缓存文件
            if (_steps == ESteps.CacheBundleFile)
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

                // 注意：缓存完成后直接删除临时文件
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
        }
        internal override void InternalAbort()
        {
            if (_request != null)
                _request.AbortRequest();
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                //TODO 更新下载后台，防止无限挂起
                _fileSystem.DownloadBackend.Update();

                //TODO 等待导入或解压本地文件完毕，该操作会挂起主线程！
                InternalUpdate();
                if (IsDone)
                    break;

                //TODO 短暂休眠避免完全卡死
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}