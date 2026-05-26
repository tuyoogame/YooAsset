
namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的查询包裹版本操作
    /// </summary>
    internal sealed class WNFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private RequestWebPackageVersionOperation _requestWebPackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal WNFSRequestPackageVersionOperation(WebNetworkFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_requestWebPackageVersionOp == null)
                {
                    var options = new RequestWebPackageVersionOptions(
                        packageName: _fileSystem.PackageName,
                        appendTimeTicks: _appendTimeTicks,
                        timeout: _timeout,
                        remoteService: _fileSystem.RemoteService,
                        downloadBackend: _fileSystem.DownloadBackend,
                        downloadUrlPolicy: _fileSystem.DownloadUrlPolicy);
                    _requestWebPackageVersionOp = new RequestWebPackageVersionOperation(options);
                    _requestWebPackageVersionOp.StartOperation();
                    AddChildOperation(_requestWebPackageVersionOp);
                }

                _requestWebPackageVersionOp.UpdateOperation();
                Progress = _requestWebPackageVersionOp.Progress;
                if (_requestWebPackageVersionOp.IsDone == false)
                    return;

                if (_requestWebPackageVersionOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _requestWebPackageVersionOp.PackageVersion;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestWebPackageVersionOp.Error);
                }
            }
        }
    }
}
