using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 模拟文件系统
    /// </summary>
    internal class DefaultEditorFileSystem : IFileSystem
    {
        protected readonly Dictionary<string, string> _records = new Dictionary<string, string>(10000);
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
                return 0;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 模拟WebGL平台模式
        /// </summary>
        public bool VirtualWebGLMode { private set; get; } = false;

        /// <summary>
        /// 模拟虚拟下载模式
        /// </summary>
        public bool VirtualDownloadMode { private set; get; } = false;

        /// <summary>
        /// 模拟虚拟下载的网速（单位：字节）
        /// </summary>
        public int VirtualDownloadSpeed { private set; get; } = 1024;

        /// <summary>
        /// 异步模拟加载最小帧数
        /// </summary>
        public int AsyncSimulateMinFrame { private set; get; } = 1;

        /// <summary>
        /// 异步模拟加载最大帧数
        /// </summary>
        public int AsyncSimulateMaxFrame { private set; get; } = 1;
        #endregion

        public DefaultEditorFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DEFSInitializeOperation(this);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DEFSLoadPackageManifestOperation(this, packageVersion);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DEFSRequestPackageVersionOperation(this);
            return operation;
        }
        public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, ClearCacheFilesOptions options)
        {
            var operation = new FSClearCacheFilesCompleteOperation();
            return operation;
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadFileOptions options)
        {
            string mainURL = bundle.BundleName;
            options.SetURL(mainURL, mainURL);
            var downloader = new DownloadVirtualBundleOperation(this, bundle, options);
            return downloader;
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (bundle.BundleType == (int)EBuildBundleType.VirtualBundle)
            {
                var operation = new DEFSLoadBundleOperation(this, bundle);
                return operation;
            }
            else
            {
                string error = $"{nameof(DefaultEditorFileSystem)} not support load bundle type : {bundle.BundleType}";
                var operation = new FSLoadBundleCompleteOperation(error);
                return operation;
            }
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.VIRTUAL_WEBGL_MODE)
            {
                VirtualWebGLMode = Convert.ToBoolean(value);
            }
            else if (name == FileSystemParametersDefine.VIRTUAL_DOWNLOAD_MODE)
            {
                VirtualDownloadMode = Convert.ToBoolean(value);
            }
            else if (name == FileSystemParametersDefine.VIRTUAL_DOWNLOAD_SPEED)
            {
                VirtualDownloadSpeed = Convert.ToInt32(value);
            }
            else if (name == FileSystemParametersDefine.ASYNC_SIMULATE_MIN_FRAME)
            {
                AsyncSimulateMinFrame = Convert.ToInt32(value);
            }
            else if (name == FileSystemParametersDefine.ASYNC_SIMULATE_MAX_FRAME)
            {
                AsyncSimulateMaxFrame = Convert.ToInt32(value);
            }
            else
            {
                YooLogger.Warning($"Invalid parameter : {name}");
            }
        }
        public virtual void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(packageRoot))
                throw new Exception($"{nameof(DefaultEditorFileSystem)} root directory is null or empty !");

            _packageRoot = packageRoot;
        }
        public virtual void OnDestroy()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            if (VirtualDownloadMode)
            {
                return _records.ContainsKey(bundle.BundleGUID);
            }
            else
            {
                return true;
            }
        }
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            if (Belong(bundle) == false)
                return false;

            return Exists(bundle) == false;
        }
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual string GetBundleFilePath(PackageBundle bundle)
        {
            if (bundle.IncludeMainAssets.Count == 0)
                return string.Empty;

            var pacakgeAsset = bundle.IncludeMainAssets[0];
            return pacakgeAsset.AssetPath;
        }
        public virtual byte[] ReadBundleFileData(PackageBundle bundle)
        {
            if (bundle.IncludeMainAssets.Count == 0)
                return null;

            var pacakgeAsset = bundle.IncludeMainAssets[0];
            return FileUtility.ReadAllBytes(pacakgeAsset.AssetPath);
        }
        public virtual string ReadBundleFileText(PackageBundle bundle)
        {
            if (bundle.IncludeMainAssets.Count == 0)
                return null;

            var pacakgeAsset = bundle.IncludeMainAssets[0];
            return FileUtility.ReadAllText(pacakgeAsset.AssetPath);
        }

        #region 内部方法
        public void RecordDownloadFile(PackageBundle bundle)
        {
            if (_records.ContainsKey(bundle.BundleGUID) == false)
                _records.Add(bundle.BundleGUID, bundle.BundleName);
        }
        public string GetEditorPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public string GetEditorPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public string GetEditorPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public int GetAsyncSimulateFrame()
        {
            if (AsyncSimulateMinFrame > AsyncSimulateMaxFrame)
            {
                AsyncSimulateMinFrame = AsyncSimulateMaxFrame;
            }

            return UnityEngine.Random.Range(AsyncSimulateMinFrame, AsyncSimulateMaxFrame + 1);
        }
        #endregion
    }
}