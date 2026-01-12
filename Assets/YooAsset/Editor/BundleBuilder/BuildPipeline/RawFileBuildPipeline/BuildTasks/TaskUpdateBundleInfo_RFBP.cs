using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

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
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var rawFileBuildParameters = buildParametersContext.Parameters as RawFileBuildParameters;
            if (rawFileBuildParameters.IncludePathInHash)
            {
                string filePath = bundleInfo.PackageSourceFilePath;
                return GetFileMD5IncludePath(filePath);
            }
            else
            {
                string filePath = bundleInfo.PackageSourceFilePath;
                return HashUtility.ComputeFileMD5(filePath);
            }
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var rawFileBuildParameters = buildParametersContext.Parameters as RawFileBuildParameters;
            if (rawFileBuildParameters.IncludePathInHash)
            {
                string filePath = bundleInfo.PackageSourceFilePath;
                return GetFileMD5IncludePath(filePath);
            }
            else
            {
                string filePath = bundleInfo.PackageSourceFilePath;
                return HashUtility.ComputeFileMD5(filePath);
            }
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

        private string GetFileMD5IncludePath(string filePath)
        {
            string pathHash = HashUtility.ComputeMD5(filePath.ToLowerInvariant());
            string contentHash = HashUtility.ComputeFileMD5(filePath);
            string combined = pathHash + contentHash;
            return HashUtility.ComputeMD5(combined);
        }
    }
}