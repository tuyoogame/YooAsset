using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YooAsset;

/// <summary>
/// 获取包体里的内置资源清单版本
/// </summary>
public class GetBuildinPackageVersionOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        GetPackageVersion,
        Done,
    }

    private readonly string _packageName;
    private readonly IDownloadBackend _backend;
    private IDownloadTextRequest _downloadTextRequest;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 内置资源清单版本
    /// </summary>
    public string PackageVersion { private set; get; }

    public GetBuildinPackageVersionOperation(string packageName)
    {
        _packageName = packageName;
        _backend = new UnityWebRequestBackend();
    }
    protected override void InternalStart()
    {
        _steps = ESteps.GetPackageVersion;
    }
    protected override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetPackageVersion)
        {
            if (_downloadTextRequest == null)
            {
                string filePath = GetBuildinPackageVersionFilePath();
                string url = DownloadUrlHelper.ToLocalFileUrl(filePath);
                var args = new DownloadDataRequestArgs(url, 60, 0);
                _downloadTextRequest = _backend.CreateTextRequest(args);
                _downloadTextRequest.SendRequest();
            }

            if (_downloadTextRequest.IsDone == false)
                return;

            if (_downloadTextRequest.Status == EDownloadRequestStatus.Succeeded)
            {
                _steps = ESteps.Done;
                SetResult();
                PackageVersion = _downloadTextRequest.Result;
            }
            else
            {
                _steps = ESteps.Done;
                SetError(_downloadTextRequest.Error);
            }
        }
    }

    private string GetBuildinYooRoot()
    {
        return YooAssetConfiguration.GetDefaultBuiltinRoot();
    }
    private string GetBuildinPackageVersionFilePath()
    {
        string fileRoot = GetBuildinYooRoot();
        string fileName = YooAssetConfiguration.GetPackageVersionFileName(_packageName);
        return PathUtility.Combine(fileRoot, _packageName, fileName);
    }
}