#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;
using WeChatWASM;


internal class WXFSClearUnusedBundleFilesAsync : FSClearUnusedBundleFilesOperation
{
    private enum ESteps
    {
        None,
        GetAllCacheFiles,
        WaitResult,
        GetUnusedCacheFiles,
        ClearUnusedCacheFiles,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private List<string> _unusedCacheFiles;
    private int _unusedFileTotalCount = 0;
    private GetSavedFileListSuccessCallbackResult _result;
    private ESteps _steps = ESteps.None;

    internal WXFSClearUnusedBundleFilesAsync(WechatFileSystem fileSystem, PackageManifest manifest)
    {
        _fileSystem = fileSystem;
        _manifest = manifest;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.GetAllCacheFiles;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetAllCacheFiles)
        {
            _steps = ESteps.WaitResult;

            var fileSystemMgr = WX.GetFileSystemManager();
            var option = new GetSavedFileListOption();
            fileSystemMgr.GetSavedFileList(option);
            option.fail += (FileError error) =>
            {
                _steps = ESteps.Done;
                Error = error.errMsg;
                Status = EOperationStatus.Failed;
            };
            option.success += (GetSavedFileListSuccessCallbackResult result) =>
            {
                _result = result;
                _steps = ESteps.GetUnusedCacheFiles;
            };
        }

        if (_steps == ESteps.WaitResult)
        {
            return;
        }

        if (_steps == ESteps.GetUnusedCacheFiles)
        {
            _unusedCacheFiles = GetUnusedCacheFiles();
            _unusedFileTotalCount = _unusedCacheFiles.Count;
            _steps = ESteps.ClearUnusedCacheFiles;
            YooLogger.Log($"Found unused cache files count : {_unusedFileTotalCount}");
        }

        if (_steps == ESteps.ClearUnusedCacheFiles)
        {
            for (int i = _unusedCacheFiles.Count - 1; i >= 0; i--)
            {
                string cacheFilePath = _unusedCacheFiles[i];
                WX.RemoveFile(cacheFilePath, null);
                _unusedCacheFiles.RemoveAt(i);

                if (OperationSystem.IsBusy)
                    break;
            }

            if (_unusedFileTotalCount == 0)
                Progress = 1.0f;
            else
                Progress = 1.0f - (_unusedCacheFiles.Count / _unusedFileTotalCount);

            if (_unusedCacheFiles.Count == 0)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }

    private List<string> GetUnusedCacheFiles()
    {
        List<string> result = new List<string>(_result.fileList.Length);
        foreach (var fileInfo in _result.fileList)
        {
            // 如果存储文件名是按照Bundle文件哈希值存储
            string bundleGUID = Path.GetFileNameWithoutExtension(fileInfo.filePath);
            if (_manifest.TryGetPackageBundleByBundleGUID(bundleGUID, out PackageBundle value) == false)
            {
                result.Add(fileInfo.filePath);
            }

            // 如果存储文件名是按照Bundle文件名称存储
            /*
            string bundleName = Path.GetFileNameWithoutExtension(fileInfo.filePath);
            if (_manifest.TryGetPackageBundleByBundleName(bundleName, out PackageBundle value) == false)
            {
                result.Add(fileInfo.filePath);
            }
            */
        }
        return result;
    }
}
#endif