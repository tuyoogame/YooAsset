using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存写入操作
    /// </summary>
    internal sealed class EBCWriteCacheOperation : BCWriteCacheOperation
    {
        private enum ESteps
        {
            None,
            CheckCache,
            CacheFile,
            Done,
        }

        private readonly EditorBundleCache _fileCache;
        private readonly BCWriteCacheOptions _options;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建编辑器写入缓存操作实例
        /// </summary>
        /// <param name="fileCache">编辑器文件缓存系统</param>
        /// <param name="options">写入缓存选项</param>
        public EBCWriteCacheOperation(EditorBundleCache fileCache, BCWriteCacheOptions options)
        {
            _fileCache = fileCache;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckCache;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckCache)
            {
                if (_fileCache.IsCached(_options.Bundle.BundleGuid))
                {
                    _steps = ESteps.Done;
                    SetError("Bundle is already cached.");
                }
                else
                {
                    _steps = ESteps.CacheFile;
                }
            }

            if (_steps == ESteps.CacheFile)
            {
                string markerFilePath = _fileCache.GetMarkerFilePath(_options.Bundle);
                string markerTempPath = _fileCache.GetMarkerTempFilePath(_options.Bundle);

                try
                {
                    // 阶段A：准备目标目录，清理可能存在的残留临时文件
                    FileUtility.EnsureParentDirectoryExists(markerFilePath);
                    DeleteFileSafely(markerTempPath);

                    // 阶段B：写入临时文件，内容仅用于人工调试
                    string debugContent = $"BundleName={_options.Bundle.BundleName}\n"
                        + $"BundleGuid={_options.Bundle.BundleGuid}\n";
                    File.WriteAllText(markerTempPath, debugContent);

                    // 阶段C：原子提交
                    if (File.Exists(markerFilePath))
                        File.Delete(markerFilePath);
                    File.Move(markerTempPath, markerFilePath);
                }
                catch (Exception ex)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to write marker file: {ex.Message}.");
                    YooLogger.LogError(Error);

                    // 回滚：清理临时文件，正式文件不受影响
                    DeleteFileSafely(markerTempPath);
                    return;
                }

                // 阶段D：注册内存缓存条目
                var cacheEntry = new EditorBundleCacheEntry(_options.Bundle.BundleGuid, markerFilePath);
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
