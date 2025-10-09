using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清单文件
    /// </summary>
    [Serializable]
    internal class PackageManifest
    {
        /// <summary>
        /// 文件版本
        /// </summary>
        public string FileVersion;

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
        public bool IncludeAssetGUID;

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
        /// 资源列表（主动收集的资源列表）
        /// </summary>
        public List<PackageAsset> AssetList = new List<PackageAsset>();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<PackageBundle> BundleList = new List<PackageBundle>();

        /// <summary>
        /// 资源映射集合（提供AssetPath获取PackageAsset）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PackageAsset> AssetDic;

        /// <summary>
        /// 资源路径映射集合（提供Location获取AssetPath）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, string> AssetPathMapping1;

        /// <summary>
        /// 资源路径映射集合（提供AssetGUID获取AssetPath）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, string> AssetPathMapping2;

        /// <summary>
        /// 资源包集合（提供BundleName获取PackageBundle）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PackageBundle> BundleDic1;

        /// <summary>
        /// 资源包集合（提供FileName获取PackageBundle）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PackageBundle> BundleDic2;

        /// <summary>
        /// 资源包集合（提供BundleGUID获取PackageBundle）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PackageBundle> BundleDic3;


        /// <summary>
        /// 初始化资源清单
        /// </summary>
        public void Initialize()
        {
            // 填充资源包内包含的主资源列表
            foreach (var packageAsset in AssetList)
            {
                int bundleID = packageAsset.BundleID;
                if (bundleID >= 0 && bundleID < BundleList.Count)
                {
                    var packageBundle = BundleList[bundleID];
                    packageBundle.IncludeMainAssets.Add(packageAsset);
                }
                else
                {
                    throw new Exception($"Invalid bundle id : {bundleID} Asset path : {packageAsset.AssetPath}");
                }
            }

            // 填充资源包引用关系
            for (int index = 0; index < BundleList.Count; index++)
            {
                var sourceBundle = BundleList[index];
                foreach (int dependIndex in sourceBundle.DependBundleIDs)
                {
                    var dependBundle = BundleList[dependIndex];
                    dependBundle.AddReferenceBundleID(index);
                }
            }
        }

        /// <summary>
        /// 获取包裹的详细信息
        /// </summary>
        public PackageDetails GetPackageDetails()
        {
            PackageDetails details = new PackageDetails();
            details.FileVersion = FileVersion;
            details.EnableAddressable = EnableAddressable;
            details.SupportExtensionless = SupportExtensionless;
            details.LocationToLower = LocationToLower;
            details.IncludeAssetGUID = IncludeAssetGUID;
            details.ReplaceAssetPathWithAddress = ReplaceAssetPathWithAddress;
            details.OutputNameStyle = OutputNameStyle;
            details.BuildBundleType = BuildBundleType;
            details.BuildPipeline = BuildPipeline;
            details.PackageName = PackageName;
            details.PackageVersion = PackageVersion;
            details.PackageNote = PackageNote;
            details.AssetTotalCount = AssetList.Count;
            details.BundleTotalCount = BundleList.Count;
            return details;
        }

        /// <summary>
        /// 尝试映射为资源路径
        /// </summary>
        public string TryMappingToAssetPath(string location)
        {
            if (string.IsNullOrEmpty(location))
                return string.Empty;

            if (AssetPathMapping1.TryGetValue(location, out string assetPath))
                return assetPath;
            else
                return string.Empty;
        }

        /// <summary>
        /// 获取主资源包
        /// 注意：传入的资源包ID一定合法有效！
        /// </summary>
        public PackageBundle GetMainPackageBundle(int bundleID)
        {
            if (bundleID >= 0 && bundleID < BundleList.Count)
            {
                var packageBundle = BundleList[bundleID];
                return packageBundle;
            }
            else
            {
                throw new Exception($"Invalid bundle id : {bundleID}");
            }
        }

        /// <summary>
        /// 获取主资源包
        /// 注意：传入的资源对象一定合法有效！
        /// </summary>
        public PackageBundle GetMainPackageBundle(PackageAsset packageAsset)
        {
            return GetMainPackageBundle(packageAsset.BundleID);
        }

        /// <summary>
        /// 获取资源对象的依赖列表（框架层查询结果）
        /// 注意：传入的资源对象一定合法有效！
        /// </summary>
        public List<PackageBundle> GetAssetAllDependencies(PackageAsset packageAsset)
        {
            List<PackageBundle> result = new List<PackageBundle>(packageAsset.DependBundleIDs.Length);
            foreach (var dependID in packageAsset.DependBundleIDs)
            {
                var dependBundle = GetMainPackageBundle(dependID);
                result.Add(dependBundle);
            }
            return result;
        }

        /// <summary>
        /// 获取资源包的依赖列表（引擎层查询结果）
        /// 注意：传入的资源包对象一定合法有效！
        /// </summary>
        public List<PackageBundle> GetBundleAllDependencies(PackageBundle packageBundle)
        {
            List<PackageBundle> result = new List<PackageBundle>(packageBundle.DependBundleIDs.Length);
            foreach (var dependID in packageBundle.DependBundleIDs)
            {
                var dependBundle = GetMainPackageBundle(dependID);
                result.Add(dependBundle);
            }
            return result;
        }

        /// <summary>
        /// 尝试获取包裹的资源
        /// </summary>
        public bool TryGetPackageAsset(string assetPath, out PackageAsset result)
        {
            return AssetDic.TryGetValue(assetPath, out result);
        }

        /// <summary>
        /// 尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByFileName(string fileName, out PackageBundle result)
        {
            return BundleDic2.TryGetValue(fileName, out result);
        }

        /// <summary>
        /// 尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByBundleName(string bundleName, out PackageBundle result)
        {
            return BundleDic1.TryGetValue(bundleName, out result);
        }

        /// <summary>
        /// 尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByBundleGUID(string bundleGUID, out PackageBundle result)
        {
            return BundleDic3.TryGetValue(bundleGUID, out result);
        }

        /// <summary>
        /// 是否包含资源文件
        /// </summary>
        public bool IsIncludeBundleFile(string bundleGUID)
        {
            return BundleDic3.ContainsKey(bundleGUID);
        }

        /// <summary>
        /// 获取所有的资源信息
        /// </summary>
        public AssetInfo[] GetAllAssetInfos()
        {
            AssetInfo[] result = new AssetInfo[AssetList.Count];
            for (int i = 0; i < AssetList.Count; i++)
            {
                var packageAsset = AssetList[i];
                AssetInfo assetInfo = new AssetInfo(PackageName, packageAsset, null);
                result[i] = assetInfo;
            }
            return result;
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        public AssetInfo[] GetAssetInfosByTags(string[] tags)
        {
            List<AssetInfo> result = new List<AssetInfo>(AssetList.Count);
            foreach (var packageAsset in AssetList)
            {
                if (packageAsset.HasTag(tags))
                {
                    AssetInfo assetInfo = new AssetInfo(PackageName, packageAsset, null);
                    result.Add(assetInfo);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 资源定位地址转换为资源信息。
        /// </summary>
        /// <returns>如果转换失败会返回一个无效的资源信息类</returns>
        public AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
        {
            DebugCheckLocation(location);

            string assetPath = ConvertLocationToAssetInfoMapping(location);
            if (TryGetPackageAsset(assetPath, out PackageAsset packageAsset))
            {
                AssetInfo assetInfo = new AssetInfo(PackageName, packageAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error;
                if (string.IsNullOrEmpty(location))
                    error = $"The location is null or empty !";
                else
                    error = $"The location is invalid : {location}";
                AssetInfo assetInfo = new AssetInfo(PackageName, error);
                return assetInfo;
            }
        }
        private string ConvertLocationToAssetInfoMapping(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                YooLogger.Error("Failed to mapping location to asset path, The location is null or empty.");
                return string.Empty;
            }

            if (AssetPathMapping1.TryGetValue(location, out string assetPath))
            {
                return assetPath;
            }
            else
            {
                YooLogger.Warning($"Failed to mapping location to asset path : {location}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 资源GUID转换为资源信息。
        /// </summary>
        /// <returns>如果转换失败会返回一个无效的资源信息类</returns>
        public AssetInfo ConvertAssetGUIDToAssetInfo(string assetGUID, System.Type assetType)
        {
            if (IncludeAssetGUID == false)
            {
                YooLogger.Warning("Package manifest not include asset guid ! Please check asset bundle collector settings.");
                AssetInfo assetInfo = new AssetInfo(PackageName, "AssetGUID data is empty !");
                return assetInfo;
            }

            string assetPath = ConvertAssetGUIDToAssetInfoMapping(assetGUID);
            if (TryGetPackageAsset(assetPath, out PackageAsset packageAsset))
            {
                AssetInfo assetInfo = new AssetInfo(PackageName, packageAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error;
                if (string.IsNullOrEmpty(assetGUID))
                    error = $"The assetGUID is null or empty !";
                else
                    error = $"The assetGUID is invalid : {assetGUID}";
                AssetInfo assetInfo = new AssetInfo(PackageName, error);
                return assetInfo;
            }
        }
        private string ConvertAssetGUIDToAssetInfoMapping(string assetGUID)
        {
            if (string.IsNullOrEmpty(assetGUID))
            {
                YooLogger.Error("Failed to mapping assetGUID to asset path, The assetGUID is null or empty.");
                return string.Empty;
            }

            if (AssetPathMapping2.TryGetValue(assetGUID, out string assetPath))
            {
                return assetPath;
            }
            else
            {
                YooLogger.Warning($"Failed to mapping assetGUID to asset path : {assetGUID}");
                return string.Empty;
            }
        }

        #region 调试方法
        [Conditional("DEBUG")]
        private void DebugCheckLocation(string location)
        {
            if (string.IsNullOrEmpty(location) == false)
            {
                // 检查路径末尾是否有空格
                int index = location.LastIndexOf(' ');
                if (index != -1)
                {
                    if (location.Length == index + 1)
                        YooLogger.Warning($"Found blank character in location : \"{location}\"");
                }

                if (location.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    YooLogger.Warning($"Found illegal character in location : \"{location}\"");
            }
        }
        #endregion
    }
}