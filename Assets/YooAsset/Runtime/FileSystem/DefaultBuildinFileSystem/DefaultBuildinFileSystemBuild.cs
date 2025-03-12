#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    public class DefaultBuildinFileSystemBuild
    {
        /// <summary>
        /// 输出包裹的内置资源目录文件
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public static void ExportBuildinCatalogFile()
        {
            YooLogger.Log("Begin to create catalog file !");

            string rootPath = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists == false)
            {
                UnityEngine.Debug.LogWarning($"Can not found StreamingAssets root directory : {rootPath}");
                return;
            }

            // 搜索所有Package目录
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                string packageName = subDirectory.Name;
                string pacakgeDirectory = subDirectory.FullName;
                bool result = CreateBuildinCatalogFile(packageName, pacakgeDirectory);
                if (result == false)
                {
                    throw new System.Exception($"Create package {packageName} catalog file failed ! See the detail error in console !");
                }
            }
        }

        /// <summary>
        /// 生成包裹的内置资源目录文件
        /// </summary>
        public static bool CreateBuildinCatalogFile(string packageName, string pacakgeDirectory)
        {
            // 获取资源清单版本
            string packageVersion;
            {
                string versionFileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
                string versionFilePath = $"{pacakgeDirectory}/{versionFileName}";
                if (File.Exists(versionFilePath) == false)
                {
                    Debug.LogError($"Can not found package version file : {versionFilePath}");
                    return false;
                }

                packageVersion = FileUtility.ReadAllText(versionFilePath);
            }

            // 加载资源清单文件
            PackageManifest packageManifest;
            {
                string manifestFileName = YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion);
                string manifestFilePath = $"{pacakgeDirectory}/{manifestFileName}";
                if (File.Exists(manifestFilePath) == false)
                {
                    Debug.LogError($"Can not found package manifest file : {manifestFilePath}");
                    return false;
                }

                var binaryData = FileUtility.ReadAllBytes(manifestFilePath);
                packageManifest = ManifestTools.DeserializeFromBinary(binaryData);
            }

            // 获取文件名映射关系
            Dictionary<string, string> fileMapping = new Dictionary<string, string>();
            {
                foreach (var packageBundle in packageManifest.BundleList)
                {
                    fileMapping.Add(packageBundle.FileName, packageBundle.BundleGUID);
                }
            }

            // 创建内置清单实例
            var buildinFileCatalog = new DefaultBuildinFileCatalog();
            buildinFileCatalog.PackageName = packageName;
            buildinFileCatalog.PackageVersion = packageVersion;

            // 记录所有内置资源文件
            DirectoryInfo rootDirectory = new DirectoryInfo(pacakgeDirectory);
            FileInfo[] fileInfos = rootDirectory.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".meta")
                    continue;

                if (fileInfo.Name == "link.xml" || fileInfo.Name == "buildlogtep.json")
                    continue;
                if (fileInfo.Name == $"{packageName}.version")
                    continue;
                if (fileInfo.Name == $"{packageName}_{packageVersion}.bytes")
                    continue;
                if (fileInfo.Name == $"{packageName}_{packageVersion}.hash")
                    continue;
                if (fileInfo.Name == $"{packageName}_{packageVersion}.json")
                    continue;
                if (fileInfo.Name == $"{packageName}_{packageVersion}.report")
                    continue;
                if (fileInfo.Name == DefaultBuildinFileSystemDefine.BuildinCatalogFileName)
                    continue;

                string fileName = fileInfo.Name;
                if (fileMapping.TryGetValue(fileName, out string bundleGUID))
                {
                    var wrapper = new DefaultBuildinFileCatalog.FileWrapper(bundleGUID, fileName);
                    buildinFileCatalog.Wrappers.Add(wrapper);
                }
                else
                {
                    Debug.LogWarning($"Failed mapping file : {fileName}");
                }
            }

            // 创建输出目录
            string fullPath = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            string saveFilePath = $"{fullPath}/{packageName}/{DefaultBuildinFileSystemDefine.BuildinCatalogFileName}";
            FileUtility.CreateFileDirectory(saveFilePath);

            // 创建输出文件
            File.WriteAllText(saveFilePath, JsonUtility.ToJson(buildinFileCatalog, false));
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Succeed to save buildin file catalog : {saveFilePath}");
            return true;
        }
    }
}
#endif