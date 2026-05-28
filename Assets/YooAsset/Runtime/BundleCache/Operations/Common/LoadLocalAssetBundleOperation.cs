using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 从本地文件加载 AssetBundle 操作
    /// </summary>
    internal sealed class LoadLocalAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundle,
            CheckResult,
            Done,
        }

        private readonly LoadLocalAssetBundleOptions _options;
        private AssetBundleCreateRequest _createRequest;
        private AssetBundle _assetBundle;
        private Stream _loadStream;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// Unity引擎加载是否失败
        /// </summary>
        public bool UnityEngineLoadFailed { get; private set; } = false;

        /// <summary>
        /// 创建本地 AssetBundle 加载操作实例
        /// </summary>
        /// <param name="options">从本地加载 AssetBundle 的操作选项</param>
        public LoadLocalAssetBundleOperation(LoadLocalAssetBundleOptions options)
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
                    LoadFromFile();
                }
                else
                {
                    var decryptor = _options.AssetBundleDecryptor;
                    if (decryptor == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"{_options.CacheName} asset bundle decryptor is null.");
                        return;
                    }

                    LoadResult result;
                    if (decryptor is IBundleOffsetDecryptor offsetDecryptor)
                    {
                        result = LoadFromFile(offsetDecryptor);
                    }
                    else if (decryptor is IBundleMemoryDecryptor memoryDecryptor)
                    {
                        result = LoadFromMemory(memoryDecryptor);
                    }
                    else if (decryptor is IBundleStreamDecryptor streamDecryptor)
                    {
                        result = LoadFromStream(streamDecryptor);
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError($"{_options.CacheName} does not support '{decryptor.GetType().Name}' for AssetBundle.");
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
                if (_createRequest != null)
                {
                    // 注意: 异步加载过程中，业务逻辑可能会强制转换为同步加载
                    if (IsWaitForCompletion)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.LogWarning("Suspending the main thread to load Unity bundle.");
                        _assetBundle = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        _assetBundle = _createRequest.assetBundle;
                    }
                }

                if (_assetBundle == null)
                {
                    _steps = ESteps.Done;
                    SetError("Unity engine load failed.");
                    UnityEngineLoadFailed = true;
                    CleanupStream();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = new AssetBundleHandle(_options.Bundle, _assetBundle, _loadStream);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private void LoadFromFile()
        {
            if (IsWaitForCompletion)
                _assetBundle = AssetBundle.LoadFromFile(_options.FilePath);
            else
                _createRequest = AssetBundle.LoadFromFileAsync(_options.FilePath);
        }
        private LoadResult LoadFromFile(IBundleOffsetDecryptor decryptor)
        {
            var args = new BundleDecryptArgs(_options.Bundle, null, _options.FilePath);
            long offset = decryptor.GetFileOffset(args);
            if (offset < 0)
                return LoadResult.Failure($"{_options.CacheName} decryptor returned negative offset: {offset}.");

            if (IsWaitForCompletion)
                _assetBundle = AssetBundle.LoadFromFile(_options.FilePath, 0, (ulong)offset);
            else
                _createRequest = AssetBundle.LoadFromFileAsync(_options.FilePath, 0, (ulong)offset);

            return LoadResult.Default();
        }
        private LoadResult LoadFromMemory(IBundleMemoryDecryptor decryptor)
        {
            var args = new BundleDecryptArgs(_options.Bundle, null, _options.FilePath);
            var binaryData = decryptor.GetDecryptedData(args);
            if (binaryData == null)
                return LoadResult.Failure($"{_options.CacheName} decryptor returned null data.");

            if (IsWaitForCompletion)
                _assetBundle = AssetBundle.LoadFromMemory(binaryData);
            else
                _createRequest = AssetBundle.LoadFromMemoryAsync(binaryData);

            return LoadResult.Default();
        }
        private LoadResult LoadFromStream(IBundleStreamDecryptor decryptor)
        {
            var args = new BundleDecryptArgs(_options.Bundle, null, _options.FilePath);
            uint bufferSize = (uint)decryptor.GetBufferSize(args);
            _loadStream = decryptor.CreateDecryptionStream(args);
            if (_loadStream == null)
                return LoadResult.Failure($"{_options.CacheName} decryptor returned null stream.");

            uint unityCRC = 0;
            if (IsWaitForCompletion)
                _assetBundle = AssetBundle.LoadFromStream(_loadStream, unityCRC, bufferSize);
            else
                _createRequest = AssetBundle.LoadFromStreamAsync(_loadStream, unityCRC, bufferSize);

            return LoadResult.Default();
        }
        private void CleanupStream()
        {
            if (_loadStream != null)
            {
                _loadStream.Close();
                _loadStream.Dispose();
                _loadStream = null;
            }
        }
    }
}
