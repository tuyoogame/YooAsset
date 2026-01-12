
namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线的构建映射生成任务
    /// </summary>
    public class TaskGetBuildMap_ESBP : TaskGetBuildMap, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(true, buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);

            if (buildParametersContext.Parameters.BuildBundleType == (int)EBundleType.RawBundle)
            {
                CheckRawBundleMapContent(buildMapContext);
            }
        }
    }
}