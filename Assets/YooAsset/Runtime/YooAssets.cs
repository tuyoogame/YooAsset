using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 资源系统的主入口
    /// </summary>
    public static partial class YooAssets
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            s_isInitialized = false;
            s_driver = null;
            s_packages.Clear();
        }
#endif

        private static bool s_isInitialized;
        private static GameObject s_driver;
        private static readonly Dictionary<string, ResourcePackage> s_packages = new Dictionary<string, ResourcePackage>(10);

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool IsInitialized
        {
            get { return s_isInitialized; }
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        public static void Initialize()
        {
            Initialize(null);
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        /// <param name="logger">自定义日志处理</param>
        public static void Initialize(ILogger logger)
        {
            if (s_isInitialized)
                throw new System.InvalidOperationException("YooAssets is already initialized.");

            YooLogger.Current = logger;

            // 创建驱动器
            s_driver = new UnityEngine.GameObject($"[{nameof(YooAssets)}]");
            s_driver.AddComponent<YooAssetsDriver>();
            UnityEngine.Object.DontDestroyOnLoad(s_driver);

#if DEBUG
            // 添加远程调试脚本
            s_driver.AddComponent<DiagnosticBehaviour>();
#endif

            // 初始化异步操作系统
            AsyncOperationSystem.Initialize();

            s_isInitialized = true;
        }

        /// <summary>
        /// 销毁资源系统
        /// </summary>
        public static void Destroy()
        {
            if (s_isInitialized)
            {
                s_isInitialized = false;

                // 销毁驱动器
                if (s_driver != null)
                {
                    GameObject.Destroy(s_driver);
                    s_driver = null;
                }

                // 销毁异步操作系统
                AsyncOperationSystem.Shutdown();

                // 清空资源包裹列表
                foreach (var kv in s_packages)
                {
                    kv.Value.InternalDestroy();
                }
                s_packages.Clear();
            }
        }

        /// <summary>
        /// 更新资源系统
        /// </summary>
        internal static void Update()
        {
            if (s_isInitialized)
            {
                AsyncOperationSystem.Update();
            }
        }

        /// <summary>
        /// 创建资源包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>新创建的资源包裹实例</returns>
        public static ResourcePackage CreatePackage(string packageName)
        {
            return CreatePackage(packageName, 0);
        }

        /// <summary>
        /// 创建资源包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packagePriority">包裹优先级（值越大越优先更新）</param>
        /// <returns>新创建的资源包裹实例</returns>
        public static ResourcePackage CreatePackage(string packageName, uint packagePriority)
        {
            CheckInitialized(packageName);
            if (ContainsPackage(packageName))
                throw new System.InvalidOperationException($"Resource package '{packageName}' already exists.");

            ResourcePackage package = new ResourcePackage(packageName);
            s_packages.Add(packageName, package);

            // 注册包裹调度器
            AsyncOperationSystem.CreatePackageScheduler(packageName, packagePriority);

            return package;
        }

        /// <summary>
        /// 获取资源包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>指定名称的资源包裹实例</returns>
        public static ResourcePackage GetPackage(string packageName)
        {
            CheckInitialized(packageName);
            var package = GetPackageInternal(packageName);
            if (package == null)
                throw new System.InvalidOperationException($"Could not find resource package: '{packageName}'.");
            return package;
        }

        /// <summary>
        /// 尝试获取资源包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="package">获取到的资源包裹，如果不存在则为null。</param>
        /// <returns>如果资源包裹存在返回true，否则返回false。</returns>
        public static bool TryGetPackage(string packageName, out ResourcePackage package)
        {
            CheckInitialized(packageName);
            package = GetPackageInternal(packageName);
            return package != null;
        }

        /// <summary>
        /// 获取所有资源包裹
        /// </summary>
        /// <returns>当前已注册的所有资源包裹的只读列表</returns>
        public static IReadOnlyList<ResourcePackage> GetPackages()
        {
            CheckInitialized();
            return s_packages.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 移除资源包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public static void RemovePackage(string packageName)
        {
            CheckInitialized(packageName);
            ResourcePackage package = GetPackageInternal(packageName);
            if (package == null)
                throw new System.InvalidOperationException($"Could not find resource package: '{packageName}'.");

            if (package.InitializeStatus != EOperationStatus.None)
                throw new System.InvalidOperationException($"Resource package '{packageName}' has not been destroyed. Call {nameof(ResourcePackage.DestroyPackageAsync)} before removing it.");

            // 先销毁调度器，再移除包裹
            AsyncOperationSystem.DestroyPackageScheduler(packageName);
            s_packages.Remove(packageName);
        }

        /// <summary>
        /// 检测资源包裹是否存在
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>如果资源包裹存在返回true，否则返回false。</returns>
        public static bool ContainsPackage(string packageName)
        {
            CheckInitialized(packageName);
            var package = GetPackageInternal(packageName);
            return package != null;
        }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片。
        /// </summary>
        /// <param name="milliseconds">最大时间切片（单位：毫秒），不能为负数。</param>
        public static void SetAsyncOperationMaxTimeSlice(long milliseconds)
        {
            if (milliseconds < 0)
                throw new System.ArgumentOutOfRangeException(nameof(milliseconds), milliseconds, "Max time slice cannot be negative.");
            AsyncOperationSystem.MaxTimeSlice = milliseconds;
        }

        private static ResourcePackage GetPackageInternal(string packageName)
        {
            s_packages.TryGetValue(packageName, out var package);
            return package;
        }

        #region 调试方法
        private static void CheckInitialized()
        {
            if (s_isInitialized == false)
                throw new System.InvalidOperationException($"YooAssets is not initialized. Please call {nameof(YooAssets.Initialize)} first.");
        }

        private static void CheckInitialized(string packageName)
        {
            CheckInitialized();

            if (string.IsNullOrEmpty(packageName))
                throw new System.ArgumentException("Package name cannot be null or empty.", nameof(packageName));
        }
        #endregion

        #region 调试信息
        /// <summary>
        /// 收集所有资源包裹的诊断数据
        /// </summary>
        /// <returns>包含所有资源包裹诊断数据的报告</returns>
        internal static DiagnosticReport GetDiagnosticReport()
        {
            DiagnosticReport report = DiagnosticReport.Create();
            foreach (var kv in s_packages)
            {
                var packageData = kv.Value.GetDiagnosticData();
                report.PackageDataList.Add(packageData);
            }
            return report;
        }
        #endregion
    }
}