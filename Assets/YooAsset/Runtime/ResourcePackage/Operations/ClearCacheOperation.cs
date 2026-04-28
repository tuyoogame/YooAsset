using System.Collections.Generic;
using System.Linq;

namespace YooAsset
{
    /// <summary>
    /// 清理缓存操作
    /// </summary>
    public sealed class ClearCacheOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Prepare,
            ClearCacheFiles,
            CheckClearResult,
            Done,
        }

        private readonly FileSystemHost _host;
        private readonly ClearCacheOptions _options;
        private List<IFileSystem> _cloneList;
        private FSClearCacheOperation _clearCacheFilesOp;
        private ESteps _steps = ESteps.None;

        internal ClearCacheOperation(FileSystemHost host, ClearCacheOptions options)
        {
            _host = host;
            _options = options;
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
                var fileSystems = _host.FileSystems;
                if (fileSystems == null || fileSystems.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetError("File system list is null or empty.");
                    return;
                }

                foreach (var fileSystem in fileSystems)
                {
                    if (fileSystem == null)
                    {
                        _steps = ESteps.Done;
                        SetError("List contains a null element.");
                        return;
                    }
                }

                _cloneList = fileSystems.ToList();
                _steps = ESteps.ClearCacheFiles;
            }

            if (_steps == ESteps.ClearCacheFiles)
            {
                if (_cloneList.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    var fileSystem = _cloneList[0];
                    _cloneList.RemoveAt(0);

                    _clearCacheFilesOp = fileSystem.ClearCacheAsync(_options.ConvertTo(_host.ActiveManifest));
                    _clearCacheFilesOp.StartOperation();
                    AddChildOperation(_clearCacheFilesOp);
                    _steps = ESteps.CheckClearResult;
                }
            }

            if (_steps == ESteps.CheckClearResult)
            {
                _clearCacheFilesOp.UpdateOperation();
                Progress = _clearCacheFilesOp.Progress;
                if (_clearCacheFilesOp.IsDone == false)
                    return;

                if (_clearCacheFilesOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.ClearCacheFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_clearCacheFilesOp.Error);
                }
            }
        }
        /// <inheritdoc />
        protected override string InternalGetDescription()
        {
            return $"ClearMethod: {_options.ClearMethod}";
        }
    }
}