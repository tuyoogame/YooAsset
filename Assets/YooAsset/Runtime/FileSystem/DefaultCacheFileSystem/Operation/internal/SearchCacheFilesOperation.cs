using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal sealed class SearchCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Prepare,
            SearchFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private IEnumerator<DirectoryInfo> _filesEnumerator = null;
        private float _verifyStartTime;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 需要验证的元素
        /// </summary>
        public readonly List<CacheFileElement> Result = new List<CacheFileElement>(5000);


        internal SearchCacheFilesOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.Prepare;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Prepare)
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(_fileSystem.GetCacheFilesRoot());
                if (rootDirectory.Exists)
                {
                    var directorieInfos = rootDirectory.EnumerateDirectories();
                    _filesEnumerator = directorieInfos.GetEnumerator();
                }
                _steps = ESteps.SearchFiles;
            }

            if (_steps == ESteps.SearchFiles)
            {
                if (SearchFiles())
                    return;

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
                float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
                YooLogger.Log($"Search cache files elapsed time {costTime:f1} seconds");
            }
        }

        private bool SearchFiles()
        {
            if (_filesEnumerator == null)
                return false;

            bool isFindItem;
            while (true)
            {
                isFindItem = _filesEnumerator.MoveNext();
                if (isFindItem == false)
                    break;

                var rootFoder = _filesEnumerator.Current;
                var childDirectories = rootFoder.GetDirectories();
                foreach (var chidDirectory in childDirectories)
                {
                    string bundleGUID = chidDirectory.Name;
                    if (_fileSystem.IsRecordFile(bundleGUID))
                        continue;

                    // 创建验证元素类
                    string fileRootPath = chidDirectory.FullName;
                    string dataFilePath = $"{fileRootPath}/{ DefaultCacheFileSystemDefine.SaveBundleDataFileName}";
                    string infoFilePath = $"{fileRootPath}/{ DefaultCacheFileSystemDefine.SaveBundleInfoFileName}";

                    // 存储的数据文件追加文件格式
                    if (_fileSystem.AppendFileExtension)
                    {
                        string dataFileExtension = FindDataFileExtension(chidDirectory);
                        if (string.IsNullOrEmpty(dataFileExtension) == false)
                            dataFilePath += dataFileExtension;
                    }

                    var element = new CacheFileElement(_fileSystem.PackageName, bundleGUID, fileRootPath, dataFilePath, infoFilePath);
                    Result.Add(element);
                }

                if (OperationSystem.IsBusy)
                    break;
            }

            return isFindItem;
        }
        private string FindDataFileExtension(DirectoryInfo directoryInfo)
        {
            string dataFileExtension = string.Empty;
            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Name.StartsWith(DefaultCacheFileSystemDefine.SaveBundleDataFileName))
                {
                    dataFileExtension = fileInfo.Extension;
                    break;
                }
            }
            return dataFileExtension;
        }
    }
}