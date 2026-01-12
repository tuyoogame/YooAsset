using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 导入并缓存文件操作
    /// </summary>
    internal sealed class ImportAndCacheFileOperation : DownloadFileBaseOperation
    {
        private enum ESteps
        {
            None,
            CheckTempFile,
            CopyLocalFile,
            CacheFile,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly string _sourceFilePath;
        private readonly string _tempFilePath;
        private BCWriteCacheOperation _bundleCacheOp;
        private ESteps _steps = ESteps.None;

        internal ImportAndCacheFileOperation(SandboxFileSystem fileSystem, PackageBundle bundle, string sourceFilePath) : base(bundle, sourceFilePath)
        {
            _fileSystem = fileSystem;
            _sourceFilePath = sourceFilePath;
            _tempFilePath = _fileSystem.GetTempFilePath(bundle);
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckTempFile;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 检测临时文件
            if (_steps == ESteps.CheckTempFile)
            {
                // 删除历史临时文件
                FileUtility.EnsureParentDirectoryExists(_tempFilePath);
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                _steps = ESteps.CopyLocalFile;
            }

            // 拷贝本地文件
            if (_steps == ESteps.CopyLocalFile)
            {
                try
                {
                    File.Copy(_sourceFilePath, _tempFilePath, true);

                    // 更新下载报告
                    LatestReport = DownloadReport.CreateProgress(Bundle.FileSize, 1f);
                    _steps = ESteps.CacheFile;
                }
                catch (System.Exception ex)
                {
                    _steps = ESteps.Done;
                    LatestReport = DownloadReport.CreateFinished(-1, ex.Message, 0, 0f);
                    SetError($"Failed to copy local file: {ex.Message}.");

                    // 注意：拷贝失败后直接删除临时文件
                    DeleteTempFile();
                }
            }

            // 缓存文件
            if (_steps == ESteps.CacheFile)
            {
                if (_bundleCacheOp == null)
                {
                    var options = new BCWriteCacheOptions(Bundle, _tempFilePath);
                    _bundleCacheOp = _fileSystem.BundleCache.WriteCacheAsync(options);
                    _bundleCacheOp.StartOperation();
                    AddChildOperation(_bundleCacheOp);
                }

                if (IsWaitForCompletion)
                    _bundleCacheOp.WaitForCompletion();

                _bundleCacheOp.UpdateOperation();
                if (_bundleCacheOp.IsDone == false)
                    return;

                if (_bundleCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_bundleCacheOp.Error);
                }

                // 注意：缓存完成后直接删除临时文件
                DeleteTempFile();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private void DeleteTempFile()
        {
            try
            {
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
            catch (System.Exception ex)
            {
                YooLogger.LogWarning($"Failed to delete temp file '{_tempFilePath}': {ex.Message}.");
            }
        }
    }
}