#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - 文件系统参数兼容
// 通过 partial class 为 FileSystemParameters 补充 v2.3 旧工厂方法，
// 并恢复 FileSystemParametersDefine、IRemoteServices、IManifestRestoreServices 等旧类型。

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    #region FileSystemParameters partial

    public partial class FileSystemParameters
    {
        /// <summary>
        /// v2.3 属性名，转发到 v3 的 FileSystemTypeName
        /// </summary>
        [Obsolete("Use FileSystemTypeName instead.")]
        public string FileSystemClass
        {
            get { return FileSystemTypeName; }
        }

        /// <summary>
        /// 兼容 v2.3 REMOTE_SERVICES 参数对象
        /// </summary>
        [Obsolete("Use AddParameter(EFileSystemParameter.RemoteService, IRemoteService) instead.")]
        public void AddParameter(string paramName, IRemoteServices value)
        {
            if (paramName == FileSystemParametersDefine.REMOTE_SERVICES && value != null)
                AddParameter(paramName, new RemoteServicesAdapter(value));
            else
                AddParameter(paramName, (object)value);
        }

        /// <summary>
        /// 兼容 v2.3 MANIFEST_SERVICES 参数对象
        /// </summary>
        [Obsolete("Use AddParameter(EFileSystemParameter.ManifestDecryptor, IManifestDecryptor) instead.")]
        public void AddParameter(string paramName, IManifestRestoreServices value)
        {
            if (paramName == FileSystemParametersDefine.MANIFEST_SERVICES && value != null)
                AddParameter(paramName, new ManifestRestoreServicesAdapter(value));
            else
                AddParameter(paramName, (object)value);
        }

        /// <summary>
        /// 兼容 v2.3 INSTALL_CLEAR_MODE 枚举值
        /// </summary>
        [Obsolete("Use AddParameter(EFileSystemParameter.InstallCleanupMode, EInstallCleanupMode) instead.")]
        public void AddParameter(string paramName, EOverwriteInstallClearMode value)
        {
            if (paramName == FileSystemParametersDefine.INSTALL_CLEAR_MODE)
                AddParameter(paramName, (EInstallCleanupMode)(int)value);
            else
                AddParameter(paramName, (object)value);
        }

        /// <summary>
        /// v2.3 旧拼写入口，转发到 CreateDefaultBuiltinFileSystemParameters
        /// </summary>
        [Obsolete("Use CreateDefaultBuiltinFileSystemParameters instead. IDecryptionServices is no longer supported; pass null or migrate to IBundleDecryptor.")]
        public static FileSystemParameters CreateDefaultBuildinFileSystemParameters(IDecryptionServices decryptionServices = null, string packageRoot = null)
        {
            return CreateDefaultBuiltinFileSystemParameters(packageRoot);
        }

        /// <summary>
        /// v2.3 缓存文件系统入口，转发到 CreateDefaultSandboxFileSystemParameters
        /// </summary>
        [Obsolete("Use CreateDefaultSandboxFileSystemParameters instead. IDecryptionServices is no longer supported; pass null or migrate to IBundleDecryptor.")]
        public static FileSystemParameters CreateDefaultCacheFileSystemParameters(IRemoteServices remoteServices, IDecryptionServices decryptionServices = null, string packageRoot = null)
        {
            var adapter = remoteServices != null ? new RemoteServicesAdapter(remoteServices) : null;
            return CreateDefaultSandboxFileSystemParameters(adapter, packageRoot);
        }

        /// <summary>
        /// v2.3 WebServer 文件系统入口
        /// </summary>
        [Obsolete("Use CreateDefaultWebServerFileSystemParameters(bool) instead. IWebDecryptionServices is no longer supported.")]
        public static FileSystemParameters CreateDefaultWebServerFileSystemParameters(IWebDecryptionServices decryptionServices, bool disableUnityWebCache = false)
        {
            return CreateDefaultWebServerFileSystemParameters(disableUnityWebCache);
        }

        /// <summary>
        /// WebRemote 文件系统入口，转发到 WebNetwork 文件系统
        /// </summary>
        [Obsolete("Use CreateDefaultWebNetworkFileSystemParameters instead.")]
        public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteService remoteService, bool disableUnityWebCache)
        {
            return CreateDefaultWebNetworkFileSystemParameters(remoteService, disableUnityWebCache);
        }

        /// <summary>
        /// WebRemote 文件系统入口，转发到 WebNetwork 文件系统
        /// </summary>
        [Obsolete("Use CreateDefaultWebNetworkFileSystemParameters instead.")]
        public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteService remoteService)
        {
            return CreateDefaultWebNetworkFileSystemParameters(remoteService);
        }

        /// <summary>
        /// v2.3 WebRemote 文件系统入口，转发到 WebNetwork 文件系统
        /// </summary>
        [Obsolete("Use CreateDefaultWebNetworkFileSystemParameters(IRemoteService, bool) instead. IWebDecryptionServices is no longer supported.")]
        public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteServices remoteServices, IWebDecryptionServices decryptionServices = null, bool disableUnityWebCache = false)
        {
            var adapter = remoteServices != null ? new RemoteServicesAdapter(remoteServices) : null;
            return CreateDefaultWebNetworkFileSystemParameters(adapter, disableUnityWebCache);
        }
    }

    #endregion

    #region FileSystemParametersDefine

    /// <summary>
    /// v2.3 文件系统参数常量定义
    /// </summary>
    [Obsolete("Use EFileSystemParameter enum instead.")]
    public static class FileSystemParametersDefine
    {
        public const string FILE_VERIFY_LEVEL = "FileVerifyLevel";
        public const string FILE_VERIFY_MAX_CONCURRENCY = "FileVerifyMaxConcurrency";
        public const string INSTALL_CLEAR_MODE = "InstallCleanupMode";
        public const string REMOTE_SERVICES = "RemoteService";
        public const string DECRYPTION_SERVICES = "DECRYPTION_SERVICES";
        public const string MANIFEST_SERVICES = "ManifestDecryptor";
        public const string APPEND_FILE_EXTENSION = "APPEND_FILE_EXTENSION";
        public const string DISABLE_CATALOG_FILE = "DISABLE_CATALOG_FILE";
        public const string DISABLE_UNITY_WEB_CACHE = "DisableUnityWebCache";
        public const string DISABLE_ONDEMAND_DOWNLOAD = "DownloadDisableOndemand";
        public const string DOWNLOAD_MAX_CONCURRENCY = "DownloadMaxConcurrency";
        public const string DOWNLOAD_MAX_REQUEST_PER_FRAME = "DownloadMaxRequestPerFrame";
        public const string DOWNLOAD_WATCH_DOG_TIME = "DownloadWatchdogTimeout";
        public const string RESUME_DOWNLOAD_MINMUM_SIZE = "DownloadResumeMinimumSize";
        public const string RESUME_DOWNLOAD_RESPONSE_CODES = "RESUME_DOWNLOAD_RESPONSE_CODES";
        public const string VIRTUAL_WEBGL_MODE = "VirtualWebglMode";
        public const string VIRTUAL_DOWNLOAD_MODE = "VirtualDownloadMode";
        public const string VIRTUAL_DOWNLOAD_SPEED = "VirtualDownloadSpeed";
        public const string ASYNC_SIMULATE_MIN_FRAME = "AsyncSimulateMinFrame";
        public const string ASYNC_SIMULATE_MAX_FRAME = "AsyncSimulateMaxFrame";
        public const string COPY_BUILDIN_PACKAGE_MANIFEST = "CopyBuiltinPackageManifest";
        public const string COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT = "CopyBuiltinPackageManifestDestRoot";
        public const string COPY_LOCAL_FILE_SERVICES = "COPY_LOCAL_FILE_SERVICES";
        public const string UNPACK_FILE_SYSTEM_ROOT = "UnpackFileSystemRoot";
    }

    #endregion

    #region EOverwriteInstallClearMode

    /// <summary>
    /// v2.3 覆盖安装清理模式（v3 改为 EInstallCleanupMode）
    /// </summary>
    [Obsolete("Use EInstallCleanupMode instead.")]
    public enum EOverwriteInstallClearMode
    {
        None = 0,
        ClearAllCacheFiles = 1,
        ClearAllBundleFiles = 2,
        ClearAllManifestFiles = 3,
    }

    #endregion

    #region IRemoteServices

    /// <summary>
    /// v2.3 远端资源地址查询服务接口
    /// </summary>
    [Obsolete("Implement IRemoteService instead.")]
    public interface IRemoteServices
    {
        string GetRemoteMainURL(string fileName);
        string GetRemoteFallbackURL(string fileName);
    }

    /// <summary>
    /// 将 v2.3 IRemoteServices 适配为 v3 IRemoteService
    /// </summary>
    internal sealed class RemoteServicesAdapter : IRemoteService
    {
        private readonly IRemoteServices _legacy;

        public RemoteServicesAdapter(IRemoteServices legacy)
        {
            _legacy = legacy;
        }

        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            var urls = new List<string>(2);
            string main = _legacy.GetRemoteMainURL(fileName);
            if (!string.IsNullOrEmpty(main))
                urls.Add(main);
            string fallback = _legacy.GetRemoteFallbackURL(fileName);
            if (!string.IsNullOrEmpty(fallback))
                urls.Add(fallback);
            return urls;
        }
    }

    #endregion

    #region IManifestRestoreServices

    /// <summary>
    /// v2.3 资源清单文件处理服务接口
    /// </summary>
    [Obsolete("Implement IManifestDecryptor instead.")]
    public interface IManifestRestoreServices
    {
        byte[] RestoreManifest(byte[] fileData);
    }

    /// <summary>
    /// 将 v2.3 IManifestRestoreServices 适配为 v3 IManifestDecryptor
    /// </summary>
    internal sealed class ManifestRestoreServicesAdapter : IManifestDecryptor
    {
        private readonly IManifestRestoreServices _legacy;

        public ManifestRestoreServicesAdapter(IManifestRestoreServices legacy)
        {
            _legacy = legacy;
        }

        public byte[] Decrypt(byte[] fileData)
        {
            return _legacy.RestoreManifest(fileData);
        }
    }

    #endregion

    #region IDecryptionServices / IWebDecryptionServices

    /// <summary>
    /// v2.3 解密文件信息（仅用于旧接口编译兼容）
    /// </summary>
    [Obsolete("v3 has split decryption into dedicated decryptor interfaces. Manual migration required.")]
    public struct DecryptFileInfo
    {
        public string BundleName;
        public string FileLoadPath;
        public uint FileLoadCRC;
    }

    /// <summary>
    /// v2.3 解密结果（仅用于旧接口编译兼容）
    /// </summary>
    [Obsolete("v3 has split decryption into dedicated decryptor interfaces. Manual migration required.")]
    public struct DecryptResult
    {
        public AssetBundle Result;
        public AssetBundleCreateRequest CreateRequest;
        public Stream ManagedStream;
    }

    /// <summary>
    /// v2.3 加密文件解密服务接口（仅用于编译兼容，无法自动转换为 v3 解密器）
    /// </summary>
    [Obsolete("v3 has split decryption into IBundleOffsetDecryptor, IBundleMemoryDecryptor, IBundleStreamDecryptor. Manual migration required.")]
    public interface IDecryptionServices
    {
        DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo);
        DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo);
        DecryptResult LoadAssetBundleFallback(DecryptFileInfo fileInfo);
        byte[] ReadFileData(DecryptFileInfo fileInfo);
        string ReadFileText(DecryptFileInfo fileInfo);
    }

    /// <summary>
    /// v2.3 Web 解密文件信息（仅用于旧接口编译兼容）
    /// </summary>
    [Obsolete("v3 no longer supports IWebDecryptionServices. Manual migration required.")]
    public struct WebDecryptFileInfo
    {
        public string BundleName;
        public uint FileLoadCRC;
        public byte[] FileData;
    }

    /// <summary>
    /// v2.3 Web 解密结果（仅用于旧接口编译兼容）
    /// </summary>
    [Obsolete("v3 no longer supports IWebDecryptionServices. Manual migration required.")]
    public struct WebDecryptResult
    {
        public AssetBundle Result;
    }

    /// <summary>
    /// v2.3 Web 解密服务接口（仅用于编译兼容，无法自动转换为 v3 解密器）
    /// </summary>
    [Obsolete("v3 no longer supports IWebDecryptionServices. Manual migration required.")]
    public interface IWebDecryptionServices
    {
        WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo);
    }

    #endregion
}
#endif
