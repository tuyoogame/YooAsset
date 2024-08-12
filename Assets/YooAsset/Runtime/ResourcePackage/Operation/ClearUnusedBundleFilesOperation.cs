
namespace YooAsset
{
    /// <summary>
    /// 清理未使用的文件
    /// </summary>
    public abstract class ClearUnusedBundleFilesOperation : AsyncOperationBase
    {
    }
    internal sealed class ClearUnusedBundleFilesImplOperation : ClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearFileSystemA,
            ClearFileSystemB,
            ClearFileSystemC,
            Done,
        }

        private readonly IPlayMode _impl;
        private readonly IFileSystem _fileSystemA;
        private readonly IFileSystem _fileSystemB;
        private readonly IFileSystem _fileSystemC;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOpA;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOpB;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOpC;
        private ESteps _steps = ESteps.None;

        internal ClearUnusedBundleFilesImplOperation(IPlayMode impl, IFileSystem fileSystemA, IFileSystem fileSystemB, IFileSystem fileSystemC)
        {
            _impl = impl;
            _fileSystemA = fileSystemA;
            _fileSystemB = fileSystemB;
            _fileSystemC = fileSystemC;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearFileSystemA;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearFileSystemA)
            {
                if (_fileSystemA == null)
                {
                    _steps = ESteps.ClearFileSystemB;
                    return;
                }

                if (_clearUnusedBundleFilesOpA == null)
                    _clearUnusedBundleFilesOpA = _fileSystemA.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);

                Progress = _clearUnusedBundleFilesOpA.Progress;
                if (_clearUnusedBundleFilesOpA.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOpA.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemB;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOpA.Error;
                }
            }

            if (_steps == ESteps.ClearFileSystemB)
            {
                if (_fileSystemB == null)
                {
                    _steps = ESteps.ClearFileSystemC;
                    return;
                }

                if (_clearUnusedBundleFilesOpB == null)
                    _clearUnusedBundleFilesOpB = _fileSystemB.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);

                Progress = _clearUnusedBundleFilesOpB.Progress;
                if (_clearUnusedBundleFilesOpB.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOpB.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemC;

                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOpB.Error;
                }
            }

            if (_steps == ESteps.ClearFileSystemC)
            {
                if (_fileSystemC == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    return;
                }

                if (_clearUnusedBundleFilesOpC == null)
                    _clearUnusedBundleFilesOpC = _fileSystemC.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);

                Progress = _clearUnusedBundleFilesOpC.Progress;
                if (_clearUnusedBundleFilesOpC.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOpC.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOpC.Error;
                }
            }
        }
    }
}
