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
        /// 原理：搜索StreamingAssets目录下的所有资源文件，然后将这些文件信息写入文件，并存储在Resources目录下。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            YooLogger.Log("Begin to create catalog file !");

            string savePath = $"Assets/Resources/{YooAssetSettingsData.Setting.DefaultYooFolderName}";
            DirectoryInfo saveDirectory = new DirectoryInfo(savePath);
            if (saveDirectory.Exists)
                saveDirectory.Delete(true);

            string rootPath = $"{Application.dataPath}/StreamingAssets/{YooAssetSettingsData.Setting.DefaultYooFolderName}";
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists == false)
            {
                throw new System.Exception($"Can not found StreamingAssets root directory : {rootPath}");
            }

            // 搜索所有Package目录
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                CreateBuildinCatalogFile(subDirectory.Name, subDirectory.FullName);
            }
        }

        /// <summary>
        /// 生成包裹的内置资源目录文件
        /// </summary>
        public static void CreateBuildinCatalogFile(string packageName, string pacakgeDirectory)
        {
            // 获取资源清单版本
            string packageVersion;
            {
                string versionFileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
                string versionFilePath = $"{pacakgeDirectory}/{versionFileName}";
                if (File.Exists(versionFilePath) == false)
                {
                    throw new System.Exception($"Can not found package version file : {versionFilePath}");
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
                    throw new System.Exception($"Can not found package manifest file : {manifestFilePath}");
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
            var buildinFileCatalog = ScriptableObject.CreateInstance<DefaultBuildinFileCatalog>();
            buildinFileCatalog.PackageName = packageName;
            buildinFileCatalog.PackageVersion = packageVersion;

            // 记录所有内置资源文件
            DirectoryInfo rootDirectory = new DirectoryInfo(pacakgeDirectory);
            FileInfo[] fileInfos = rootDirectory.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".meta" || fileInfo.Extension == ".version" ||
                    fileInfo.Extension == ".hash" || fileInfo.Extension == ".bytes" ||
                    fileInfo.Extension == ".json")
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

            string saveFilePath = $"Assets/Resources/{YooAssetSettingsData.Setting.DefaultYooFolderName}/{packageName}/{DefaultBuildinFileSystemDefine.BuildinCatalogFileName}";
            FileUtility.CreateFileDirectory(saveFilePath);

            UnityEditor.AssetDatabase.CreateAsset(buildinFileCatalog, saveFilePath);
            UnityEditor.EditorUtility.SetDirty(buildinFileCatalog);
#if UNITY_2019
            UnityEditor.AssetDatabase.SaveAssets();
#else
            UnityEditor.AssetDatabase.SaveAssetIfDirty(buildinFileCatalog);
#endif
            Debug.Log($"Succeed to save buildin file catalog : {saveFilePath}");
        }
    }
}
#endif