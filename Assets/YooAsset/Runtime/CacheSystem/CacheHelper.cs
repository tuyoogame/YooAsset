using System;
using System.IO;

namespace YooAsset
{
    internal static class CacheHelper
    {
        /// <summary>
        /// 禁用Unity缓存系统在WebGL平台
        /// </summary>
        public static bool DisableUnityCacheOnWebGL = false;

        #region 资源信息文件相关
        private static readonly BufferWriter SharedBuffer = new BufferWriter(1024);

        /// <summary>
        /// 写入资源包信息
        /// </summary>
        public static void WriteInfoToFile(string filePath, string dataFileCRC, long dataFileSize)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                SharedBuffer.Clear();
                SharedBuffer.WriteUTF8(dataFileCRC);
                SharedBuffer.WriteInt64(dataFileSize);
                SharedBuffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 读取资源包信息
        /// </summary>
        public static void ReadInfoFromFile(string filePath, out string dataFileCRC, out long dataFileSize)
        {
            byte[] binaryData = FileUtility.ReadAllBytes(filePath);
            BufferReader buffer = new BufferReader(binaryData);
            dataFileCRC = buffer.ReadUTF8();
            dataFileSize = buffer.ReadInt64();
        }
        #endregion

        #region 资源文件验证相关
        /// <summary>
        /// 验证缓存文件（子线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingCacheFile(VerifyCacheFileElement element, EVerifyLevel verifyLevel)
        {
            try
            {
                if (verifyLevel == EVerifyLevel.Low)
                {
                    if (File.Exists(element.InfoFilePath) == false)
                        return EVerifyResult.InfoFileNotExisted;
                    if (File.Exists(element.DataFilePath) == false)
                        return EVerifyResult.DataFileNotExisted;
                    return EVerifyResult.Succeed;
                }
                else
                {
                    if (File.Exists(element.InfoFilePath) == false)
                        return EVerifyResult.InfoFileNotExisted;

                    // 解析信息文件获取验证数据
                    CacheHelper.ReadInfoFromFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
                }
            }
            catch (Exception)
            {
                return EVerifyResult.Exception;
            }

            return VerifyingInternal(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
        }

        /// <summary>
        /// 验证下载文件（子线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingTempFile(VerifyTempFileElement element)
        {
            return VerifyingInternal(element.TempDataFilePath, element.FileSize, element.FileCRC, EVerifyLevel.High);
        }

        /// <summary>
        /// 验证记录文件（主线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingRecordFile(CacheManager cache, string cacheGUID)
        {
            var wrapper = cache.TryGetWrapper(cacheGUID);
            if (wrapper == null)
                return EVerifyResult.CacheNotFound;

            EVerifyResult result = VerifyingInternal(wrapper.DataFilePath, wrapper.DataFileSize, wrapper.DataFileCRC, EVerifyLevel.High);
            return result;
        }

        private static EVerifyResult VerifyingInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EVerifyResult.DataFileNotExisted;

                // 先验证文件大小
                long size = FileUtility.GetFileSize(filePath);
                if (size < fileSize)
                    return EVerifyResult.FileNotComplete;
                else if (size > fileSize)
                    return EVerifyResult.FileOverflow;

                // 再验证文件CRC
                if (verifyLevel == EVerifyLevel.High)
                {
                    string crc = HashUtility.FileCRC32Safely(filePath);
                    if (crc == fileCRC)
                        return EVerifyResult.Succeed;
                    else
                        return EVerifyResult.FileCrcError;
                }
                else
                {
                    return EVerifyResult.Succeed;
                }
            }
            catch (Exception)
            {
                return EVerifyResult.Exception;
            }
        }
        #endregion
    }
}