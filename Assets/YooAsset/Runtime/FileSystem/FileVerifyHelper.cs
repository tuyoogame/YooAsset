using System;
using System.IO;

namespace YooAsset
{
    internal class FileVerifyHelper
    {
        /// <summary>
        /// 文件校验
        /// </summary>
        public static EFileVerifyResult FileVerify(string filePath, long fileSize, uint fileCRC, EFileVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EFileVerifyResult.DataFileNotExisted;

                // 先验证文件大小
                long size = FileUtility.GetFileSize(filePath);
                if (size < fileSize)
                    return EFileVerifyResult.FileNotComplete;
                else if (size > fileSize)
                    return EFileVerifyResult.FileOverflow;

                // 再验证文件CRC
                if (verifyLevel == EFileVerifyLevel.High)
                {
                    uint crc = HashUtility.FileCRC32Value(filePath);
                    if (crc == fileCRC)
                        return EFileVerifyResult.Succeed;
                    else
                        return EFileVerifyResult.FileCrcError;
                }
                else
                {
                    return EFileVerifyResult.Succeed;
                }
            }
            catch (Exception)
            {
                return EFileVerifyResult.Exception;
            }
        }
    }
}