namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的补丁包创建任务
    /// </summary>
    public class TaskCreatePackage_LBP : TaskCreatePackage, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            CreatePackagePatch(buildParametersContext, buildMapContext);
        }

        /// <summary>
        /// 拷贝补丁文件到补丁包目录
        /// </summary>
        private void CreatePackagePatch(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            BuildLogger.Log($"Start making patch package: '{packageOutputDirectory}'.");

            // 拷贝UnityManifest序列化文件
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, YooAssetSettings.OutputFolderName);

            // 拷贝UnityManifest文本文件
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, $"{YooAssetSettings.OutputFolderName}.manifest");

            // 拷贝所有补丁文件
            CopyPackageBundles(buildMapContext);
        }
    }
}
