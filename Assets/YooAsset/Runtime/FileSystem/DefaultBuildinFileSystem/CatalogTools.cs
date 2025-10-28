using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal static class CatalogTools
    {
#if UNITY_EDITOR
        /// <summary>
        /// 生成包裹的内置资源目录文件
        /// 说明：根据指定目录下的文件生成清单文件。
        /// </summary>
        public static bool CreateCatalogFile(IManifestRestoreServices services, string packageName, string packageDirectory)
        {
            // 获取资源清单版本
            string packageVersion;
            {
                string versionFileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
                string versionFilePath = $"{packageDirectory}/{versionFileName}";
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
                string manifestFilePath = $"{packageDirectory}/{manifestFileName}";
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
                DefaultBuildinFileSystemDefine.BuildinCatalogJsonFileName,
                DefaultBuildinFileSystemDefine.BuildinCatalogBinaryFileName
            };
            string packageVersionFileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
            string packageHashFileName = YooAssetSettingsData.GetPackageHashFileName(packageName, packageVersion);
            string manifestBinaryFIleName = YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion);
            string manifestJsonFIleName = YooAssetSettingsData.GetManifestJsonFileName(packageName, packageVersion);
            string reportFileName = YooAssetSettingsData.GetBuildReportFileName(packageName, packageVersion);
            whiteFileList.Add(packageVersionFileName);
            whiteFileList.Add(packageHashFileName);
            whiteFileList.Add(manifestBinaryFIleName);
            whiteFileList.Add(manifestJsonFIleName);
            whiteFileList.Add(reportFileName);

            // 记录所有内置资源文件
            DirectoryInfo rootDirectory = new DirectoryInfo(packageDirectory);
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
            string jsonFilePath = $"{packageDirectory}/{DefaultBuildinFileSystemDefine.BuildinCatalogJsonFileName}";
            if (File.Exists(jsonFilePath))
                File.Delete(jsonFilePath);
            SerializeToJson(jsonFilePath, buildinFileCatalog);

            // 创建输出文件
            string binaryFilePath = $"{packageDirectory}/{DefaultBuildinFileSystemDefine.BuildinCatalogBinaryFileName}";
            if (File.Exists(binaryFilePath))
                File.Delete(binaryFilePath);
            SerializeToBinary(binaryFilePath, buildinFileCatalog);

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"Succeed to save catalog file : {binaryFilePath}");
            return true;
        }
#endif

        /// <summary>
        /// 序列化（JSON文件）
        /// </summary>
        public static void SerializeToJson(string savePath, DefaultBuildinFileCatalog catalog)
        {
            string json = JsonUtility.ToJson(catalog, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 反序列化（JSON文件）
        /// </summary>
        public static DefaultBuildinFileCatalog DeserializeFromJson(string jsonContent)
        {
            return JsonUtility.FromJson<DefaultBuildinFileCatalog>(jsonContent);
        }

        /// <summary>
        /// 序列化（二进制文件）
        /// </summary>
        public static void SerializeToBinary(string savePath, DefaultBuildinFileCatalog catalog)
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new BufferWriter(CatalogDefine.FileMaxSize);

                // 写入文件标记
                buffer.WriteUInt32(CatalogDefine.FileSign);

                // 写入文件版本
                buffer.WriteUTF8(CatalogDefine.FileVersion);

                // 写入文件头信息
                buffer.WriteUTF8(catalog.PackageName);
                buffer.WriteUTF8(catalog.PackageVersion);

                // 写入资源包列表
                buffer.WriteInt32(catalog.Wrappers.Count);
                for (int i = 0; i < catalog.Wrappers.Count; i++)
                {
                    var fileWrapper = catalog.Wrappers[i];
                    buffer.WriteUTF8(fileWrapper.BundleGUID);
                    buffer.WriteUTF8(fileWrapper.FileName);
                }

                // 写入文件流
                buffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 反序列化（二进制文件）
        /// </summary>
        public static DefaultBuildinFileCatalog DeserializeFromBinary(byte[] binaryData)
        {
            // 创建缓存器
            BufferReader buffer = new BufferReader(binaryData);

            // 读取文件标记
            uint fileSign = buffer.ReadUInt32();
            if (fileSign != CatalogDefine.FileSign)
                throw new Exception("Invalid catalog file !");

            // 读取文件版本
            string fileVersion = buffer.ReadUTF8();
            if (fileVersion != CatalogDefine.FileVersion)
                throw new Exception($"The catalog file version are not compatible : {fileVersion} != {CatalogDefine.FileVersion}");

            DefaultBuildinFileCatalog catalog = new DefaultBuildinFileCatalog();
            {
                // 读取文件头信息
                catalog.FileVersion = fileVersion;
                catalog.PackageName = buffer.ReadUTF8();
                catalog.PackageVersion = buffer.ReadUTF8();

                // 读取资源包列表
                int fileCount = buffer.ReadInt32();
                catalog.Wrappers = new List<DefaultBuildinFileCatalog.FileWrapper>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    var fileWrapper = new DefaultBuildinFileCatalog.FileWrapper();
                    fileWrapper.BundleGUID = buffer.ReadUTF8();
                    fileWrapper.FileName = buffer.ReadUTF8();
                    catalog.Wrappers.Add(fileWrapper);
                }
            }

            return catalog;
        }
    }
}