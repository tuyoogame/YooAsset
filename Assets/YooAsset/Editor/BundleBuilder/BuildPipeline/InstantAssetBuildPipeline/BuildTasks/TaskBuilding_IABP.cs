#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的资源包构建任务
    /// </summary>
    public class TaskBuilding_IABP : IBuildTask
    {
        /// <summary>
        /// 团结构建管线的构建结果上下文
        /// </summary>
        [ContextObject]
        public class BuildResultContext
        {
            /// <summary>
            /// InstantAssetTable 名称
            /// </summary>
            public string TableName;
        }

        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as InstantAssetBuildParameters;

            // 构建内容
            var bundleBuilds = buildMapContext.GetPipelineBuilds(false);
            var assetNames = new List<string>(10000);
            foreach (var bundleBuild in bundleBuilds)
            {
                assetNames.AddRange(bundleBuild.assetNames);
            }
            var instantTable = new InstantAssetAliasTable();
            instantTable.aliasTableName = buildParameters.PackageName;
            instantTable.assetNames = assetNames.ToArray();
            InstantAssetAliasTable[] instantTables = new InstantAssetAliasTable[] { instantTable };
            
            // 开始构建
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            InstantAssetOptions buildOptions = buildParameters.GetInstantAssetBuildOptions();
            int bundleSize = int.MaxValue;
            bool success = InstantAssetEditorUtility.BuildAssetPacker(pipelineOutputDirectory, instantTables, buildOptions, bundleSize, buildParameters.BuildTarget);
            if (success == false)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, "InstantAsset build failed.");
                throw new InvalidOperationException(message);
            }

            BuildLogger.Log("InstantAsset build succeeded.");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.TableName = buildParameters.PackageName;
            context.SetContextObject(buildResultContext);
        }
    }
}
#endif
