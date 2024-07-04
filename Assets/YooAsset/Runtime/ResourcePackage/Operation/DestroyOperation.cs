
namespace YooAsset
{
    public class DestroyOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            UnloadAllAssets,
            DestroyPackage,
            Done,
        }

        private readonly ResourcePackage _resourcePackage;
        private UnloadAllAssetsOperation _unloadAllAssetsOp;
        private ESteps _steps = ESteps.None;


        public DestroyOperation(ResourcePackage resourcePackage)
        {
            _resourcePackage = resourcePackage;
        }

        internal override void InternalOnStart()
        {
            _steps = ESteps.UnloadAllAssets;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.UnloadAllAssets)
            {
                if (_unloadAllAssetsOp == null)
                    _unloadAllAssetsOp = _resourcePackage.UnloadAllAssetsAsync();

                if (_unloadAllAssetsOp.IsDone == false)
                    return;

                if (_unloadAllAssetsOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.DestroyPackage;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _unloadAllAssetsOp.Error;
                }
            }

            if (_steps == ESteps.DestroyPackage)
            {
                // 销毁包裹
                _resourcePackage.DestroyPackage();

                // 最后清理该包裹的异步任务
                // 注意：对于有线程操作的异步任务，需要保证线程安全释放。
                OperationSystem.ClearPackageOperation(_resourcePackage.PackageName);

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}