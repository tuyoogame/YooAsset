using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 内置文件系统
    /// </summary>
    internal class DefaultBuildinFileSystem : IFileSystem
    {
        private class UnpackRemoteServices : IRemoteServices
        {
            private readonly string _buildinPackageRoot;
            protected readonly Dictionary<string, string> _mapping = new Dictionary<string, string>(10000);

            public UnpackRemoteServices(string buildinPackRoot)
            {
                _buildinPackageRoot = buildinPackRoot;
            }
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return GetFileLoadURL(fileName);
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return GetFileLoadURL(fileName);
            }

            private string GetFileLoadURL(string fileName)
            {
                if (_mapping.TryGetValue(fileName, out string url) == false)
                {
                    string filePath = PathUtility.Combine(_buildinPackageRoot, fileName);
                    url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _mapping.Add(fileName, url);
                }
                return url;
            }
        }

        public class FileWrapper
        {
            public string FileName { private set; get; }

            public FileWrapper(string fileName)
            {
                FileName = fileName;
            }
        }

        protected readonly Dictionary<string, FileWrapper> _wrappers = new Dictionary<string, FileWrapper>(10000);
        protected readonly Dictionary<string, Stream> _loadedStream = new Dictionary<string, Stream>(10000);
        protected readonly Dictionary<string, string> _buildinFilePaths = new Dictionary<string, string>(10000);
        protected IFileSystem _unpackFileSystem;
        protected string _packageRoot;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return _packageRoot;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return _wrappers.Count;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验级别
        /// </summary>
        public EFileVerifyLevel FileVerifyLevel { private set; get; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 自定义参数：数据文件追加文件格式
        /// </summary>
        public bool AppendFileExtension { private set; get; } = false;

        /// <summary>
        /// 自定义参数：原生文件构建管线
        /// </summary>
        public bool RawFileBuildPipeline { private set; get; } = false;

        /// <summary>
        ///  自定义参数：解密方法类
        /// </summary>
        public IDecryptionServices DecryptionServices { private set; get; }
        #endregion


        public DefaultBuildinFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DBFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DBFSLoadPackageManifestOperation(this, packageVersion);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DBFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            return _unpackFileSystem.ClearAllBundleFilesAsync();
        }
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
        {
            return _unpackFileSystem.ClearUnusedBundleFilesAsync(manifest);
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            param.ImportFilePath = GetBuildinFileLoadPath(bundle);
            return _unpackFileSystem.DownloadFileAsync(bundle, param);
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
            {
                return _unpackFileSystem.LoadBundleFile(bundle);
            }

            if (RawFileBuildPipeline)
            {
                var operation = new DBFSLoadRawBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                var operation = new DBFSLoadAssetBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
            AssetBundle assetBundle = result as AssetBundle;
            if (assetBundle == null)
                return;

            if (_unpackFileSystem.Exists(bundle))
            {
                _unpackFileSystem.UnloadBundleFile(bundle, assetBundle);
            }
            else
            {
                if (assetBundle != null)
                    assetBundle.Unload(true);

                if (_loadedStream.TryGetValue(bundle.BundleGUID, out Stream managedStream))
                {
                    if (managedStream != null)
                    {
                        managedStream.Close();
                        managedStream.Dispose();
                    }
                    _loadedStream.Remove(bundle.BundleGUID);
                }
            }
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.FILE_VERIFY_LEVEL)
            {
                FileVerifyLevel = (EFileVerifyLevel)value;
            }
            else if (name == FileSystemParametersDefine.APPEND_FILE_EXTENSION)
            {
                AppendFileExtension = (bool)value;
            }
            else if (name == FileSystemParametersDefine.RAW_FILE_BUILD_PIPELINE)
            {
                RawFileBuildPipeline = (bool)value;
            }
            else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
            {
                DecryptionServices = (IDecryptionServices)value;
            }
            else
            {
                YooLogger.Warning($"Invalid parameter : {name}");
            }
        }
        public virtual void OnCreate(string packageName, string rootDirectory)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(rootDirectory))
                rootDirectory = GetDefaultRoot();

            _packageRoot = PathUtility.Combine(rootDirectory, packageName);

            // 创建解压文件系统
            var remoteServices = new UnpackRemoteServices(_packageRoot);
            _unpackFileSystem = new DefaultUnpackFileSystem();
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.FILE_VERIFY_LEVEL, FileVerifyLevel);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, AppendFileExtension);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.RAW_FILE_BUILD_PIPELINE, RawFileBuildPipeline);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, DecryptionServices);
            _unpackFileSystem.OnCreate(packageName, null);
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            if (Belong(bundle) == false)
                return false;

#if UNITY_ANDROID
            return RawFileBuildPipeline || bundle.Encrypted;
#else
            return false;
#endif
        }
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual byte[] ReadFileData(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
                return _unpackFileSystem.ReadFileData(bundle);

            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo()
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileData(fileInfo);
            }
            else
            {
                string filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllBytes(filePath);
            }
        }
        public virtual string ReadFileText(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
                return _unpackFileSystem.ReadFileText(bundle);

            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo()
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileText(fileInfo);
            }
            else
            {
                string filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllText(filePath);
            }
        }

        #region 内部方法
        protected string GetDefaultRoot()
        {
            return PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
        }
        public string GetBuildinFileLoadPath(PackageBundle bundle)
        {
            if (_buildinFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_packageRoot, bundle.FileName);
                _buildinFilePaths.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetBuildinCatalogFileLoadPath()
        {
            string fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return PathUtility.Combine(YooAssetSettingsData.Setting.DefaultYooFolderName, PackageName, fileName);
        }
        public string GetBuildinPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetBuildinPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetBuildinPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetStreamingAssetsPackageRoot()
        {
            string rootPath = PathUtility.Combine(Application.dataPath, "StreamingAssets", YooAssetSettingsData.Setting.DefaultYooFolderName);
            return PathUtility.Combine(rootPath, PackageName);
        }

        /// <summary>
        /// 记录文件信息
        /// </summary>
        public bool RecordFile(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultBuildinFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }

        /// <summary>
        /// 初始化解压文件系统
        /// </summary>
        public FSInitializeFileSystemOperation InitializeUpackFileSystem()
        {
            return _unpackFileSystem.InitializeFileSystemAsync();
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        public AssetBundle LoadEncryptedAssetBundle(PackageBundle bundle)
        {
            string filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };

            var assetBundle = DecryptionServices.LoadAssetBundle(fileInfo, out var managedStream);
            _loadedStream.Add(bundle.BundleGUID, managedStream);
            return assetBundle;
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        public AssetBundleCreateRequest LoadEncryptedAssetBundleAsync(PackageBundle bundle)
        {
            string filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };

            var createRequest = DecryptionServices.LoadAssetBundleAsync(fileInfo, out var managedStream);
            _loadedStream.Add(bundle.BundleGUID, managedStream);
            return createRequest;
        }
        #endregion
    }
}