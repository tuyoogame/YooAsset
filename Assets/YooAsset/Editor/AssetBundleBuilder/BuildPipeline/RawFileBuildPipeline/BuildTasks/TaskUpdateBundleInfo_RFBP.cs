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
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.FileMD5(filePath);
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.FileMD5(filePath);
        }
        protected override uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.FileCRC32Value(filePath);
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return FileUtility.GetFileSize(filePath);
        }
    }
}