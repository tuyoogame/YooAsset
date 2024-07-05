
namespace YooAsset
{
    internal class DWFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            GetPackageVersion,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private DefaultGetRemotePackageVersionOperation _getRemotePackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal DWFSRequestPackageVersionOperation(DefaultWebFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.GetPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetPackageVersion)
            {
                if (_getRemotePackageVersionOp == null)
                {
                    string packageName = _fileSystem.PackageName;
                    string fileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
                    string mainURL;
                    string fallbackURL;
                    if (_fileSystem.AllowCrossAccess)
                    {
                        mainURL = _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
                        fallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);
                    }
                    else
                    {
                        string filePath = _fileSystem.GetWebPackageVersionFilePath();
                        mainURL = DownloadSystemHelper.ConvertToWWWPath(filePath);
                        fallbackURL = mainURL;
                    }
                    _getRemotePackageVersionOp = new DefaultGetRemotePackageVersionOperation(packageName, mainURL, fallbackURL, _appendTimeTicks, _timeout);
                    OperationSystem.StartOperation(packageName, _getRemotePackageVersionOp);
                }

                Progress = _getRemotePackageVersionOp.Progress;
                if (_getRemotePackageVersionOp.IsDone == false)
                    return;

                if (_getRemotePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _getRemotePackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _getRemotePackageVersionOp.Error;
                }
            }
        }
    }
}