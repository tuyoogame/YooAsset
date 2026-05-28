using System;

namespace YooAsset
{
    /// <summary>
    /// 从本地加载 RawBundle 操作
    /// </summary>
    internal sealed class LoadLocalRawBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundle,
            CheckResult,
            Done,
        }

        private readonly LoadLocalRawBundleOptions _options;
        private RawBundle _rawBundle;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建本地 RawBundle 加载操作实例
        /// </summary>
        /// <param name="options">从本地加载 RawBundle 的操作选项</param>
        public LoadLocalRawBundleOperation(LoadLocalRawBundleOptions options)
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
                    var decryptor = _options.RawBundleDecryptor;
                    if (decryptor == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"{_options.CacheName} raw bundle decryptor is null.");
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
                        SetError($"{_options.CacheName} does not support '{decryptor.GetType().Name}' for RawBundle.");
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
                if (_rawBundle == null)
                {
                    _steps = ESteps.Done;
                    SetError($"Loaded raw bundle is null.");
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = new RawBundleHandle(_options.Bundle, _rawBundle);
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
                _rawBundle = RawBundleHelper.LoadFromFile(_options.FilePath);
                return LoadResult.Default();
            }
            catch (Exception ex)
            {
                return LoadResult.Failure($"Failed to load raw bundle file: {ex.Message}.");
            }
        }
        private LoadResult LoadFromMemory(IBundleMemoryDecryptor decryptor)
        {
            var args = new BundleDecryptArgs(_options.Bundle, null, _options.FilePath);
            var binaryData = decryptor.GetDecryptedData(args);
            if (binaryData == null)
                return LoadResult.Failure($"{_options.CacheName} decryptor returned null data.");

            _rawBundle = RawBundleHelper.LoadFromMemory(binaryData);
            return LoadResult.Default();
        }
    }
}
