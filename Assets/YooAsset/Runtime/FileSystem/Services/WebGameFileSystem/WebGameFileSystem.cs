using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台文件系统抽象基类，封装各小游戏平台的公共逻辑
    /// </summary>
    internal abstract class WebGameFileSystem : IFileSystem
    {
        /// <summary>
        /// 平台实现实例
        /// </summary>
        internal IWebGamePlatform Platform { get; private set; }

        /// <summary>
        /// 缓存系统
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
        /// 自定义参数：禁用 Unity 内置网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; private set; } = true;

        /// <summary>
        /// 自定义参数：下载看门狗超时时间（秒）
        /// </summary>
        public int DownloadWatchdogTimeout { get; private set; } = 0;

        /// <summary>
        /// 自定义参数：下载数据校验级别
        /// </summary>
        public EFileVerifyLevel DownloadVerifyLevel { get; private set; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 自定义参数：远端资源服务
        /// </summary>
        public IRemoteService RemoteService { get; private set; }

        /// <summary>
        /// 自定义参数：资源包解密器
        /// </summary>
        public IBundleDecryptor AssetBundleDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：资源清单解密器
        /// </summary>
        public IManifestDecryptor ManifestDecryptor { get; private set; }

        /// <summary>
        /// 自定义参数：下载重试策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; private set; }

        /// <summary>
        /// 自定义参数：URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; private set; }
        #endregion

        /// <inheritdoc />
        public virtual FSInitializeOperation InitializeAsync()
        {
            var operation = new WGFSInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options)
        {
            var operation = new WGFSRequestPackageVersionOperation(this, options.AppendTimeTicks, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options)
        {
            var operation = new WGFSLoadPackageManifestOperation(this, options.PackageVersion, options.Timeout);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
        {
            var operation = new WGFSLoadPackageBundleOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public virtual FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options)
        {
            var operation = new FSDownloadBundleCompleteOperation($"{nameof(WebGameFileSystem)} does not support download operation.");
            return operation;
        }
        /// <inheritdoc />
        public virtual FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
        {
            var operation = new FSClearCacheCompleteOperation();
            return operation;
        }

        /// <inheritdoc />
        public virtual void SetParameter(string paramName, object value)
        {
            if (paramName == nameof(EFileSystemParameter.DownloadBackend))
            {
                DownloadBackend = FileSystemHelper.CastParameter<IDownloadBackend>(paramName, value);
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
        public virtual void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(packageRoot))
            {
                throw new ArgumentException("The package root for mini game cache must be configured.", nameof(packageRoot));
            }

            // 创建默认的下载后台接口
            if (DownloadBackend == null)
                DownloadBackend = new UnityWebRequestBackend();

            // 创建默认的下载重试策略
            if (DownloadRetryPolicy == null)
                DownloadRetryPolicy = new DefaultDownloadRetryPolicy();

            // 创建默认的 URL 选择策略
            if (DownloadUrlPolicy == null)
                DownloadUrlPolicy = new DefaultDownloadUrlPolicy();

            // 创建Web游戏缓存系统
            Platform = CreatePlatform(packageRoot);
            var cacheConfig = new WebGameBundleCache.Configuration(
                gamePlatform: Platform,
                watchdogTimeout: DownloadWatchdogTimeout,
                disableUnityWebCache: DisableUnityWebCache,
                downloadVerifyLevel: DownloadVerifyLevel,
                assetBundleDecryptor: AssetBundleDecryptor,
                remoteService: RemoteService,
                downloadBackend: DownloadBackend,
                downloadRetryPolicy: DownloadRetryPolicy,
                downloadUrlPolicy: DownloadUrlPolicy);
            BundleCache = new WebGameBundleCache(packageName, packageRoot, cacheConfig);
        }
        /// <inheritdoc />
        public virtual void OnDestroy()
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
        public virtual bool CanAcceptBundle(PackageBundle bundle)
        {
            // 注意：保底加载！
            return true;
        }
        /// <inheritdoc />
        public virtual bool IsDownloadRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public virtual bool IsUnpackRequired(PackageBundle bundle)
        {
            return false;
        }
        /// <inheritdoc />
        public virtual bool IsImportRequired(PackageBundle bundle)
        {
            return false;
        }

        /// <summary>
        /// 创建平台实现实例
        /// </summary>
        /// <param name="packageRoot">包裹缓存根目录</param>
        /// <returns>平台实现实例</returns>
        protected abstract IWebGamePlatform CreatePlatform(string packageRoot);
    }
}
