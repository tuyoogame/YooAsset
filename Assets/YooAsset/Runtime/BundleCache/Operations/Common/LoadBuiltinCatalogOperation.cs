using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 加载内置资源目录操作
    /// </summary>
    internal sealed class LoadBuiltinCatalogOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadFileData,
            RequestFileData,
            LoadCatalog,
            CheckResult,
            Done,
        }

        private readonly LoadBuiltinCatalogOptions _options;
        private IDownloadBytesRequest _downloadBytesRequest;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 加载完成的内置资源目录
        /// </summary>
        public BuiltinCatalog Catalog;

        /// <summary>
        /// 创建加载内置资源目录操作实例
        /// </summary>
        /// <param name="options">加载内置资源目录的操作选项</param>
        internal LoadBuiltinCatalogOperation(LoadBuiltinCatalogOptions options)
        {
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.TryLoadFileData;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadFileData)
            {
                if (File.Exists(_options.FilePath))
                {
                    try
                    {
                        _fileData = File.ReadAllBytes(_options.FilePath);
                        _steps = ESteps.LoadCatalog;
                    }
                    catch (Exception ex)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to read file: {ex.Message}.");
                    }
                }
                else
                {
                    _steps = ESteps.RequestFileData;
                }
            }

            if (_steps == ESteps.RequestFileData)
            {
                // 注意：从安装包体里解压数据
                // 注意：从Web服务器下载数据
                if (_downloadBytesRequest == null)
                {
                    string url = DownloadUrlHelper.ToLocalFileUrl(_options.FilePath);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: 60,
                        watchdogTimeout: 0);
                    _downloadBytesRequest = _options.DownloadBackend.CreateBytesRequest(args);
                    _downloadBytesRequest.SendRequest();
                }

                if (_downloadBytesRequest.IsDone == false)
                    return;

                if (_downloadBytesRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _fileData = _downloadBytesRequest.Result;
                    _steps = ESteps.LoadCatalog;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadBytesRequest.Error);
                }
            }

            if (_steps == ESteps.LoadCatalog)
            {
                try
                {
                    Catalog = BuiltinCatalogHelper.DeserializeFromBinary(_fileData);
                    _steps = ESteps.CheckResult;
                }
                catch (Exception ex)
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to load catalog: {ex.Message}.");
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (Catalog.PackageName == _options.PackageName)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Catalog package name '{Catalog.PackageName}' does not match file cache package name '{_options.PackageName}'.");
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadBytesRequest != null)
            {
                _downloadBytesRequest.Dispose();
                _downloadBytesRequest = null;
            }
        }
    }
}