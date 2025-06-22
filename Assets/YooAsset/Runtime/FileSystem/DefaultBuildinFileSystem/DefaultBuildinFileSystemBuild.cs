#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    public class DefaultBuildinFileSystemBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// 在构建应用程序前自动生成内置资源目录文件。
        /// 原理：搜索StreamingAssets目录下的所有资源文件，将这些文件信息写入文件，然后在运行时做查询用途。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            YooLogger.Log("Begin to create catalog file !");

            string rootPath = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists == false)
            {
                Debug.LogWarning($"Can not found StreamingAssets root directory : {rootPath}");
                return;
            }

            // 搜索所有Package目录
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                string packageName = subDirectory.Name;
                string pacakgeDirectory = subDirectory.FullName;
                try
                {
                    bool result = CreateBuildinCatalogFile(null, packageName, pacakgeDirectory);
                    if (result == false)
                    {
                        Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Create package {packageName} catalog file failed ! {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 生成包裹的内置资源目录文件
        /// </summary>
        public static bool CreateBuildinCatalogFile(IManifestServices services, string packageName, string pacakgeDirectory)
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
                packageManifest = ManifestTools.DeserializeFromBinary(binaryData, services);
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
            buildinFileCatalog.FileVersion = CatalogDefine.FileVersion;
            buildinFileCatalog.PackageName = packageName;
            buildinFileCatalog.PackageVersion = packageVersion;

            // 创建白名单查询集合
            HashSet<string> whiteFileList = new HashSet<string>
            {
                "link.xml",
                "buildlogtep.json",
                $"{packageName}.version",
                $"{packageName}_{packageVersion}.bytes",
                $"{packageName}_{packageVersion}.hash",
                $"{packageName}_{packageVersion}.json",
                $"{packageName}_{packageVersion}.report",
                DefaultBuildinFileSystemDefine.BuildinCatalogJsonFileName,
                DefaultBuildinFileSystemDefine.BuildinCatalogBinaryFileName
            };

            // 记录所有内置资源文件
            DirectoryInfo rootDirectory = new DirectoryInfo(pacakgeDirectory);
            FileInfo[] fileInfos = rootDirectory.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".meta")
                    continue;

                if (whiteFileList.Contains(fileInfo.Name))
                    continue;

                string fileName = fileInfo.Name;
                if (fileMapping.TryGetValue(fileName, out string bundleGUID))
                {
                    var wrapper = new DefaultBuildinFileCatalog.FileWrapper();
                    wrapper.BundleGUID = bundleGUID;
                    wrapper.FileName = fileName;
                    buildinFileCatalog.Wrappers.Add(wrapper);
                }
                else
                {
                    Debug.LogWarning($"Failed mapping file : {fileName}");
                }
            }

            // 创建输出文件
            string jsonFilePath = $"{pacakgeDirectory}/{DefaultBuildinFileSystemDefine.BuildinCatalogJsonFileName}";
            if (File.Exists(jsonFilePath))
                File.Delete(jsonFilePath);
            CatalogTools.SerializeToJson(jsonFilePath, buildinFileCatalog);

            // 创建输出文件
            string binaryFilePath = $"{pacakgeDirectory}/{DefaultBuildinFileSystemDefine.BuildinCatalogBinaryFileName}";
            if (File.Exists(binaryFilePath))
                File.Delete(binaryFilePath);
            CatalogTools.SerializeToBinary(binaryFilePath, buildinFileCatalog);

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"Succeed to save buildin file catalog : {binaryFilePath}");
            return true;
        }
    }
}
#endif