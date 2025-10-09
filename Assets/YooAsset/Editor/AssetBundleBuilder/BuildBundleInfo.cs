using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
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
        /// 文件哈希值
        /// </summary>
        public uint PackageFileCRC { set; get; }

        /// <summary>
        /// 文件哈希值
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

        private readonly Dictionary<string, BuildAssetInfo> _packAssetDic = new Dictionary<string, BuildAssetInfo>(100);

        /// <summary>
        /// 参与构建的资源列表
        /// 注意：不包含零依赖资源和冗余资源
        /// </summary>
        public readonly List<BuildAssetInfo> AllPackAssets = new List<BuildAssetInfo>(100);

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 加密文件
        /// </summary>
        public bool Encrypted { set; get; }


        public BuildBundleInfo(string bundleName)
        {
            BundleName = bundleName;
        }

        /// <summary>
        /// 添加一个打包资源
        /// </summary>
        public void PackAsset(BuildAssetInfo buildAsset)
        {
            string assetPath = buildAsset.AssetInfo.AssetPath;
            if (_packAssetDic.ContainsKey(assetPath))
                throw new System.Exception($"Should never get here ! Asset is existed : {assetPath}");

            _packAssetDic.Add(assetPath, buildAsset);
            AllPackAssets.Add(buildAsset);
        }

        /// <summary>
        /// 是否包含指定资源
        /// </summary>
        public bool IsContainsPackAsset(string assetPath)
        {
            return _packAssetDic.ContainsKey(assetPath);
        }

        /// <summary>
        /// 获取构建的资源路径列表
        /// </summary>
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
        public BuildAssetInfo GetPackAssetInfo(string assetPath)
        {
            if (_packAssetDic.TryGetValue(assetPath, out BuildAssetInfo value))
            {
                return value;
            }
            else
            {
                throw new Exception($"Can not found pack asset info {assetPath} in bundle : {BundleName}");
            }
        }

        /// <summary>
        /// 获取资源包内部所有资产
        /// </summary>
        public List<AssetInfo> GetBundleContents()
        {
            Dictionary<string, AssetInfo> result = new Dictionary<string, AssetInfo>(AllPackAssets.Count);
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
        /// 创建AssetBundleBuild类
        /// </summary>
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
            packageBundle.UnityCRC = PackageUnityCRC;
            packageBundle.FileHash = PackageFileHash;
            packageBundle.FileCRC = PackageFileCRC;
            packageBundle.FileSize = PackageFileSize;
            packageBundle.Encrypted = Encrypted;
            return packageBundle;
        }
    }
}