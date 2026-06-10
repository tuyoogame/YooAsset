using System.Collections.Generic;
using System.Linq;

namespace YooAsset
{
    /// <summary>
    /// 构建期专用的资源包描述
    /// </summary>
    internal class BuildBundle
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// Unity引擎计算的内容校验码
        /// </summary>
        public uint UnityCrc;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash;

        /// <summary>
        /// 文件校验码
        /// </summary>
        public uint FileCrc;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 是否为加密文件
        /// </summary>
        public bool IsEncrypted;

        /// <summary>
        /// 依赖的资源包ID集合
        /// 注意：引擎层构建查询结果
        /// </summary>
        public int[] DependentBundleIDs;

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 引用该资源包的资源包ID集合
        /// </summary>
        public readonly List<int> ReferrerBundleIDs = new List<int>();


        /// <summary>
        /// 获取资源包文件名称
        /// </summary>
        /// <param name="outputNameStyle">文件名称样式</param>
        /// <returns>返回根据命名样式生成的远端文件名</returns>
        public string GetFileName(int outputNameStyle)
        {
            return BundleFileNaming.GetBundleFileName(outputNameStyle, BundleName, FileHash);
        }

        /// <summary>
        /// 是否包含指定标签数组中的任意一个
        /// </summary>
        public bool HasAnyTag(string[] tags)
        {
            if (tags == null || tags.Length == 0 || Tags == null || Tags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (Tags.Contains(tag))
                    return true;
            }
            return false;
        }
    }
}
