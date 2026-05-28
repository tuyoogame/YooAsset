#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// InstantAsset 构建管线的补丁包创建任务
    /// </summary>
    public class TaskCreatePackage_IABP : TaskCreatePackage, IBuildTask
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
            string packageName = buildParametersContext.Parameters.PackageName;
            BuildLogger.Log($"Start making patch package: '{packageOutputDirectory}'.");

            // 拷贝 InstantAssetTable 文件
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, packageName);
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, $"{packageName}-scene");

            // 拷贝构建记录文件
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "AssetPackerRecordFile.json");

            // 拷贝内置资源文件
            CopyPipelineFile(pipelineOutputDirectory, packageOutputDirectory, "Built-In-Extra-Resources");

            // 拷贝所有补丁文件
            CopyPackageBundles(buildMapContext);
        }
    }
}
#endif
