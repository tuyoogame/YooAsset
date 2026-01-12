
namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的加密任务
    /// </summary>
    public class TaskEncryption_RFBP : TaskEncryption, IBuildTask
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