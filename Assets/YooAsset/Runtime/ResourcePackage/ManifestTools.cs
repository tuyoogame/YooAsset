using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal static class ManifestTools
    {
        /// <summary>
        /// 验证清单文件的二进制数据
        /// </summary>
        public static bool VerifyManifestData(byte[] fileData, string hashValue)
        {
            if (fileData == null || fileData.Length == 0)
                return false;
            if (string.IsNullOrEmpty(hashValue))
                return false;

            // 注意：兼容俩种验证方式
            // 注意：计算MD5的哈希值通常为32个字符
            string fileHash;
            if (hashValue.Length == 32)
                fileHash = HashUtility.BytesMD5(fileData);
            else
                fileHash = HashUtility.BytesCRC32(fileData);

            if (fileHash == hashValue)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 序列化（JSON文件）
        /// </summary>
        public static void SerializeToJson(string savePath, PackageManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 序列化（二进制文件）
        /// </summary>
        public static void SerializeToBinary(string savePath, PackageManifest manifest, IManifestProcessServices services)
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new BufferWriter(ManifestDefine.FileMaxSize);

                // 写入文件标记
                buffer.WriteUInt32(ManifestDefine.FileSign);

                // 写入文件版本
                buffer.WriteUTF8(manifest.FileVersion);

                // 写入文件头信息
                buffer.WriteBool(manifest.EnableAddressable);
                buffer.WriteBool(manifest.SupportExtensionless);
                buffer.WriteBool(manifest.LocationToLower);
                buffer.WriteBool(manifest.IncludeAssetGUID);
                buffer.WriteBool(manifest.ReplaceAssetPathWithAddress);
                buffer.WriteInt32(manifest.OutputNameStyle);
                buffer.WriteInt32(manifest.BuildBundleType);
                buffer.WriteUTF8(manifest.BuildPipeline);
                buffer.WriteUTF8(manifest.PackageName);
                buffer.WriteUTF8(manifest.PackageVersion);
                buffer.WriteUTF8(manifest.PackageNote);

                // 写入资源列表
                buffer.WriteInt32(manifest.AssetList.Count);
                for (int i = 0; i < manifest.AssetList.Count; i++)
                {
                    var packageAsset = manifest.AssetList[i];
                    buffer.WriteUTF8(packageAsset.Address);
                    buffer.WriteUTF8(packageAsset.AssetPath);
                    buffer.WriteUTF8(packageAsset.AssetGUID);
                    buffer.WriteUTF8Array(packageAsset.AssetTags);
                    buffer.WriteInt32(packageAsset.BundleID);
                    buffer.WriteInt32Array(packageAsset.DependBundleIDs);
                }

                // 写入资源包列表
                buffer.WriteInt32(manifest.BundleList.Count);
                for (int i = 0; i < manifest.BundleList.Count; i++)
                {
                    var packageBundle = manifest.BundleList[i];
                    buffer.WriteUTF8(packageBundle.BundleName);
                    buffer.WriteUInt32(packageBundle.UnityCRC);
                    buffer.WriteUTF8(packageBundle.FileHash);
                    buffer.WriteUInt32(packageBundle.FileCRC);
                    buffer.WriteInt64(packageBundle.FileSize);
                    buffer.WriteBool(packageBundle.Encrypted);
                    buffer.WriteUTF8Array(packageBundle.Tags);
                    buffer.WriteInt32Array(packageBundle.DependBundleIDs);
                }

                // 清单处理操作
                if (services != null)
                {
                    var tempBytes = buffer.GetBytes();
                    var resultBytes = services.ProcessManifest(tempBytes);
                    fs.Write(resultBytes, 0, resultBytes.Length);
                    fs.Flush();
                }
                else
                {
                    // 写入文件流
                    buffer.WriteToStream(fs);
                    fs.Flush();
                }
            }
        }

        /// <summary>
        /// 反序列化（JSON文件）
        /// </summary>
        public static PackageManifest DeserializeFromJson(string jsonContent)
        {
            var manifest = JsonUtility.FromJson<PackageManifest>(jsonContent);

            // 初始化资源包
            for (int i = 0; i < manifest.BundleList.Count; i++)
            {
                var packageBundle = manifest.BundleList[i];
                packageBundle.InitBundle(manifest);
            }

            // 初始化资源清单
            manifest.Initialize();
            return manifest;
        }

        /// <summary>
        /// 反序列化（二进制文件）
        /// </summary>
        public static PackageManifest DeserializeFromBinary(byte[] binaryData, IManifestRestoreServices services)
        {
            DeserializeManifestOperation operation = new DeserializeManifestOperation(services, binaryData);
            operation.StartOperation();
            operation.WaitForAsyncComplete();
            return operation.Manifest;
        }

        /// <summary>
        /// 获取资源文件的后缀名
        /// </summary>
        public static string GetRemoteBundleFileExtension(string bundleName)
        {
            string fileExtension = Path.GetExtension(bundleName);
            return fileExtension;
        }

        /// <summary>
        /// 获取远端的资源文件名
        /// </summary>
        public static string GetRemoteBundleFileName(int nameStyle, string bundleName, string fileExtension, string fileHash)
        {
            if (nameStyle == (int)EFileNameStyle.HashName)
            {
                return StringUtility.Format("{0}{1}", fileHash, fileExtension);
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName)
            {
                return bundleName;
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName_HashName)
            {
                if (string.IsNullOrEmpty(fileExtension))
                {
                    return StringUtility.Format("{0}_{1}", bundleName, fileHash);
                }
                else
                {
                    string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
                    return StringUtility.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
                }
            }
            else
            {
                throw new NotImplementedException($"Invalid name style : {nameStyle}");
            }
        }
    }
}