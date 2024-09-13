#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using WeChatWASM;
using YooAsset;
using UnityEngine;
using System.Linq;

internal class WXFSClearAllBundleFilesOperation : FSClearAllBundleFilesOperation
{
    private enum ESteps
    {
        None,
        GetAllCacheFiles,
        ClearAllWXCacheBundleFiles,
        Done,
    }

    private List<string> _wxBundleFilePaths;
    private int _fileTotalCount = 0;
    private WechatFileSystem _fileSystem;
    private ESteps _steps = ESteps.None;
    internal WXFSClearAllBundleFilesOperation(WechatFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        var allCacheFilePathDic = _fileSystem.GetWXAllCacheFilePath();
        _wxBundleFilePaths = allCacheFilePathDic.Values.ToList();
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
            if (_wxBundleFilePaths == null)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                return;
            }
            else
            {
                _steps = ESteps.ClearAllWXCacheBundleFiles;
                _fileTotalCount = _wxBundleFilePaths.Count;
            }
        }

        if (_steps == ESteps.ClearAllWXCacheBundleFiles)
        {
            for (int i = _wxBundleFilePaths.Count - 1; i >= 0; i--)
            {
                string bundlePath = _wxBundleFilePaths[i];
                if (_fileSystem.CheckWXFileIsExist(bundlePath))
                {
                    WX.RemoveFile(bundlePath, (bool isOk) =>
                    {
                        Debug.Log($"{_wxBundleFilePaths.Count}---删除缓存文件路径成功====={bundlePath}==");
                        _wxBundleFilePaths.Remove(bundlePath);
                    });
                }
                else
                {
                    _wxBundleFilePaths.Remove(bundlePath);
                    //Debug.LogWarning($"Not Exit Cache file:{bundlePath}");
                }

                if (OperationSystem.IsBusy)
                    break;
            }

            if (_fileTotalCount == 0)
                Progress = 1.0f;
            else
                Progress = 1.0f - (_wxBundleFilePaths.Count / _fileTotalCount);

            if (_wxBundleFilePaths.Count == 0)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}
#endif
