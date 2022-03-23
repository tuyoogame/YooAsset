using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class TaskGetBuildMap : IBuildTask
	{
		public class BuildMapContext : IContextObject
		{
			/// <summary>
			/// 参与构建的资源总数
			/// 说明：包括主动收集的资源以及其依赖的所有资源
			/// </summary>
			public int AssetFileCount;

			/// <summary>
			/// 资源包列表
			/// </summary>
			public readonly List<BuildBundleInfo> BundleInfos = new List<BuildBundleInfo>(1000);

			/// <summary>
			/// 冗余的资源列表
			/// </summary>
			public readonly List<string> RedundancyAssetList = new List<string>(1000);


			/// <summary>
			/// 添加一个打包资源
			/// </summary>
			public void PackAsset(BuildAssetInfo assetInfo)
			{
				if (TryGetBundleInfo(assetInfo.BundleName, out BuildBundleInfo bundleInfo))
				{
					bundleInfo.PackAsset(assetInfo);
				}
				else
				{
					BuildBundleInfo newBundleInfo = new BuildBundleInfo(assetInfo.BundleName);
					newBundleInfo.PackAsset(assetInfo);
					BundleInfos.Add(newBundleInfo);
				}
			}

			/// <summary>
			/// 获取所有的打包资源
			/// </summary>
			public List<BuildAssetInfo> GetAllAssets()
			{
				List<BuildAssetInfo> result = new List<BuildAssetInfo>(BundleInfos.Count);
				foreach (var bundleInfo in BundleInfos)
				{
					result.AddRange(bundleInfo.BuildinAssets);
				}
				return result;
			}

			/// <summary>
			/// 获取AssetBundle内包含的标记列表
			/// </summary>
			public string[] GetAssetTags(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo))
				{
					return bundleInfo.GetAssetTags();
				}
				throw new Exception($"Not found {nameof(BuildBundleInfo)} : {bundleFullName}");
			}

			/// <summary>
			/// 获取AssetBundle内构建的资源路径列表
			/// </summary>
			public string[] GetBuildinAssetPaths(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo))
				{
					return bundleInfo.GetBuildinAssetPaths();
				}
				throw new Exception($"Not found {nameof(BuildBundleInfo)} : {bundleFullName}");
			}

			/// <summary>
			/// 获取构建管线里需要的数据
			/// </summary>
			public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
			{
				List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(BundleInfos.Count);
				foreach (var bundleInfo in BundleInfos)
				{
					if (bundleInfo.IsRawFile == false)
						builds.Add(bundleInfo.CreatePipelineBuild());
				}
				return builds.ToArray();
			}

			/// <summary>
			/// 是否包含资源包
			/// </summary>
			public bool IsContainsBundle(string bundleFullName)
			{
				return TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo);
			}

			public bool TryGetBundleInfo(string bundleFullName, out BuildBundleInfo result)
			{
				foreach (var bundleInfo in BundleInfos)
				{
					if (bundleInfo.BundleName == bundleFullName)
					{
						result = bundleInfo;
						return true;
					}
				}
				result = null;
				return false;
			}
		}


		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			BuildMapContext buildMapContext = new BuildMapContext();
			context.SetContextObject(buildMapContext);
			SetupBuildMap(buildMapContext, buildParametersContext);

			// 检测构建结果
			CheckBuildMapContent(buildMapContext);
		}

		/// <summary>
		/// 组织构建的资源包
		/// </summary>
		private void SetupBuildMap(BuildMapContext buildMapContext, AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			Dictionary<string, BuildAssetInfo> buildAssetDic = new Dictionary<string, BuildAssetInfo>();

			// 1. 获取主动收集的资源
			List<CollectAssetInfo> allCollectInfos = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 2. 对收集的资源进行依赖分析
			int progressValue = 0;
			foreach (CollectAssetInfo collectInfo in allCollectInfos)
			{
				string mainAssetPath = collectInfo.AssetPath;

				// 获取所有依赖资源
				List<BuildAssetInfo> depends = GetAllDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					string assetPath = depends[i].AssetPath;

					// 如果已经存在，则增加该资源的依赖计数
					if (buildAssetDic.ContainsKey(assetPath))
						buildAssetDic[assetPath].DependCount++;
					else
						buildAssetDic.Add(assetPath, depends[i]);

					// 添加资源标记
					buildAssetDic[assetPath].AddAssetTags(collectInfo.AssetTags);

					// 注意：检测是否为主动收集资源
					if (assetPath == mainAssetPath)
					{
						buildAssetDic[mainAssetPath].IsCollectAsset = true;
						buildAssetDic[mainAssetPath].IsRawAsset = collectInfo.IsRawAsset;
					}
				}

				// 添加所有的依赖资源列表
				// 注意：不包括自己
				var allDependAssetInfos = new List<BuildAssetInfo>(depends.Count);
				for (int i = 0; i < depends.Count; i++)
				{
					string assetPath = depends[i].AssetPath;
					if (assetPath != mainAssetPath)
						allDependAssetInfos.Add(buildAssetDic[assetPath]);
				}
				buildAssetDic[mainAssetPath].SetAllDependAssetInfos(allDependAssetInfos);

				EditorTools.DisplayProgressBar("依赖文件分析", ++progressValue, allCollectInfos.Count);
			}
			EditorTools.ClearProgressBar();

			// 3. 记录参与构建的资源总数
			buildMapContext.AssetFileCount = buildAssetDic.Values.Count;

			// 4. 移除零依赖的资源
			var redundancyServices = buildParameters.Parameters.RedundancyServices;
			List<BuildAssetInfo> undependentAssets = new List<BuildAssetInfo>();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.IsCollectAsset)
					continue;

				// 零依赖资源
				if (buildAssetInfo.DependCount == 0)
				{
					undependentAssets.Add(buildAssetInfo);
					continue;
				}

				// 冗余扩展
				if (redundancyServices != null && redundancyServices.Check(buildAssetInfo.AssetPath))
				{
					undependentAssets.Add(buildAssetInfo);
					buildMapContext.RedundancyAssetList.Add(buildAssetInfo.AssetPath);
					continue;
				}

				// 如果没有开启自动分包，没有被收集到的资源会造成冗余
				if (buildParameters.Parameters.EnableAutoCollect == false)
				{
					if (AssetBundleCollectorSettingData.HasCollector(buildAssetInfo.AssetPath) == false)
					{
						undependentAssets.Add(buildAssetInfo);
						buildMapContext.RedundancyAssetList.Add(buildAssetInfo.AssetPath);
					}
				}
			}
			foreach (var assetInfo in undependentAssets)
			{
				buildAssetDic.Remove(assetInfo.AssetPath);
			}

			// 5. 设置资源包名
			progressValue = 0;
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var assetInfo = pair.Value;
				var bundleLabel = AssetBundleCollectorSettingData.GetBundleLabel(assetInfo.AssetPath);
				if (assetInfo.IsRawAsset)
					assetInfo.SetBundleLabelAndVariant(bundleLabel, YooAssetSettingsData.Setting.RawFileVariant);
				else
					assetInfo.SetBundleLabelAndVariant(bundleLabel, YooAssetSettingsData.Setting.AssetBundleFileVariant);
				EditorTools.DisplayProgressBar("设置资源包名", ++progressValue, buildAssetDic.Count);
			}
			EditorTools.ClearProgressBar();

			// 6. 构建资源包
			var allAssets = buildAssetDic.Values.ToList();
			if (allAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");
			foreach (var assetInfo in allAssets)
			{
				buildMapContext.PackAsset(assetInfo);
			}
		}

		/// <summary>
		/// 获取指定资源依赖的所有资源列表
		/// 注意：返回列表里已经包括主资源自己
		/// </summary>
		private List<BuildAssetInfo> GetAllDependencies(string mainAssetPath)
		{
			List<BuildAssetInfo> result = new List<BuildAssetInfo>();
			string[] depends = AssetDatabase.GetDependencies(mainAssetPath, true);
			foreach (string assetPath in depends)
			{
				if (AssetBundleCollectorSettingData.IsValidateAsset(assetPath))
				{
					BuildAssetInfo assetInfo = new BuildAssetInfo(assetPath);
					result.Add(assetInfo);
				}
			}
			return result;
		}

		/// <summary>
		/// 检测构建结果
		/// </summary>
		private void CheckBuildMapContent(BuildMapContext buildMapContext)
		{
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{
					if (bundleInfo.BuildinAssets.Count != 1)
						throw new Exception("The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
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