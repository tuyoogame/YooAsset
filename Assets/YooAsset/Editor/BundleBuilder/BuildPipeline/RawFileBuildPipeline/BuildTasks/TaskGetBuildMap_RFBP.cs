
namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的构建映射生成任务
    /// </summary>
    public class TaskGetBuildMap_RFBP : TaskGetBuildMap, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(true, buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);

            // 检测构建结果
            CheckRawBundleMapContent(buildMapContext);
        }
    }
}