namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的加密任务
    /// </summary>
    public class TaskEncryption_LBP : TaskEncryption, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            EncryptingBundleFiles(buildParameters, buildMapContext);
        }
    }
}