using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_BBP : TaskUpdateBundleInfo, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildResult = context.GetContextObject<TaskBuilding_BBP.BuildResultContext>();
            var hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
            if (hash.isValid)
            {
                return hash.ToString();
            }
            else
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleHash, $"Not found unity bundle hash : {bundleInfo.BundleName}");
                throw new Exception(message);
            }
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.BuildOutputFilePath;
            if (BuildPipeline.GetCRCForAssetBundle(filePath, out uint crc))
            {
                return crc;
            }
            else
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleCRC, $"Not found unity bundle crc : {bundleInfo.BundleName}");
                throw new Exception(message);
            }
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