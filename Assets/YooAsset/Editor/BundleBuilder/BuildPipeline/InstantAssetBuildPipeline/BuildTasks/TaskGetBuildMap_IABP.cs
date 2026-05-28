#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的资源收集任务
    /// </summary>
    public class TaskGetBuildMap_IABP : TaskGetBuildMap, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(false, buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);
        }
    }
}
#endif
