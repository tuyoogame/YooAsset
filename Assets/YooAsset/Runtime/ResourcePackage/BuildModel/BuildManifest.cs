using System;
using System.Collections.Generic;
using System.Linq;

namespace YooAsset
{
    /// <summary>
    /// 构建期专用的清单模型
    /// </summary>
    internal class BuildManifest
    {
        private bool _referrerGraphBuilt;

        // 全局标签表
        private bool _tagTableBuilt;
        private string[] _tagTable;
        private Dictionary<string, int> _tagToIndex;

        // 全局目录表
        private bool _directoryTableBuilt;
        private string[] _directoryTable;
        private Dictionary<string, int> _directoryToIndex;

        /// <summary>
        /// 文件版本
        /// </summary>
        public int FileVersion;

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        /// 支持无后缀名的资源定位地址
        /// </summary>
        public bool SupportExtensionless;

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower;

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGuid;

        /// <summary>
        /// 使用可寻址地址代替资源路径
        /// </summary>
        public bool ReplaceAssetPathWithAddress;

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle;

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType;

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 资源包裹的备注信息
        /// </summary>
        public string PackageNote;

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<BuildAsset> AssetList = new List<BuildAsset>();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<BuildBundle> BundleList = new List<BuildBundle>();

        /// <summary>
        /// 全局标签表（去重后的标签数组）
        /// </summary>
        public string[] TagTable
        {
            get { return _tagTable; }
        }

        /// <summary>
        /// 全局目录表（去重后的目录数组）
        /// </summary>
        public string[] DirectoryTable
        {
            get { return _directoryTable; }
        }


        /// <summary>
        /// 构建资源包的引用关系图
        /// 说明：根据每个资源包的依赖列表，反向填充被依赖方的引用者集合。
        /// </summary>
        public void BuildReferrerGraph()
        {
            if (_referrerGraphBuilt)
                throw new YooInternalException("BuildReferrerGraph has already been called.");
            _referrerGraphBuilt = true;

            for (int index = 0; index < BundleList.Count; index++)
            {
                var sourceBundle = BundleList[index];
                if (sourceBundle.DependentBundleIDs == null)
                    continue;

                foreach (int dependIndex in sourceBundle.DependentBundleIDs)
                {
                    if (dependIndex >= 0 && dependIndex < BundleList.Count)
                    {
                        var dependBundle = BundleList[dependIndex];
                        if (dependBundle.ReferrerBundleIDs.Contains(index))
                            throw new YooInternalException($"Duplicate referrer bundle ID detected: referrer {index} -> bundle {dependIndex}.");
                        dependBundle.ReferrerBundleIDs.Add(index);
                    }
                    else
                    {
                        throw new System.ArgumentOutOfRangeException($"Invalid dependent bundle index: {dependIndex}. Valid range is 0 to {BundleList.Count - 1}.");
                    }
                }
            }
        }

        /// <summary>
        /// 构建去重后的全局标签表，并填充标签到索引的映射
        /// </summary>
        public void BuildTagTable()
        {
            if (_tagTableBuilt)
                throw new YooInternalException("BuildTagTable has already been called.");
            _tagTableBuilt = true;

            HashSet<string> tagSet = new HashSet<string>();
            foreach (var buildAsset in AssetList)
            {
                if (buildAsset.Tags == null)
                    continue;
                foreach (var tag in buildAsset.Tags)
                {
                    tagSet.Add(tag);
                }
            }
            foreach (var buildBundle in BundleList)
            {
                if (buildBundle.Tags == null)
                    continue;
                foreach (var tag in buildBundle.Tags)
                {
                    tagSet.Add(tag);
                }
            }

            var tagTable = tagSet.ToList();
            tagTable.Sort(StringComparer.Ordinal); //注意：排序增加序列化稳定性

            if (tagTable.Count > ushort.MaxValue)
                throw new YooInternalException($"Tag count exceeds the maximum value of {ushort.MaxValue}.");

            _tagTable = tagTable.ToArray();
            _tagToIndex = new Dictionary<string, int>(_tagTable.Length);
            for (int index = 0; index < _tagTable.Length; index++)
            {
                _tagToIndex.Add(_tagTable[index], index);
            }
        }

        /// <summary>
        /// 将标签字符串数组转换为全局标签表的索引数组
        /// </summary>
        public ushort[] ConvertTagsToIndices(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return Array.Empty<ushort>();

            ushort[] indices = new ushort[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                indices[i] = (ushort)_tagToIndex[tags[i]];
            }
            return indices;
        }

        /// <summary>
        /// 构建去重后的全局目录表，并填充目录到索引的映射
        /// </summary>
        public void BuildDirectoryTable()
        {
            if (_directoryTableBuilt)
                throw new YooInternalException("BuildDirectoryTable has already been called.");
            _directoryTableBuilt = true;

            HashSet<string> dirSet = new HashSet<string>();
            foreach (var buildAsset in AssetList)
            {
                SplitAssetPath(buildAsset.AssetPath, out string directory, out string fileName);
                dirSet.Add(directory);
            }

            var dirTable = dirSet.ToList();
            dirTable.Sort(StringComparer.Ordinal); //注意：排序增加序列化稳定性

            if (dirTable.Count > ushort.MaxValue)
                throw new YooInternalException($"Directory count exceeds the maximum value of {ushort.MaxValue}.");

            _directoryTable = dirTable.ToArray();
            _directoryToIndex = new Dictionary<string, int>(_directoryTable.Length);
            for (int index = 0; index < _directoryTable.Length; index++)
            {
                _directoryToIndex.Add(_directoryTable[index], index);
            }
        }

        /// <summary>
        /// 获取资源父目录在全局目录表中的索引
        /// </summary>
        public ushort ConvertDirectoryToIndex(string directory)
        {
            return (ushort)_directoryToIndex[directory];
        }

        /// <summary>
        /// 拆分资源路径为「父目录」与「文件名」
        /// </summary>
        public static void SplitAssetPath(string assetPath, out string directory, out string fileName)
        {
            int slash = assetPath.LastIndexOf('/');
            if (slash < 0)
            {
                directory = string.Empty;
                fileName = assetPath;
            }
            else
            {
                directory = assetPath.Substring(0, slash);
                fileName = assetPath.Substring(slash + 1);
            }
        }
    }
}
