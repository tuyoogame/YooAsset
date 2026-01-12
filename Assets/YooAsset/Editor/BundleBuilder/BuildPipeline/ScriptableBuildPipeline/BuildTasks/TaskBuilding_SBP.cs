using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的资源包构建任务
    /// </summary>
    public class TaskBuilding_SBP : IBuildTask
    {
        /// <summary>
        /// 可编程构建管线的构建结果上下文
        /// </summary>
        [ContextObject]
        public class BuildResultContext
        {
            /// <summary>
            /// SBP 构建结果数据
            /// </summary>
            public IBundleBuildResults Results;

            /// <summary>
            /// 内置着色器资源包名称
            /// </summary>
            public string BuiltinShadersBundleName;

            /// <summary>
            /// MonoScript 资源包名称
            /// </summary>
            public string MonoScriptsBundleName;
        }

        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 构建内容
            var bundleBuilds = buildMapContext.GetPipelineBuilds(scriptableBuildParameters.ReplaceAssetPathWithAddress);
            var buildContent = new BundleBuildContent(bundleBuilds);

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = scriptableBuildParameters.GetBundleBuildParameters();
            string builtinShadersBundleName = scriptableBuildParameters.BuiltinShadersBundleName;
            string monoScriptsBundleName = scriptableBuildParameters.MonoScriptsBundleName;
            var taskList = SBPBuildTasks.Create(builtinShadersBundleName, monoScriptsBundleName);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
            if (exitCode < 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, $"UnityEngine build failed. ReturnCode: {exitCode}.");
                throw new InvalidOperationException(message);
            }

            // 说明：解决因为特殊资源包导致验证失败。
            // 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
            if (string.IsNullOrEmpty(builtinShadersBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(builtinShadersBundleName))
                    buildMapContext.CreateEmptyBundleInfo(builtinShadersBundleName);
            }
            if (string.IsNullOrEmpty(monoScriptsBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(monoScriptsBundleName))
                    buildMapContext.CreateEmptyBundleInfo(monoScriptsBundleName);
            }

            BuildLogger.Log("UnityEngine build succeeded.");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            buildResultContext.BuiltinShadersBundleName = builtinShadersBundleName;
            buildResultContext.MonoScriptsBundleName = monoScriptsBundleName;
            context.SetContextObject(buildResultContext);
        }
    }
}