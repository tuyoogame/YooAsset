#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的首包资源的拷贝任务
    /// </summary>
    public class TaskCopyBundledFiles_IABP : TaskCopyBundledFiles, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();

            if (buildParametersContext.Parameters.BundledCopyOption != EBundledCopyOption.None)
            {
                CopyBundledFilesToStreaming(buildParametersContext, manifestContext.Manifest);
                CopyInstantAssetTable(buildParametersContext);
            }
        }

        /// <summary>
        /// 拷贝 InstantAssetTable 文件到首包目录
        /// </summary>
        private void CopyInstantAssetTable(BuildParametersContext buildParametersContext)
        {
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            string bundledRootDirectory = buildParametersContext.GetBundledRootDirectory();
            string packageName = buildParametersContext.Parameters.PackageName;

            // 拷贝 InstantAssetTable 主文件
            CopyPipelineFile(packageOutputDirectory, bundledRootDirectory, packageName);

            // 拷贝 InstantAssetTable 场景文件
            CopyPipelineFile(packageOutputDirectory, bundledRootDirectory, $"{packageName}-scene");

            // 刷新目录
            AssetDatabase.Refresh();
            BuildLogger.Log($"InstantAssetTable files copy complete: '{bundledRootDirectory}'.");
        }
    }
}
#endif
