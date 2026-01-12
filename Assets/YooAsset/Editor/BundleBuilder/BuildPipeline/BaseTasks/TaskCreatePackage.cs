
namespace YooAsset.Editor
{
    /// <summary>
    /// 创建补丁包的任务辅助基类
    /// </summary>
    public class TaskCreatePackage
    {
        /// <summary>
        /// 从指定目录拷贝到目标目录
        /// </summary>
        protected void CopyPipelineFile(string sourceRootDirectory, string destRootDirectory, string fileName)
        {
            string sourcePath = $"{sourceRootDirectory}/{fileName}";
            string destPath = $"{destRootDirectory}/{fileName}";
            EditorFileUtility.CopyFile(sourcePath, destPath, true);
        }

        /// <summary>
        /// 拷贝所有补丁文件
        /// </summary>
        protected void CopyPackageBundles(BuildMapContext buildMapContext)
        {
            int progressValue = 0;
            int fileTotalCount = buildMapContext.Collection.Count;
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                EditorFileUtility.CopyFile(bundleInfo.PackageSourceFilePath, bundleInfo.PackageDestFilePath, true);
                EditorDialogUtility.DisplayProgressBar("Copy patch file", ++progressValue, fileTotalCount);
            }
            EditorDialogUtility.ClearProgressBar();
        }
    }
}
