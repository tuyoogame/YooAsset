using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 拷贝内置文件操作
    /// </summary>
    internal sealed class CopyBuiltinFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckFileExist,
            TryCopyFile,
            UnpackFile,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly string _sourceFilePath;
        private readonly string _destFilePath;
        private IDownloadFileRequest _downloadFileRequest;
        private ESteps _steps = ESteps.None;

        internal CopyBuiltinFileOperation(BuiltinFileSystem fileSystem, string sourceFilePath, string destFilePath)
        {
            _fileSystem = fileSystem;
            _sourceFilePath = sourceFilePath;
            _destFilePath = destFilePath;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckFileExist;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckFileExist)
            {
                // 注意：只检查目标文件是否存在，不校验完整性。
                // 说明：文件校验由后续的缓存写入流程负责。
                if (File.Exists(_destFilePath))
                {
                    _steps = ESteps.Done;
                    SetResult();
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
                        FileUtility.EnsureParentDirectoryExists(_destFilePath);
                        File.Copy(_sourceFilePath, _destFilePath, true);
                        _steps = ESteps.Done;
                        SetResult();
                    }
                    catch (Exception ex)
                    {
                        YooLogger.LogWarning($"Failed to copy builtin file: {ex.Message}.");
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
                if (_downloadFileRequest == null)
                {
                    // TODO: 团结引擎，在某些安卓机型（红米），通过UnityWebRequest拷贝包内文件会小概率失败！需要借助其它方式来拷贝包内文件。
                    string url = DownloadUrlHelper.ToLocalFileUrl(_sourceFilePath);
                    var args = new DownloadFileRequestArgs(
                        url: url,
                        timeout: 60,
                        watchdogTimeout: 0,
                        savePath: _destFilePath);
                    _downloadFileRequest = _fileSystem.DownloadBackend.CreateFileRequest(args);
                    _downloadFileRequest.SendRequest();
                }

                if (_downloadFileRequest.IsDone == false)
                    return;

                if (_downloadFileRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadFileRequest.Error);
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadFileRequest != null)
            {
                _downloadFileRequest.Dispose();
                _downloadFileRequest = null;
            }
        }
        protected override void InternalWaitForCompletion()
        {
            //注意：等待解压本地文件完毕，该操作会挂起主线程！
            ExecuteUntilComplete();
        }
    }
}