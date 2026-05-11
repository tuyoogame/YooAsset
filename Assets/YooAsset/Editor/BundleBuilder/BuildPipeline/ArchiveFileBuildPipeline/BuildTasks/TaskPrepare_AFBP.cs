
namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的准备任务
    /// </summary>
    public class TaskPrepare_AFBP : TaskPrepare, IBuildTask
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
        }
    }
}
