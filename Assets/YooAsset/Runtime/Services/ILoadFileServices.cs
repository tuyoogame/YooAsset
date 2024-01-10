//using System.Collections;
//using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEngine;
//using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace YooAsset
{
    public interface ILoadFileServices
    {
        /// <summary>
        /// 判断是否可以直接从 StreamingAsset 读取文件
        /// </summary>
        public bool EnableLoadStreamingAsset { get; }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Exists(string filePath);

        /// <summary>
        /// 读取文件内容 byte[]
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public byte[] ReadAllBytes(string filePath);

        /// <summary>
        /// 读取文件内容 string
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string ReadAllText(string filePath);

        /// <summary>
        /// 写入bytes
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public void WriteAllBytes(string filePath, byte[] data);

        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public void WriteAllText(string filePath, string text);

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath"></param>
        public long GetFileSize(string filePath);
    }

    internal class DefaultLoadFileServices : ILoadFileServices
    {
        /// <summary>
        /// 默认平台不支持android 直接读取streamingAsset文件
        /// </summary>
        public bool EnableLoadStreamingAsset 
        { 
            get
            {
#if UNITY_ANDROID
                return false;
#else
                return true;
#endif
            }
        }

        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        public long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        public byte[] ReadAllBytes(string filePath)
        {
            if (File.Exists(filePath) == false)
                return null;
            return File.ReadAllBytes(filePath);
        }

        public string ReadAllText(string filePath)
        {
            if (File.Exists(filePath) == false)
                return string.Empty;
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void WriteAllBytes(string filePath, byte[] data)
        {
            // 创建文件夹路径
            FileUtility.CreateFileDirectory(filePath);

            File.WriteAllBytes(filePath, data);
        }

        public void WriteAllText(string filePath, string content)
        {
            // 创建文件夹路径
            FileUtility.CreateFileDirectory(filePath);
            //避免写入BOM标记
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(filePath, bytes);
        }

    }
}

