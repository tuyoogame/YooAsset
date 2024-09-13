#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using WeChatWASM;
using YooAsset;
using UnityEngine;
using System.Linq;

internal class WXFSClearUnusedBundleFilesAsync : FSClearUnusedBundleFilesOperation
{
    private enum ESteps
    {
        None,
        LoadCachePackageInfo,
        VerifyFileData,
        LoadManifest,
        GetUnusedCacheFiles,
        ClearUnusedCacheFiles,
        Done,
    }

    private WechatFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private PackageManifest _cacheManifest;
    private List<string> _unusedBundleGUIDs;
    private ESteps _steps = ESteps.None;
    private DeserializeManifestOperation _deserializer;
    private byte[] _fileData;
    private string _packageHash;
    private int _unusedFileTotalCount = 0;
    private string _lastPackageVersion;

    internal WXFSClearUnusedBundleFilesAsync(WechatFileSystem fileSystem, PackageManifest manifest)
    {
        _fileSystem = fileSystem;
        _manifest = manifest;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.LoadCachePackageInfo;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadCachePackageInfo)
        {
            LoadManifestInfo();
            if(_fileData != null && _fileData.Length > 0 && !string.IsNullOrEmpty(_packageHash))
            {
                _steps = ESteps.VerifyFileData;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Failed to load cache package manifest file!";
            }
        }

        if(_steps == ESteps.VerifyFileData) 
        {
            string fileHash = HashUtility.BytesMD5(_fileData);
            if (fileHash == _packageHash)
            {
                _steps = ESteps.LoadManifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Failed to verify cache package manifest file!";
            }
        }

        if (_steps == ESteps.LoadManifest)
        {
            if (_deserializer == null)
            {
                _deserializer = new DeserializeManifestOperation(_fileData);
                OperationSystem.StartOperation(_fileSystem.PackageName, _deserializer);
            }

            Progress = _deserializer.Progress;
            if (_deserializer.IsDone == false)
                return;

            if (_deserializer.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.GetUnusedCacheFiles;
                _cacheManifest = _deserializer.Manifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _deserializer.Error;
            }
        }

        if(_steps == ESteps.GetUnusedCacheFiles) 
        {
            _unusedBundleGUIDs = GetUnusedBundleGUIDs();
            _unusedFileTotalCount = _unusedBundleGUIDs.Count;
            _steps = ESteps.ClearUnusedCacheFiles;
            YooLogger.Log($"Found unused cache files count : {_unusedFileTotalCount}");
        }

        if (_steps == ESteps.ClearUnusedCacheFiles)
        {
            for (int i = _unusedBundleGUIDs.Count - 1; i >= 0; i--)
            {
                string bundleGUID = _unusedBundleGUIDs[i];
                PackageBundle bundle = null;
                if(_cacheManifest.TryGetPackageBundleByBundleGUID(bundleGUID,out bundle))
                {
                    if (bundle != null)
                    {
                        var cachePath = GetUnuseCachePathByBundleName(bundle.FileName);
                        WX.RemoveFile(cachePath, (bool isOk) =>
                        {
                            Debug.Log($"{_unusedBundleGUIDs.Count}---删除缓存文件路径成功====={cachePath}==");
                            //_unusedBundleGUIDs.Remove(cachePath);
                        });

                        //_fileSystem.DeleteCacheFile(bundleGUID);
                        _unusedBundleGUIDs.RemoveAt(i);
                    }
                }
                
                if (OperationSystem.IsBusy)
                    break;
            }

            if (_unusedFileTotalCount == 0)
                Progress = 1.0f;
            else
                Progress = 1.0f - (_unusedBundleGUIDs.Count / _unusedFileTotalCount);

            if (_unusedBundleGUIDs.Count == 0)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }

    private List<string> GetUnusedBundleGUIDs()
    {
        var allBundleGUIDs = _cacheManifest.BundleDic3.Keys.ToList();
        List<string> result = new List<string>(allBundleGUIDs.Count);
        foreach (var bundleGUID in allBundleGUIDs)
        {
            if (_manifest.IsIncludeBundleFile(bundleGUID) == false)
            {
                result.Add(bundleGUID);
            }
        }
        return result;
    }

    private void LoadManifestInfo()
    {
        var packageName = _fileSystem.PackageName;
        _lastPackageVersion = WX.StorageGetStringSync(YooAssets.DefaultPackageVersion_Key, YooAssets.DefaultPcakageVersion);
        Debug.Log($"==========取出本地数据版本文件成功==={_lastPackageVersion}");
        if (!string.IsNullOrEmpty(_lastPackageVersion))
        {
            var cacheManifestHashPath = GetUnuseCachePathByBundleName(YooAssetSettingsData.GetPackageHashFileName(packageName, _lastPackageVersion));
            var cacheManifestPath = GetUnuseCachePathByBundleName(YooAssetSettingsData.GetManifestBinaryFileName(packageName, _lastPackageVersion));
            if(string.IsNullOrEmpty(cacheManifestHashPath) || string.IsNullOrEmpty(cacheManifestPath)) { return; }

            _packageHash = _fileSystem.ReadFileText(cacheManifestHashPath);
            _fileData = _fileSystem.ReadFileData(cacheManifestPath);
        }
    }

    private string GetUnuseCachePathByBundleName(string fileName)
    {
        var filePath = $"StreamingAssets/WebGL/{fileName}";
        return WX.GetCachePath(filePath);
    }
}
#endif