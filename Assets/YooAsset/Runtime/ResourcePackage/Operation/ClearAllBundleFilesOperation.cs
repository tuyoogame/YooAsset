
namespace YooAsset
{
    /// <summary>
    /// 清理所有文件
    /// </summary>
    public abstract class ClearAllBundleFilesOperation : AsyncOperationBase
    {
    }
    internal sealed class ClearAllBundleFilesImplOperation : ClearAllBundleFilesOperation
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
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOpA;
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOpB;
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOpC;
        private ESteps _steps = ESteps.None;

        internal ClearAllBundleFilesImplOperation(IPlayMode impl, IFileSystem fileSystemA, IFileSystem fileSystemB, IFileSystem fileSystemC)
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

                if (_clearAllBundleFilesOpA == null)
                    _clearAllBundleFilesOpA = _fileSystemA.ClearAllBundleFilesAsync();

                Progress = _clearAllBundleFilesOpA.Progress;
                if (_clearAllBundleFilesOpA.IsDone == false)
                    return;

                if (_clearAllBundleFilesOpA.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemB;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOpA.Error;
                }
            }

            if (_steps == ESteps.ClearFileSystemB)
            {
                if (_fileSystemB == null)
                {
                    _steps = ESteps.ClearFileSystemC;
                    return;
                }

                if (_clearAllBundleFilesOpB == null)
                    _clearAllBundleFilesOpB = _fileSystemB.ClearAllBundleFilesAsync();

                Progress = _clearAllBundleFilesOpB.Progress;
                if (_clearAllBundleFilesOpB.IsDone == false)
                    return;

                if (_clearAllBundleFilesOpB.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearFileSystemC;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOpB.Error;
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

                if (_clearAllBundleFilesOpC == null)
                    _clearAllBundleFilesOpC = _fileSystemC.ClearAllBundleFilesAsync();

                Progress = _clearAllBundleFilesOpC.Progress;
                if (_clearAllBundleFilesOpC.IsDone == false)
                    return;

                if (_clearAllBundleFilesOpC.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOpC.Error;
                }
            }
        }
    }
}