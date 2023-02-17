using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public static class BuildMapCreater
	{
		/// <summary>
		/// 执行资源构建上下文
		/// </summary>
		public static BuildMapContext CreateBuildMap(EBuildMode buildMode, string packageName)
		{	
			Dictionary<string, BuildAssetInfo> buildAssetDic = new Dictionary<string, BuildAssetInfo>(1000);

			// 1. 检测配置合法性
			AssetBundleCollectorSettingData.Setting.CheckConfigError();

			// 2. 获取所有收集器收集的资源
			var buildResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
			List<CollectAssetInfo> allCollectAssets = buildResult.CollectAssets;

			// 3. 剔除未被引用的依赖项资源
			List<CollectAssetInfo> removeDependList = new List<CollectAssetInfo>();
			foreach (var collectAssetInfo in allCollectAssets)
			{
				if (collectAssetInfo.CollectorType == ECollectorType.DependAssetCollector)
				{
					if (IsRemoveDependAsset(allCollectAssets, collectAssetInfo.AssetPath))
						removeDependList.Add(collectAssetInfo);
				}
			}
			foreach (var removeValue in removeDependList)
			{
				allCollectAssets.Remove(removeValue);
			}

			// 4. 录入所有收集器收集的资源
			foreach (var collectAssetInfo in allCollectAssets)
			{
				if (buildAssetDic.ContainsKey(collectAssetInfo.AssetPath) == false)
				{
					var buildAssetInfo = new BuildAssetInfo(
						collectAssetInfo.CollectorType, collectAssetInfo.BundleName,
						collectAssetInfo.Address, collectAssetInfo.AssetPath, collectAssetInfo.IsRawAsset);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
					buildAssetDic.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 5. 录入所有收集资源的依赖资源
			foreach (var collectAssetInfo in allCollectAssets)
			{
				string collectAssetBundleName = collectAssetInfo.BundleName;
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (buildAssetDic.ContainsKey(dependAssetPath))
					{
						buildAssetDic[dependAssetPath].AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetDic[dependAssetPath].AddReferenceBundleName(collectAssetBundleName);
					}
					else
					{
						var buildAssetInfo = new BuildAssetInfo(dependAssetPath);
						buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetInfo.AddReferenceBundleName(collectAssetBundleName);
						buildAssetDic.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}

			// 6. 填充所有收集资源的依赖列表
			foreach (var collectAssetInfo in allCollectAssets)
			{
				var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (buildAssetDic.TryGetValue(dependAssetPath, out BuildAssetInfo value))
						dependAssetInfos.Add(value);
					else
						throw new Exception("Should never get here !");
				}
				buildAssetDic[collectAssetInfo.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
			}

			// 7. 记录关键信息
			BuildMapContext context = new BuildMapContext();
			context.AssetFileCount = buildAssetDic.Count;
			context.EnableAddressable = buildResult.Command.EnableAddressable;
			context.UniqueBundleName = buildResult.Command.UniqueBundleName;
			context.ShadersBundleName = buildResult.ShadersBundleName;

			// 8. 计算共享的资源包名
			var command = buildResult.Command;
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				pair.Value.CalculateShareBundleName(command.UniqueBundleName, command.PackageName, buildResult.ShadersBundleName);
			}

			// 9. 移除不参与构建的资源
			List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.HasBundleName() == false)
					removeBuildList.Add(buildAssetInfo);
			}
			foreach (var removeValue in removeBuildList)
			{
				buildAssetDic.Remove(removeValue.AssetPath);
			}

			// 10. 构建资源包
			var allBuildinAssets = buildAssetDic.Values.ToList();
			if (allBuildinAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allBuildinAssets)
			{
				context.PackAsset(assetInfo);
			}
			return context;
		}
		private static bool IsRemoveDependAsset(List<CollectAssetInfo> allCollectAssets, string dependAssetPath)
		{
			foreach (var collectAssetInfo in allCollectAssets)
			{
				var collectorType = collectAssetInfo.CollectorType;
				if (collectorType == ECollectorType.MainAssetCollector || collectorType == ECollectorType.StaticAssetCollector)
				{
					if (collectAssetInfo.DependAssets.Contains(dependAssetPath))
						return false;
				}
			}

			BuildRunner.Log($"发现未被依赖的资源并自动移除 : {dependAssetPath}");
			return true;
		}
	}
}