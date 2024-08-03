
namespace YooAsset.Editor
{
    public class TaskEncryption_RFBP : TaskEncryption, IBuildTask
    {
        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="context"></param>
        public void Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();

            var buildMode = buildParameters.Parameters.BuildMode;

            if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
            {
                EncryptingBundleFiles(buildParameters, buildMapContext);
            }
        }
    }
}