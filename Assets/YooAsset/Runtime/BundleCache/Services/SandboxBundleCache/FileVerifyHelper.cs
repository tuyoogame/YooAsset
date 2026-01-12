using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 文件校验工具类
    /// </summary>
    internal class FileVerifyHelper
    {
        /// <summary>
        /// 校验文件完整性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileSize">期望的文件大小</param>
        /// <param name="fileCrc">期望的文件CRC值</param>
        /// <returns>校验结果</returns>
        public static EFileVerifyResult VerifyFile(string filePath, long fileSize, uint fileCrc)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EFileVerifyResult.DataFileNotExisted;

                // 可选条件：验证文件大小
                // fileSize == 0 表示调用方不要求大小校验（哨兵值）
                if (fileSize > 0)
                {
                    long size = FileUtility.GetFileSize(filePath);
                    if (size < fileSize)
                        return EFileVerifyResult.DataFileNotComplete;
                    else if (size > fileSize)
                        return EFileVerifyResult.DataFileOverflow;
                }

                // 可选条件：验证文件CRC
                // fileCRC == 0 表示调用方不要求 CRC 校验（哨兵值）
                if (fileCrc > 0)
                {
                    uint crc = HashUtility.ComputeFileCrc32AsUInt(filePath);
                    if (crc == fileCrc)
                        return EFileVerifyResult.Succeed;
                    else
                        return EFileVerifyResult.DataFileCrcError;
                }
                else
                {
                    return EFileVerifyResult.Succeed;
                }
            }
            catch (Exception ex)
            {
                YooLogger.LogError($"File verification exception: {ex.Message}.");
                return EFileVerifyResult.Exception;
            }
        }

        /// <summary>
        /// 校验文件完整性
        /// </summary>
        /// <param name="fileData">文件数据</param>
        /// <param name="fileSize">期望的文件大小</param>
        /// <param name="fileCrc">期望的文件CRC值</param>
        /// <returns>校验结果</returns>
        public static EFileVerifyResult VerifyFile(byte[] fileData, long fileSize, uint fileCrc)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                    return EFileVerifyResult.DataFileInvalid;

                // 可选条件：验证文件大小
                // fileSize == 0 表示调用方不要求大小校验（哨兵值）
                if (fileSize > 0)
                {
                    long size = fileData.Length;
                    if (size < fileSize)
                        return EFileVerifyResult.DataFileNotComplete;
                    else if (size > fileSize)
                        return EFileVerifyResult.DataFileOverflow;
                }

                // 可选条件：验证文件CRC
                // fileCRC == 0 表示调用方不要求 CRC 校验（哨兵值）
                if (fileCrc > 0)
                {
                    uint crc = HashUtility.ComputeCrc32AsUInt(fileData);
                    if (crc == fileCrc)
                        return EFileVerifyResult.Succeed;
                    else
                        return EFileVerifyResult.DataFileCrcError;
                }
                else
                {
                    return EFileVerifyResult.Succeed;
                }
            }
            catch (Exception ex)
            {
                YooLogger.LogError($"File verification exception: {ex.Message}.");
                return EFileVerifyResult.Exception;
            }
        }
    }
}