using System.Collections.Generic;
using System.Linq;

namespace YooAsset
{
    /// <summary>
    /// 初始化文件系统操作
    /// </summary>
    public sealed class InitializeFileSystemOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Prepare,
            InitFileSystem,
            CheckInitResult,
            Done,
        }

        private readonly FileSystemHost _host;
        private readonly List<FileSystemParameters> _parametersList;
        private List<FileSystemParameters> _cloneList;
        private FSInitializeOperation _initFileSystemOp;
        private ESteps _steps = ESteps.None;
        
        internal InitializeFileSystemOperation(FileSystemHost host, List<FileSystemParameters> parametersList)
        {
            _host = host;
            _parametersList = parametersList;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.Prepare;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Prepare)
            {
                if (_parametersList == null || _parametersList.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetError("File system parameters list is null or empty.");
                    return;
                }

                foreach (var fileSystemParam in _parametersList)
                {
                    if (fileSystemParam == null)
                    {
                        _steps = ESteps.Done;
                        SetError("List contains a null element.");
                        return;
                    }
                }

                _cloneList = _parametersList.ToList();
                _steps = ESteps.InitFileSystem;
            }

            if (_steps == ESteps.InitFileSystem)
            {
                if (_cloneList.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    var fileSystemParams = _cloneList[0];
                    _cloneList.RemoveAt(0);

                    IFileSystem fileSystemInstance = fileSystemParams.CreateFileSystem(_host.PackageName);
                    if (fileSystemInstance == null)
                    {
                        _steps = ESteps.Done;
                        SetError("Failed to create file system instance.");
                        return;
                    }

                    _host.AddFileSystem(fileSystemInstance);
                    _initFileSystemOp = fileSystemInstance.InitializeAsync();
                    _initFileSystemOp.StartOperation();
                    AddChildOperation(_initFileSystemOp);
                    _steps = ESteps.CheckInitResult;
                }
            }

            if (_steps == ESteps.CheckInitResult)
            {
                _initFileSystemOp.UpdateOperation();
                Progress = _initFileSystemOp.Progress;
                if (_initFileSystemOp.IsDone == false)
                    return;

                if (_initFileSystemOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.InitFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_initFileSystemOp.Error);
                    return;
                }
            }
        }
    }
}