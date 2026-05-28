#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;
using System.Linq;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的构建结果验证任务
    /// </summary>
    public class TaskVerifyBuildResult_IABP : IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as InstantAssetBuildParameters;

            // 验证构建结果
            if (buildParameters.VerifyBuildingResult)
            {
                VerifyingBuildingResult(context);
            }
        }

        /// <summary>
        /// 验证构建结果
        /// </summary>
        private void VerifyingBuildingResult(BuildContext context)
        {
            var recordFileContext = context.GetContextObject<TaskCreateRecordFile_IABP.RecordFileContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string buildLogFilePath = $"{pipelineOutputDirectory}/{InstantAssetBuildLog.BuildLogFileName}";

            // 1. 加载构建日志
            var buildLog = InstantAssetBuildLog.LoadFromFile(buildLogFilePath);

            // 2. 验证资源包差异
            VerifyBundleList(recordFileContext, buildLog);

            // 3. 验证资源包内容差异
            VerifyBundleAssets(recordFileContext, buildLog);

            BuildLogger.Log("Build results verified successfully.");
        }

        /// <summary>
        /// 验证资源包列表
        /// </summary>
        private void VerifyBundleList(TaskCreateRecordFile_IABP.RecordFileContext recordFileContext, InstantAssetBuildLog buildLog)
        {
            var planningContent = recordFileContext.RecordMap;
            List<string> planningBundles = planningContent.Keys.ToList();
            List<string> buildBundles = buildLog.GetAllBundleNames();

            // 1. 验证差异
            List<string> exceptBundleList1 = buildBundles.Except(planningBundles).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    string displayBundleName = recordFileContext.GetDisplayBundleName(exceptBundle);
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle, $"Found unintended build bundle: '{displayBundleName}'.");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, "Unintended build result, see warnings above.");
                throw new InvalidOperationException(exception);
            }

            // 2. 验证差异
            List<string> exceptBundleList2 = planningBundles.Except(buildBundles).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    string displayBundleName = recordFileContext.GetDisplayBundleName(exceptBundle);
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.MissingExpectedBundle, $"Missing expected build bundle: '{displayBundleName}'.");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, "Unintended build result, see warnings above.");
                throw new InvalidOperationException(exception);
            }
        }

        /// <summary>
        /// 验证资源包内容
        /// </summary>
        private void VerifyBundleAssets(TaskCreateRecordFile_IABP.RecordFileContext recordFileContext, InstantAssetBuildLog buildLog)
        {
            var planningContent = recordFileContext.RecordMap;
            bool hasError = false;
            foreach (var bundleName in planningContent.Keys)
            {
                if (buildLog.TryGetAssetGuids(bundleName, out var buildAssetGuids) == false)
                    continue;

                var planningAssetGuids = planningContent[bundleName].Where(assetGuid => string.IsNullOrEmpty(assetGuid) == false).ToList();

                // 1. 验证差异
                List<string> exceptAssetList1 = buildAssetGuids.Except(planningAssetGuids).ToList();
                if (exceptAssetList1.Count > 0)
                {
                    hasError = true;
                    string displayBundleName = recordFileContext.GetDisplayBundleName(bundleName);
                    foreach (var exceptAsset in exceptAssetList1)
                    {
                        string warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle, $"Found unintended build asset: '{displayBundleName}' -> '{exceptAsset}'.");
                        BuildLogger.Warning(warning);
                    }
                }

                // 2. 验证差异
                List<string> exceptAssetList2 = planningAssetGuids.Except(buildAssetGuids).ToList();
                if (exceptAssetList2.Count > 0)
                {
                    hasError = true;
                    string displayBundleName = recordFileContext.GetDisplayBundleName(bundleName);
                    foreach (var exceptAsset in exceptAssetList2)
                    {
                        string warning = BuildLogger.GetErrorMessage(ErrorCode.MissingExpectedBundle, $"Missing expected build asset: '{displayBundleName}' -> '{exceptAsset}'.");
                        BuildLogger.Warning(warning);
                    }
                }
            }

            if (hasError)
            {
                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, "Unintended build result, see warnings above.");
                throw new InvalidOperationException(exception);
            }
        }
    }
}
#endif
