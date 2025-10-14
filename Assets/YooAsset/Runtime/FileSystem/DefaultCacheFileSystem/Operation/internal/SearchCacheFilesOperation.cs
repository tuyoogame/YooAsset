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
        private IEnumerator<string> _filesEnumerator = null;
        private float _verifyStartTime;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 需要验证的元素
        /// </summary>
        public readonly List<VerifyFileElement> Result = new List<VerifyFileElement>(5000);


        internal SearchCacheFilesOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.Prepare;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Prepare)
            {
                string rootDirectory = _fileSystem.GetCacheBundleFilesRoot();
                if (Directory.Exists(rootDirectory))
                {
                    var directories = Directory.EnumerateDirectories(rootDirectory);
                    _filesEnumerator = directories.GetEnumerator();
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
                var childDirectories = Directory.EnumerateDirectories(rootFoder);
                foreach (var chidDirectory in childDirectories)
                {
                    string bundleGUID = Path.GetFileName(chidDirectory);
                    if (_fileSystem.IsRecordBundleFile(bundleGUID))
                        continue;

                    // 创建验证元素类
                    string fileRootPath = chidDirectory;
                    string dataFilePath = $"{fileRootPath}/{DefaultCacheFileSystemDefine.BundleDataFileName}";
                    string infoFilePath = $"{fileRootPath}/{DefaultCacheFileSystemDefine.BundleInfoFileName}";

                    // 存储的数据文件追加文件格式
                    if (_fileSystem.AppendFileExtension)
                    {
                        string dataFileExtension = FindDataFileExtension(chidDirectory);
                        if (string.IsNullOrEmpty(dataFileExtension) == false)
                        {
                            dataFilePath += dataFileExtension;
                        }
                    }

                    var element = new VerifyFileElement(_fileSystem.PackageName, bundleGUID, fileRootPath, dataFilePath, infoFilePath);
                    Result.Add(element);
                }

                if (OperationSystem.IsBusy)
                    break;
            }

            return isFindItem;
        }
        private string FindDataFileExtension(string directory)
        {
            string dataFileExtension = string.Empty;
            string searchPattern = DefaultCacheFileSystemDefine.BundleDataFileName + "*";
            var dataFiles = Directory.EnumerateFiles(directory, searchPattern);
            foreach (var filePath in dataFiles)
            {
                dataFileExtension = Path.GetExtension(filePath);
                break;
            }
            return dataFileExtension;
        }
    }
}