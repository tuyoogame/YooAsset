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
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags;

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
        /// 包含的主资源集合
        /// </summary>
        [NonSerialized]
        public readonly List<PackageAsset> MainAssets = new List<PackageAsset>(10);

        /// <summary>
        /// 引用该资源包的资源包列表
        /// 说明：谁引用了该资源包
        /// </summary>
        [NonSerialized]
        public readonly List<int> ReferrerBundleIDs = new List<int>(10);
        [NonSerialized]
        private readonly HashSet<int> _referrerBundleIDs = new HashSet<int>();

        [NonSerialized]
        private PackageManifest _manifest;
        [NonSerialized]
        private bool _isInitialized;
        [NonSerialized]
        private int _bundleType;
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
        /// <param name="manifest">所属的资源清单</param>
        public void Initialize(PackageManifest manifest)
        {
            _isInitialized = true;
            _manifest = manifest;
            _bundleType = manifest.BuildBundleType;
            string fileExtension = PackageManifestHelper.GetRemoteBundleFileExtension(BundleName);
            _fileName = PackageManifestHelper.GetRemoteBundleFileName(manifest.OutputNameStyle, BundleName, fileExtension, FileHash);
        }

        /// <summary>
        /// 获取资源包类型
        /// </summary>
        /// <returns>返回资源包类型</returns>
        public int GetBundleType()
        {
            if (_isInitialized == false)
                throw new YooInternalException("PackageBundle is not initialized.");
            return _bundleType;
        }

        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <returns>返回资源包文件名称</returns>
        public string GetFileName()
        {
            if (_isInitialized == false)
                throw new YooInternalException("PackageBundle is not initialized.");
            return _fileName;
        }

        /// <summary>
        /// 添加引用该资源包的资源包ID
        /// </summary>
        /// <param name="bundleID">引用该资源包的资源包ID</param>
        /// <remarks>记录谁引用了该资源包</remarks>
        public void AddReferrerBundleID(int bundleID)
        {
            if (_referrerBundleIDs.Contains(bundleID) == false)
            {
                _referrerBundleIDs.Add(bundleID);
                ReferrerBundleIDs.Add(bundleID);
            }
        }

        /// <summary>
        /// 是否包含指定的单个标签
        /// </summary>
        public bool HasTag(string tag)
        {
            if (Tags == null || Tags.Length == 0)
                return false;

            for (int i = 0; i < Tags.Length; i++)
            {
                if (Tags[i] == tag)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含指定标签数组中的任意一个
        /// </summary>
        public bool HasAnyTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (Tags == null || Tags.Length == 0)
                return false;

            for (int i = 0; i < tags.Length; i++)
            {
                if (HasTag(tags[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含分类标签
        /// </summary>
        /// <returns>如果包含至少一个标签返回true，否则返回false。</returns>
        public bool IsTagged()
        {
            return Tags != null && Tags.Length > 0;
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