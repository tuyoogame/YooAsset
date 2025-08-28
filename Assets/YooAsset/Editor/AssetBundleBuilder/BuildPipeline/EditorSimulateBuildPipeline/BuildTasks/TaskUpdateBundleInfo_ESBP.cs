
using System.Text;
using System;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_ESBP : TaskUpdateBundleInfo, IBuildTask
    {
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
        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            string filePath = bundleInfo.PackageSourceFilePath;
            return GetFilePathTempHash(filePath);
        }
        protected override uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            return 0;
        }
        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
        {
            return GetBundleTempSize(bundleInfo);
        }

        private string GetFilePathTempHash(string filePath)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(filePath);
            return HashUtility.BytesMD5(bytes);

            // 注意：在文件路径的哈希值冲突的情况下，可以使用下面的方法
            //return $"{HashUtility.BytesMD5(bytes)}-{Guid.NewGuid():N}";
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
                string message = BuildLogger.GetErrorMessage(ErrorCode.BundleTempSizeIsZero, $"Bundle temp size is zero, check bundle main asset list : {bundleInfo.BundleName}");
                throw new Exception(message);
            }
            return tempSize;
        }
    }
}