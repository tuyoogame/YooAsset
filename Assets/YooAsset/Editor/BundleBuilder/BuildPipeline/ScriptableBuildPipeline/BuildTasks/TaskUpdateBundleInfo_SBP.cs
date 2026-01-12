using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的资源包信息更新任务
    /// </summary>
    public class TaskUpdateBundleInfo_SBP : TaskUpdateBundleInfo, IBuildTask
    {
        /// <inheritdoc/>
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
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleHash, $"Unity bundle hash not found: '{bundleInfo.BundleName}'.");
                throw new InvalidOperationException(message);
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
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleCRC, $"Unity bundle CRC not found: '{bundleInfo.BundleName}'.");
                throw new InvalidOperationException(message);
            }
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return HashUtility.ComputeFileMD5(filePath);
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
    }
}