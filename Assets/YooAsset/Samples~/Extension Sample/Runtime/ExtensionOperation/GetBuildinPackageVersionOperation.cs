using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YooAsset;

/// <summary>
/// 获取包体里的内置资源清单版本
/// </summary>
public class GetBuildinPackageVersionOperation : GameAsyncOperation
{
    private enum ESteps
    {
        None,
        GetPackageVersion,
        Done,
    }

    private readonly string _packageName;
    private readonly IDownloadBackend _backend;
    private IDownloadTextRequest _versionFileRequestOp;
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
    protected override void OnStart()
    {
        _steps = ESteps.GetPackageVersion;
    }
    protected override void OnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.GetPackageVersion)
        {
            if (_versionFileRequestOp == null)
            {
                string filePath = GetBuildinPackageVersionFilePath();
                string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                var args = new DownloadDataRequestArgs(url, 60, 0);
                _versionFileRequestOp = _backend.CreateTextRequest(args);
                _versionFileRequestOp.SendRequest();
            }

            if (_versionFileRequestOp.IsDone == false)
                return;

            if (_versionFileRequestOp.Status == EDownloadRequestStatus.Succeed)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
                PackageVersion = _versionFileRequestOp.Result;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _versionFileRequestOp.Error;
            }
        }
    }
    protected override void OnAbort()
    {
    }

    private string GetBuildinYooRoot()
    {
        return YooAssetSettingsData.GetYooDefaultBuildinRoot();
    }
    private string GetBuildinPackageVersionFilePath()
    {
        string fileRoot = GetBuildinYooRoot();
        string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
        return PathUtility.Combine(fileRoot, _packageName, fileName);
    }
}