using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 清单文件工具类
    /// </summary>
    internal static class PackageManifestHelper
    {
        /// <summary>
        /// MD5哈希值长度（32个字符）
        /// </summary>
        private const int MD5HashLength = 32;

        /// <summary>
        /// 验证清单文件的二进制数据
        /// </summary>
        /// <param name="fileData">文件二进制数据</param>
        /// <param name="hashValue">期望的哈希值</param>
        /// <returns>如果验证通过返回true，否则返回false。</returns>
        public static bool VerifyManifestData(byte[] fileData, string hashValue)
        {
            if (fileData == null || fileData.Length == 0)
                return false;
            if (string.IsNullOrEmpty(hashValue))
                return false;

            // 注意:兼容两种验证方式
            string fileHash;
            if (hashValue.Length == MD5HashLength)
                fileHash = HashUtility.ComputeMD5(fileData);
            else
                fileHash = HashUtility.ComputeCrc32(fileData);

            return fileHash == hashValue;
        }

        /// <summary>
        /// 将清单文件序列化为二进制格式
        /// </summary>
        /// <param name="savePath">保存路径</param>
        /// <param name="manifest">清单对象</param>
        /// <param name="encryptor">清单加密（可为null）</param>
        public static void SerializeManifestToBinary(string savePath, BuildManifest manifest, IManifestEncryptor encryptor)
        {
            SerializeManifestOperation operation = new SerializeManifestOperation(manifest, encryptor);
            operation.StartOperation();
            operation.WaitForCompletion();
            FileUtility.WriteAllBytes(savePath, operation.FileData);
        }

        /// <summary>
        /// 从二进制数据反序列化清单文件
        /// </summary>
        /// <param name="binaryData">二进制数据</param>
        /// <param name="decryptor">清单解密（可为null）</param>
        /// <returns>返回反序列化后的清单对象</returns>
        public static PackageManifest DeserializeManifestFromBinary(byte[] binaryData, IManifestDecryptor decryptor)
        {
            DeserializeManifestOperation operation = new DeserializeManifestOperation(decryptor, binaryData);
            operation.StartOperation();
            operation.WaitForCompletion();
            return operation.Manifest;
        }
    }
}