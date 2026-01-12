namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的补丁包创建任务
    /// </summary>
    public class TaskCreatePackage_SBP : TaskCreatePackage, IBuildTask
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
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            BuildLogger.Log($"Start making patch package: '{packageOutputDirectory}'.");

            // 拷贝构建日志
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "buildlogtep.json");

            // 拷贝代码防裁剪配置
            if (scriptableBuildParameters.WriteLinkXML)
            {
                CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "link.xml");
            }

            // 拷贝所有补丁文件
            CopyPackageBundles(buildMapContext);
        }
    }
}