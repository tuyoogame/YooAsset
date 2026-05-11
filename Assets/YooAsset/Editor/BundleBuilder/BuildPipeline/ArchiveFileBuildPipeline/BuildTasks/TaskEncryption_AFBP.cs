
namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的加密任务
    /// </summary>
    public class TaskEncryption_AFBP : TaskEncryption, IBuildTask
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
