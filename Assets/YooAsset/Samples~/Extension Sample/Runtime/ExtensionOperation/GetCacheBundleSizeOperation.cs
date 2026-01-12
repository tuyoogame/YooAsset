using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YooAsset;

/// <summary>
/// 获取沙盒目录里缓存文件大小
/// </summary>
public class GetCacheBundleSizeOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        GetCacheFiles,
        Done,
    }

    private readonly string _packageName;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 总大小（单位：字节）
    /// </summary>
    public long TotalSize = 0;


    public GetCacheBundleSizeOperation(string packageName)
    {
        _packageName = packageName;
    }
    protected override void InternalStart()
    {
        _steps = ESteps.GetCacheFiles;
    }
    protected override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetCacheFiles)
        {
            long totalSize = 0;
            string directoryRoot = GetCacheDirectoryRoot();
            var directoryInfo = new DirectoryInfo(directoryRoot);
            if (directoryInfo.Exists)
            {
                FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo fileInfo in fileInfos)
                {
                    totalSize += fileInfo.Length;
                }
            }

            TotalSize = totalSize;
            _steps = ESteps.Done;
            SetResult();
        }
    }

    private string GetCacheDirectoryRoot()
    {
        string rootDirectory = YooAssetConfiguration.GetDefaultCacheRoot();
        string packageRoot = PathUtility.Combine(rootDirectory, _packageName);
        return PathUtility.Combine(packageRoot, SandboxFileSystemConsts.BundleFilesFolderName);
    }
}