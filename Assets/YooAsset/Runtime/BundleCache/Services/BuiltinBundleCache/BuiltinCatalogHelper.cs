using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 内置资源目录工具类
    /// </summary>
    internal static class BuiltinCatalogHelper
    {
#if UNITY_EDITOR
        /// <summary>
        /// 生成包裹的内置资源目录文件
        /// 说明：根据指定目录下的文件生成清单文件
        /// </summary>
        /// <param name="decryptor">资源清单解密器</param>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageDirectory">包裹在目录路径</param>
        /// <returns>是否创建文件成功</returns>
        public static bool CreateFile(IManifestDecryptor decryptor, string packageName, string packageDirectory)
        {
            // 获取资源清单版本
            string packageVersion;
            {
                string versionFileName = YooAssetConfiguration.GetPackageVersionFileName(packageName);
                string versionFilePath = $"{packageDirectory}/{versionFileName}";
                if (File.Exists(versionFilePath) == false)
                {
                    Debug.LogError($"Package version file not found: '{versionFilePath}'.");
                    return false;
                }

                packageVersion = FileUtility.ReadAllText(versionFilePath);
            }

            // 加载资源清单文件
            PackageManifest packageManifest;
            {
                string manifestFileName = YooAssetConfiguration.GetManifestBinaryFileName(packageName, packageVersion);
                string manifestFilePath = $"{packageDirectory}/{manifestFileName}";
                if (File.Exists(manifestFilePath) == false)
                {
                    Debug.LogError($"Package manifest file not found: '{manifestFilePath}'.");
                    return false;
                }

                var binaryData = FileUtility.ReadAllBytes(manifestFilePath);
                packageManifest = PackageManifestHelper.DeserializeManifestFromBinary(binaryData, decryptor);
            }

            // 获取文件名映射关系
            Dictionary<string, string> fileMapping = new Dictionary<string, string>();
            {
                foreach (var packageBundle in packageManifest.BundleList)
                {
                    fileMapping.Add(packageBundle.GetFileName(), packageBundle.BundleGuid);
                }
            }

            // 创建内置清单实例
            var buildinCatalog = new BuiltinCatalog();
            buildinCatalog.FileVersion = BuiltinCatalogConsts.FileVersion;
            buildinCatalog.PackageName = packageName;
            buildinCatalog.PackageVersion = packageVersion;

            // 创建白名单查询集合
            HashSet<string> whiteFileNameList = new HashSet<string>
            {
                "link.xml",
                "buildlogtep.json",
                BuiltinCatalogConsts.JsonFileName,
                BuiltinCatalogConsts.BinaryFileName
            };
            string packageVersionFileName = YooAssetConfiguration.GetPackageVersionFileName(packageName);
            string packageHashFileName = YooAssetConfiguration.GetPackageHashFileName(packageName, packageVersion);
            string manifestBinaryFileName = YooAssetConfiguration.GetManifestBinaryFileName(packageName, packageVersion);
            string manifestJsonFileName = YooAssetConfiguration.GetManifestJsonFileName(packageName, packageVersion);
            string reportFileName = YooAssetConfiguration.GetBuildReportFileName(packageName, packageVersion);
            whiteFileNameList.Add(packageVersionFileName);
            whiteFileNameList.Add(packageHashFileName);
            whiteFileNameList.Add(manifestBinaryFileName);
            whiteFileNameList.Add(manifestJsonFileName);
            whiteFileNameList.Add(reportFileName);

            // 记录所有内置资源文件
            DirectoryInfo rootDirectory = new DirectoryInfo(packageDirectory);
            FileInfo[] fileInfos = rootDirectory.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".meta")
                    continue;

                if (whiteFileNameList.Contains(fileInfo.Name))
                    continue;

                string fileName = fileInfo.Name;
                if (fileMapping.TryGetValue(fileName, out string bundleGuid))
                {
                    var fileEntry = new BuiltinCatalog.CatalogEntry();
                    fileEntry.BundleGuid = bundleGuid;
                    fileEntry.FileName = fileName;
                    buildinCatalog.Entries.Add(fileEntry);
                }
                else
                {
                    Debug.LogWarning($"Failed to map file: '{fileName}'.");
                }
            }

            // 创建输出文件
            string jsonFilePath = $"{packageDirectory}/{BuiltinCatalogConsts.JsonFileName}";
            if (File.Exists(jsonFilePath))
                File.Delete(jsonFilePath);
            SerializeToJson(jsonFilePath, buildinCatalog);

            // 创建输出文件
            string binaryFilePath = $"{packageDirectory}/{BuiltinCatalogConsts.BinaryFileName}";
            if (File.Exists(binaryFilePath))
                File.Delete(binaryFilePath);
            SerializeToBinary(binaryFilePath, buildinCatalog);

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"Successfully saved catalog file: {binaryFilePath}");
            return true;
        }

        /// <summary>
        /// 生成空的包裹内置资源目录文件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageVersion">包裹版本</param>
        /// <param name="outputPath">输出目录路径</param>
        /// <returns>是否创建文件成功</returns>
        public static bool CreateEmptyFile(string packageName, string packageVersion, string outputPath)
        {
            // 创建内置清单实例
            var buildinFileCatalog = new BuiltinCatalog();
            buildinFileCatalog.FileVersion = BuiltinCatalogConsts.FileVersion;
            buildinFileCatalog.PackageName = packageName;
            buildinFileCatalog.PackageVersion = packageVersion;

            // 创建输出文件
            string jsonFilePath = $"{outputPath}/{BuiltinCatalogConsts.JsonFileName}";
            if (File.Exists(jsonFilePath))
                File.Delete(jsonFilePath);
            SerializeToJson(jsonFilePath, buildinFileCatalog);

            // 创建输出文件
            string binaryFilePath = $"{outputPath}/{BuiltinCatalogConsts.BinaryFileName}";
            if (File.Exists(binaryFilePath))
                File.Delete(binaryFilePath);
            SerializeToBinary(binaryFilePath, buildinFileCatalog);

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"Successfully saved catalog file: {binaryFilePath}");
            return true;
        }

        /// <summary>
        /// 序列化为 JSON 文件
        /// </summary>
        /// <param name="savePath">文件的保存路径</param>
        /// <param name="catalog">要序列化的内置目录</param>
        public static void SerializeToJson(string savePath, BuiltinCatalog catalog)
        {
            string json = JsonUtility.ToJson(catalog, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 序列化为二进制文件
        /// </summary>
        /// <param name="savePath">文件的保存路径</param>
        /// <param name="catalog">要序列化的内置目录</param>
        public static void SerializeToBinary(string savePath, BuiltinCatalog catalog)
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new BufferWriter(BuiltinCatalogConsts.MaxFileSize);

                // 写入文件标记
                buffer.WriteUInt32(BuiltinCatalogConsts.FileMagic);

                // 写入文件版本
                buffer.WriteInt32(BuiltinCatalogConsts.FileVersion);

                // 写入文件头信息
                buffer.WriteString(catalog.PackageName);
                buffer.WriteString(catalog.PackageVersion);

                // 写入资源包列表
                buffer.WriteInt32(catalog.Entries.Count);
                for (int i = 0; i < catalog.Entries.Count; i++)
                {
                    var fileWrapper = catalog.Entries[i];
                    buffer.WriteString(fileWrapper.BundleGuid);
                    buffer.WriteString(fileWrapper.FileName);
                }

                // 写入文件流
                buffer.WriteToStream(fs);
                fs.Flush();
            }
        }
#endif

        /// <summary>
        /// 从 JSON 反序列化
        /// </summary>
        /// <param name="jsonContent">文本内容</param>
        /// <returns>反序列化得到的内置目录对象</returns>
        public static BuiltinCatalog DeserializeFromJson(string jsonContent)
        {
            return JsonUtility.FromJson<BuiltinCatalog>(jsonContent);
        }

        /// <summary>
        /// 从二进制数据反序列化
        /// </summary>
        /// <param name="binaryData">二进制数据</param>
        /// <returns>反序列化得到的内置目录对象</returns>
        public static BuiltinCatalog DeserializeFromBinary(byte[] binaryData)
        {
            if (binaryData == null || binaryData.Length == 0)
                throw new Exception("Catalog file data is null or empty.");

            // 创建缓存器
            BufferReader buffer = new BufferReader(binaryData);

            // 读取文件标记
            uint fileMagic = buffer.ReadUInt32();
            if (fileMagic != BuiltinCatalogConsts.FileMagic)
                throw new Exception("Catalog file is invalid.");

            // 读取文件版本
            int fileVersion = buffer.ReadInt32();
            if (fileVersion != BuiltinCatalogConsts.FileVersion)
                throw new Exception($"Catalog file version is not compatible: {fileVersion} != {BuiltinCatalogConsts.FileVersion}.");

            BuiltinCatalog catalog = new BuiltinCatalog();
            {
                // 读取文件头信息
                catalog.FileVersion = fileVersion;
                catalog.PackageName = buffer.ReadString();
                catalog.PackageVersion = buffer.ReadString();

                // 读取文件条目列表
                int fileCount = buffer.ReadInt32();
                catalog.Entries = new List<BuiltinCatalog.CatalogEntry>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    var fileEntry = new BuiltinCatalog.CatalogEntry();
                    fileEntry.BundleGuid = buffer.ReadString();
                    fileEntry.FileName = buffer.ReadString();
                    catalog.Entries.Add(fileEntry);
                }
            }

            return catalog;
        }
    }
}
