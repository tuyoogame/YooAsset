
namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的构建映射生成任务
    /// </summary>
    public class TaskGetBuildMap_AFBP : TaskGetBuildMap, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(true, buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);
        }
    }
}
