using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建资源包信息，记录单个资源包在构建过程中的完整元数据
    /// </summary>
    public class BuildBundleInfo
    {
        #region 补丁文件的关键信息
        /// <summary>
        /// Unity引擎生成的哈希值（构建内容的哈希值）
        /// </summary>
        public string PackageUnityHash { set; get; }

        /// <summary>
        /// Unity引擎生成的CRC
        /// </summary>
        public uint PackageUnityCRC { set; get; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string PackageFileHash { set; get; }

        /// <summary>
        /// 文件 CRC 校验值
        /// </summary>
        public uint PackageFileCRC { set; get; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long PackageFileSize { set; get; }

        /// <summary>
        /// 构建输出的文件路径
        /// </summary>
        public string BuildOutputFilePath { set; get; }

        /// <summary>
        /// 补丁包的源文件路径
        /// </summary>
        public string PackageSourceFilePath { set; get; }

        /// <summary>
        /// 补丁包的目标文件路径
        /// </summary>
        public string PackageDestFilePath { set; get; }

        /// <summary>
        /// 加密生成文件的路径
        /// 注意：如果未加密该路径为空
        /// </summary>
        public string EncryptedFilePath { set; get; }
        #endregion

        private readonly Dictionary<string, BuildAssetInfo> _packAssetDictionary = new Dictionary<string, BuildAssetInfo>(100);
        private readonly List<BuildAssetInfo> _allPackAssets = new List<BuildAssetInfo>(100);

        /// <summary>
        /// 参与构建的资源列表
        /// 注意：不包含零依赖资源和冗余资源
        /// </summary>
        public IReadOnlyList<BuildAssetInfo> AllPackAssets
        {
            get { return _allPackAssets; }
        }

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 是否已加密
        /// </summary>
        public bool Encrypted { set; get; }


        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        public BuildBundleInfo(string bundleName)
        {
            BundleName = bundleName;
        }

        /// <summary>
        /// 添加一个打包资源
        /// </summary>
        /// <param name="buildAsset">待打包的资源信息</param>
        public void PackAsset(BuildAssetInfo buildAsset)
        {
            string assetPath = buildAsset.AssetInfo.AssetPath;
            if (_packAssetDictionary.ContainsKey(assetPath))
                throw new YooInternalException($"Should never get here. Asset already exists: '{assetPath}'.");

            _packAssetDictionary.Add(assetPath, buildAsset);
            _allPackAssets.Add(buildAsset);
        }

        /// <summary>
        /// 检查是否包含指定资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>包含返回 true，否则返回 false</returns>
        public bool IsContainsPackAsset(string assetPath)
        {
            return _packAssetDictionary.ContainsKey(assetPath);
        }

        /// <summary>
        /// 获取构建的资源路径列表
        /// </summary>
        /// <returns>所有打包资源的路径数组</returns>
        public string[] GetAllPackAssetPaths()
        {
            List<string> results = new List<string>(AllPackAssets.Count);
            for (int i = 0; i < AllPackAssets.Count; i++)
            {
                var packAsset = AllPackAssets[i];
                results.Add(packAsset.AssetInfo.AssetPath);
            }
            return results.ToArray();
        }

        /// <summary>
        /// 获取构建的资源可寻址列表
        /// </summary>
        /// <returns>所有打包资源的可寻址地址数组</returns>
        public string[] GetAllPackAssetAddress()
        {
            List<string> results = new List<string>(AllPackAssets.Count);
            for (int i = 0; i < AllPackAssets.Count; i++)
            {
                var packAsset = AllPackAssets[i];
                results.Add(packAsset.Address);
            }
            return results.ToArray();
        }

        /// <summary>
        /// 获取构建的主资源信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>对应的构建资源信息</returns>
        public BuildAssetInfo GetPackAssetInfo(string assetPath)
        {
            if (_packAssetDictionary.TryGetValue(assetPath, out BuildAssetInfo value))
            {
                return value;
            }
            else
            {
                throw new InvalidOperationException($"Could not find pack asset info '{assetPath}' in bundle: '{BundleName}'.");
            }
        }

        /// <summary>
        /// 获取资源包内部所有资产
        /// </summary>
        /// <returns>资源包包含的所有资源信息列表</returns>
        public List<EditorAssetInfo> GetBundleContents()
        {
            Dictionary<string, EditorAssetInfo> result = new Dictionary<string, EditorAssetInfo>(AllPackAssets.Count);
            foreach (var packAsset in AllPackAssets)
            {
                result.Add(packAsset.AssetInfo.AssetPath, packAsset.AssetInfo);
                if (packAsset.AllDependAssetInfos != null)
                {
                    foreach (var dependAssetInfo in packAsset.AllDependAssetInfos)
                    {
                        // 注意：依赖资源里只添加零依赖资源和冗余资源
                        if (dependAssetInfo.HasBundleName() == false)
                        {
                            string dependAssetPath = dependAssetInfo.AssetInfo.AssetPath;
                            if (result.ContainsKey(dependAssetPath) == false)
                                result.Add(dependAssetPath, dependAssetInfo.AssetInfo);
                        }
                    }
                }
            }
            return result.Values.ToList();
        }

        /// <summary>
        /// 创建 Unity 构建管线所需的数据
        /// </summary>
        /// <param name="replaceAssetPathWithAddress">是否使用可寻址地址替代资源路径</param>
        /// <returns>构建管线所需的 AssetBundleBuild 数据</returns>
        public UnityEditor.AssetBundleBuild CreatePipelineBuild(bool replaceAssetPathWithAddress)
        {
            // 注意：我们不再支持AssetBundle的变种机制
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = BundleName;
            build.assetBundleVariant = string.Empty;
            build.assetNames = GetAllPackAssetPaths();
            if (replaceAssetPathWithAddress)
                build.addressableNames = GetAllPackAssetAddress();
            return build;
        }

        /// <summary>
        /// 获取所有写入补丁清单的资源
        /// </summary>
        /// <returns>需要写入清单的主资源信息数组</returns>
        public BuildAssetInfo[] GetAllManifestAssetInfos()
        {
            return AllPackAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector).ToArray();
        }

        /// <summary>
        /// 创建PackageBundle类
        /// </summary>
        internal PackageBundle CreatePackageBundle()
        {
            PackageBundle packageBundle = new PackageBundle();
            packageBundle.BundleName = BundleName;
            packageBundle.UnityCrc = PackageUnityCRC;
            packageBundle.FileHash = PackageFileHash;
            packageBundle.FileCrc = PackageFileCRC;
            packageBundle.FileSize = PackageFileSize;
            packageBundle.IsEncrypted = Encrypted;
            packageBundle.DependentBundleIDs = Array.Empty<int>();
            return packageBundle;
        }
    }
}