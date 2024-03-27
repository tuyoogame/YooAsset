using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    public class GetAllCacheFileInfosOperation : AsyncOperationBase
    {
        public class CacheInfo
        {
            public string FishHash { private set; get; }
            public string FilePath { private set; get; }
            public string FileCRC { private set; get; }
            public long FileSize { private set; get; }

            public CacheInfo(string fishHash, string filePath, string fileCRC, long fileSize)
            {
                FishHash = fishHash;
                FilePath = filePath;
                FileCRC = fileCRC;
                FileSize = fileSize;
            }
        }

        private enum ESteps
        {
            None,
            GetCacheFileInfos,
            Done,
        }

        private readonly string _packageName;
        private ESteps _steps = ESteps.None;
        private List<CacheInfo> _cacheFileInfos;

        /// <summary>
        /// 搜索结果
        /// </summary>
        public List<CacheInfo> Result
        {
            get { return _cacheFileInfos; }
        }

        internal GetAllCacheFileInfosOperation(string packageName)
        {
            _packageName = packageName;
        }
        internal override void Start()
        {
            _steps = ESteps.GetCacheFileInfos;
        }
        internal override void Update()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetCacheFileInfos)
            {
                var allCachedGUIDs = CacheSystem.GetAllCacheGUIDs(_packageName);
                _cacheFileInfos = new List<CacheInfo>(allCachedGUIDs.Count);
                for (int i = 0; i < allCachedGUIDs.Count; i++)
                {
                    var cachedGUID = allCachedGUIDs[i];
                    var wrapper = CacheSystem.TryGetWrapper(_packageName, cachedGUID);
                    if (wrapper != null)
                    {
                        string directoryName = Path.GetDirectoryName(wrapper.DataFilePath);
                        var directoryInfo = new DirectoryInfo(directoryName);
                        if (directoryInfo.Exists)
                        {
                            string fishHash = directoryInfo.Name;
                            var cacheFileInfo = new CacheInfo(fishHash, wrapper.DataFilePath, wrapper.DataFileCRC, wrapper.DataFileSize);
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