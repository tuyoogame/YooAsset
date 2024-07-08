
namespace YooAsset
{
    internal class BundledRawFileProvider : ProviderOperation
    {
        public BundledRawFileProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                    return;

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                if (LoadBundleFileOp.Result is RawBundle == false)
                {
                    string error = "Try load AssetBundle file using load raw file method !";
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Checking;
            }

            // 2. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                RawBundleObject = LoadBundleFileOp.Result as RawBundle;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }
    }
}