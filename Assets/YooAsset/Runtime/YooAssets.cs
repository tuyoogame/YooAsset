using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public static partial class YooAssets
    {
        private static bool _isInitialize = false;
        private static GameObject _driver = null;
        private static readonly List<ResourcePackage> _packages = new List<ResourcePackage>();

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool Initialized
        {
            get { return _isInitialize; }
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        /// <param name="logger">自定义日志处理</param>
        public static void Initialize(ILogger logger = null)
        {
            if (_isInitialize)
            {
                UnityEngine.Debug.LogWarning($"{nameof(YooAssets)} is initialized !");
                return;
            }

            if (_isInitialize == false)
            {
                YooLogger.Logger = logger;

                // 创建驱动器
                _isInitialize = true;
                _driver = new UnityEngine.GameObject($"[{nameof(YooAssets)}]");
                _driver.AddComponent<YooAssetsDriver>();
                UnityEngine.Object.DontDestroyOnLoad(_driver);
                YooLogger.Log($"{nameof(YooAssets)} initialize !");

#if DEBUG
                // 添加远程调试脚本
                _driver.AddComponent<RemoteDebuggerInRuntime>();
#endif

                OperationSystem.Initialize();
            }
        }

        /// <summary>
        /// 更新资源系统
        /// </summary>
        internal static void Update()
        {
            if (_isInitialize)
            {
                OperationSystem.Update();

                for (int i = 0; i < _packages.Count; i++)
                {
                    _packages[i].UpdatePackage();
                }
            }
        }

        /// <summary>
        /// 应用程序退出处理
        /// </summary>
        internal static void OnApplicationQuit()
        {
            // 说明：在编辑器下确保播放被停止时IO类操作被终止。
            foreach (var package in _packages)
            {
                OperationSystem.ClearPackageOperation(package.PackageName);
            }
            OperationSystem.DestroyAll();
        }

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage CreatePackage(string packageName)
        {
            CheckException(packageName);
            if (ContainsPackage(packageName))
                throw new Exception($"Package {packageName} already existed !");

            YooLogger.Log($"Create resource package : {packageName}");
            ResourcePackage package = new ResourcePackage(packageName);
            _packages.Add(package);
            return package;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage GetPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            if (package == null)
                YooLogger.Error($"Can not found resource package : {packageName}");
            return package;
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage TryGetPackage(string packageName)
        {
            CheckException(packageName);
            return GetPackageInternal(packageName);
        }

        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static bool RemovePackage(string packageName)
        {
            CheckException(packageName);
            ResourcePackage package = GetPackageInternal(packageName);
            if (package == null)
                return false;

            if (package.InitializeStatus != EOperationStatus.None)
            {
                YooLogger.Error($"The resource package {packageName} has not been destroyed, please call the method {nameof(ResourcePackage.DestroyAsync)} to destroy!");
                return false;
            }

            YooLogger.Log($"Remove resource package : {packageName}");
            _packages.Remove(package);
            return true;
        }

        /// <summary>
        /// 检测资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static bool ContainsPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            return package != null;
        }

        /// <summary>
        /// 开启一个异步操作
        /// </summary>
        /// <param name="operation">异步操作对象</param>
        public static void StartOperation(GameAsyncOperation operation)
        {
            // 注意：游戏业务逻辑的包裹填写为空
            OperationSystem.StartOperation(string.Empty, operation);
        }


        private static ResourcePackage GetPackageInternal(string packageName)
        {
            foreach (var package in _packages)
            {
                if (package.PackageName == packageName)
                    return package;
            }
            return null;
        }
        private static void CheckException(string packageName)
        {
            if (_isInitialize == false)
                throw new Exception($"{nameof(YooAssets)} not initialize !");

            if (string.IsNullOrEmpty(packageName))
                throw new Exception("Package name is null or empty !");
        }

        #region 系统参数
        /// <summary>
        /// 设置下载系统参数，自定义下载请求
        /// </summary>
        public static void SetDownloadSystemUnityWebRequest(UnityWebRequestDelegate createDelegate)
        {
            DownloadSystemHelper.UnityWebRequestCreater = createDelegate;
        }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public static void SetOperationSystemMaxTimeSlice(long milliseconds)
        {
            if (milliseconds < 10)
            {
                milliseconds = 10;
                YooLogger.Warning($"MaxTimeSlice minimum value is 10 milliseconds.");
            }
            OperationSystem.MaxTimeSlice = milliseconds;
        }
        #endregion

        #region 调试信息
        internal static DebugReport GetDebugReport()
        {
            DebugReport report = new DebugReport();
            report.FrameCount = Time.frameCount;

            foreach (var package in _packages)
            {
                var packageData = package.GetDebugPackageData();
                report.PackageDatas.Add(packageData);
            }
            return report;
        }
        #endregion
    }
}