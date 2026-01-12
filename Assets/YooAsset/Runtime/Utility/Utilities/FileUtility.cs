using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YooAsset
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    internal static class FileUtility
    {
        /// <summary>
        /// 指定路径是否支持文件IO操作
        /// </summary>
        public static bool IsFileIOSupported(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (filePath.StartsWith("jar:") || filePath.StartsWith("content:"))
                return false;
            return true;
        }

        /// <summary>
        /// 读取文件的文本数据
        /// </summary>
        public static string ReadAllText(string filePath)
        {
            if (File.Exists(filePath) == false)
                return string.Empty;

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件的字节数据
        /// </summary>
        public static byte[] ReadAllBytes(string filePath)
        {
            if (File.Exists(filePath) == false)
                return Array.Empty<byte>();

            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 写入文本数据（会覆盖指定路径的文件）
        /// </summary>
        public static void WriteAllText(string filePath, string content)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            EnsureParentDirectoryExists(filePath);

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(filePath, bytes); //避免写入BOM标记
        }

        /// <summary>
        /// 写入字节数据（会覆盖指定路径的文件）
        /// </summary>
        public static void WriteAllBytes(string filePath, byte[] data)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            EnsureParentDirectoryExists(filePath);

            File.WriteAllBytes(filePath, data);
        }

        /// <summary>
        /// 确保文件的父目录存在，如不存在则创建
        /// </summary>
        public static void EnsureParentDirectoryExists(string filePath)
        {
            // 获取文件的文件夹路径
            string directory = Path.GetDirectoryName(filePath);
            EnsureDirectoryExists(directory);
        }

        /// <summary>
        /// 确保目录存在，如不存在则创建
        /// </summary>
        public static void EnsureDirectoryExists(string directory)
        {
            // If the directory doesn't exist, create it.
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// 获取文件大小（字节数）
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            if (File.Exists(filePath) == false)
                return 0;

            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
    }
}