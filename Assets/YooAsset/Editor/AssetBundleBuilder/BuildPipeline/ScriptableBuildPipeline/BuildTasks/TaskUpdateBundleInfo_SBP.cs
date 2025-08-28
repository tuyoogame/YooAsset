using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_SBP : TaskUpdateBundleInfo, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            // 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
            var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
            if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
            {
                return value.Hash.ToString();
            }
            else
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleHash, $"Not found unity bundle hash : {bundleInfo.BundleName}");
                throw new Exception(message);
            }
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
            if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
            {
                return value.Crc;
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