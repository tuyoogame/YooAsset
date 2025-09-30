using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuiltinFiles
    {
        /// <summary>
        /// 拷贝首包资源文件
        /// </summary>
        internal void CopyBuiltinFilesToStreaming(BuildParametersContext buildParametersContext, PackageManifest manifest)
        {
            EBuiltinFileCopyOption copyOption = buildParametersContext.Parameters.BuiltinFileCopyOption;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            string builtinRootDirectory = buildParametersContext.GetBuiltinRootDirectory();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

            // 清空内置文件的目录
            if (copyOption == EBuiltinFileCopyOption.ClearAndCopyAll || copyOption == EBuiltinFileCopyOption.ClearAndCopyByTags)
            {
                EditorTools.ClearFolder(builtinRootDirectory);
            }

            // 拷贝补丁清单文件
            {
                string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{builtinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = YooAssetSettingsData.GetPackageHashFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{builtinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildPackageName);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{builtinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            if (copyOption == EBuiltinFileCopyOption.ClearAndCopyAll || copyOption == EBuiltinFileCopyOption.OnlyCopyAll)
            {
                foreach (var packageBundle in manifest.BundleList)
                {
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    string destPath = $"{builtinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }
            }

            // 拷贝文件列表（带标签的文件）
            if (copyOption == EBuiltinFileCopyOption.ClearAndCopyByTags || copyOption == EBuiltinFileCopyOption.OnlyCopyByTags)
            {
                string[] tags = buildParametersContext.Parameters.BuiltinFileCopyParams.Split(';');
                foreach (var packageBundle in manifest.BundleList)
                {
                    if (packageBundle.HasTag(tags) == false)
                        continue;
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    string destPath = $"{builtinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }
            }

            // 刷新目录
            AssetDatabase.Refresh();
            BuildLogger.Log($"Builtin files copy complete: {builtinRootDirectory}");
        }
    }
}