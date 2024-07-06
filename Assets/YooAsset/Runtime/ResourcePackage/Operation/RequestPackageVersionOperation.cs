
namespace YooAsset
{
    /// <summary>
    /// 查询远端包裹的最新版本
    /// </summary>
    public abstract class RequestPackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前最新的包裹版本
        /// </summary>
        public string PackageVersion { protected set; get; }
    }
    internal sealed class RequestPackageVersionImplOperation : RequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly IFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private FSRequestPackageVersionOperation _requestPackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal RequestPackageVersionImplOperation(IFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_requestPackageVersionOp == null)
                    _requestPackageVersionOp = _fileSystem.RequestPackageVersionAsync(_appendTimeTicks, _timeout);

                if (_requestPackageVersionOp.IsDone == false)
                    return;

                if (_requestPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _requestPackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestPackageVersionOp.Error;
                }
            }
        }
    }
}