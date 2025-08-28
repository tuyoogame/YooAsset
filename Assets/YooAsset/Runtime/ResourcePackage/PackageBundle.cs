using System;
using System.Linq;
using System.Collections.Generic;

namespace YooAsset
{
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
        public uint UnityCRC;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash;

        /// <summary>
        /// 文件校验码
        /// </summary>
        public uint FileCRC;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 文件是否加密
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 依赖的资源包ID集合
        /// 注意：引擎层构建查询结果
        /// </summary>
        public int[] DependBundleIDs;

        /// <summary>
        /// 资源包GUID
        /// </summary>
        public string BundleGUID
        {
            get { return FileHash; }
        }

        /// <summary>
        /// 资源包类型
        /// </summary>
        public int BundleType
        {
            get
            {
                return _bundleType;
            }
        }
        private int _bundleType;

        /// <summary>
        /// 文件名称
        /// </summary>  
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                    throw new Exception("Should never get here !");
                return _fileName;
            }
        }
        private string _fileName;

        /// <summary>
        /// 文件后缀名
        /// </summary>
        public string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(_fileExtension))
                    throw new Exception("Should never get here !");
                return _fileExtension;
            }
        }
        private string _fileExtension;

        /// <summary>
        /// 包含的主资源集合
        /// </summary>
        [NonSerialized]
        public readonly List<PackageAsset> IncludeMainAssets = new List<PackageAsset>(10);

        /// <summary>
        /// 引用该资源包的资源包列表
        /// 说明：谁引用了该资源包
        /// </summary>
        [NonSerialized]
        public readonly List<int> ReferenceBundleIDs = new List<int>(10);
        private readonly HashSet<int> _referenceBundleIDs = new HashSet<int>();


        public PackageBundle()
        {
        }

        /// <summary>
        /// 初始化资源包
        /// </summary>
        public void InitBundle(PackageManifest manifest)
        {
            _mainfest = manifest;
            _bundleType = manifest.BuildBundleType;
            _fileExtension = ManifestTools.GetRemoteBundleFileExtension(BundleName);
            _fileName = ManifestTools.GetRemoteBundleFileName(manifest.OutputNameStyle, BundleName, _fileExtension, FileHash);
        }

        /// <summary>
        /// 添加引用该资源包的资源包ID
        /// 说明：谁引用了该资源包
        /// </summary>
        public void AddReferenceBundleID(int bundleID)
        {
            if (_referenceBundleIDs.Contains(bundleID) == false)
            {
                _referenceBundleIDs.Add(bundleID);
                ReferenceBundleIDs.Add(bundleID);
            }
        }

        /// <summary>
        /// 是否包含Tag
        /// </summary>
        public bool HasTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (Tags == null || Tags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (Tags.Contains(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含任意Tags
        /// </summary>
        public bool HasAnyTags()
        {
            if (Tags != null && Tags.Length > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 检测资源包文件内容是否相同
        /// </summary>
        public bool Equals(PackageBundle otherBundle)
        {
            if (FileHash == otherBundle.FileHash)
                return true;

            return false;
        }

        #region 调试信息
        private PackageManifest _mainfest;
        private List<string> _debugReferenceBundles;
        public List<string> GetDebugReferenceBundles()
        {
            if (_debugReferenceBundles == null)
            {
                _debugReferenceBundles = new List<string>(ReferenceBundleIDs.Count);
                foreach (int bundleID in ReferenceBundleIDs)
                {
                    var packageBundle = _mainfest.BundleList[bundleID];
                    _debugReferenceBundles.Add(packageBundle.BundleName);
                }
            }
            return _debugReferenceBundles;
        }
        #endregion
    }
}