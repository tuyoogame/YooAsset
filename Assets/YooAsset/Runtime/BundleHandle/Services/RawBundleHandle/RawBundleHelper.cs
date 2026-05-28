using System.IO;

namespace YooAsset
{
    internal static class RawBundleHelper
    {
        /// <summary>
        /// 从本地文件加载 RawBundle
        /// </summary>
        public static RawBundle LoadFromFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            return new RawBundle(fileData);
        }

        /// <summary>
        /// 从内存数据加载 RawBundle
        /// </summary>
        public static RawBundle LoadFromMemory(byte[] fileData)
        {
            return new RawBundle(fileData);
        }
    }
}
