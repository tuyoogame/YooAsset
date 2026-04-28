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
    private IDownloadTextRequest _downloadTextRequest;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 内置资源清单版本
    /// </summary>
    public string PackageVersion { private set; get; }

    /// <summary>
    /// 创建内置资源清单版本查询操作实例
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    public GetBuildinPackageVersionOperation(string packageName)
    {
        if (string.IsNullOrEmpty(packageName))
            throw new System.ArgumentNullException(nameof(packageName));

        _packageName = packageName;
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
                _downloadTextRequest = new UnityWebRequestText(args, null);
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
    protected override void InternalDispose()
    {
        if (_downloadTextRequest != null)
        {
            _downloadTextRequest.Dispose();
            _downloadTextRequest = null;
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