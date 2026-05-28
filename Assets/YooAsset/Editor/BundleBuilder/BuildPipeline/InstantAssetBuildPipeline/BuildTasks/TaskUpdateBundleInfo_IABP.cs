#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的资源包信息更新任务
    /// </summary>
    public class TaskUpdateBundleInfo_IABP : TaskUpdateBundleInfo, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfoForInstantAsset(context);
        }

        /// <summary>
        /// 根据 InstantAsset 的构建输出更新资源包信息
        /// </summary>
        private void UpdateBundleInfoForInstantAsset(BuildContext context)
        {
            var recordFileContext = context.GetContextObject<TaskCreateRecordFile_IABP.RecordFileContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            int outputNameStyle = (int)buildParametersContext.Parameters.FileNameStyle;

            // 1.检测文件名长度
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string fileName = bundleInfo.BundleName;
                if (fileName.Length >= 260)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.CharactersOverTheLimit, $"Bundle file name character count exceeds limit: '{fileName}'.");
                    throw new InvalidOperationException(message);
                }
            }

            // 2.更新构建输出的文件路径
            var guidMap = recordFileContext.BundleNameToGuidMap;
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (guidMap.TryGetValue(bundleInfo.BundleName, out string guid))
                {
                    bundleInfo.BuildOutputFilePath = $"{pipelineOutputDirectory}/{guid}";
                    bundleInfo.PackageSourceFilePath = bundleInfo.BuildOutputFilePath;
                }
                else
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundInstantAssetBundleGuid, $"InstantAsset bundle GUID not found: '{bundleInfo.BundleName}'.");
                    throw new InvalidOperationException(message);
                }
            }

            // 3.更新文件其它信息
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                bundleInfo.PackageUnityHash = GetUnityHash(bundleInfo, context);
                bundleInfo.PackageUnityCRC = GetUnityCRC(bundleInfo, context);
                bundleInfo.PackageFileHash = GetBundleFileHash(bundleInfo, context);
                bundleInfo.PackageFileCRC = GetBundleFileCRC(bundleInfo, context);
                bundleInfo.PackageFileSize = GetBundleFileSize(bundleInfo, context);
            }

            // 4.更新补丁包输出的文件路径
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string bundleName = bundleInfo.BundleName;
                string fileHash = bundleInfo.PackageFileHash;
                string fileExtension = PackageManifestHelper.GetRemoteBundleFileExtension(bundleName);
                string fileName = PackageManifestHelper.GetRemoteBundleFileName(outputNameStyle, bundleName, fileExtension, fileHash);
                bundleInfo.PackageDestFilePath = $"{packageOutputDirectory}/{fileName}";
            }
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return string.Empty;
        }
        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            return 0;
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
#endif
