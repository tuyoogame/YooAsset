using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class TaskGetBuildMap
	{
		/// <summary>
		/// 生成资源构建上下文
		/// </summary>
		public BuildMapContext CreateBuildMap(BuildParameters buildParameters)
		{
			var buildMode = buildParameters.BuildMode;
			var packageName = buildParameters.PackageName;

			Dictionary<string, BuildAssetInfo> allBuildAssetInfos = new Dictionary<string, BuildAssetInfo>(1000);

			// 1. 获取所有收集器收集的资源
			var collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
			List<CollectAssetInfo> allCollectAssetInfos = collectResult.CollectAssets;

			// 2. 剔除未被引用的依赖项资源
			RemoveZeroReferenceAssets(allCollectAssetInfos);

			// 3. 录入所有收集器收集的资源
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				if (allBuildAssetInfos.ContainsKey(collectAssetInfo.AssetPath) == false)
				{
					if (collectAssetInfo.CollectorType != ECollectorType.MainAssetCollector)
					{
						if (collectAssetInfo.AssetTags.Count > 0)
						{
							collectAssetInfo.AssetTags.Clear();
							BuildLogger.Warning($"The tags has been cleared ! {collectAssetInfo.AssetPath} ");
						}
					}

					var buildAssetInfo = new BuildAssetInfo(collectAssetInfo.CollectorType, collectAssetInfo.BundleName, collectAssetInfo.Address, collectAssetInfo.AssetPath);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					allBuildAssetInfos.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 4. 录入所有收集资源的依赖资源
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				string bundleName = collectAssetInfo.BundleName;
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfos.ContainsKey(dependAssetPath))
					{
						allBuildAssetInfos[dependAssetPath].AddReferenceBundleName(bundleName);
					}
					else
					{
						var buildAssetInfo = new BuildAssetInfo(dependAssetPath);
						buildAssetInfo.AddReferenceBundleName(bundleName);
						allBuildAssetInfos.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}

			// 5. 填充所有收集资源的依赖列表
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (allBuildAssetInfos.TryGetValue(dependAssetPath, out BuildAssetInfo value))
						dependAssetInfos.Add(value);
					else
						throw new Exception("Should never get here !");
				}
				allBuildAssetInfos[collectAssetInfo.AssetPath].SetDependAssetInfos(dependAssetInfos);
			}

			// 6. 记录关键信息
			BuildMapContext context = new BuildMapContext();
			context.AssetFileCount = allBuildAssetInfos.Count;
			context.Command = collectResult.Command;

			// 7. 记录冗余资源
			foreach (var buildAssetInfo in allBuildAssetInfos.Values)
			{
				if (buildAssetInfo.IsRedundancyAsset())
				{
					var redundancyInfo = new ReportRedundancyInfo();
					redundancyInfo.AssetPath = buildAssetInfo.AssetPath;
					redundancyInfo.AssetType = buildAssetInfo.AssetType.Name;
					redundancyInfo.AssetGUID = buildAssetInfo.AssetGUID;
					redundancyInfo.FileSize = FileUtility.GetFileSize(buildAssetInfo.AssetPath);
					redundancyInfo.Number = buildAssetInfo.GetReferenceBundleCount();
					context.RedundancyInfos.Add(redundancyInfo);
				}
			}

			// 8. 移除不参与构建的资源
			List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
			foreach (var buildAssetInfo in allBuildAssetInfos.Values)
			{
				if (buildAssetInfo.HasBundleName() == false)
					removeBuildList.Add(buildAssetInfo);
			}
			foreach (var removeValue in removeBuildList)
			{
				allBuildAssetInfos.Remove(removeValue.AssetPath);
			}

			// 9. 构建资源列表
			var allPackAssets = allBuildAssetInfos.Values.ToList();
			if (allPackAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allPackAssets)
			{
				context.PackAsset(assetInfo);
			}

			return context;
		}
		private void RemoveZeroReferenceAssets(List<CollectAssetInfo> allCollectAssetInfos)
		{
			// 1. 检测是否任何存在依赖资源
			if (allCollectAssetInfos.Exists(x => x.CollectorType == ECollectorType.DependAssetCollector) == false)
				return;

			// 2. 获取所有主资源的依赖资源集合
			HashSet<string> allDependAsset = new HashSet<string>();
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.MainAssetCollector || collectorType == ECollectorType.StaticAssetCollector)
				{
					foreach (var dependAsset in collectAssetInfo.DependAssets)
					{
						if (allDependAsset.Contains(dependAsset) == false)
							allDependAsset.Add(dependAsset);
					}
				}
			}

			// 3. 找出所有零引用的依赖资源集合
			List<CollectAssetInfo> removeList = new List<CollectAssetInfo>();
			foreach (var collectAssetInfo in allCollectAssetInfos)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.DependAssetCollector)
				{
					if (allDependAsset.Contains(collectAssetInfo.AssetPath) == false)
						removeList.Add(collectAssetInfo);
				}
			}

			// 4. 移除所有零引用的依赖资源
			foreach (var removeValue in removeList)
			{
				BuildLogger.Warning($"发现未被依赖的资源并自动移除 : {removeValue.AssetPath}");
				allCollectAssetInfos.Remove(removeValue);
			}
		}
	}
}