using System;
using System.IO;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的资源包信息更新任务
    /// </summary>
    public class TaskUpdateBundleInfo_RFBP : TaskUpdateBundleInfo, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return ComputeFileHash(bundleInfo, context);
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return ComputeFileHash(bundleInfo, context);
        }
        protected override uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.ComputeFileCrc32AsUInt(filePath);
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return FileUtility.GetFileSize(filePath);
        }

        private string ComputeFileHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var parameters = buildParametersContext.Parameters as RawFileBuildParameters;
            string filePath = bundleInfo.PackageSourceFilePath;
            if (parameters.IncludePathInHash)
                return GetFileMD5IncludePath(filePath);
            else
                return HashUtility.ComputeFileMD5(filePath);
        }
    }
}
