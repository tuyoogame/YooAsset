using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "BundleCollectorSetting", menuName = "YooAsset/Create Bundle Collector Settings")]
    public class BundleCollectorSetting : ScriptableObject
    {
        /// <summary>
        /// 是否显示包裹列表视图
        /// </summary>
        public bool ShowPackageView = false;

        /// <summary>
        /// 是否显示编辑器别名
        /// </summary>
        public bool ShowEditorAlias = false;

        /// <summary>
        /// 是否启用资源包名唯一化
        /// </summary>
        public bool UniqueBundleName = false;

        /// <summary>
        /// 包裹列表
        /// </summary>
        public List<BundleCollectorPackage> Packages = new List<BundleCollectorPackage>();


        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            ShowPackageView = false;
            UniqueBundleName = false;
            ShowEditorAlias = false;
            Packages.Clear();
        }

        /// <summary>
        /// 检测包裹配置错误
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public void CheckPackageConfigError(string packageName)
        {
            var package = GetPackage(packageName);
            package.CheckConfigError();
        }

        /// <summary>
        /// 检测所有配置错误
        /// </summary>
        public void CheckAllPackageConfigError()
        {
            foreach (var package in Packages)
            {
                package.CheckConfigError();
            }
        }

        /// <summary>
        /// 修复所有配置错误
        /// </summary>
        /// <returns>如果修复了配置错误返回 true</returns>
        public bool FixAllPackageConfigError()
        {
            bool isFixed = false;
            foreach (var package in Packages)
            {
                if (package.FixConfigError())
                {
                    isFixed = true;
                }
            }
            return isFixed;
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>指定包裹的所有资源标签</returns>
        public List<string> GetPackageAllTags(string packageName)
        {
            var package = GetPackage(packageName);
            return package.GetAllTags();
        }

        /// <summary>
        /// 收集指定包裹的资源文件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="simulateBuild">是否模拟构建</param>
        /// <param name="useAssetDependencyDB">是否使用资源依赖数据库</param>
        /// <returns>资源收集结果</returns>
        public CollectResult BeginCollect(string packageName, bool simulateBuild, bool useAssetDependencyDB)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentException("Build package name is null or empty.", nameof(packageName));

            // 检测配置合法性
            var package = GetPackage(packageName);
            package.CheckConfigError();

            // 创建资源收集命令
            IAssetIgnoreRule ignoreRule = BundleCollectorSettingData.GetAssetIgnoreRuleInstance(package.IgnoreRuleName);
            var command = new CollectCommand(packageName, ignoreRule);
            command.SetSimulateBuild(simulateBuild);
            command.UniqueBundleName = UniqueBundleName;
            command.UseAssetDependencyDB = useAssetDependencyDB;
            command.EnableAddressable = package.EnableAddressable;
            command.SupportExtensionless = package.SupportExtensionless;
            command.LocationToLower = package.LocationToLower;
            command.IncludeAssetGUID = package.IncludeAssetGUID;
            command.AutoCollectShaders = package.AutoCollectShaders;

            // 开始收集工作
            var collectAssets = package.GetCollectAssets(command);
            var collectResult = new CollectResult(command, collectAssets);
            return collectResult;
        }

        /// <summary>
        /// 获取指定名称的收集器包裹
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>包裹对象</returns>
        public BundleCollectorPackage GetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));

            foreach (var package in Packages)
            {
                if (package.PackageName == packageName)
                    return package;
            }
            throw new ArgumentException($"Package not found: '{packageName}'.", nameof(packageName));
        }
    }
}