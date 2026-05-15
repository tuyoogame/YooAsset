using System;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存加载 AssetBundle 操作
    /// </summary>
    internal sealed class SBCLoadAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            GetEntry,
            LoadBundle,
            VerifyFile,
            TryFallback,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly PackageBundle _bundle;
        private LoadLocalAssetBundleOperation _loadLocalAssetBundleOp;
        private BCVerifyCacheOperation _verifyCacheOp;
        private SandboxBundleCacheEntry _cacheEntry;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒 AssetBundle 加载操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="packageBundle">资源包描述</param>
        public SBCLoadAssetBundleOperation(SandboxBundleCache fileCache, PackageBundle packageBundle)
        {
            _fileCache = fileCache;
            _bundle = packageBundle;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.GetEntry;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetEntry)
            {
                _cacheEntry = _fileCache.GetEntry(_bundle.BundleGuid);
                if (_cacheEntry == null)
                {
                    _steps = ESteps.Done;
                    SetError($"File cache entry not found: '{_bundle.BundleGuid}'.");
                }
                else
                {
                    _steps = ESteps.LoadBundle;
                }
            }

            if (_steps == ESteps.LoadBundle)
            {
                if (_loadLocalAssetBundleOp == null)
                {
                    var options = new LoadLocalAssetBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _bundle,
                        filePath: _cacheEntry.DataFilePath,
                        assetBundleDecryptor: _fileCache.Config.AssetBundleDecryptor);
                    _loadLocalAssetBundleOp = new LoadLocalAssetBundleOperation(options);
                    _loadLocalAssetBundleOp.StartOperation();
                    AddChildOperation(_loadLocalAssetBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadLocalAssetBundleOp.WaitForCompletion();

                _loadLocalAssetBundleOp.UpdateOperation();
                if (_loadLocalAssetBundleOp.IsDone == false)
                    return;

                if (_loadLocalAssetBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadLocalAssetBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded asset bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadLocalAssetBundleOp.BundleHandle;
                }
                else
                {
                    // 注意：如果引擎加载失败，需要重新验证文件
                    if (_loadLocalAssetBundleOp.UnityEngineLoadFailed)
                    {
                        _steps = ESteps.VerifyFile;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(_loadLocalAssetBundleOp.Error);
                    }
                }
            }

            if (_steps == ESteps.VerifyFile)
            {
                // 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
                // 说明：在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
                if (_verifyCacheOp == null)
                {
                    var options = new BCVerifyCacheOptions(_bundle, true);
                    _verifyCacheOp = _fileCache.VerifyCacheAsync(options);
                    _verifyCacheOp.StartOperation();
                    AddChildOperation(_verifyCacheOp);
                }

                if (IsWaitForCompletion)
                    _verifyCacheOp.WaitForCompletion();

                _verifyCacheOp.UpdateOperation();
                if (_verifyCacheOp.IsDone == false)
                    return;

                if (_verifyCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.TryFallback;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_verifyCacheOp.Error);
                }
            }

            if (_steps == ESteps.TryFallback)
            {
                // 调用后备加载方法
                // 注意：在安卓移动平台，华为和三星真机上有极小概率加载资源包失败。
                // 说明：大多数情况在首次安装下载资源到沙盒内，游戏过程中切换到后台再回到游戏内有很大概率触发！
                AssetBundle assetBundle;
                if (_bundle.IsEncrypted)
                {
                    if (_fileCache.Config.AssetBundleFallbackDecryptor == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"{nameof(SandboxBundleCache)} fallback decryptor is null.");
                        return;
                    }

                    assetBundle = FallbackLoadEncryptedAssetBundle(_fileCache.Config.AssetBundleFallbackDecryptor);
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to load encrypted asset bundle '{_bundle.BundleName}' with fallback.");
                    }
                }
                else
                {
                    assetBundle = FallbackLoadAssetBundle();
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to load asset bundle '{_bundle.BundleName}' with fallback.");
                    }
                }

                if (assetBundle != null)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = new AssetBundleHandle(_bundle, assetBundle, null);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private AssetBundle FallbackLoadAssetBundle()
        {
            try
            {
                byte[] fileData = FileUtility.ReadAllBytes(_cacheEntry.DataFilePath);
                return AssetBundle.LoadFromMemory(fileData);
            }
            catch (Exception ex)
            {
                YooLogger.LogWarning($"Fallback load failed: {ex.Message}.");
                return null;
            }
        }
        private AssetBundle FallbackLoadEncryptedAssetBundle(IBundleMemoryDecryptor decryptor)
        {
            try
            {
                var args = new BundleDecryptArgs(_bundle, null, _cacheEntry.DataFilePath);
                var fileData = decryptor.GetDecryptedData(args);
                if (fileData == null)
                    return null;
                return AssetBundle.LoadFromMemory(fileData);
            }
            catch (Exception ex)
            {
                YooLogger.LogWarning($"Fallback encrypted load failed: {ex.Message}.");
                return null;
            }
        }
    }
}