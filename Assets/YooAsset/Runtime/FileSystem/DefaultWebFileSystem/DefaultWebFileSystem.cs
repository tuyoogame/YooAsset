using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal class DefaultWebFileSystem : IFileSystem
    {
        public class FileWrapper
        {
            public string FileName { private set; get; }

            public FileWrapper(string fileName)
            {
                FileName = fileName;
            }
        }

        protected readonly Dictionary<string, FileWrapper> _wrappers = new Dictionary<string, FileWrapper>(10000);
        protected readonly Dictionary<string, string> _webFilePaths = new Dictionary<string, string>(10000);
        protected string _webPackageRoot = string.Empty;


        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件访问权限
        /// </summary>
        public EFileAccess FileSystemAccess
        {
            get
            {
                return EFileAccess.ReadWrite;
            }
        }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return _webPackageRoot;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return 0;
            }
        }

        #region 自定义参数
        public bool AllowCrossAccess { private set; get; } = false;

        /// <summary>
        /// 自定义参数：远程服务接口
        /// </summary>
        public IRemoteServices RemoteServices { private set; get; } = null;
        #endregion


        public DefaultWebFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DWFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(params object[] args)
        {
            string packageVersion = args[0] as string;
            int timeout = (int)args[1];

            if (AllowCrossAccess)
            {
                var operation = new DWFSLoadRemotePackageManifestOperation(this, packageVersion, timeout);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                var operation = new DWFSLoadWebPackageManifestOperation(this, timeout);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(params object[] args)
        {
            bool appendTimeTicks = (bool)args[0];
            int timeout = (int)args[1];
            var operation = new DWFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync(params object[] args)
        {
            var operation = new DWFSClearAllBundleFilesOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(params object[] args)
        {
            var operation = new DWFSClearUnusedBundleFilesOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == "ALLOW_CROSS_ACCESS")
            {
                AllowCrossAccess = (bool)value;
            }
            else if (name == "REMOTE_SERVICES")
            {
                RemoteServices = (IRemoteServices)value;
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
                rootDirectory = GetDefaultWebRoot();

            _webPackageRoot = PathUtility.Combine(rootDirectory, packageName);
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Belong(string bundleGUID)
        {
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool Exists(string bundleGUID)
        {
            return false;
        }

        public virtual bool CheckNeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool CheckNeedUnpack(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool CheckNeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual bool WriteFile(PackageBundle bundle, string copyPath)
        {
            throw new System.NotImplementedException();
        }
        public virtual bool DeleteFile(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual bool DeleteFile(string bundleGUID)
        {
            throw new System.NotImplementedException();
        }
        public virtual EFileVerifyResult VerifyFile(PackageBundle bundle)
        {
            return EFileVerifyResult.Succeed;
        }

        public virtual byte[] ReadFileBytes(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual string ReadFileText(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }

        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            var operation = new DWFSLoadAssetBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
            AssetBundle assetBundle = result as AssetBundle;
            if (assetBundle == null)
                return;

            if (assetBundle != null)
                assetBundle.Unload(true);
        }

        #region 内部方法
        protected string GetDefaultWebRoot()
        {
            string path = PathUtility.Combine(UnityEngine.Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
            return path;
        }
        public string GetWebFileLoadPath(PackageBundle bundle)
        {
            if (_webFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_webPackageRoot, bundle.FileName);
                _webFilePaths.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetCatalogFileLoadPath()
        {
            string fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return PathUtility.Combine(YooAssetSettingsData.Setting.DefaultYooFolderName, PackageName, fileName);
        }
        public string GetWebPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetWebPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetWebPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }

        /// <summary>
        /// 记录缓存信息
        /// </summary>
        public bool Record(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultWebFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }
        #endregion
    }
}