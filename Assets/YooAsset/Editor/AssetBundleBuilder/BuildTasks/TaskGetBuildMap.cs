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
			public BuildReport Report = new BuildReport();

			/// <summary>
			/// 添加一个打包资源
			/// </summary>
			public void PackAsset(BuildAssetInfo assetInfo)
			{
				if (Report.TryGetBundleInfo(assetInfo.BundleName, out BuildBundleInfo bundleInfo))
				{
					bundleInfo.PackAsset(assetInfo);
				}
				else
				{
					BuildBundleInfo newBundleInfo = new BuildBundleInfo(assetInfo.BundleName);
					newBundleInfo.PackAsset(assetInfo);
					Report.BundleInfos.Add(newBundleInfo);
				}
			}

			/// <summary>
			/// 获取所有的打包资源
			/// </summary>
			public List<BuildAssetInfo> GetAllAssets()
			{
				List<BuildAssetInfo> result = new List<BuildAssetInfo>(Report.BundleInfos.Count);
				foreach (var bundleInfo in Report.BundleInfos)
				{
					result.AddRange(bundleInfo.Assets);
				}
				return result;
			}

			/// <summary>
			/// 获取AssetBundle内包含的标记列表
			/// </summary>
			public string[] GetAssetTags(string bundleFullName)
			{
				if (Report.TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo))
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
				if (Report.TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo))
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
				List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(Report.BundleInfos.Count);
				foreach (var bundleInfo in Report.BundleInfos)
				{
					if (bundleInfo.IsRawFile == false)
						builds.Add(bundleInfo.CreatePipelineBuild());
				}
				return builds.ToArray();
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

			// 3. 移除零依赖的资源
			var redundancy = CreateAssetRedundancy();
			List<BuildAssetInfo> undependentAssets = new List<BuildAssetInfo>();
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var buildAssetInfo = pair.Value;
				if (buildAssetInfo.IsCollectAsset)
					continue;

				if (buildAssetInfo.DependCount == 0)
				{
					undependentAssets.Add(buildAssetInfo);
					continue;
				}

				// 冗余扩展
				if(redundancy != null)
				{
					if(redundancy.Check(buildAssetInfo.AssetPath))
					{
						undependentAssets.Add(buildAssetInfo);
						buildMapContext.Report.RedundancyList.Add(buildAssetInfo.AssetPath);
						continue;
					}
				}

				// 冗余机制
				if (buildParameters.Parameters.ApplyRedundancy)
				{
					if (AssetBundleCollectorSettingData.HasCollector(buildAssetInfo.AssetPath) == false)
					{
						undependentAssets.Add(buildAssetInfo);
						buildMapContext.Report.RedundancyList.Add(buildAssetInfo.AssetPath);
					}
				}
			}
			foreach (var assetInfo in undependentAssets)
			{
				buildAssetDic.Remove(assetInfo.AssetPath);
			}

			// 4. 设置资源包名
			progressValue = 0;
			foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
			{
				var assetInfo = pair.Value;
				var bundleLabel = AssetBundleCollectorSettingData.GetBundleLabel(assetInfo.AssetPath);
				if (assetInfo.IsRawAsset)
					assetInfo.SetBundleLabelAndVariant(bundleLabel, ResourceSettingData.Setting.RawFileVariant);
				else
					assetInfo.SetBundleLabelAndVariant(bundleLabel, ResourceSettingData.Setting.AssetBundleFileVariant);
				EditorTools.DisplayProgressBar("设置资源包名", ++progressValue, buildAssetDic.Count);
			}
			EditorTools.ClearProgressBar();

			// 4. 构建资源包
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
			foreach (var bundleInfo in buildMapContext.Report.BundleInfos)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{
					if (bundleInfo.Assets.Count != 1)
						throw new Exception("The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
					continue;
				}

				// 注意：原生文件不能被其它资源文件依赖
				foreach (var assetInfo in bundleInfo.Assets)
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

		/// <summary>
		/// 创建冗余类
		/// </summary>
		/// <returns>如果没有定义类型，则返回NULL</returns>
		private IAssetRedundancy CreateAssetRedundancy()
		{
			var types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetRedundancy));
			if (types.Count == 0)
				return null;
			if (types.Count != 1)
				throw new Exception($"Found more {nameof(IAssetRedundancy)} types. We only support one.");

			UnityEngine.Debug.Log($"创建实例类 : {types[0].FullName}");
			return (IAssetRedundancy)Activator.CreateInstance(types[0]);
		}
	}
}