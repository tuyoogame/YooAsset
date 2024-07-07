using UnityEngine;

namespace YooAsset
{
    public sealed class UnloadUnusedAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            UnloadUnused,
            Done,
        }

        private readonly ResourceManager _resManager;
        private ESteps _steps = ESteps.None;

        internal UnloadUnusedAssetsOperation(ResourceManager resourceManager)
        {
            _resManager = resourceManager;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.UnloadUnused;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.UnloadUnused)
            {
                var loaderList = _resManager._loaderList;
                var loaderDic = _resManager._loaderDic;

                for (int i = loaderList.Count - 1; i >= 0; i--)
                {
                    BundleFileLoader loader = loaderList[i];
                    loader.TryDestroyProviders();
                }

                for (int i = loaderList.Count - 1; i >= 0; i--)
                {
                    BundleFileLoader loader = loaderList[i];
                    if (loader.CanDestroy())
                    {
                        string bundleName = loader.MainBundleInfo.Bundle.BundleName;
                        loader.Destroy();
                        loaderList.RemoveAt(i);
                        loaderDic.Remove(bundleName);
                    }
                }

                // 注意：调用底层接口释放所有资源
                Resources.UnloadUnusedAssets();

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
        public override void WaitForAsyncComplete()
        {
            InternalOnUpdate();
            DebugCheckWaitForAsyncComplete();
        }
    }
}