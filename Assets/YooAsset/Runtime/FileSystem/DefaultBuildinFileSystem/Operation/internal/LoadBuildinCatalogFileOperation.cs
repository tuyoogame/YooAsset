using System;
using System.IO;

namespace YooAsset
{
    internal sealed class LoadBuildinCatalogFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadFileData,
            RequestFileData,
            LoadCatalog,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private UnityWebDataRequestOperation _webDataRequestOp;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 内置资源目录
        /// </summary>
        public DefaultBuildinFileCatalog Catalog;

        internal LoadBuildinCatalogFileOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.TryLoadFileData;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadFileData)
            {
                string filePath = _fileSystem.GetCatalogBinaryFileLoadPath();
                if (File.Exists(filePath))
                {
                    _fileData = File.ReadAllBytes(filePath);
                    _steps = ESteps.LoadCatalog;
                }
                else
                {
                    _steps = ESteps.RequestFileData;
                }
            }

            if (_steps == ESteps.RequestFileData)
            {
                if (_webDataRequestOp == null)
                {
                    string filePath = _fileSystem.GetCatalogBinaryFileLoadPath();
                    string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webDataRequestOp = new UnityWebDataRequestOperation(url, 60);
                    _webDataRequestOp.StartOperation();
                    AddChildOperation(_webDataRequestOp);
                }

                _webDataRequestOp.UpdateOperation();
                if (_webDataRequestOp.IsDone == false)
                    return;

                if (_webDataRequestOp.Status == EOperationStatus.Succeed)
                {
                    _fileData = _webDataRequestOp.Result;
                    _steps = ESteps.LoadCatalog;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webDataRequestOp.Error;
                }
            }

            if (_steps == ESteps.LoadCatalog)
            {
                try
                {
                    Catalog = CatalogTools.DeserializeFromBinary(_fileData);
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                catch (Exception e)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load catalog file : {e.Message}";
                }
            }
        }
    }
}