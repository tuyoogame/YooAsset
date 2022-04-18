using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public static class BuildMapHelper
	{
		/// <summary>
		/// 执行资源构建上下文
		/// </summary>
		public static BuildMapContext SetupBuildMap()
		{
			BuildMapContext context = new BuildMapContext();
			Dictionary<string, BuildAssetInfo> buildAssetDic = new Dictionary<string, BuildAssetInfo>();

			// 0. 检测配置合法性
			AssetBundleGrouperSettingData.Setting.CheckConfigError();

			// 1. 获取主动收集的资源
			List<CollectAssetInfo> collectAssetInfos = AssetBundleGrouperSettingData.Setting.GetAllCollectAssets();

			// 2. 录入主动收集的资源
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				if (buildAssetDic.ContainsKey(collectAssetInfo.AssetPath) == false)
				{
					var buildAssetInfo = new BuildAssetInfo(collectAssetInfo.AssetPath, collectAssetInfo.IsRawAsset, collectAssetInfo.NotWriteToAssetList);
					buildAssetInfo.SetBundleName(collectAssetInfo.BundleName);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					buildAssetDic.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 3. 录入并分析依赖资源
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (buildAssetDic.ContainsKey(dependAssetPath))
					{
						buildAssetDic[dependAssetPath].DependCount++;
						buildAssetDic[dependAssetPath].AddAssetTags(collectAssetInfo.AssetTags);
					}
					else
					{
						var buildAssetInfo = new BuildAssetInfo(dependAssetPath);
						buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
						buildAssetDic.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}
			context.AssetFileCount = buildAssetDic.Count;

			// 4. 设置主动收集资源的依赖列表
			foreach (var collectAssetInfo in collectAssetInfos)
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

			// 5. 移除零依赖的资源
			List<BuildAssetInfo> removeList = new List<BuildAssetInfo>();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.IsCollectAsset)
					continue;

				if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
				{
					if (buildAssetInfo.IsShaderAsset)
						continue;
				}

				if (buildAssetInfo.DependCount == 0)
					removeList.Add(buildAssetInfo);
			}
			foreach (var removeValue in removeList)
			{
				buildAssetDic.Remove(removeValue.AssetPath);
			}

			// 6. 设置未命名的资源包
			IPackRule defaultPackRule = new PackDirectory();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.BundleNameIsValid() == false)
				{
					string shaderBundleName = AssetBundleCollector.CollectShaderBundleName(buildAssetInfo.AssetPath);
					if (string.IsNullOrEmpty(shaderBundleName) == false)
					{
						buildAssetInfo.SetBundleName(shaderBundleName);
					}
					else
					{
						string bundleName = defaultPackRule.GetBundleName(new PackRuleData(buildAssetInfo.AssetPath));
						bundleName = AssetBundleCollector.CorrectBundleName(bundleName, false);
						buildAssetInfo.SetBundleName(bundleName);
					}
				}
			}

			// 7. 构建资源包
			var allBuildAssets = buildAssetDic.Values.ToList();
			if (allBuildAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allBuildAssets)
			{
				context.PackAsset(assetInfo);
			}
			return context;
		}
	}
}