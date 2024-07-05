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
        protected string _packageRoot;

        // 解压文件系统
        public IFileSystem UnpackFileSystem { private set; get; }

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
                return EFileAccess.Read;
            }
        }

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
        #endregion


        public DefaultBuildinFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
#if UNITY_EDITOR
            var operation = new DBFSInitializeInEditorPlayModeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
#else
            var operation = new DBFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
#endif
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(params object[] args)
        {
            var operation = new DBFSLoadPackageManifestOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(params object[] args)
        {
            var operation = new DBFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync(params object[] args)
        {
            return UnpackFileSystem.ClearAllBundleFilesAsync();
        }
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(params object[] args)
        {
            PackageManifest manifest = args[0] as PackageManifest;
            return UnpackFileSystem.ClearUnusedBundleFilesAsync(manifest);
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(params object[] args)
        {
            PackageBundle bundle = args[0] as PackageBundle;
            int failedTryAgain = (int)args[2];
            int timeout = (int)args[3];
            string buidlinFilePath = GetBuildinFileLoadPath(bundle);
            return UnpackFileSystem.DownloadFileAsync(bundle, buidlinFilePath, failedTryAgain, timeout);
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == "FILE_VERIFY_LEVEL")
            {
                FileVerifyLevel = (EFileVerifyLevel)value;
            }
            else if (name == "APPEND_FILE_EXTENSION")
            {
                AppendFileExtension = (bool)value;
            }
            else if (name == "RAW_FILE_BUILD_PIPELINE")
            {
                RawFileBuildPipeline = (bool)value;
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
            UnpackFileSystem = new DefaultUnpackFileSystem();
            UnpackFileSystem.SetParameter("FILE_VERIFY_LEVEL", FileVerifyLevel);
            UnpackFileSystem.SetParameter("APPEND_FILE_EXTENSION", AppendFileExtension);
            UnpackFileSystem.SetParameter("RAW_FILE_BUILD_PIPELINE", RawFileBuildPipeline);
            UnpackFileSystem.OnCreate(packageName, null);
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return Belong(bundle.BundleGUID);
        }
        public virtual bool Belong(string bundleGUID)
        {
            return _wrappers.ContainsKey(bundleGUID);
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return Exists(bundle.BundleGUID);
        }
        public virtual bool Exists(string bundleGUID)
        {
            return _wrappers.ContainsKey(bundleGUID);
        }

        public virtual bool CheckNeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool CheckNeedUnpack(PackageBundle bundle)
        {
            if (Belong(bundle) == false)
                return false;

#if UNITY_ANDROID
            return RawFileBuildPipeline || bundle.Encrypted;
#else
            return false;
#endif
        }
        public virtual bool CheckNeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual bool WriteFile(PackageBundle bundle, string copyPath)
        {
            return UnpackFileSystem.WriteFile(bundle, copyPath);
        }
        public virtual bool DeleteFile(PackageBundle bundle)
        {
            return UnpackFileSystem.DeleteFile(bundle);
        }
        public virtual bool DeleteFile(string bundleGUID)
        {
            return UnpackFileSystem.DeleteFile(bundleGUID);
        }
        public virtual EFileVerifyResult VerifyFile(PackageBundle bundle)
        {
            return UnpackFileSystem.VerifyFile(bundle);
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

            if (UnpackFileSystem.Exists(bundle))
            {
                UnpackFileSystem.UnloadBundleFile(bundle, assetBundle);
            }
            else
            {
                if (assetBundle != null)
                    assetBundle.Unload(true);

                if (_loadedStream.TryGetValue(bundle.BundleGUID, out Stream managedStream))
                {
                    managedStream.Close();
                    managedStream.Dispose();
                    _loadedStream.Remove(bundle.BundleGUID);
                }
            }
        }

        #region 内部方法
        protected string GetDefaultRoot()
        {
            string path = PathUtility.Combine(UnityEngine.Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
#if UNITY_OPENHARMONY
            return $"file://{path}";
#else
            return path;
#endif
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

        /// <summary>
        /// 记录缓存信息
        /// </summary>
        public bool Record(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultBuildinFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }
        #endregion
    }
}