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
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private AssetBundleCreateRequest _createRequest;
        private AssetBundle _assetBundle;
        private Stream _managedStream;
        private ESteps _steps = ESteps.None;


        internal DBFSLoadAssetBundleOperation(DefaultBuildinFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadAssetBundle;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadAssetBundle)
            {
                if (_bundle.Encrypted)
                {
                    if (_fileSystem.DecryptionServices == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The {nameof(IDecryptionServices)} is null !";
                        YooLogger.Error(Error);
                        return;
                    }
                }

                if (IsWaitForAsyncComplete)
                {
                    if (_bundle.Encrypted)
                    {
                        var decryptResult = _fileSystem.LoadEncryptedAssetBundle(_bundle);
                        _assetBundle = decryptResult.Result;
                        _managedStream = decryptResult.ManagedStream;
                    }
                    else
                    {
                        string filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                        _assetBundle = AssetBundle.LoadFromFile(filePath);
                    }
                }
                else
                {
                    if (_bundle.Encrypted)
                    {
                        var decryptResult = _fileSystem.LoadEncryptedAssetBundleAsync(_bundle);
                        _createRequest = decryptResult.CreateRequest;
                        _managedStream = decryptResult.ManagedStream;
                    }
                    else
                    {
                        string filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                        _createRequest = AssetBundle.LoadFromFileAsync(filePath);
                    }
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_createRequest != null)
                {
                    if (IsWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity bundle.");
                        _assetBundle = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        _assetBundle = _createRequest.assetBundle;
                    }
                }

                if (_assetBundle == null)
                {
                    if (_bundle.Encrypted)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed to load encrypted buildin asset bundle file : {_bundle.BundleName}";
                        YooLogger.Error(Error);
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed to load buildin asset bundle file : {_bundle.BundleName}";
                        YooLogger.Error(Error);
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Result = new AssetBundleResult(_fileSystem, _bundle, _assetBundle, _managedStream);
                    Status = EOperationStatus.Succeed;
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
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
        internal override void InternalStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadBuildinRawBundle;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuildinRawBundle)
            {
                string filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);

#if UNITY_ANDROID
                //TODO : 安卓平台内置文件属于APK压缩包内的文件。
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"Can not load android buildin raw bundle file : {filePath}";
                YooLogger.Error(Error);
#else
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Result = new RawBundleResult(_fileSystem, _bundle);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found buildin raw bundle file : {filePath}";
                    YooLogger.Error(Error);
                }
#endif
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
    }
}