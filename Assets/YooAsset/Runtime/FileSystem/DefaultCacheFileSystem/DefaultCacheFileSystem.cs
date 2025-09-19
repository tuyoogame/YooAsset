using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 缓存文件系统
    /// 说明：正在进行的下载器会在ResourcePackage销毁的时候执行Abort操作！
    /// </summary>
    internal class DefaultCacheFileSystem : IFileSystem
    {
        protected readonly Dictionary<string, RecordFileElement> _records = new Dictionary<string, RecordFileElement>(10000);
        protected readonly Dictionary<string, string> _bundleDataFilePathMapping = new Dictionary<string, string>(10000);
        protected readonly Dictionary<string, string> _bundleInfoFilePathMapping = new Dictionary<string, string>(10000);
        protected readonly Dictionary<string, string> _tempFilePathMapping = new Dictionary<string, string>(10000);

        protected string _packageRoot;
        protected string _tempFilesRoot;
        protected string _cacheBundleFilesRoot;
        protected string _cacheManifestFilesRoot;

        /// <summary>
        /// 下载中心
        /// 说明：当异步操作任务终止的时候，所有下载子任务都会一同被终止！
        /// </summary>
        public DownloadCenterOperation DownloadCenter { set; get; }

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
                return _records.Count;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：远程服务接口的实例类
        /// </summary>
        public IRemoteServices RemoteServices { private set; get; }

        /// <summary>
        /// 自定义参数：覆盖安装缓存清理模式
        /// </summary>
        public EOverwriteInstallClearMode InstallClearMode { private set; get; } = EOverwriteInstallClearMode.ClearAllManifestFiles;

        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验级别
        /// </summary>
        public EFileVerifyLevel FileVerifyLevel { private set; get; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验最大并发数
        /// </summary>
        public int FileVerifyMaxConcurrency { private set; get; } = int.MaxValue;

        /// <summary>
        /// 自定义参数：数据文件追加文件格式
        /// </summary>
        public bool AppendFileExtension { private set; get; } = false;

        /// <summary>
        /// 自定义参数：禁用边玩边下机制
        /// </summary>
        public bool DisableOnDemandDownload { private set; get; } = false;

        /// <summary>
        /// 自定义参数：最大并发连接数
        /// </summary>
        public int DownloadMaxConcurrency { private set; get; } = int.MaxValue;

        /// <summary>
        /// 自定义参数：每帧发起的最大请求数
        /// </summary>
        public int DownloadMaxRequestPerFrame { private set; get; } = int.MaxValue;

        /// <summary>
        /// 自定义参数：下载任务的看门狗机制监控时间
        /// </summary>
        public int DownloadWatchDogTime { private set; get; } = int.MaxValue;

        /// <summary>
        /// 自定义参数：启用断点续传的最小尺寸
        /// </summary>
        public long ResumeDownloadMinimumSize { private set; get; } = long.MaxValue;

        /// <summary>
        /// 自定义参数：断点续传下载器关注的错误码
        /// </summary>
        public List<long> ResumeDownloadResponseCodes { private set; get; } = null;

        /// <summary>
        ///  自定义参数：解密服务接口的实例类
        /// </summary>
        public IDecryptionServices DecryptionServices { private set; get; }

        /// <summary>
        /// 自定义参数：资源清单服务类
        /// </summary>
        public IManifestRestoreServices ManifestServices { private set; get; }

        /// <summary>
        /// 自定义参数：拷贝内置文件接口的实例类
        /// </summary>
        public ICopyLocalFileServices CopyLocalFileServices { private set; get; }
        #endregion


        public DefaultCacheFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DCFSInitializeOperation(this);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DCFSLoadPackageManifestOperation(this, packageVersion, timeout);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DCFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
            return operation;
        }
        public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, ClearCacheFilesOptions options)
        {
            if (options.ClearMode == EFileClearMode.ClearAllBundleFiles.ToString())
            {
                var operation = new ClearAllCacheBundleFilesOperation(this);
                return operation;
            }
            else if (options.ClearMode == EFileClearMode.ClearUnusedBundleFiles.ToString())
            {
                var operation = new ClearUnusedCacheBundleFilesOperation(this, manifest);
                return operation;
            }
            else if (options.ClearMode == EFileClearMode.ClearBundleFilesByTags.ToString())
            {
                var operation = new ClearCacheBundleFilesByTagsOperaiton(this, manifest, options.ClearParam);
                return operation;
            }
            else if (options.ClearMode == EFileClearMode.ClearAllManifestFiles.ToString())
            {
                var operation = new ClearAllCacheManifestFilesOperation(this);
                return operation;
            }
            else if (options.ClearMode == EFileClearMode.ClearUnusedManifestFiles.ToString())
            {
                var operation = new ClearUnusedCacheManifestFilesOperation(this, manifest);
                return operation;
            }
            else
            {
                string error = $"Invalid clear mode : {options.ClearMode}";
                var operation = new FSClearCacheFilesCompleteOperation(error);
                return operation;
            }
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadFileOptions options)
        {
            // 获取下载地址
            if (string.IsNullOrEmpty(options.ImportFilePath))
            {
                // 注意：如果是解压文件系统类，这里会返回本地内置文件的下载路径
                string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
                string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
                options.SetURL(mainURL, fallbackURL);
            }
            else
            {
                // 注意：把本地导入文件路径转换为下载器请求地址
                string mainURL = DownloadSystemHelper.ConvertToWWWPath(options.ImportFilePath);
                options.SetURL(mainURL, mainURL);
            }

            var downloader = new DownloadPackageBundleOperation(this, bundle, options);
            return downloader;
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
            {
                var operation = new DCFSLoadAssetBundleOperation(this, bundle);
                return operation;
            }
            else if (bundle.BundleType == (int)EBuildBundleType.RawBundle)
            {
                var operation = new DCFSLoadRawBundleOperation(this, bundle);
                return operation;
            }
            else
            {
                string error = $"{nameof(DefaultCacheFileSystem)} not support load bundle type : {bundle.BundleType}";
                var operation = new FSLoadBundleCompleteOperation(error);
                return operation;
            }
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.REMOTE_SERVICES)
            {
                RemoteServices = (IRemoteServices)value;
            }
            else if (name == FileSystemParametersDefine.INSTALL_CLEAR_MODE)
            {
                InstallClearMode = (EOverwriteInstallClearMode)value;
            }
            else if (name == FileSystemParametersDefine.FILE_VERIFY_LEVEL)
            {
                FileVerifyLevel = (EFileVerifyLevel)value;
            }
            else if (name == FileSystemParametersDefine.FILE_VERIFY_MAX_CONCURRENCY)
            {
                int convertValue = Convert.ToInt32(value);
                FileVerifyMaxConcurrency = Mathf.Clamp(convertValue, 1, int.MaxValue);
            }
            else if (name == FileSystemParametersDefine.APPEND_FILE_EXTENSION)
            {
                AppendFileExtension = Convert.ToBoolean(value);
            }
            else if (name == FileSystemParametersDefine.DISABLE_ONDEMAND_DOWNLOAD)
            {
                DisableOnDemandDownload = Convert.ToBoolean(value);
            }
            else if (name == FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY)
            {
                int convertValue = Convert.ToInt32(value);
                DownloadMaxConcurrency = Mathf.Clamp(convertValue, 1, int.MaxValue);
            }
            else if (name == FileSystemParametersDefine.DOWNLOAD_MAX_REQUEST_PER_FRAME)
            {
                int convertValue = Convert.ToInt32(value);
                DownloadMaxRequestPerFrame = Mathf.Clamp(convertValue, 1, int.MaxValue);
            }
            else if (name == FileSystemParametersDefine.DOWNLOAD_WATCH_DOG_TIME)
            {
                int convertValue = Convert.ToInt32(value);
                DownloadWatchDogTime = Mathf.Clamp(convertValue, 1, int.MaxValue);
            }
            else if (name == FileSystemParametersDefine.RESUME_DOWNLOAD_MINMUM_SIZE)
            {
                ResumeDownloadMinimumSize = Convert.ToInt64(value);
            }
            else if (name == FileSystemParametersDefine.RESUME_DOWNLOAD_RESPONSE_CODES)
            {
                ResumeDownloadResponseCodes = (List<long>)value;
            }
            else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
            {
                DecryptionServices = (IDecryptionServices)value;
            }
            else if (name == FileSystemParametersDefine.MANIFEST_SERVICES)
            {
                ManifestServices = (IManifestRestoreServices)value;
            }
            else if (name == FileSystemParametersDefine.COPY_LOCAL_FILE_SERVICES)
            {
                CopyLocalFileServices = (ICopyLocalFileServices)value;
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
                _packageRoot = GetDefaultCachePackageRoot(packageName);
            else
                _packageRoot = packageRoot;

            _cacheBundleFilesRoot = PathUtility.Combine(_packageRoot, DefaultCacheFileSystemDefine.BundleFilesFolderName);
            _cacheManifestFilesRoot = PathUtility.Combine(_packageRoot, DefaultCacheFileSystemDefine.ManifestFilesFolderName);
            _tempFilesRoot = PathUtility.Combine(_packageRoot, DefaultCacheFileSystemDefine.TempFilesFolderName);
        }
        public virtual void OnDestroy()
        {
            if (DownloadCenter != null)
            {
                DownloadCenter.AbortOperation();
                DownloadCenter = null;
            }
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            // 注意：缓存文件系统保底加载！
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return _records.ContainsKey(bundle.BundleGUID);
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
            if (Belong(bundle) == false)
                return false;

            return Exists(bundle) == false;
        }

        public virtual string GetBundleFilePath(PackageBundle bundle)
        {
            return GetCacheBundleFileLoadPath(bundle);
        }
        public virtual byte[] ReadBundleFileData(PackageBundle bundle)
        {
            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetCacheBundleFileLoadPath(bundle);
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
                string filePath = GetCacheBundleFileLoadPath(bundle);
                return FileUtility.ReadAllBytes(filePath);
            }
        }
        public virtual string ReadBundleFileText(PackageBundle bundle)
        {
            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetCacheBundleFileLoadPath(bundle);
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
                string filePath = GetCacheBundleFileLoadPath(bundle);
                return FileUtility.ReadAllText(filePath);
            }
        }

        #region 缓存相关
        public List<string> GetAllCachedBundleGUIDs()
        {
            return _records.Keys.ToList();
        }
        public RecordFileElement GetRecordFileElement(PackageBundle bundle)
        {
            if (_records.TryGetValue(bundle.BundleGUID, out RecordFileElement element))
                return element;
            else
                return null;
        }

        public string GetTempFilePath(PackageBundle bundle)
        {
            if (_tempFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_tempFilesRoot, bundle.BundleGUID);
                _tempFilePathMapping.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetBundleDataFilePath(PackageBundle bundle)
        {
            if (_bundleDataFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                string folderName = bundle.FileHash.Substring(0, 2);
                filePath = PathUtility.Combine(_cacheBundleFilesRoot, folderName, bundle.BundleGUID, DefaultCacheFileSystemDefine.BundleDataFileName);
                if (AppendFileExtension)
                    filePath += bundle.FileExtension;
                _bundleDataFilePathMapping.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetBundleInfoFilePath(PackageBundle bundle)
        {
            if (_bundleInfoFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                string folderName = bundle.FileHash.Substring(0, 2);
                filePath = PathUtility.Combine(_cacheBundleFilesRoot, folderName, bundle.BundleGUID, DefaultCacheFileSystemDefine.BundleInfoFileName);
                _bundleInfoFilePathMapping.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }

        public bool IsRecordBundleFile(string bundleGUID)
        {
            return _records.ContainsKey(bundleGUID);
        }
        public bool RecordBundleFile(string bundleGUID, RecordFileElement element)
        {
            if (_records.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultCacheFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _records.Add(bundleGUID, element);
            return true;
        }

        public EFileVerifyResult VerifyCacheFile(PackageBundle bundle)
        {
            if (_records.TryGetValue(bundle.BundleGUID, out RecordFileElement element) == false)
                return EFileVerifyResult.CacheNotFound;

            EFileVerifyResult result = FileVerifyHelper.FileVerify(element.DataFilePath, element.DataFileSize, element.DataFileCRC, EFileVerifyLevel.High);
            return result;
        }
        public bool WriteCacheBundleFile(PackageBundle bundle, string copyPath)
        {
            if (_records.ContainsKey(bundle.BundleGUID))
            {
                throw new Exception("Should never get here !");
            }

            string infoFilePath = GetBundleInfoFilePath(bundle);
            string dataFilePath = GetBundleDataFilePath(bundle);

            try
            {
                if (File.Exists(infoFilePath))
                    File.Delete(infoFilePath);
                if (File.Exists(dataFilePath))
                    File.Delete(dataFilePath);

                FileUtility.CreateFileDirectory(dataFilePath);

                // 拷贝数据文件
                FileInfo fileInfo = new FileInfo(copyPath);
                fileInfo.CopyTo(dataFilePath);

                // 写入文件信息
                WriteBundleInfoFile(infoFilePath, bundle.FileCRC, bundle.FileSize);
            }
            catch (Exception e)
            {
                YooLogger.Error($"Failed to write cache file ! {e.Message}");
                return false;
            }

            var recordFileElement = new RecordFileElement(infoFilePath, dataFilePath, bundle.FileCRC, bundle.FileSize);
            return RecordBundleFile(bundle.BundleGUID, recordFileElement);
        }
        public bool DeleteCacheBundleFile(string bundleGUID)
        {
            if (_records.TryGetValue(bundleGUID, out RecordFileElement element))
            {
                _records.Remove(bundleGUID);
                return element.DeleteFolder();
            }
            else
            {
                return false;
            }
        }

        private readonly BufferWriter _sharedBuffer = new BufferWriter(1024);
        public void WriteBundleInfoFile(string filePath, uint dataFileCRC, long dataFileSize)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                _sharedBuffer.Clear();
                _sharedBuffer.WriteUInt32(dataFileCRC);
                _sharedBuffer.WriteInt64(dataFileSize);
                _sharedBuffer.WriteToStream(fs);
                fs.Flush();
            }
        }
        public void ReadBundleInfoFile(string filePath, out uint dataFileCRC, out long dataFileSize)
        {
            byte[] binaryData = FileUtility.ReadAllBytes(filePath);
            BufferReader buffer = new BufferReader(binaryData);
            dataFileCRC = buffer.ReadUInt32();
            dataFileSize = buffer.ReadInt64();
        }
        #endregion

        #region 内部方法
        public string GetDefaultCachePackageRoot(string packageName)
        {
            string rootDirectory = YooAssetSettingsData.GetYooDefaultCacheRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }
        public string GetCacheBundleFileLoadPath(PackageBundle bundle)
        {
            return GetBundleDataFilePath(bundle);
        }
        public string GetCachePackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_cacheManifestFilesRoot, fileName);
        }
        public string GetCachePackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_cacheManifestFilesRoot, fileName);
        }
        public string GetSandboxAppFootPrintFilePath()
        {
            return PathUtility.Combine(_cacheManifestFilesRoot, DefaultCacheFileSystemDefine.AppFootPrintFileName);
        }
        public string GetCacheBundleFilesRoot()
        {
            return _cacheBundleFilesRoot;
        }
        public string GetCacheManifestFilesRoot()
        {
            return _cacheManifestFilesRoot;
        }

        /// <summary>
        /// 删除所有缓存的资源文件
        /// </summary>
        public void DeleteAllBundleFiles()
        {
            if (Directory.Exists(_cacheBundleFilesRoot))
            {
                Directory.Delete(_cacheBundleFilesRoot, true);
            }
        }

        /// <summary>
        /// 删除所有缓存的清单文件
        /// </summary>
        public void DeleteAllManifestFiles()
        {
            if (Directory.Exists(_cacheManifestFilesRoot))
            {
                Directory.Delete(_cacheManifestFilesRoot, true);
            }
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        public DecryptResult LoadEncryptedAssetBundle(PackageBundle bundle)
        {
            string filePath = GetCacheBundleFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };
            return DecryptionServices.LoadAssetBundle(fileInfo);
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        public DecryptResult LoadEncryptedAssetBundleAsync(PackageBundle bundle)
        {
            string filePath = GetCacheBundleFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };
            return DecryptionServices.LoadAssetBundleAsync(fileInfo);
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        public DecryptResult LoadEncryptedAssetBundleFallback(PackageBundle bundle)
        {
            string filePath = GetCacheBundleFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };
            return DecryptionServices.LoadAssetBundleFallback(fileInfo);
        }
        #endregion
    }
}