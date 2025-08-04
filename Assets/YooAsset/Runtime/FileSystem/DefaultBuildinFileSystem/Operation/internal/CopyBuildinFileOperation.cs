using System;
using System.IO;

namespace YooAsset
{
    internal class CopyBuildinFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckFileExist,
            TryCopyFile,
            UnpackFile,
            Done,
        }

        private UnityWebFileRequestOperation _webFileRequestOp;
        private readonly string _sourceFilePath;
        private readonly string _destFilePath;
        private ESteps _steps = ESteps.None;

        public CopyBuildinFileOperation(string sourceFilePath, string destFilePath)
        {
            _sourceFilePath = sourceFilePath;
            _destFilePath = destFilePath;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckFileExist;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckFileExist)
            {
                if (File.Exists(_destFilePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.TryCopyFile;
                }
            }

            if (_steps == ESteps.TryCopyFile)
            {
                if (File.Exists(_sourceFilePath))
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(_destFilePath);
                        if (Directory.Exists(directory) == false)
                            Directory.CreateDirectory(directory);
                        File.Copy(_sourceFilePath, _destFilePath, true);
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    catch (Exception ex)
                    {
                        YooLogger.Warning($"Failed copy buildin file : {ex.Message}");
                        _steps = ESteps.UnpackFile;
                    }
                }
                else
                {
                    _steps = ESteps.UnpackFile;
                }
            }

            if (_steps == ESteps.UnpackFile)
            {
                if (_webFileRequestOp == null)
                {
                    string url = DownloadSystemHelper.ConvertToWWWPath(_sourceFilePath);
                    _webFileRequestOp = new UnityWebFileRequestOperation(url, _destFilePath, 60);
                    _webFileRequestOp.StartOperation();
                    AddChildOperation(_webFileRequestOp);
                }

                _webFileRequestOp.UpdateOperation();
                if (_webFileRequestOp.IsDone == false)
                    return;

                if (_webFileRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webFileRequestOp.Error;
                }
            }
        }
    }
}