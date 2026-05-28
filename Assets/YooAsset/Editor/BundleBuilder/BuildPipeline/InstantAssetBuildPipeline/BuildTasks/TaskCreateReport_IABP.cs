#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的构建报告创建任务
    /// </summary>
    public class TaskCreateReport_IABP : TaskCreateReport, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();
            CreateReportFile(buildParameters, buildMapContext, manifestContext);
        }
    }
}
#endif
