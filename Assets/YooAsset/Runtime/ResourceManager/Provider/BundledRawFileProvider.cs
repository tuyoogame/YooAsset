
namespace YooAsset
{
    internal class BundledRawFileProvider : ProviderBase
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
                if (IsWaitForAsyncComplete)
                    FileLoader.WaitForAsyncComplete();

                if (FileLoader.IsDone() == false)
                    return;

                if (FileLoader.Status != BundleFileLoader.EStatus.Succeed)
                {
                    string error = FileLoader.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (FileLoader.Result is string == false)
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
                RawFilePath = FileLoader.Result as string;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }
    }
}