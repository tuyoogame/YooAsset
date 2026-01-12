using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 加载缓存包裹哈希文件操作
    /// </summary>
    internal sealed class LoadCachePackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadPackageHash,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly string _packageVersion;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; private set; }


        internal LoadCachePackageHashOperation(SandboxFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadPackageHash)
            {
                string filePath = _fileSystem.GetCachePackageHashFilePath(_packageVersion);
                if (File.Exists(filePath) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"Could not find cache package hash file: '{filePath}'.");
                    return;
                }

                PackageHash = FileUtility.ReadAllText(filePath);
                if (TextUtility.ValidateContent(PackageHash, out string validateError) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"Cache package hash file validation failed: {validateError}.");
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }
        }
    }
}