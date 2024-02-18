using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    public class GetAllCacheFileInfosOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadCacheManifest,
            GetCacheFileInfos,
            Done,
        }

        private readonly PersistentManager _persistent;
        private readonly CacheManager _cache;
        private readonly string _packageVersion;
        private LoadCacheManifestOperation _tryLoadCacheManifestOp;
        private PackageManifest _manifest;
        private ESteps _steps = ESteps.None;

        private List<CacheFileInfo> _cacheFileInfos;

        /// <summary>
        /// 搜索结果
        /// </summary>
        public List<CacheFileInfo> Result
        {
            get { return _cacheFileInfos; }
        }


        internal GetAllCacheFileInfosOperation(PersistentManager persistent, CacheManager cache, string packageVersion)
        {
            _persistent = persistent;
            _cache = cache;
            _packageVersion = packageVersion;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.TryLoadCacheManifest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadCacheManifest)
            {
                if (_tryLoadCacheManifestOp == null)
                {
                    _tryLoadCacheManifestOp = new LoadCacheManifestOperation(_persistent, _packageVersion);
                    OperationSystem.StartOperation(_cache.PackageName, _tryLoadCacheManifestOp);
                }

                if (_tryLoadCacheManifestOp.IsDone == false)
                    return;

                if (_tryLoadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    _manifest = _tryLoadCacheManifestOp.Manifest;
                    _steps = ESteps.GetCacheFileInfos;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _tryLoadCacheManifestOp.Error;
                }
            }

            if (_steps == ESteps.GetCacheFileInfos)
            {
                var allCachedGUIDs = _cache.GetAllCachedGUIDs();
                _cacheFileInfos = new List<CacheFileInfo>(allCachedGUIDs.Count);
                for (int i = 0; i < allCachedGUIDs.Count; i++)
                {
                    var cachedGUID = allCachedGUIDs[i];
                    var wrapper = _cache.TryGetWrapper(cachedGUID);
                    if (wrapper != null)
                    {
                        if (_manifest.TryGetPackageBundleByCacheGUID(cachedGUID, out var packageBundle))
                        {
                            var cacheFileInfo = new CacheFileInfo(packageBundle.FileName, wrapper.DataFilePath, wrapper.DataFileCRC, wrapper.DataFileSize);
                            _cacheFileInfos.Add(cacheFileInfo);
                        }
                    }
                }

                // 注意：总是返回成功
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}