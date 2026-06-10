using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清单中的资源包描述
    /// </summary>
    [Serializable]
    internal class PackageBundle
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// Unity引擎生成的CRC
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
        /// 文件是否加密
        /// </summary>
        public bool IsEncrypted;

        /// <summary>
        /// 依赖的资源包ID集合
        /// 注意：引擎层构建查询结果
        /// </summary>
        public int[] DependentBundleIDs;

        /// <summary>
        /// 资源包唯一标识符
        /// </summary>
        /// <remarks>使用FileHash作为GUID</remarks>
        public string BundleGuid
        {
            get { return FileHash; }
        }

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        [NonSerialized]
        public PackageTags Tags;

        /// <summary>
        /// 包含的主资源集合
        /// </summary>
        [NonSerialized]
        public List<PackageAsset> MainAssets;

        /// <summary>
        /// 引用该资源包的资源包ID列表
        /// 说明：谁引用了该资源包
        /// </summary>
        [NonSerialized]
        public List<int> ReferrerBundleIDs;

        [NonSerialized]
        private PackageManifest _manifest;
        [NonSerialized]
        private string _fileName;


        /// <summary>
        /// 创建资源包实例
        /// </summary>
        public PackageBundle()
        {
        }

        /// <summary>
        /// 初始化资源包
        /// </summary>
        public void Initialize(PackageManifest manifest)
        {
            _manifest = manifest;
            _fileName = BundleFileNaming.GetBundleFileName(manifest.OutputNameStyle, BundleName, FileHash);
        }

        /// <summary>
        /// 获取资源包类型
        /// </summary>
        /// <returns>返回资源包类型</returns>
        public int GetBundleType()
        {
            if (_manifest == null)
                throw new YooInternalException("PackageBundle is not initialized.");
            return _manifest.BuildBundleType;
        }

        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <returns>返回资源包文件名称</returns>
        public string GetFileName()
        {
            if (_manifest == null)
                throw new YooInternalException("PackageBundle is not initialized.");
            return _fileName;
        }

        #region 调试信息
        [NonSerialized]
        private List<string> _debugReferrerBundleNames;

        /// <summary>
        /// 获取引用该资源包的资源包名称列表
        /// </summary>
        /// <returns>返回引用该资源包的所有资源包名称列表</returns>
        /// <remarks>仅用于调试目的，结果会被缓存。</remarks>
        public List<string> DebugGetReferrerBundleNames()
        {
            if (_debugReferrerBundleNames == null)
            {
                _debugReferrerBundleNames = new List<string>(ReferrerBundleIDs.Count);
                foreach (int bundleID in ReferrerBundleIDs)
                {
                    var packageBundle = _manifest.BundleList[bundleID];
                    _debugReferrerBundleNames.Add(packageBundle.BundleName);
                }
            }
            return _debugReferrerBundleNames;
        }
        #endregion
    }
}