using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 获取资源构建映射的任务，负责从收集器生成构建映射上下文。
    /// </summary>
    public class TaskGetBuildMap
    {
        /// <summary>
        /// 生成资源构建上下文
        /// </summary>
        /// <param name="simulateBuild">是否模拟构建</param>
        /// <param name="buildParameters">构建参数</param>
        /// <returns>构建映射上下文</returns>
        public BuildMapContext CreateBuildMap(bool simulateBuild, BuildParameters buildParameters)
        {
            BuildMapContext context = new BuildMapContext();
            var packageName = buildParameters.PackageName;

            Dictionary<string, BuildAssetInfo> allBuildAssetInfos = new Dictionary<string, BuildAssetInfo>(1000);

            // 1. 获取所有收集器收集的资源
            bool useAssetDependencyDB = buildParameters.UseAssetDependencyDB;
            var collectResult = BundleCollectorSettingData.Setting.BeginCollect(packageName, simulateBuild, useAssetDependencyDB);
            List<CollectAssetInfo> allCollectAssets = collectResult.CollectAssets;

            // 2. 剔除未被引用的依赖项资源
            RemoveZeroReferenceAssets(context, allCollectAssets);

            // 3. 录入所有收集器主动收集的资源
            foreach (var collectAssetInfo in allCollectAssets)
            {
                if (allBuildAssetInfos.ContainsKey(collectAssetInfo.AssetInfo.AssetPath))
                {
                    throw new YooInternalException("Should never get here.");
                }

                if (collectAssetInfo.CollectorType != ECollectorType.MainAssetCollector)
                {
                    if (collectAssetInfo.AssetTags.Count > 0)
                    {
                        collectAssetInfo.AssetTags.Clear();
                        string warning = BuildLogger.GetErrorMessage(ErrorCode.RemoveInvalidTags, $"Removed invalid asset tags for asset: '{collectAssetInfo.AssetInfo.AssetPath}'.");
                        BuildLogger.Warning(warning);
                    }
                }

                var buildAssetInfo = new BuildAssetInfo(collectAssetInfo.CollectorType, collectAssetInfo.BundleName, collectAssetInfo.Address, collectAssetInfo.AssetInfo);
                buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
                allBuildAssetInfos.Add(collectAssetInfo.AssetInfo.AssetPath, buildAssetInfo);
            }

            // 4. 录入所有收集资源依赖的其它资源
            foreach (var collectAssetInfo in allCollectAssets)
            {
                string bundleName = collectAssetInfo.BundleName;
                foreach (var dependAsset in collectAssetInfo.DependAssets)
                {
                    if (allBuildAssetInfos.TryGetValue(dependAsset.AssetPath, out var value))
                    {
                        value.AddReferenceBundleName(bundleName);
                    }
                    else
                    {
                        var buildAssetInfo = new BuildAssetInfo(dependAsset);
                        buildAssetInfo.AddReferenceBundleName(bundleName);
                        allBuildAssetInfos.Add(dependAsset.AssetPath, buildAssetInfo);
                    }
                }
            }

            // 5. 填充所有收集资源的依赖列表
            foreach (var collectAssetInfo in allCollectAssets)
            {
                var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
                foreach (var dependAsset in collectAssetInfo.DependAssets)
                {
                    if (allBuildAssetInfos.TryGetValue(dependAsset.AssetPath, out BuildAssetInfo value))
                        dependAssetInfos.Add(value);
                    else
                        throw new YooInternalException("Should never get here.");
                }
                allBuildAssetInfos[collectAssetInfo.AssetInfo.AssetPath].SetDependAssetInfos(dependAssetInfos);
            }

            // 6. 自动收集所有依赖的着色器
            if (collectResult.Command.AutoCollectShaders)
            {
                // 获取着色器打包规则结果
                BundlePackRuleResult shaderPackRuleResult = DefaultBundlePackRule.CreateShadersPackRuleResult();
                string shaderBundleName = shaderPackRuleResult.GetBundleName(collectResult.Command.PackageName, collectResult.Command.UniqueBundleName);
                foreach (var buildAssetInfo in allBuildAssetInfos.Values)
                {
                    if (buildAssetInfo.CollectorType == ECollectorType.None)
                    {
                        if (buildAssetInfo.AssetInfo.IsShaderAsset())
                        {
                            buildAssetInfo.SetBundleName(shaderBundleName);
                        }
                    }
                }
            }

            // 7. 计算共享资源的包名
            if (buildParameters.EnableSharePackRule)
            {
                PreProcessPackShareBundle(buildParameters, collectResult.Command, allBuildAssetInfos);
                foreach (var buildAssetInfo in allBuildAssetInfos.Values)
                {
                    if (buildAssetInfo.HasBundleName() == false)
                    {
                        ProcessingPackShareBundle(buildParameters, collectResult.Command, buildAssetInfo);
                    }
                }
                PostProcessPackShareBundle(buildParameters, collectResult.Command, allBuildAssetInfos);
            }

            // 8. 记录关键信息
            context.AssetFileCount = allBuildAssetInfos.Count;
            context.Command = collectResult.Command;

            // 9. 移除不参与构建的资源
            List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
            foreach (var buildAssetInfo in allBuildAssetInfos.Values)
            {
                if (buildAssetInfo.HasBundleName() == false)
                    removeBuildList.Add(buildAssetInfo);
            }
            foreach (var removeValue in removeBuildList)
            {
                allBuildAssetInfos.Remove(removeValue.AssetInfo.AssetPath);
            }

            // 10. 构建资源列表
            var allPackAssets = allBuildAssetInfos.Values.ToList();
            if (allPackAssets.Count == 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackAssetListIsEmpty, "Pack asset list is empty.");
                throw new InvalidOperationException(message);
            }
            foreach (var assetInfo in allPackAssets)
            {
                context.PackAsset(assetInfo);
            }

            return context;
        }
        private void RemoveZeroReferenceAssets(BuildMapContext context, List<CollectAssetInfo> allCollectAssets)
        {
            // 1. 检测依赖资源收集器是否存在
            if (allCollectAssets.Exists(x => x.CollectorType == ECollectorType.DependAssetCollector) == false)
                return;

            // 2. 获取所有主资源的依赖资源集合
            HashSet<string> allDependAsset = new HashSet<string>();
            foreach (var collectAsset in allCollectAssets)
            {
                var collectorType = collectAsset.CollectorType;
                if (collectorType == ECollectorType.MainAssetCollector || collectorType == ECollectorType.StaticAssetCollector)
                {
                    foreach (var dependAsset in collectAsset.DependAssets)
                    {
                        if (allDependAsset.Contains(dependAsset.AssetPath) == false)
                            allDependAsset.Add(dependAsset.AssetPath);
                    }
                }
            }

            // 3. 找出所有零引用的依赖资源集合
            List<CollectAssetInfo> removeList = new List<CollectAssetInfo>();
            foreach (var collectAssetInfo in allCollectAssets)
            {
                var collectorType = collectAssetInfo.CollectorType;
                if (collectorType == ECollectorType.DependAssetCollector)
                {
                    if (allDependAsset.Contains(collectAssetInfo.AssetInfo.AssetPath) == false)
                        removeList.Add(collectAssetInfo);
                }
            }

            // 4. 移除所有零引用的依赖资源
            foreach (var removeValue in removeList)
            {
                string warning = BuildLogger.GetErrorMessage(ErrorCode.FoundUndependedAsset, $"Found unreferenced asset and removed it: '{removeValue.AssetInfo.AssetPath}'.");
                BuildLogger.Warning(warning);

                var independentAsset = new ReportIndependentAsset();
                independentAsset.AssetPath = removeValue.AssetInfo.AssetPath;
                independentAsset.AssetGuid = removeValue.AssetInfo.AssetGUID;
                independentAsset.AssetType = removeValue.AssetInfo.AssetType.ToString();
                independentAsset.FileSize = FileUtility.GetFileSize(removeValue.AssetInfo.AssetPath);
                context.AddIndependentAsset(independentAsset);
            }
            allCollectAssets.RemoveAll(x => removeList.Contains(x));
        }

        #region 共享资源打包规则
        /// <summary>
        /// 共享资源打包前置处理
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="command">收集命令</param>
        /// <param name="allBuildAssetInfos">全部构建资源信息字典</param>
        protected virtual void PreProcessPackShareBundle(BuildParameters buildParameters, CollectCommand command, Dictionary<string, BuildAssetInfo> allBuildAssetInfos)
        {
        }

        /// <summary>
        /// 共享资源打包机制
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="command">收集命令</param>
        /// <param name="buildAssetInfo">当前处理的构建资源信息</param>
        protected virtual void ProcessingPackShareBundle(BuildParameters buildParameters, CollectCommand command, BuildAssetInfo buildAssetInfo)
        {
            BundlePackRuleResult packRuleResult = GetShareBundleName(buildAssetInfo);
            if (packRuleResult.IsValid() == false)
                return;

            // 处理单个引用的共享资源
            if (buildAssetInfo.GetReferenceBundleCount() <= 1)
            {
                if (buildParameters.SingleReferencedPackAlone == false)
                    return;
            }

            // 设置共享资源包名
            string shareBundleName = packRuleResult.GetShareBundleName(command.PackageName, command.UniqueBundleName);
            buildAssetInfo.SetBundleName(shareBundleName);
        }
        private BundlePackRuleResult GetShareBundleName(BuildAssetInfo buildAssetInfo)
        {
            string bundleName = Path.GetDirectoryName(buildAssetInfo.AssetInfo.AssetPath);
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
            return result;
        }

        /// <summary>
        /// 共享资源打包后置处理
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="command">收集命令</param>
        /// <param name="allBuildAssetInfos">全部构建资源信息字典</param>
        protected virtual void PostProcessPackShareBundle(BuildParameters buildParameters, CollectCommand command, Dictionary<string, BuildAssetInfo> allBuildAssetInfos)
        {
        }
        #endregion

        /// <summary>
        /// 检测原生文件资源包的构建规则
        /// </summary>
        /// <remarks>
        /// 每个原生文件资源包只能包含一个原生文件
        /// </remarks>
        /// <param name="buildMapContext">构建映射上下文</param>
        protected void CheckRawBundleMapContent(BuildMapContext buildMapContext)
        {
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (bundleInfo.AllPackAssets.Count != 1)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotSupportMultipleRawAsset, $"Bundle does not support multiple raw assets: '{bundleInfo.BundleName}'.");
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// 检测归档资源包内每个子文件是否超过最大允许大小
        /// </summary>
        /// <param name="buildMapContext">构建映射上下文</param>
        protected void CheckArchiveBundleMapContent(BuildMapContext buildMapContext)
        {
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                foreach (var asset in bundleInfo.AllPackAssets)
                {
                    string assetPath = asset.AssetInfo.AssetPath;
                    long fileSize = EditorFileUtility.GetFileSize(assetPath);
                    if (fileSize > ArchiveBundleConsts.MaxChildFileSize)
                    {
                        throw new InvalidOperationException(
                            $"Archive child file exceeds maximum size ({ArchiveBundleConsts.MaxChildFileSize} bytes): " +
                            $"'{assetPath}' ({fileSize} bytes) in bundle '{bundleInfo.BundleName}'.");
                    }
                }
            }
        }
    }
}