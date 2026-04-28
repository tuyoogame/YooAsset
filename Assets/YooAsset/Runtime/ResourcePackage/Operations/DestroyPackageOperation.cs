
namespace YooAsset
{
    /// <summary>
    /// 销毁资源包裹操作
    /// </summary>
    public sealed class DestroyPackageOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckInitStatus,
            UnloadAllAssets,
            DestroyPackage,
            Done,
        }

        private readonly ResourcePackage _resourcePackage;
        private readonly ResourceManager _resourceManager;
        private readonly UnloadAllAssetsOptions _options;
        private UnloadAllAssetsOperation _unloadAllAssetsOp;
        private string _packageVersion = string.Empty;
        private ESteps _steps = ESteps.None;


        internal DestroyPackageOperation(ResourcePackage resourcePackage, ResourceManager resourceManager, UnloadAllAssetsOptions options)
        {
            _resourcePackage = resourcePackage;
            _resourceManager = resourceManager;
            _options = options;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.CheckInitStatus;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckInitStatus)
            {
                switch (_resourcePackage.InitializeStatus)
                {
                    case EOperationStatus.None:
                    case EOperationStatus.Failed:
                        _steps = ESteps.DestroyPackage;
                        break;

                    case EOperationStatus.Processing:
                        _steps = ESteps.Done;
                        SetError("Package is initializing. Please try to destroy it again later.");
                        break;

                    case EOperationStatus.Succeeded:
                        if (_resourcePackage.PackageValid)
                        {
                            _packageVersion = _resourcePackage.GetPackageVersion();
                            _steps = ESteps.UnloadAllAssets;
                        }
                        else
                        {
                            _steps = ESteps.DestroyPackage;
                        }
                        break;

                    default:
                        throw new System.NotImplementedException($"Initialize status is not implemented: {_resourcePackage.InitializeStatus}.");
                }
            }

            if (_steps == ESteps.UnloadAllAssets)
            {
                if (_unloadAllAssetsOp == null)
                {
                    _unloadAllAssetsOp = new UnloadAllAssetsOperation(_resourceManager, _options);
                    _unloadAllAssetsOp.StartOperation();
                    AddChildOperation(_unloadAllAssetsOp);
                }

                _unloadAllAssetsOp.UpdateOperation();
                if (_unloadAllAssetsOp.IsDone == false)
                    return;

                if (_unloadAllAssetsOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.DestroyPackage;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_unloadAllAssetsOp.Error);
                }
            }

            if (_steps == ESteps.DestroyPackage)
            {
                _resourcePackage.InternalDestroy();

                // 最后清理该包裹的异步任务
                // 注意：对于有线程操作的异步任务，需要保证线程安全释放。
                AsyncOperationSystem.ClearPackageOperations(_resourcePackage.PackageName);

                _steps = ESteps.Done;
                SetResult();
            }
        }
        /// <inheritdoc />
        protected override string InternalGetDescription()
        {
            return $"PackageVersion: {_packageVersion}";
        }
    }
}