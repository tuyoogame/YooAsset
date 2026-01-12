
using System.IO;
using System.Text;
using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线的资源包信息更新任务
    /// </summary>
    public class TaskUpdateBundleInfo_ESBP : TaskUpdateBundleInfo, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return "00000000000000000000000000000000"; //32位
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
        }
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return GetFilePathTempHash(filePath);
        }
        protected override uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return GetBundleTempSize(bundleInfo);
        }

        private string GetFilePathTempHash(string filePath)
        {
            string lastWrite = File.GetLastWriteTimeUtc(filePath).Ticks.ToString();
            string fileSize = FileUtility.GetFileSize(filePath).ToString();
            byte[] bytes = Encoding.UTF8.GetBytes($"{filePath}|{lastWrite}|{fileSize}");
            return HashUtility.ComputeMD5(bytes);
        }
        private long GetBundleTempSize(BuildBundleInfo bundleInfo)
        {
            long tempSize = 0;

            var assetPaths = bundleInfo.GetAllPackAssetPaths();
            foreach (var assetPath in assetPaths)
            {
                long size = FileUtility.GetFileSize(assetPath);
                tempSize += size;
            }

            if (tempSize == 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BundleTempSizeIsZero, $"Bundle temporary size is zero. Verify the main asset list: '{bundleInfo.BundleName}'.");
                throw new InvalidOperationException(message);
            }
            return tempSize;
        }
    }
}