using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 拷贝首包资源文件的任务，将清单与指定资源包复制到首包目录。
    /// </summary>
    public class TaskCopyBundledFiles
    {
        /// <summary>
        /// 从指定目录拷贝到目标目录
        /// </summary>
        protected void CopyPipelineFile(string sourceRootDirectory, string destRootDirectory, string fileName)
        {
            string sourcePath = $"{sourceRootDirectory}/{fileName}";
            string destPath = $"{destRootDirectory}/{fileName}";
            EditorFileUtility.CopyFile(sourcePath, destPath, true);
        }

        /// <summary>
        /// 拷贝首包资源文件
        /// </summary>
        internal void CopyBundledFilesToStreaming(BuildParametersContext buildParametersContext, PackageManifest manifest)
        {
            EBundledCopyOption copyOption = buildParametersContext.Parameters.BundledCopyOption;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            string bundledRootDirectory = buildParametersContext.GetBundledRootDirectory();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

            // 清空首包资源目录
            if (copyOption == EBundledCopyOption.ClearAndCopyAll || copyOption == EBundledCopyOption.ClearAndCopyByTags)
            {
                EditorFileUtility.DeleteDirectory(bundledRootDirectory);
                EditorFileUtility.CreateDirectory(bundledRootDirectory);
            }

            // 拷贝补丁清单文件
            {
                string fileName = YooAssetConfiguration.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{bundledRootDirectory}/{fileName}";
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = YooAssetConfiguration.GetPackageHashFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{bundledRootDirectory}/{fileName}";
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = YooAssetConfiguration.GetPackageVersionFileName(buildPackageName);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{bundledRootDirectory}/{fileName}";
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            if (copyOption == EBundledCopyOption.ClearAndCopyAll || copyOption == EBundledCopyOption.OnlyCopyAll)
            {
                foreach (var packageBundle in manifest.BundleList)
                {
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.GetFileName()}";
                    string destPath = $"{bundledRootDirectory}/{packageBundle.GetFileName()}";
                    EditorFileUtility.CopyFile(sourcePath, destPath, true);
                }
            }

            // 拷贝文件列表（带标签的文件）
            if (copyOption == EBundledCopyOption.ClearAndCopyByTags || copyOption == EBundledCopyOption.OnlyCopyByTags)
            {
                string copyParams = buildParametersContext.Parameters.BundledCopyParams;
                if (string.IsNullOrEmpty(copyParams))
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.BundledCopyParamsIsNullOrEmpty, $"BundledCopyParams is required when using '{copyOption}' copy option.");
                    throw new InvalidOperationException(message);
                }
                string[] tags = copyParams.Split(';');
                foreach (var packageBundle in manifest.BundleList)
                {
                    if (packageBundle.HasAnyTag(tags) == false)
                        continue;
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.GetFileName()}";
                    string destPath = $"{bundledRootDirectory}/{packageBundle.GetFileName()}";
                    EditorFileUtility.CopyFile(sourcePath, destPath, true);
                }
            }

            // 刷新目录
            AssetDatabase.Refresh();
            BuildLogger.Log($"Bundled files copy complete: '{bundledRootDirectory}'.");
        }
    }
}