using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// Web远端文件系统，管理 WebRemote 平台的远程文件系统访问。
    /// </summary>
    internal class WebRemoteFileSystem : IFileSystem
    {
        /// <summary>
        /// Web 文件缓存系统
        /// </summary>
        public IBundleCache BundleCache { get; private set; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; private set; }

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; private set; }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：UnityWebRequest 创建委托
        /// </summary>
        public UnityWebRequestCreator WebRequestCreator { get; private set; }

        /// <summary>
        /// 自定义参数：禁用 Unity 内置网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; private set; } = false;

        /// <summary>
        /// 自定义参数：下载任务的看门狗机制超时时间
        /// </summary>
        public int DownloadWatchdogTimeout { get; private set; } = 0;

        /// <summary>
        /// 自定义参数：下载的资源包数据的校验级别
        /// </summary>
        public EFileVerifyLevel DownloadVerifyLevel { get; private set; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 支持跨域下载的远程服务接口实例
        /// </summary>
        public IRemoteService RemoteService { get; private set; }

        /// <summary>
        /// 自定义参数：AssetBundle 解密器
        /// </summary>
        public IBundleDecryptor AssetBundleDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：资源清单解密器
        /// </summary>
        public IManifestDecryptor ManifestDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：下载重试判定策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; private set; }

        /// <summary>
        /// 自定义参数：URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; private set; }
        #endregion

        /// <summary>
        /// 创建实例
        /// </summary>
        public WebRemoteFileSystem()
        {
        }
        /// <inheritdoc />
        public FSInitializeOperation InitializeAsync()
        {
            var operation = new WRFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new WRFSRequestPackageVersionOperation(this, options.AppendTimeTicks, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new WRFSLoadPackageManifestOperation(this, options.PackageVersion, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new WRFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var operation = new FSDownloadBundleCompleteOperation($"{nameof(WebRemoteFileSystem)} does not support download operation.");
            return operation;
        }
        /// <inheritdoc />
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            var operation = new FSClearCacheCompleteOperation();
            return operation;
        }

        /// <inheritdoc />
        public void SetParameter(string paramName, object value)
        {
            if (paramName == nameof(EFileSystemParameter.DownloadBackend))
            {
                DownloadBackend = FileSystemHelper.CastParameter<IDownloadBackend>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.UnityWebRequestCreator))
            {
                WebRequestCreator = FileSystemHelper.CastParameter<UnityWebRequestCreator>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DisableUnityWebCache))
            {
                DisableUnityWebCache = FileSystemHelper.CastParameter<bool>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadWatchdogTimeout))
            {
                int convertValue = FileSystemHelper.CastParameter<int>(paramName, value);
                DownloadWatchdogTimeout = Mathf.Max(convertValue, 0);
            }
            else if (paramName == nameof(EFileSystemParameter.FileVerifyLevel))
            {
                DownloadVerifyLevel = FileSystemHelper.CastParameter<EFileVerifyLevel>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.RemoteService))
            {
                RemoteService = FileSystemHelper.CastParameter<IRemoteService>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.AssetbundleDecryptor))
            {
                AssetBundleDecryptor = FileSystemHelper.CastParameter<IBundleDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.ManifestDecryptor))
            {
                ManifestDecryptor = FileSystemHelper.CastParameter<IManifestDecryptor>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadRetryPolicy))
            {
                DownloadRetryPolicy = FileSystemHelper.CastParameter<IDownloadRetryPolicy>(paramName, value);
            }
            else if (paramName == nameof(EFileSystemParameter.DownloadUrlPolicy))
            {
                DownloadUrlPolicy = FileSystemHelper.CastParameter<IDownloadUrlPolicy>(paramName, value);
            }
            else
            {
                throw new ArgumentException($"Unrecognized parameter name: '{paramName}'.", nameof(paramName));
            }
        }
        /// <inheritdoc />
        public void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend(WebRequestCreator);

            // 创建默认的下载重试策略
            if (DownloadRetryPolicy == null)
                DownloadRetryPolicy = new DefaultDownloadRetryPolicy();

            // 创建默认的 URL 选择策略
            if (DownloadUrlPolicy == null)
                DownloadUrlPolicy = new DefaultDownloadUrlPolicy();

            // 创建Web文件缓存系统
            var cacheConfig = new WebRemoteBundleCache.Configuration(
                watchdogTimeout: DownloadWatchdogTimeout,
                disableUnityWebCache: DisableUnityWebCache,
                downloadVerifyLevel: DownloadVerifyLevel,
                assetBundleDecryptor: AssetBundleDecryptor,
                remoteService: RemoteService,
                downloadBackend: DownloadBackend,
                downloadRetryPolicy: DownloadRetryPolicy,
                downloadUrlPolicy: DownloadUrlPolicy);
            BundleCache = new WebRemoteBundleCache(packageName, packageRoot, cacheConfig);
        }
        /// <inheritdoc />
        public void OnDestroy()
        {
            if (BundleCache != null)
            {
                BundleCache.Dispose();
                BundleCache = null;
            }

            if (DownloadBackend != null)
            {
                DownloadBackend.Dispose();
                DownloadBackend = null;
            }
        }

        /// <inheritdoc />
        public bool CanAcceptBundle(PackageBundle bundle)
        {
            // 注意：保底加载！
            return true;
        }
        /// <inheritdoc />
        public bool IsDownloadRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public bool IsUnpackRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public bool IsImportRequired(PackageBundle bundle)
        {
            return false;
        }
    }
}
