
namespace YooAsset
{
    /// <summary>
    /// 确保资源包已就绪的异步操作
    /// </summary>
    public sealed class EnsureBundleFileOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源包文件详情
        /// </summary>
        public readonly struct BundleDetail
        {
            /// <summary>
            /// 资源包名称
            /// </summary>
            public readonly string BundleName;

            /// <summary>
            /// 资源包文件的本地路径
            /// </summary>
            public readonly string BundleFilePath;

            /// <summary>
            /// 资源包类型
            /// </summary>
            public readonly int BundleType;

            /// <summary>
            /// 文件是否加密
            /// </summary>
            public readonly bool IsEncrypted;

            internal BundleDetail(string bundleName, string bundleFilePath, int bundleType, bool isEncrypted)
            {
                BundleName = bundleName;
                BundleFilePath = bundleFilePath;
                BundleType = bundleType;
                IsEncrypted = isEncrypted;
            }
        }

        private enum ESteps
        {
            None,
            Validate,
            EnsureFile,
            Done,
        }

        private readonly FileSystemHost _host;
        private readonly EnsureBundleFileOptions _options;
        private FSEnsurePackageBundleOperation _ensurePackageBundleOp;
        private BundleInfo _bundleInfo;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 资源包文件详情
        /// </summary>
        public BundleDetail Detail { get; private set; }


        internal EnsureBundleFileOperation(FileSystemHost host, EnsureBundleFileOptions options)
        {
            _host = host;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.Validate;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Validate)
            {
                AssetInfo assetInfo = _options.AssetInfo;
                if (assetInfo == null)
                {
                    _steps = ESteps.Done;
                    SetError("Failed to ensure bundle file. Error: AssetInfo is null.");
                    return;
                }

                if (assetInfo.IsValid == false)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to ensure bundle file. Error: {assetInfo.Error}");
                    return;
                }

                _steps = ESteps.EnsureFile;
            }

            if (_steps == ESteps.EnsureFile)
            {
                if (_ensurePackageBundleOp == null)
                {
                    _bundleInfo = _host.GetMainBundleInfo(_options.AssetInfo);
                    _ensurePackageBundleOp = _bundleInfo.CreateBundleEnsurer();
                    _ensurePackageBundleOp.StartOperation();
                    AddChildOperation(_ensurePackageBundleOp);
                }

                _ensurePackageBundleOp.UpdateOperation();
                Progress = _ensurePackageBundleOp.Progress;
                if (_ensurePackageBundleOp.IsDone == false)
                    return;

                if (_ensurePackageBundleOp.Status == EOperationStatus.Succeeded)
                {
                    var bundle = _bundleInfo.Bundle;
                    Detail = new BundleDetail(
                        bundleName: bundle.BundleName,
                        bundleFilePath: _ensurePackageBundleOp.BundleFilePath,
                        bundleType: bundle.GetBundleType(),
                        isEncrypted: bundle.IsEncrypted);

                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_ensurePackageBundleOp.Error);
                }
            }
        }
    }
}
