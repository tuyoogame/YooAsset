using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的构建结果验证任务
    /// </summary>
    public class TaskVerifyBuildResult_LBP : IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as LegacyBuildParameters;

            // 验证构建结果
            if (buildParameters.VerifyBuildingResult)
            {
                var buildResultContext = context.GetContextObject<TaskBuilding_LBP.BuildResultContext>();
                VerifyingBuildingResult(context, buildResultContext.UnityManifest);
            }
        }

        /// <summary>
        /// 验证构建结果
        /// </summary>
        private void VerifyingBuildingResult(BuildContext context, AssetBundleManifest unityManifest)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            string[] unityBuildContent = unityManifest.GetAllAssetBundles();

            // 1. 计划内容
            string[] planningContent = buildMapContext.Collection.Select(t => t.BundleName).ToArray();

            // 2. 验证差异
            List<string> exceptBundleList1 = unityBuildContent.Except(planningContent).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle, $"Found unintended build bundle: '{exceptBundle}'.");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, "Unintended build result, see warnings above.");
                throw new InvalidOperationException(exception);
            }

            // 3. 验证差异
            List<string> exceptBundleList2 = planningContent.Except(unityBuildContent).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.MissingExpectedBundle, $"Missing expected build bundle: '{exceptBundle}'.");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, "Unintended build result, see warnings above.");
                throw new InvalidOperationException(exception);
            }

            BuildLogger.Log("Build results verified successfully.");
        }
    }
}