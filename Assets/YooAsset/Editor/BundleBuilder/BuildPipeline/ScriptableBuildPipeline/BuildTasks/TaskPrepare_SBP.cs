using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的准备任务
    /// </summary>
    public class TaskPrepare_SBP : TaskPrepare, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 检测构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测未保存场景
            CheckDirtyScenes();

            // 删除历史缓存
            if (buildParameters.ClearBuildCacheFiles)
            {
                // Deletes the build cache directory.
                UnityEditor.Build.Pipeline.Utilities.BuildCache.PurgeCache(false);
                DeletePackageRootDirectory(buildParameters);
            }

            // 准备输出目录
            PrepareOutputDirectory(buildParameters);

            // 检测内置着色器资源包名称
            if (string.IsNullOrEmpty(buildParameters.BuiltinShadersBundleName))
            {
                string warning = BuildLogger.GetErrorMessage(ErrorCode.BuiltinShadersBundleNameIsNull, "Builtin shaders bundle name is null. It will cause resource redundancy.");
                BuildLogger.Warning(warning);
            }
        }
    }
}
