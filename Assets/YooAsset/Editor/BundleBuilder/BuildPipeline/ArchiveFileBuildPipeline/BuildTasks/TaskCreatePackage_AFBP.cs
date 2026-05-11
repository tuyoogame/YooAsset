namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的补丁包创建任务
    /// </summary>
    public class TaskCreatePackage_AFBP : TaskCreatePackage, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            CreatePackagePatch(buildParametersContext, buildMapContext);
        }

        private void CreatePackagePatch(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            BuildLogger.Log($"Start making patch package: '{packageOutputDirectory}'.");
            CopyPackageBundles(buildMapContext);
        }
    }
}
