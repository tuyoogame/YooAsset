using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 资源包文件命名工具
    /// </summary>
    internal static class BundleFileNaming
    {
        /// <summary>
        /// 获取资源包的文件名
        /// </summary>
        /// <param name="nameStyle">文件名称样式</param>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="fileHash">文件哈希值</param>
        /// <returns>返回根据命名样式生成的远端文件名</returns>
        public static string GetBundleFileName(int nameStyle, string bundleName, string fileHash)
        {
            EFileNameStyle style = (EFileNameStyle)nameStyle;
            switch (style)
            {
                case EFileNameStyle.HashName:
                    {
                        string fileExtension = Path.GetExtension(bundleName);
                        return StringUtility.Format("{0}{1}", fileHash, fileExtension);
                    }

                case EFileNameStyle.BundleName:
                    return bundleName;

                case EFileNameStyle.BundleName_HashName:
                    {
                        string fileExtension = Path.GetExtension(bundleName);
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

                default:
                    throw new System.NotImplementedException($"Invalid name style: {nameStyle}.");
            }
        }
    }
}
