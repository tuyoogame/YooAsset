
namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的核心构建任务
    /// </summary>
    public class TaskBuilding_AFBP : IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as ArchiveFileBuildParameters;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();

            int progressValue = 0;
            int fileTotalCount = buildMapContext.Collection.Count;
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string archiveFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                var entries = ArchiveFileBuildHelper.CollectEntries(bundleInfo.AllPackAssets);
                ArchiveFileBuildHelper.BuildArchiveFile(archiveFilePath, entries, buildParameters.FileAlignment);
                EditorDialogUtility.DisplayProgressBar("Build archive files", ++progressValue, fileTotalCount);
            }
            EditorDialogUtility.ClearProgressBar();
        }
    }
}
