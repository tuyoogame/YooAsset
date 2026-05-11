using System;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存加载 RawBundle 操作
    /// </summary>
    internal sealed class SBCLoadRawBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            GetEntry,
            LoadBundle,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly PackageBundle _bundle;
        private LoadLocalRawBundleOperation _loadLocalRawBundleOp;
        private SandboxBundleCacheEntry _cacheEntry;

        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒 RawBundle 加载操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="bundle">资源包描述</param>
        public SBCLoadRawBundleOperation(SandboxBundleCache fileCache, PackageBundle bundle)
        {
            _fileCache = fileCache;
            _bundle = bundle;
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
                if (_loadLocalRawBundleOp == null)
                {
                    var options = new LoadLocalRawBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _bundle,
                        filePath: _cacheEntry.DataFilePath,
                        rawBundleDecryptor: _fileCache.Config.RawBundleDecryptor);
                    _loadLocalRawBundleOp = new LoadLocalRawBundleOperation(options);
                    _loadLocalRawBundleOp.StartOperation();
                    AddChildOperation(_loadLocalRawBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadLocalRawBundleOp.WaitForCompletion();

                _loadLocalRawBundleOp.UpdateOperation();
                if (_loadLocalRawBundleOp.IsDone == false)
                    return;

                if (_loadLocalRawBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadLocalRawBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded raw bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadLocalRawBundleOp.BundleHandle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadLocalRawBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}