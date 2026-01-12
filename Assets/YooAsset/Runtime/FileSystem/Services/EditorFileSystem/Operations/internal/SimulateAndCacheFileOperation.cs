
namespace YooAsset
{
    /// <summary>
    /// 模拟下载并缓存文件操作
    /// </summary>
    internal sealed class SimulateAndCacheFileOperation : DownloadFileBaseOperation
    {
        private enum ESteps
        {
            None,
            CreateRequest,
            CheckRequest,
            CacheFile,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private IDownloadRequest _downloadRequest;
        private BCWriteCacheOperation _writeCacheOp;
        private ESteps _steps = ESteps.None;

        internal SimulateAndCacheFileOperation(EditorFileSystem fileSystem, PackageBundle bundle, string filePath) : base(bundle, filePath)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载请求
            if (_steps == ESteps.CreateRequest)
            {
                int speed = _fileSystem.VirtualDownloadSpeed;
                var args = new SimulatedDownloadRequestArgs(
                    url: Url,
                    fileSize: Bundle.FileSize,
                    downloadSpeed: speed);
                _downloadRequest = _fileSystem.DownloadBackend.CreateSimulateRequest(args);
                _downloadRequest.SendRequest();
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                LatestReport = DownloadReport.CreateProgress(_downloadRequest.DownloadedBytes, _downloadRequest.DownloadProgress);
                Progress = _downloadRequest.DownloadProgress;
                if (_downloadRequest.IsDone == false)
                    return;

                // 更新下载报告
                LatestReport = DownloadReport.CreateFinished(_downloadRequest.HttpCode, _downloadRequest.HttpError,
                    _downloadRequest.DownloadedBytes, _downloadRequest.DownloadProgress);

                // 检查网络错误
                if (_downloadRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _steps = ESteps.CacheFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadRequest.Error);
                }
            }

            // 缓存文件
            if (_steps == ESteps.CacheFile)
            {
                if (_writeCacheOp == null)
                {
                    var options = new BCWriteCacheOptions(Bundle, Url);
                    _writeCacheOp = _fileSystem.BundleCache.WriteCacheAsync(options);
                    _writeCacheOp.StartOperation();
                    AddChildOperation(_writeCacheOp);
                }

                _writeCacheOp.UpdateOperation();
                if (_writeCacheOp.IsDone == false)
                    return;

                if (_writeCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_writeCacheOp.Error);
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadRequest != null)
            {
                _downloadRequest.Dispose();
                _downloadRequest = null;
            }
        }
        protected override void InternalWaitForCompletion()
        {
            throw new YooInternalException($"{nameof(SimulateAndCacheFileOperation)} does not support synchronous waiting. Bundle: '{Bundle.BundleName}', Url: '{Url}'.");
        }
    }
}