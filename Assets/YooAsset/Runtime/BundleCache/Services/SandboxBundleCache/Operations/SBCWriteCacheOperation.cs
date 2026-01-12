using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存写入操作
    /// </summary>
    internal sealed class SBCWriteCacheOperation : BCWriteCacheOperation
    {
        private enum ESteps
        {
            None,
            Check,
            VerifyFile,
            CacheFile,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly BCWriteCacheOptions _options;
        private VerifyTempFileOperation _verifyTempFileOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒写入缓存操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="options">写入缓存选项</param>
        public SBCWriteCacheOperation(SandboxBundleCache fileCache, BCWriteCacheOptions options)
        {
            _fileCache = fileCache;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.Check;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Check)
            {
                if (_fileCache.IsCached(_options.Bundle.BundleGuid))
                {
                    _steps = ESteps.Done;
                    SetError("Bundle is already cached.");
                }
                else
                {
                    _steps = ESteps.VerifyFile;
                }
            }

            if (_steps == ESteps.VerifyFile)
            {
                if (_verifyTempFileOp == null)
                {
                    var element = new TempFileInfo(_options.FilePath, _options.Bundle.FileCrc, _options.Bundle.FileSize);
                    _verifyTempFileOp = new VerifyTempFileOperation(element);
                    _verifyTempFileOp.StartOperation();
                    AddChildOperation(_verifyTempFileOp);
                }

                if (IsWaitForCompletion)
                    _verifyTempFileOp.WaitForCompletion();

                _verifyTempFileOp.UpdateOperation();
                if (_verifyTempFileOp.IsDone == false)
                    return;

                if (_verifyTempFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CacheFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_verifyTempFileOp.Error);
                }
            }

            if (_steps == ESteps.CacheFile)
            {
                string dataFilePath = _fileCache.GetDataFilePath(_options.Bundle);
                string infoFilePath = _fileCache.GetInfoFilePath(_options.Bundle);
                string dataTempPath = _fileCache.GetDataTempFilePath(_options.Bundle);
                string infoTempPath = _fileCache.GetInfoTempFilePath(_options.Bundle);

                long nowTicks = DateTime.UtcNow.Ticks;
                try
                {
                    // 阶段A：准备目标目录，清理可能存在的残留文件
                    FileUtility.EnsureParentDirectoryExists(dataFilePath);
                    DeleteFileSafely(dataTempPath);
                    DeleteFileSafely(infoTempPath);

                    // 阶段B：写入临时文件
                    FileInfo fileInfo = new FileInfo(_options.FilePath);
                    fileInfo.CopyTo(dataTempPath, true);
                    using (var fs = new FileStream(infoTempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new BufferWriter(64);
                        buffer.WriteUInt32(SandboxBundleCacheConsts.InfoFileMagic);
                        buffer.WriteInt32(SandboxBundleCacheConsts.InfoFileVersion);
                        buffer.WriteUInt32(_options.Bundle.FileCrc);
                        buffer.WriteInt64(_options.Bundle.FileSize);
                        buffer.WriteInt64(nowTicks); // CreatedAtTicks
                        buffer.WriteInt64(nowTicks); // LastAccessAtTicks
                        buffer.WriteToStream(fs);
                        fs.Flush();
                    }

                    // 阶段C：原子提交
                    if (File.Exists(dataFilePath))
                        File.Delete(dataFilePath);
                    File.Move(dataTempPath, dataFilePath);

                    if (File.Exists(infoFilePath))
                        File.Delete(infoFilePath);
                    File.Move(infoTempPath, infoFilePath);
                }
                catch (Exception ex)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to write cache file: {ex.Message}.");
                    YooLogger.LogError(Error);

                    // 回滚：清理临时文件，正式文件不受影响
                    DeleteFileSafely(dataTempPath);
                    DeleteFileSafely(infoTempPath);
                    return;
                }

                // 阶段D：注册内存缓存条目
                var cacheEntry = new SandboxBundleCacheEntry(_options.Bundle.BundleGuid, infoFilePath, dataFilePath);
                _fileCache.AddEntry(_options.Bundle.BundleGuid, cacheEntry);
                _steps = ESteps.Done;
                SetResult();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private static void DeleteFileSafely(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                YooLogger.LogWarning($"Failed to delete file '{filePath}': {ex.Message}.");
            }
        }
    }
}
