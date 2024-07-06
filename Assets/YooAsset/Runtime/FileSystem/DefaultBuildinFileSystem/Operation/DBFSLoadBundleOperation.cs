using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 加载AssetBundle文件
    /// </summary>
    internal class DBFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBuidlinAssetBundle,
            CheckLoadBuildinResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private AssetBundleCreateRequest _createRequest;
        private bool _isWaitForAsyncComplete = false;
        private ESteps _steps = ESteps.None;


        internal DBFSLoadAssetBundleOperation(DefaultBuildinFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadBuidlinAssetBundle;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuidlinAssetBundle)
            {
                string filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                if (_isWaitForAsyncComplete)
                {
                    Result = AssetBundle.LoadFromFile(filePath);
                }
                else
                {
                    _createRequest = AssetBundle.LoadFromFileAsync(filePath);
                }
                _steps = ESteps.CheckLoadBuildinResult;
            }

            if (_steps == ESteps.CheckLoadBuildinResult)
            {
                if (_createRequest != null)
                {
                    if (_isWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity bundle.");
                        Result = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        Result = _createRequest.assetBundle;
                    }
                }

                if (Result != null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load buildin asset bundle file : {_bundle.BundleName}";
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            while (true)
            {
                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }
        public override void AbortDownloadOperation()
        {
        }
    }

    /// <summary>
    /// 加载原生文件
    /// </summary>
    internal class DBFSLoadRawBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBuildinRawBundle,
            CheckLoadBuildinResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private ESteps _steps = ESteps.None;


        internal DBFSLoadRawBundleOperation(DefaultBuildinFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadBuildinRawBundle;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuildinRawBundle)
            {
                string filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                Result = filePath;
                _steps = ESteps.CheckLoadBuildinResult;
            }

            if (_steps == ESteps.CheckLoadBuildinResult)
            {
                if (Result != null)
                {
                    string filePath = Result as string;
                    if (File.Exists(filePath))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Can not found buildin raw bundle file : {filePath}";
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load buildin raw bundle file : {_bundle.BundleName}";
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            while (true)
            {
                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }
        public override void AbortDownloadOperation()
        {
        }
    }
}