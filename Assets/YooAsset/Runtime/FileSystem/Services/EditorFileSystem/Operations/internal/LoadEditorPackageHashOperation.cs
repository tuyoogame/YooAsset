using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 加载编辑器包裹哈希文件操作
    /// </summary>
    internal sealed class LoadEditorPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadHash,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private readonly string _packageVersion;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; private set; }


        internal LoadEditorPackageHashOperation(EditorFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadHash)
            {
                string hashFilePath = _fileSystem.GetEditorPackageHashFilePath(_packageVersion);
                if (File.Exists(hashFilePath))
                {
                    PackageHash = FileUtility.ReadAllText(hashFilePath);
                    if (TextUtility.ValidateContent(PackageHash, out string validateError) == false)
                    {
                        _steps = ESteps.Done;
                        SetError($"Simulation package hash file validation failed: {validateError}.");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetResult();
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Could not find simulation package hash file: '{hashFilePath}'.");
                }
            }
        }
    }
}