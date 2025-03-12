using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal sealed class LoadBuildinCatalogFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadCatalog,
            WaitForRequest,
            Done,
        }

        private UnityWebRequest _request;
        private readonly DefaultBuildinFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;


        internal LoadBuildinCatalogFileOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.LoadCatalog;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            string catalogFilePath = _fileSystem.GetCatalogFileLoadPath();

            if (_steps == ESteps.LoadCatalog)
            {
                _request = UnityWebRequest.Get(catalogFilePath);
                _request.SendWebRequest();
                _steps = ESteps.WaitForRequest;
                return;
            }

            if (_steps == ESteps.WaitForRequest)
            {
                // 等待请求完成
                if (!_request.isDone)
                    return;

                if (_request.result != UnityWebRequest.Result.Success)
                {
                    _request.Dispose();
                    _request = null;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load catalog file: {_request.error}";
                    return;
                }

                // 解析 JSON
                string jsonText = _request.downloadHandler.text;
                var catalog = JsonUtility.FromJson<DefaultBuildinFileCatalog>(jsonText);
                if (catalog == null)
                {
                    _request.Dispose();
                    _request = null;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load catalog file : {catalogFilePath}";
                    return;
                }

                if (catalog.PackageName != _fileSystem.PackageName)
                {
                    _request.Dispose();
                    _request = null;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Catalog file package name {catalog.PackageName} cannot match the file system package name {_fileSystem.PackageName}";
                    return;
                }

                foreach (var wrapper in catalog.Wrappers)
                {
                    var fileWrapper = new DefaultBuildinFileSystem.FileWrapper(wrapper.FileName);
                    _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
                }

                YooLogger.Log($"Package '{_fileSystem.PackageName}' buildin catalog files count : {catalog.Wrappers.Count}");
                _request.Dispose();
                _request = null;
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}