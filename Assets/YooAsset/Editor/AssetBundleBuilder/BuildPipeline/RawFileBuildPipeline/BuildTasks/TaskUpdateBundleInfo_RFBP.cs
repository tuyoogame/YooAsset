using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_RFBP : TaskUpdateBundleInfo, IBuildTask
    {
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
                return HashUtility.FileMD5(filePath);
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
                return HashUtility.FileMD5(filePath);
            }
        }
        protected override uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.FileCRC32Value(filePath);
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return FileUtility.GetFileSize(filePath);
        }

        private string GetFileMD5IncludePath(string filePath)
        {
            string pathHash = HashUtility.StringMD5(filePath.ToLowerInvariant());
            string contentHash = HashUtility.FileMD5(filePath);
            string combined = pathHash + contentHash;
            return HashUtility.StringMD5(combined);
        }
    }
}