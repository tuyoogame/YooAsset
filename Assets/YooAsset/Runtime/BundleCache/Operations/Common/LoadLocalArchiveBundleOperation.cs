using System;

namespace YooAsset
{
    /// <summary>
    /// 从本地加载 ArchiveBundle 操作
    /// </summary>
    internal sealed class LoadLocalArchiveBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundle,
            CheckResult,
            Done,
        }

        private readonly LoadLocalArchiveBundleOptions _options;
        private ArchiveBundle _archiveBundle;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建本地 ArchiveBundle 加载操作实例
        /// </summary>
        /// <param name="options">从本地加载 ArchiveBundle 的操作选项</param>
        public LoadLocalArchiveBundleOperation(LoadLocalArchiveBundleOptions options)
        {
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadBundle;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBundle)
            {
                if (_options.Bundle.IsEncrypted == false)
                {
                    if (FileUtility.IsFileIOSupported(_options.FilePath) == false)
                    {
                        _steps = ESteps.Done;
                        SetError($"FileIO is not supported for builtin path: '{_options.FilePath}'.");
                        return;
                    }

                    LoadResult result = LoadFromFile();
                    if (result.Succeeded == false)
                    {
                        _steps = ESteps.Done;
                        SetError(result.Error);
                        return;
                    }
                }
                else
                {
                    var decryptor = _options.ArchiveBundleDecryptor;
                    if (decryptor == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"{_options.CacheName} archive bundle decryptor is null.");
                        return;
                    }

                    LoadResult result;
                    if (decryptor is IBundleMemoryDecryptor memoryDecryptor)
                    {
                        result = LoadFromMemory(memoryDecryptor);
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError($"{_options.CacheName} does not support '{decryptor.GetType().Name}' for ArchiveBundle.");
                        return;
                    }

                    if (result.Succeeded == false)
                    {
                        _steps = ESteps.Done;
                        SetError(result.Error);
                        return;
                    }
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_archiveBundle == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Loaded archive bundle is null.");
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = new ArchiveBundleHandle(_options.Bundle, _archiveBundle);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private LoadResult LoadFromFile()
        {
            try
            {
                _archiveBundle = ArchiveBundleHelper.LoadFromFile(_options.FilePath);
                return LoadResult.Default();
            }
            catch (Exception ex)
            {
                return LoadResult.Failure($"Failed to load archive bundle file: {ex.Message}.");
            }
        }
        private LoadResult LoadFromMemory(IBundleMemoryDecryptor decryptor)
        {
            try
            {
                var args = new BundleDecryptArgs(_options.Bundle, null, _options.FilePath);
                byte[] binaryData = decryptor.GetDecryptedData(args);
                if (binaryData == null)
                    return LoadResult.Failure($"{_options.CacheName} decryptor returned null data.");

                _archiveBundle = ArchiveBundleHelper.LoadFromMemory(binaryData);
                return LoadResult.Default();
            }
            catch (Exception ex)
            {
                return LoadResult.Failure($"Failed to load archive bundle file from memory: {ex.Message}.");
            }
        }
    }
}
