using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建参数
    /// </summary>
    public abstract class BuildParameters
    {

        /// <summary>
        /// 构建输出的根目录
        /// </summary>
        public string BuildOutputRoot { get; set; }

        /// <summary>
        /// 首包资源的根目录
        /// </summary>
        public string BundledFileRoot { get; set; }

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline { get; set; }

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType { get; set; }

        /// <summary>
        /// 构建的平台
        /// </summary>
        public BuildTarget BuildTarget { get; set; }

        /// <summary>
        /// 构建的包裹名称
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 构建的包裹版本
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// 构建的包裹备注
        /// </summary>
        public string PackageNote { get; set; }

        /// <summary>
        /// 清空构建缓存文件
        /// </summary>
        public bool ClearBuildCacheFiles { get; set; } = false;

        /// <summary>
        /// 是否使用资源依赖缓存数据库
        /// </summary>
        /// <remarks>开启此项可以极大提高资源收集速度</remarks>
        public bool UseAssetDependencyDB { get; set; } = false;

        /// <summary>
        /// 启用共享资源打包
        /// </summary>
        public bool EnableSharePackRule { get; set; } = false;

        /// <summary>
        /// 是否对单独引用的共享资源进行独立打包
        /// </summary>
        /// <remarks>关闭该选项，单独引用的共享资源将会构建到引用它的资源包内。</remarks>
        public bool SingleReferencedPackAlone { get; set; } = true;

        /// <summary>
        /// 验证构建结果
        /// </summary>
        public bool VerifyBuildingResult { get; set; } = false;

        /// <summary>
        /// 资源包名称样式
        /// </summary>
        public EFileNameStyle FileNameStyle { get; set; } = EFileNameStyle.HashName;

        /// <summary>
        /// 首包资源的拷贝选项
        /// </summary>
        public EBundledCopyOption BundledCopyOption { get; set; } = EBundledCopyOption.None;

        /// <summary>
        /// 首包资源的拷贝参数
        /// </summary>
        public string BundledCopyParams { get; set; } = string.Empty;

        /// <summary>
        /// 资源包加密器
        /// </summary>
        public IBundleEncryptor BundleEncryptor { get; set; }

        /// <summary>
        /// 资源清单加密器
        /// </summary>
        public IManifestEncryptor ManifestEncryptor { get; set; }

        /// <summary>
        /// 资源清单解密器
        /// </summary>
        public IManifestDecryptor ManifestDecryptor { get; set; }


        /// <summary>
        /// 检测构建参数是否合法
        /// </summary>
        public void CheckBuildParameters()
        {
            // 检测当前是否正在构建资源包
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.ThePipelineIsBuiding, "Pipeline build is in progress.");
                throw new InvalidOperationException(message);
            }

            // 检测构建参数合法性
            if (BuildTarget == BuildTarget.NoTarget)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NoBuildTarget, "Build target platform is not selected.");
                throw new ArgumentException(message, nameof(BuildTarget));
            }
            if (string.IsNullOrEmpty(BuildOutputRoot))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildOutputRootIsNullOrEmpty, "Build output root is null or empty.");
                throw new ArgumentException(message, nameof(BuildOutputRoot));
            }
            if (string.IsNullOrEmpty(BundledFileRoot))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BundledRootIsNullOrEmpty, "Bundled file root is null or empty.");
                throw new ArgumentException(message, nameof(BundledFileRoot));
            }
            if (string.IsNullOrEmpty(BuildPipeline))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildPipelineIsNullOrEmpty, "Build pipeline is null or empty.");
                throw new ArgumentException(message, nameof(BuildPipeline));
            }
            if (BuildBundleType == (int)EBundleType.None)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildBundleTypeIsUnknown, $"Build bundle type is unknown: {BuildBundleType}.");
                throw new ArgumentException(message, nameof(BuildBundleType));
            }
            if (string.IsNullOrEmpty(PackageName))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackageNameIsNullOrEmpty, "Package name is null or empty.");
                throw new ArgumentException(message, nameof(PackageName));
            }
            if (string.IsNullOrEmpty(PackageVersion))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackageVersionIsNullOrEmpty, "Package version is null or empty.");
                throw new ArgumentException(message, nameof(PackageVersion));
            }

            // 设置默认备注信息
            if (string.IsNullOrEmpty(PackageNote))
            {
                PackageNote = DateTime.Now.ToString();
            }

            CheckBuildParametersCore();
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        /// <returns>构建管线的输出目录路径</returns>
        public string GetPipelineOutputDirectory()
        {
            return GetPipelineOutputDirectoryCore();
        }

        /// <summary>
        /// 获取本次构建的补丁输出目录
        /// </summary>
        /// <returns>本次构建的补丁输出目录路径</returns>
        public string GetPackageOutputDirectory()
        {
            return GetPackageOutputDirectoryCore();
        }

        /// <summary>
        /// 获取本次构建的补丁根目录
        /// </summary>
        /// <returns>本次构建的补丁根目录路径</returns>
        public string GetPackageRootDirectory()
        {
            return GetPackageRootDirectoryCore();
        }

        /// <summary>
        /// 获取首包资源的根目录
        /// </summary>
        /// <returns>首包资源的根目录路径</returns>
        public string GetBundledRootDirectory()
        {
            return GetBundledRootDirectoryCore();
        }


        /// <summary>
        /// 执行子类特定的构建参数验证
        /// </summary>
        protected virtual void CheckBuildParametersCore() { }

        /// <summary>
        /// 获取构建管线输出目录的核心实现
        /// </summary>
        /// <returns>构建管线的输出目录路径</returns>
        protected virtual string GetPipelineOutputDirectoryCore()
        {
            return $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{YooAssetSettings.OutputFolderName}";
        }

        /// <summary>
        /// 获取补丁输出目录的核心实现
        /// </summary>
        /// <returns>本次构建的补丁输出目录路径</returns>
        protected virtual string GetPackageOutputDirectoryCore()
        {
            return $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{PackageVersion}";
        }

        /// <summary>
        /// 获取补丁根目录的核心实现
        /// </summary>
        /// <returns>本次构建的补丁根目录路径</returns>
        protected virtual string GetPackageRootDirectoryCore()
        {
            return $"{BuildOutputRoot}/{BuildTarget}/{PackageName}";
        }

        /// <summary>
        /// 获取首包资源根目录的核心实现
        /// </summary>
        /// <returns>首包资源的根目录路径</returns>
        protected virtual string GetBundledRootDirectoryCore()
        {
            return $"{BundledFileRoot}/{PackageName}";
        }
    }
}