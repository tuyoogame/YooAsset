using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class LoadWebCatalogFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadCatalog,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 内置清单版本
        /// </summary>
        public string PackageVersion { private set; get; }

        internal LoadWebCatalogFileOperation(DefaultWebFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadCatalog;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadCatalog)
            {
                string catalogFilePath = _fileSystem.GetCatalogFileLoadPath();
                var catalog = Resources.Load<DefaultBuildinFileCatalog>(catalogFilePath);
                if (catalog == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load web catalog file : {catalogFilePath}";
                    return;
                }

                if (catalog.PackageName != _fileSystem.PackageName)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Web catalog file package name {catalog.PackageName} cannot match the file system package name {_fileSystem.PackageName}";
                    return;
                }

                PackageVersion = catalog.PackageVersion;
                foreach (var wrapper in catalog.Wrappers)
                {
                    var fileWrapper = new DefaultWebFileSystem.FileWrapper(wrapper.FileName);
                    _fileSystem.RecordFile(wrapper.BundleGUID, fileWrapper);
                }

                YooLogger.Log($"Package '{_fileSystem.PackageName}' catalog files count : {catalog.Wrappers.Count}");
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}