#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YooAsset;
using WeChatWASM;

internal class WXFSClearUnusedBundleFilesAsync : FSClearCacheFilesOperation
{
    private enum ESteps
    {
        None,
        GetUnusedCacheFiles,
        WaitingSearch,
        ClearUnusedCacheFiles,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private List<string> _unusedCacheFiles = new List<string>(1000);
    private int _unusedFileTotalCount = 0;
    private ESteps _steps = ESteps.None;

    internal WXFSClearUnusedBundleFilesAsync(WechatFileSystem fileSystem, PackageManifest manifest)
    {
        _fileSystem = fileSystem;
        _manifest = manifest;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.GetUnusedCacheFiles;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetUnusedCacheFiles)
        {
            _steps = ESteps.WaitingSearch;

            // 说明：__GAME_FILE_CACHE/yoo/ 目录下包含所有的资源文件和清单文件
            var fileSystemMgr = _fileSystem.GetFileSystemMgr();
            var statOption = new WXStatOption();
            statOption.path = _fileSystem.FileRoot;
            statOption.recursive = true;
            statOption.success = (WXStatResponse response) =>
            {
                foreach (var fileStat in response.stats)
                {
                    // 如果是目录文件
                    string fileExtension = Path.GetExtension(fileStat.path);
                    if (string.IsNullOrEmpty(fileExtension))
                        continue;

                    // 如果是资源清单
                    //TODO 默认的清单文件格式
                    if (fileExtension == ".bytes" || fileExtension == ".hash")
                        continue;

                    // 注意：适配不同的文件命名方式！
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileStat.path);
                    string bundleGUID = fileNameWithoutExtension.Split('_').Last();
                    if (_manifest.TryGetPackageBundleByBundleGUID(bundleGUID, out PackageBundle value) == false)
                    {
                        string filePath = _fileSystem.FileRoot + fileStat.path;
                        if (_unusedCacheFiles.Contains(filePath) == false)
                            _unusedCacheFiles.Add(filePath);
                    }
                }

                _steps = ESteps.ClearUnusedCacheFiles;
                _unusedFileTotalCount = _unusedCacheFiles.Count;
                YooLogger.Log($"Found unused cache files count : {_unusedFileTotalCount}");
            };
            statOption.fail = (WXStatResponse response) =>
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = response.errMsg;
            };
            fileSystemMgr.Stat(statOption);
        }

        if (_steps == ESteps.ClearUnusedCacheFiles)
        {
            for (int i = _unusedCacheFiles.Count - 1; i >= 0; i--)
            {
                string filePath = _unusedCacheFiles[i];
                _unusedCacheFiles.RemoveAt(i);
                WX.RemoveFile(filePath, null);

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
}
#endif