using System;
using System.IO;

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
            CheckFilePath,
            LoadBundle,
            Done,
        }

        protected readonly EditorBundleCache _fileCache;
        protected readonly PackageBundle _bundle;
        protected string _editorFilePath;
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

                _steps = ESteps.CheckFilePath;
            }

            if (_steps == ESteps.CheckFilePath)
            {
                _editorFilePath = EditorFileSystemHelper.GetEditorFilePath(_bundle);
                if (string.IsNullOrEmpty(_editorFilePath))
                {
                    _steps = ESteps.Done;
                    SetError($"Editor file path is null. Bundle: '{_bundle.BundleName}'.");
                }
                else
                {
                    _steps = ESteps.LoadBundle;
                }
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

    /// <summary>
    /// 编辑器文件缓存加载虚拟资源包操作
    /// </summary>
    internal sealed class EBCLoadVirtualBundleOperation : EBCLoadBundleBaseOperation
    {
        public EBCLoadVirtualBundleOperation(EditorBundleCache fileCache, PackageBundle bundle)
            : base(fileCache, bundle) { }

        protected override void CreateBundleHandle()
        {
            SetResult();
            BundleHandle = new VirtualBundleHandle(_editorFilePath, _bundle);
        }
    }

    /// <summary>
    /// 编辑器文件缓存加载原生资源包操作
    /// </summary>
    internal sealed class EBCLoadRawBundleOperation : EBCLoadBundleBaseOperation
    {
        public EBCLoadRawBundleOperation(EditorBundleCache fileCache, PackageBundle bundle)
            : base(fileCache, bundle) { }

        protected override void CreateBundleHandle()
        {
            try
            {
                byte[] data = File.ReadAllBytes(_editorFilePath);
                var rawBundle = new RawBundle(data);
                SetResult();
                BundleHandle = new RawBundleHandle(_editorFilePath, _bundle, rawBundle);
            }
            catch (Exception ex)
            {
                SetError($"Failed to load raw bundle: {ex.Message}.");
                YooLogger.LogError(Error);
            }
        }
    }
}
