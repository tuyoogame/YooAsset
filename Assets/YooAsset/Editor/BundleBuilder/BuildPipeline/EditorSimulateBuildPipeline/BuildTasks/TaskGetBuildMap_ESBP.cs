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

            // 注意：检查每个原生文件资源包只能包含一个原生文件
            if (buildParametersContext.Parameters.BuildBundleType == (int)EBundleType.VirtualRawBundle)
            {
                CheckRawBundleMapContent(buildMapContext);
            }

            // 检查归档资源包内每个子文件大小不超过上限
            if (buildParametersContext.Parameters.BuildBundleType == (int)EBundleType.VirtualArchiveBundle)
            {
                CheckArchiveBundleMapContent(buildMapContext);
            }
        }
    }
}