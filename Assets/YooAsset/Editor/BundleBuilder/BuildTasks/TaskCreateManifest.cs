using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源清单上下文，用于在构建流程中传递反序列化后的补丁清单。
    /// </summary>
    [ContextObject]
    public class ManifestContext
    {
        internal BuildManifest Manifest;
    }

    /// <summary>
    /// 创建补丁清单的任务抽象基类，负责序列化清单并写入输出目录。
    /// </summary>
    public abstract class TaskCreateManifest
    {
        private readonly Dictionary<string, int> _cachedBundleIndexIDs = new Dictionary<string, int>(10000);
        private readonly Dictionary<int, HashSet<string>> _cacheBundleTags = new Dictionary<int, HashSet<string>>(10000);

        /// <summary>
        /// 创建补丁清单文件到输出目录
        /// </summary>
        /// <param name="processBundleDepends">是否处理资源包依赖关系</param>
        /// <param name="processBundleTags">是否处理资源包标签</param>
        /// <param name="replaceAssetPathWithAddress">是否用可寻址地址替换资源路径</param>
        /// <param name="context">构建上下文</param>
        protected void CreateManifestFile(bool processBundleDepends, bool processBundleTags, bool replaceAssetPathWithAddress, BuildContext context)
        {
            _cachedBundleIndexIDs.Clear();
            _cacheBundleTags.Clear();

            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

            // 检测资源包哈希冲突
            CheckBundleHashConflict(buildMapContext);

            // 创建新补丁清单
            BuildManifest manifest = new BuildManifest();
            manifest.FileVersion = PackageManifestConsts.FileVersion;
            manifest.EnableAddressable = buildMapContext.Command.EnableAddressable;
            manifest.SupportExtensionless = buildMapContext.Command.SupportExtensionless;
            manifest.LocationToLower = buildMapContext.Command.LocationToLower;
            manifest.IncludeAssetGuid = buildMapContext.Command.IncludeAssetGUID;
            manifest.ReplaceAssetPathWithAddress = replaceAssetPathWithAddress;
            manifest.OutputNameStyle = (int)buildParameters.FileNameStyle;
            manifest.BuildBundleType = buildParameters.BuildBundleType;
            manifest.BuildPipeline = buildParameters.BuildPipeline;
            manifest.PackageName = buildParameters.PackageName;
            manifest.PackageVersion = buildParameters.PackageVersion;
            manifest.PackageNote = buildParameters.PackageNote;
            manifest.AssetList = CreatePackageAssetList(buildMapContext);
            manifest.BundleList = CreatePackageBundleList(buildMapContext);

            // 1. 处理资源清单的资源对象
            ProcessPackageAsset(manifest);

            // 2. 处理资源包的依赖列表
            if (processBundleDepends)
                ProcessBundleDepends(context, manifest);

            // 3. 处理资源包的标签集合
            if (processBundleTags)
                ProcessBundleTags(manifest);

            // 4. 处理首包资源包
            if (processBundleDepends)
            {
                // 注意：建立资源包引用关系图
                manifest.BuildReferrerGraph();

                ProcessBuiltinBundleDependency(context, manifest);
            }

            // 5. 构建全局标签表与目录表
            manifest.BuildTagTable();
            manifest.BuildDirectoryTable();

            // 创建资源清单二进制文件
            string packageHash;
            string packagePath;
            {
                string fileName = YooAssetConfiguration.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                packagePath = $"{packageOutputDirectory}/{fileName}";
                PackageManifestHelper.SerializeManifestToBinary(packagePath, manifest, buildParameters.ManifestEncryptor);
                packageHash = HashUtility.ComputeFileCrc32(packagePath);
                BuildLogger.Log($"Create package manifest file: '{packagePath}'.");
            }

            // 创建资源清单哈希文件
            {
                string fileName = YooAssetConfiguration.GetPackageHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, packageHash);
                BuildLogger.Log($"Create package manifest hash file: '{filePath}'.");
            }

            // 创建资源清单版本文件
            {
                string fileName = YooAssetConfiguration.GetPackageVersionFileName(buildParameters.PackageName);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, buildParameters.PackageVersion);
                BuildLogger.Log($"Create package manifest version file: '{filePath}'.");
            }

            // 填充上下文
            {
                ManifestContext manifestContext = new ManifestContext();
                manifestContext.Manifest = manifest;
                context.SetContextObject(manifestContext);
            }
        }

        /// <summary>
        /// 检测资源包哈希冲突
        /// </summary>
        private void CheckBundleHashConflict(BuildMapContext buildMapContext)
        {
            // 说明：在特殊情况下，例如某些文件加密算法会导致加密后的文件哈希值冲突！
            // 说明：二进制完全相同的原生文件也会冲突！
            HashSet<string> guids = new HashSet<string>();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (guids.Contains(bundleInfo.PackageFileHash))
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.BundleHashConflict, $"Bundle hash conflict: '{bundleInfo.BundleName}'.");
                    throw new InvalidOperationException(message);
                }
                else
                {
                    guids.Add(bundleInfo.PackageFileHash);
                }
            }
        }

        /// <summary>
        /// 获取资源包的依赖集合
        /// </summary>
        /// <param name="context">构建上下文</param>
        /// <param name="bundleName">资源包名称</param>
        /// <returns>依赖的资源包名称数组</returns>
        protected abstract string[] GetBundleDepends(BuildContext context, string bundleName);

        /// <summary>
        /// 创建资源对象列表
        /// </summary>
        private List<BuildAsset> CreatePackageAssetList(BuildMapContext buildMapContext)
        {
            List<BuildAsset> result = new List<BuildAsset>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var assetInfos = bundleInfo.GetAllManifestAssetInfos();
                foreach (var assetInfo in assetInfos)
                {
                    BuildAsset buildAsset = new BuildAsset();
                    buildAsset.Address = buildMapContext.Command.EnableAddressable ? assetInfo.Address : string.Empty;
                    buildAsset.AssetPath = assetInfo.AssetInfo.AssetPath;
                    buildAsset.AssetGuid = buildMapContext.Command.IncludeAssetGUID ? assetInfo.AssetInfo.AssetGUID : string.Empty;
                    buildAsset.Tags = assetInfo.AssetTags.ToArray();
                    buildAsset.EditorUserData = assetInfo;
                    result.Add(buildAsset);
                }
            }

            // 按照AssetPath排序
            result.Sort((a, b) => a.AssetPath.CompareTo(b.AssetPath));
            return result;
        }

        /// <summary>
        /// 创建资源包列表
        /// </summary>
        private List<BuildBundle> CreatePackageBundleList(BuildMapContext buildMapContext)
        {
            List<BuildBundle> result = new List<BuildBundle>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var buildBundle = bundleInfo.CreateBuildBundle();
                result.Add(buildBundle);
            }

            // 按照BundleName排序
            result.Sort((a, b) => a.BundleName.CompareTo(b.BundleName));
            return result;
        }

        /// <summary>
        /// 处理资源清单的资源对象列表
        /// </summary>
        private void ProcessPackageAsset(BuildManifest manifest)
        {
            // 注意：优先缓存资源包索引
            for (int index = 0; index < manifest.BundleList.Count; index++)
            {
                string bundleName = manifest.BundleList[index].BundleName;
                _cachedBundleIndexIDs.Add(bundleName, index);
            }

            // 记录资源对象所属的资源包ID
            foreach (var buildAsset in manifest.AssetList)
            {
                var assetInfo = buildAsset.EditorUserData as BuildAssetInfo;
                buildAsset.BundleID = GetCachedBundleIndexID(assetInfo.BundleName);
            }

            // 记录资源对象依赖的资源包ID集合
            // 注意：依赖关系非引擎构建结果里查询！
            foreach (var buildAsset in manifest.AssetList)
            {
                var mainAssetInfo = buildAsset.EditorUserData as BuildAssetInfo;
                buildAsset.DependentBundleIDs = GetAssetDependBundleIDs(mainAssetInfo);
            }
        }

        /// <summary>
        /// 处理资源包的依赖集合
        /// </summary>
        private void ProcessBundleDepends(BuildContext context, BuildManifest manifest)
        {
            // 查询引擎生成的资源包依赖关系，然后记录到清单
            foreach (var buildBundle in manifest.BundleList)
            {
                int mainBundleID = GetCachedBundleIndexID(buildBundle.BundleName);
                string[] dependNames = GetBundleDepends(context, buildBundle.BundleName);
                List<int> dependIDs = new List<int>(dependNames.Length);
                foreach (var dependName in dependNames)
                {
                    int dependBundleID = GetCachedBundleIndexID(dependName);
                    if (dependBundleID != mainBundleID)
                        dependIDs.Add(dependBundleID);
                }

                // 排序并填充数据
                dependIDs.Sort();
                buildBundle.DependentBundleIDs = dependIDs.ToArray();
            }
        }

        /// <summary>
        /// 处理资源包的标签集合
        /// </summary>
        private void ProcessBundleTags(BuildManifest manifest)
        {
            foreach (var buildBundle in manifest.BundleList)
            {
                buildBundle.Tags = Array.Empty<string>();
            }

            // 将主资源的标签信息传染给其依赖的资源包集合
            foreach (var buildAsset in manifest.AssetList)
            {
                var assetTags = buildAsset.Tags;
                int bundleID = buildAsset.BundleID;
                CacheBundleTags(bundleID, assetTags);
                if (buildAsset.DependentBundleIDs != null)
                {
                    foreach (var dependBundleID in buildAsset.DependentBundleIDs)
                    {
                        CacheBundleTags(dependBundleID, assetTags);
                    }
                }
            }

            // 将缓存的资源标签赋值给资源包
            for (int index = 0; index < manifest.BundleList.Count; index++)
            {
                var buildBundle = manifest.BundleList[index];
                if (_cacheBundleTags.TryGetValue(index, out var value))
                {
                    buildBundle.Tags = value.ToArray();
                }
                else
                {
                    // 注意：SBP构建管线会自动剔除一些冗余资源的引用关系，导致游离资源包没有被任何主资源包引用。
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.FoundStrayBundle, $"Found stray bundle. Bundle ID: {index}, bundle name: '{buildBundle.BundleName}'.");
                    BuildLogger.Warning(warning);
                }
            }
        }
        private void CacheBundleTags(int bundleID, string[] assetTags)
        {
            if (_cacheBundleTags.ContainsKey(bundleID) == false)
                _cacheBundleTags.Add(bundleID, new HashSet<string>());

            foreach (var assetTag in assetTags)
            {
                if (_cacheBundleTags[bundleID].Contains(assetTag) == false)
                    _cacheBundleTags[bundleID].Add(assetTag);
            }
        }

        /// <summary>
        /// 获取缓存的资源包的索引ID
        /// </summary>
        private int GetCachedBundleIndexID(string bundleName)
        {
            if (_cachedBundleIndexIDs.TryGetValue(bundleName, out int value) == false)
            {
                throw new YooInternalException($"Should never get here. Bundle index ID not found: '{bundleName}'.");
            }
            return value;
        }

        /// <summary>
        /// 是否包含该资源包的索引ID
        /// </summary>
        private bool ContainsCachedBundleIndexID(string bundleName)
        {
            return _cachedBundleIndexIDs.ContainsKey(bundleName);
        }

        #region YOOASSET_LEGACY_DEPENDENCY
        private void ProcessBuiltinBundleDependency(BuildContext context, BuildManifest manifest)
        {
            // 注意：如果是可编程构建管线，需要补充首包资源包
            // 注意：该步骤依赖前面的操作！
            var buildResultContext = context.TryGetContextObject<TaskBuilding_SBP.BuildResultContext>();

            if (buildResultContext != null)
            {
                ProcessBuiltinBundleReference(manifest, buildResultContext.BuiltinShadersBundleName);
                ProcessBuiltinBundleReference(manifest, buildResultContext.MonoScriptsBundleName);

                var buildParametersContext = context.TryGetContextObject<BuildParametersContext>();
                var buildParameters = buildParametersContext.Parameters;
                if (buildParameters is ScriptableBuildParameters scriptableBuildParameters)
                {
                    if (scriptableBuildParameters.TrackSpriteAtlasDependencies)
                    {
                        // 注意：检测是否开启图集模式
                        // 说明：需要记录主资源对象对图集的依赖关系！
                        if (EditorSettings.spritePackerMode != SpritePackerMode.Disabled)
                        {
                            var buildMapContext = context.GetContextObject<BuildMapContext>();
                            foreach (var spriteAtlasAsset in buildMapContext.SpriteAtlasAssetList)
                            {
                                string spriteAtlasBundleName = spriteAtlasAsset.BundleName;
                                ProcessBuiltinBundleReference(manifest, spriteAtlasBundleName);
                            }
                        }
                    }
                }
            }
        }
        private void ProcessBuiltinBundleReference(BuildManifest manifest, string builtinBundleName)
        {
            if (string.IsNullOrEmpty(builtinBundleName))
                return;

            // 查询首包资源包是否存在
            if (ContainsCachedBundleIndexID(builtinBundleName) == false)
                return;

            // 获取首包资源包
            int builtinBundleID = GetCachedBundleIndexID(builtinBundleName);
            var builtinBuildBundle = manifest.BundleList[builtinBundleID];

            // 更新依赖资源包ID集合
            HashSet<int> cacheBundleIDs = new HashSet<int>(builtinBuildBundle.ReferrerBundleIDs);
            HashSet<string> tempTags = new HashSet<string>();
            foreach (var buildAsset in manifest.AssetList)
            {
                if (cacheBundleIDs.Contains(buildAsset.BundleID))
                {
                    if (buildAsset.DependentBundleIDs.Contains(builtinBundleID) == false)
                    {
                        var tempBundleIDs = new List<int>(buildAsset.DependentBundleIDs);
                        tempBundleIDs.Add(builtinBundleID);
                        buildAsset.DependentBundleIDs = tempBundleIDs.ToArray();
                    }

                    foreach (var tag in buildAsset.Tags)
                    {
                        if (tempTags.Contains(tag) == false)
                            tempTags.Add(tag);
                    }
                }
            }

            // 更新首包资源包的标签集合
            foreach (var tag in builtinBuildBundle.Tags)
            {
                if (tempTags.Contains(tag) == false)
                    tempTags.Add(tag);
            }
            builtinBuildBundle.Tags = tempTags.ToArray();
        }
        private int[] GetAssetDependBundleIDs(BuildAssetInfo mainAssetInfo)
        {
            HashSet<int> result = new HashSet<int>();
            int mainBundleID = GetCachedBundleIndexID(mainAssetInfo.BundleName);
            foreach (var dependAssetInfo in mainAssetInfo.AllDependAssetInfos)
            {
                if (dependAssetInfo.HasBundleName())
                {
                    int bundleID = GetCachedBundleIndexID(dependAssetInfo.BundleName);
                    if (mainBundleID != bundleID)
                    {
                        if (result.Contains(bundleID) == false)
                            result.Add(bundleID);
                    }
                }
            }

            // 排序并返回数据
            List<int> listResult = new List<int>(result);
            listResult.Sort();
            return listResult.ToArray();
        }
        #endregion
    }
}