namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的加密任务
    /// </summary>
    public class TaskEncryption_SBP : TaskEncryption, IBuildTask
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