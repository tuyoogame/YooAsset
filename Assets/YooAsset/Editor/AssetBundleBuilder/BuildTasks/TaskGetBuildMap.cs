using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("获取资源构建内容")]
	public class TaskGetBuildMap : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = CreateBuildMap(buildParametersContext.Parameters.BuildMode, buildParametersContext.Parameters.PackageName);
			context.SetContextObject(buildMapContext);
			BuildLogger.Log("构建内容准备完毕！");

			// 检测构建结果
			CheckBuildMapContent(buildMapContext);
		}

		/// <summary>
		/// 资源构建上下文
		/// </summary>
		public BuildMapContext CreateBuildMap(EBuildMode buildMode, string packageName)
		{
			Dictionary<string, BuildAssetInfo> buildAssetInfoDic = new Dictionary<string, BuildAssetInfo>(1000);

			// 1. 检测配置合法性
			AssetBundleCollectorSettingData.Setting.CheckConfigError();

			// 2. 获取所有收集器收集的资源
			var collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
			List<CollectAssetInfo> collectAssetInfos = collectResult.CollectAssets;

			// 3. 剔除未被引用的依赖项资源
			List<CollectAssetInfo> removeDependList = new List<CollectAssetInfo>();
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				if (collectAssetInfo.CollectorType == ECollectorType.DependAssetCollector)
				{
					if (IsRemoveDependAsset(collectAssetInfos, collectAssetInfo.AssetPath))
						removeDependList.Add(collectAssetInfo);
				}
			}
			foreach (var removeValue in removeDependList)
			{
				collectAssetInfos.Remove(removeValue);
			}

			// 4. 录入所有收集器收集的资源
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				if (buildAssetInfoDic.ContainsKey(collectAssetInfo.AssetPath) == false)
				{
					var buildAssetInfo = new BuildAssetInfo(
						collectAssetInfo.CollectorType, collectAssetInfo.BundleName,
						collectAssetInfo.Address, collectAssetInfo.AssetPath, collectAssetInfo.IsRawAsset);
					buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
					buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
					buildAssetInfoDic.Add(collectAssetInfo.AssetPath, buildAssetInfo);
				}
				else
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 5. 录入所有收集资源的依赖资源
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				string collectAssetBundleName = collectAssetInfo.BundleName;
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (buildAssetInfoDic.ContainsKey(dependAssetPath))
					{
						buildAssetInfoDic[dependAssetPath].AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetInfoDic[dependAssetPath].AddReferenceBundleName(collectAssetBundleName);
					}
					else
					{
						var buildAssetInfo = new BuildAssetInfo(dependAssetPath);
						buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
						buildAssetInfo.AddReferenceBundleName(collectAssetBundleName);
						buildAssetInfoDic.Add(dependAssetPath, buildAssetInfo);
					}
				}
			}

			// 6. 填充所有收集资源的依赖列表
			foreach (var collectAssetInfo in collectAssetInfos)
			{
				var dependAssetInfos = new List<BuildAssetInfo>(collectAssetInfo.DependAssets.Count);
				foreach (var dependAssetPath in collectAssetInfo.DependAssets)
				{
					if (buildAssetInfoDic.TryGetValue(dependAssetPath, out BuildAssetInfo value))
						dependAssetInfos.Add(value);
					else
						throw new Exception("Should never get here !");
				}
				buildAssetInfoDic[collectAssetInfo.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
			}

			// 7. 记录关键信息
			BuildMapContext context = new BuildMapContext();
			context.AssetFileCount = buildAssetInfoDic.Count;
			context.EnableAddressable = collectResult.Command.EnableAddressable;
			context.UniqueBundleName = collectResult.Command.UniqueBundleName;
			context.ShadersBundleName = collectResult.Command.ShadersBundleName;

			// 8. 计算共享的资源包名
			var command = collectResult.Command;
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetInfoDic)
			{
				pair.Value.CalculateShareBundleName(command.UniqueBundleName, command.PackageName, command.ShadersBundleName);
			}

			// 9. 移除不参与构建的资源
			List<BuildAssetInfo> removeBuildList = new List<BuildAssetInfo>();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetInfoDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.HasBundleName() == false)
					removeBuildList.Add(buildAssetInfo);
			}
			foreach (var removeValue in removeBuildList)
			{
				buildAssetInfoDic.Remove(removeValue.AssetPath);
			}

			// 10. 构建资源包
			var allBuildinAssets = buildAssetInfoDic.Values.ToList();
			if (allBuildinAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allBuildinAssets)
			{
				context.PackAsset(assetInfo);
			}
			return context;
		}
		private bool IsRemoveDependAsset(List<CollectAssetInfo> allCollectAssets, string dependAssetPath)
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

			BuildLogger.Log($"发现未被依赖的资源并自动移除 : {dependAssetPath}");
			return true;
		}

		/// <summary>
		/// 检测构建结果
		/// </summary>
		private void CheckBuildMapContent(BuildMapContext buildMapContext)
		{
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{
					if (bundleInfo.BuildinAssets.Count != 1)
						throw new Exception($"The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
					continue;
				}

				// 注意：原生文件不能被其它资源文件依赖
				foreach (var assetInfo in bundleInfo.BuildinAssets)
				{
					if (assetInfo.AllDependAssetInfos != null)
					{
						foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
						{
							if (dependAssetInfo.IsRawAsset)
								throw new Exception($"{assetInfo.AssetPath} can not depend raw asset : {dependAssetInfo.AssetPath}");
						}
					}
				}
			}
		}
	}
}