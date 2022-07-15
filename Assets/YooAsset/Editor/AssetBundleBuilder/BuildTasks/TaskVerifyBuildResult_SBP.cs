using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	[TaskAttribute("验证构建结果")]
	public class TaskVerifyBuildResult_SBP : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();

			// 模拟构建模式下跳过验证
			if (buildParametersContext.Parameters.BuildMode == EBuildMode.SimulateBuild)
				return;

			// 验证构建结果
			if (buildParametersContext.Parameters.VerifyBuildingResult)
			{
				var buildResultContext = context.GetContextObject<TaskBuilding_SBP.SBPBuildResultContext>();
				VerifyingBuildingResult(context, buildResultContext.Results);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		private void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			// 1. 移除特定Bundle
			List<string> buildedBundles = buildResults.BundleInfos.Keys.ToList();
			buildedBundles.Remove(YooAssetSettings.UnityBuiltInShadersBundleName);

			// 2. 过滤掉原生Bundle
			List<string> expectBundles = buildMapContext.BundleInfos.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToList();

			// 3. 验证Bundle
			List<string> intersectBundleList = buildedBundles.Except(expectBundles).ToList();
			if (intersectBundleList.Count > 0)
			{
				foreach (var intersectBundle in intersectBundleList)
				{
					Debug.LogWarning($"差异资源包: {intersectBundle}");
				}
				throw new System.Exception("存在差异资源包！请查看警告信息！");
			}

			BuildRunner.Log("构建结果验证成功！");
		}
	}
}