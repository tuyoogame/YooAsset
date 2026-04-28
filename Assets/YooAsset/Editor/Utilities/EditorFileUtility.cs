using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YooAsset.Editor
{
    /// <summary>
    /// 文件操作工具类
    /// </summary>
    public static class EditorFileUtility
    {
        /// <summary>
        /// 创建文件所在的目录
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void CreateFileDirectory(string filePath)
        {
            string destDirectory = Path.GetDirectoryName(filePath);
            CreateDirectory(destDirectory);
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <returns>文件夹不存在并成功创建时为 true</returns>
        public static bool CreateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除文件夹及子目录
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <returns>文件夹存在并成功删除时为 true</returns>
        public static bool DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 文件重命名
        /// </summary>
        /// <param name="filePath">原文件路径</param>
        /// <param name="newName">新文件名（不含扩展名）</param>
        public static void FileRename(string filePath, string newName)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentNullException(nameof(newName));

            string dirPath = Path.GetDirectoryName(filePath);
            string destPath;
            if (Path.HasExtension(filePath))
            {
                string extension = Path.GetExtension(filePath);
                destPath = Path.Combine(dirPath, $"{newName}{extension}");
            }
            else
            {
                destPath = Path.Combine(dirPath, newName);
            }
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="filePath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        public static void MoveFile(string filePath, string destPath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrEmpty(destPath))
                throw new ArgumentNullException(nameof(destPath));

            if (File.Exists(destPath))
                File.Delete(destPath);

            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void CopyFile(string sourcePath, string destPath, bool overwrite)
        {
            if (File.Exists(sourcePath) == false)
                throw new FileNotFoundException(sourcePath);

            CreateFileDirectory(destPath);

            File.Copy(sourcePath, destPath, overwrite);
        }

        /// <summary>
        /// 获取文件字节大小
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的字节大小</returns>
        public static long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        /// <summary>
        /// 读取文件的所有文本内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的文本内容，文件不存在时为空字符串</returns>
        public static string ReadFileAllText(string filePath)
        {
            if (File.Exists(filePath) == false)
                return string.Empty;

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件的所有文本行
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的所有文本行，文件不存在时为空数组</returns>
        public static string[] ReadFileAllLine(string filePath)
        {
            if (File.Exists(filePath) == false)
                return Array.Empty<string>();

            return File.ReadAllLines(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 检测 AssetBundle 文件是否合法
        /// </summary>
        /// <param name="fileData">文件的字节数据</param>
        /// <returns>文件签名合法时为 true</returns>
        public static bool CheckBundleFileValid(byte[] fileData)
        {
            string signature = ReadStringToNull(fileData, 20);
            if (signature == "UnityFS" || signature == "UnityRaw" || signature == "UnityWeb" || signature == "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
                return true;
            else
                return false;
        }
        private static string ReadStringToNull(byte[] data, int maxLength)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < data.Length; i++)
            {
                if (i >= maxLength)
                    break;

                byte b = data[i];
                if (b == 0)
                    break;

                bytes.Add(b);
            }

            if (bytes.Count == 0)
                return string.Empty;
            else
                return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
