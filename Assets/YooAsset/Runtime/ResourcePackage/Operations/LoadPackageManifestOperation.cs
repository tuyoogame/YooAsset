
namespace YooAsset
{
    /// <summary>
    /// 加载资源清单操作
    /// </summary>
    public sealed class LoadPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadPackageManifest,
            Done,
        }

        private readonly FileSystemHost _host;
        private readonly LoadPackageManifestOptions _options;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;

        internal LoadPackageManifestOperation(FileSystemHost host, LoadPackageManifestOptions options)
        {
            _host = host;
            _options = options;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.CheckParams;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_options.PackageVersion))
                {
                    _steps = ESteps.Done;
                    SetError("Package version is null or empty.");
                }
                else
                {
                    _steps = ESteps.CheckActiveManifest;
                }
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象	
                if (_host.ActiveManifest != null && _host.ActiveManifest.PackageVersion == _options.PackageVersion)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.LoadPackageManifest;
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                {
                    var mainFileSystem = _host.GetMainFileSystem();
                    if (mainFileSystem == null)
                    {
                        _steps = ESteps.Done;
                        SetError("Main file system is null.");
                        return;
                    }

                    _loadPackageManifestOp = mainFileSystem.LoadPackageManifestAsync(_options.ConvertTo());
                    _loadPackageManifestOp.StartOperation();
                    AddChildOperation(_loadPackageManifestOp);
                }

                _loadPackageManifestOp.UpdateOperation();
                Progress = _loadPackageManifestOp.Progress;
                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    _host.SetActiveManifest(_loadPackageManifestOp.Manifest);
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadPackageManifestOp.Error);
                }
            }
        }
        /// <inheritdoc />
        protected override string InternalGetDescription()
        {
            return $"PackageVersion: {_options.PackageVersion}";
        }
    }
}