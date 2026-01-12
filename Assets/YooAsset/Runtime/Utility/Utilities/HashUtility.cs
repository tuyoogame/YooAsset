using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YooAsset
{
    /// <summary>
    /// 哈希工具类
    /// </summary>
    public static class HashUtility
    {
        private static string ToHexString(byte[] hashBytes)
        {
            char[] chars = new char[hashBytes.Length * 2];
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                chars[i * 2] = GetHexChar(b >> 4);
                chars[i * 2 + 1] = GetHexChar(b & 0xF);
            }
            return new string(chars);
        }
        private static char GetHexChar(int value)
        {
            return (char)(value < 10 ? '0' + value : 'a' + value - 10);
        }

        #region MD5
        /// <summary>
        /// 计算字符串的MD5哈希值
        /// </summary>
        public static string ComputeMD5(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return ComputeMD5(buffer);
        }

        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        public static string ComputeFileMD5(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeMD5(fs);
            }
        }

        /// <summary>
        /// 计算数据流的MD5哈希值
        /// </summary>
        public static string ComputeMD5(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// 计算字节数组的MD5哈希值
        /// </summary>
        public static string ComputeMD5(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(buffer);
                return ToHexString(hashBytes);
            }
        }
        #endregion

        #region CRC32
        /// <summary>
        /// 计算字符串的CRC32哈希值
        /// </summary>
        public static string ComputeCrc32(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return ComputeCrc32(buffer);
        }

        /// <summary>
        /// 计算字符串的CRC32值（返回无符号整数）
        /// </summary>
        public static uint ComputeCrc32AsUInt(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return ComputeCrc32AsUInt(buffer);
        }

        /// <summary>
        /// 计算文件的CRC32哈希值
        /// </summary>
        public static string ComputeFileCrc32(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeCrc32(fs);
            }
        }

        /// <summary>
        /// 计算文件的CRC32值（返回无符号整数）
        /// </summary>
        public static uint ComputeFileCrc32AsUInt(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeCrc32AsUInt(fs);
            }
        }

        /// <summary>
        /// 计算数据流的CRC32哈希值
        /// </summary>
        public static string ComputeCrc32(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (Crc32Algorithm hash = new Crc32Algorithm())
            {
                byte[] hashBytes = hash.ComputeHash(stream);
                return ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// 计算数据流的CRC32值（返回无符号整数）
        /// </summary>
        public static uint ComputeCrc32AsUInt(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (Crc32Algorithm hash = new Crc32Algorithm())
            {
                hash.ComputeHash(stream);
                return hash.Crc32Value;
            }
        }

        /// <summary>
        /// 计算字节数组的CRC32哈希值
        /// </summary>
        public static string ComputeCrc32(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            using (Crc32Algorithm hash = new Crc32Algorithm())
            {
                byte[] hashBytes = hash.ComputeHash(buffer);
                return ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// 计算字节数组的CRC32值（返回无符号整数）
        /// </summary>
        public static uint ComputeCrc32AsUInt(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            using (Crc32Algorithm hash = new Crc32Algorithm())
            {
                hash.ComputeHash(buffer);
                return hash.Crc32Value;
            }
        }
        #endregion
    }
}
