
namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的准备任务
    /// </summary>
    public class TaskPrepare_LBP : TaskPrepare, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;

            // 检测构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测未保存场景
            CheckDirtyScenes();

            // 删除历史缓存
            if (buildParameters.ClearBuildCacheFiles)
            {
                DeletePackageRootDirectory(buildParameters);
            }

            // 准备输出目录
            PrepareOutputDirectory(buildParameters);

            // 检测Unity版本
#if UNITY_2021_3_OR_NEWER
            string warning = BuildLogger.GetErrorMessage(ErrorCode.RecommendScriptBuildPipeline, "Starting with Unity 2021, Scriptable Build Pipeline (SBP) is recommended.");
            BuildLogger.Warning(warning);
#endif
        }
    }
}
