
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存加载资源包操作的基类
    /// </summary>
    internal abstract class EBCLoadBundleBaseOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckCache,
            LoadBundle,
            Done,
        }

        protected readonly EditorBundleCache _fileCache;
        protected readonly PackageBundle _bundle;
        private int _asyncSimulateFrame;
        private ESteps _steps = ESteps.None;

        protected EBCLoadBundleBaseOperation(EditorBundleCache fileCache, PackageBundle bundle)
        {
            _fileCache = fileCache;
            _bundle = bundle;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckCache;
            _asyncSimulateFrame = GetAsyncSimulateFrame();
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckCache)
            {
                if (_fileCache.IsCached(_bundle.BundleGuid) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"File cache entry not found: '{_bundle.BundleGuid}'.");
                    return;
                }

                _steps = ESteps.LoadBundle;
            }

            if (_steps == ESteps.LoadBundle)
            {
                if (IsWaitForCompletion)
                {
                    if (_fileCache.Config.VirtualWebGLMode)
                    {
                        _steps = ESteps.Done;
                        SetError("WebGL mode only supports async load method.");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        CreateBundleHandle();
                    }
                }
                else
                {
                    _asyncSimulateFrame--;
                    if (_asyncSimulateFrame <= 0)
                    {
                        _steps = ESteps.Done;
                        CreateBundleHandle();
                    }
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        /// <summary>
        /// 由子类实现，创建具体的 BundleHandle。
        /// 成功时应调用 SetResult() 并赋值 BundleHandle；
        /// 失败时应调用 SetError()。
        /// </summary>
        protected abstract void CreateBundleHandle();

        private int GetAsyncSimulateFrame()
        {
            return UnityEngine.Random.Range(_fileCache.Config.AsyncSimulateMinFrame, _fileCache.Config.AsyncSimulateMaxFrame + 1);
        }
    }
}
