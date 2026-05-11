using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 更新资源包构建信息的任务抽象基类，用于填充哈希、CRC、大小及输出路径等字段。
    /// </summary>
    public abstract class TaskUpdateBundleInfo
    {
        /// <summary>
        /// 根据构建上下文更新所有资源包的路径与校验信息
        /// </summary>
        /// <param name="context">构建上下文</param>
        public void UpdateBundleInfo(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            int outputNameStyle = (int)buildParametersContext.Parameters.FileNameStyle;

            // 1.检测文件名长度
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                // NOTE：检测文件名长度不要超过260字符。
                string fileName = bundleInfo.BundleName;
                if (fileName.Length >= 260)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.CharactersOverTheLimit, $"Bundle file name character count exceeds limit: '{fileName}'.");
                    throw new InvalidOperationException(message);
                }
            }

            // 2.更新构建输出的文件路径
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                bundleInfo.BuildOutputFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                if (bundleInfo.Encrypted)
                    bundleInfo.PackageSourceFilePath = bundleInfo.EncryptedFilePath;
                else
                    bundleInfo.PackageSourceFilePath = bundleInfo.BuildOutputFilePath;
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

        /// <summary>
        /// 获取 Unity 记录的资源包哈希值
        /// </summary>
        /// <param name="bundleInfo">资源包构建信息</param>
        /// <param name="context">构建上下文</param>
        /// <returns>Unity 哈希字符串</returns>
        protected abstract string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context);

        /// <summary>
        /// 获取 Unity 记录的资源包 CRC
        /// </summary>
        /// <param name="bundleInfo">资源包构建信息</param>
        /// <param name="context">构建上下文</param>
        /// <returns>Unity CRC 值</returns>
        protected abstract uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context);

        /// <summary>
        /// 获取资源包文件的哈希值（用于远端文件名等）
        /// </summary>
        /// <param name="bundleInfo">资源包构建信息</param>
        /// <param name="context">构建上下文</param>
        /// <returns>文件哈希字符串</returns>
        protected abstract string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildContext context);

        /// <summary>
        /// 获取资源包文件的 CRC
        /// </summary>
        /// <param name="bundleInfo">资源包构建信息</param>
        /// <param name="context">构建上下文</param>
        /// <returns>文件 CRC 值</returns>
        protected abstract uint GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildContext context);

        /// <summary>
        /// 获取资源包文件大小（字节）
        /// </summary>
        /// <param name="bundleInfo">资源包构建信息</param>
        /// <param name="context">构建上下文</param>
        /// <returns>文件大小</returns>
        protected abstract long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildContext context);

        /// <summary>
        /// 计算包含路径信息的文件 MD5
        /// </summary>
        protected string GetFileMD5IncludePath(string filePath)
        {
            string pathHash = HashUtility.ComputeMD5(filePath.ToLowerInvariant());
            string contentHash = HashUtility.ComputeFileMD5(filePath);
            string combined = pathHash + contentHash;
            return HashUtility.ComputeMD5(combined);
        }
    }
}