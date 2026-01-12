using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器包裹
    /// </summary>
    [Serializable]
    public class BundleCollectorPackage
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName = string.Empty;

        /// <summary>
        /// 包裹描述
        /// </summary>
        public string PackageDesc = string.Empty;

        /// <summary>
        /// 是否启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable = false;

        /// <summary>
        /// 是否支持无后缀名的资源定位地址
        /// </summary>
        public bool SupportExtensionless = true;

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower = false;

        /// <summary>
        /// 是否包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID = false;

        /// <summary>
        /// 是否自动收集所有着色器
        /// </summary>
        /// <remarks>所有着色器存储在一个资源包内</remarks>
        public bool AutoCollectShaders = true;

        /// <summary>
        /// 资源忽略规则名
        /// </summary>
        public string IgnoreRuleName = nameof(NormalIgnoreRule);

        /// <summary>
        /// 分组列表
        /// </summary>
        public List<BundleCollectorGroup> Groups = new List<BundleCollectorGroup>();


        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            if (string.IsNullOrEmpty(IgnoreRuleName))
            {
                throw new InvalidOperationException($"{nameof(IgnoreRuleName)} is null or empty.");
            }
            else
            {
                if (BundleCollectorSettingData.HasAssetIgnoreRuleName(IgnoreRuleName) == false)
                    throw new InvalidOperationException($"Invalid {nameof(IAssetIgnoreRule)} class type: '{IgnoreRuleName}' in package: '{PackageName}'.");
            }

            foreach (var group in Groups)
            {
                group.CheckConfigError();
            }
        }

        /// <summary>
        /// 修复配置错误
        /// </summary>
        /// <returns>如果修复了配置错误返回 true</returns>
        public bool FixConfigError()
        {
            bool isFixed = false;

            if (string.IsNullOrEmpty(IgnoreRuleName))
            {
                Debug.LogWarning($"{nameof(IgnoreRuleName)} has been set to {nameof(NormalIgnoreRule)}.");
                IgnoreRuleName = nameof(NormalIgnoreRule);
                isFixed = true;
            }

            foreach (var group in Groups)
            {
                if (group.FixConfigError())
                {
                    isFixed = true;
                }
            }

            return isFixed;
        }

        /// <summary>
        /// 获取收集的资源列表
        /// </summary>
        /// <param name="command">收集命令</param>
        /// <returns>收集到的资源信息列表</returns>
        public List<CollectAssetInfo> GetCollectAssets(CollectCommand command)
        {
            Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);

            // 收集打包资源
            foreach (var group in Groups)
            {
                var groupAssets = group.GetAllCollectAssets(command);
                foreach (var collectAsset in groupAssets)
                {
                    if (result.ContainsKey(collectAsset.AssetInfo.AssetPath) == false)
                        result.Add(collectAsset.AssetInfo.AssetPath, collectAsset);
                    else
                        throw new InvalidOperationException($"Collecting asset file already exists: '{collectAsset.AssetInfo.AssetPath}'.");
                }
            }

            // 检测可寻址地址是否重复
            if (command.EnableAddressable)
            {
                var addressLookup = new Dictionary<string, string>();
                foreach (var collectInfoPair in result)
                {
                    if (collectInfoPair.Value.CollectorType == ECollectorType.MainAssetCollector)
                    {
                        string address = collectInfoPair.Value.Address;
                        string assetPath = collectInfoPair.Value.AssetInfo.AssetPath;
                        if (string.IsNullOrEmpty(address))
                            continue;

                        if (address.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"Address cannot be an asset path in package: '{PackageName}' \nAssetPath: '{assetPath}'.");

                        if (addressLookup.TryGetValue(address, out var existed) == false)
                            addressLookup.Add(address, assetPath);
                        else
                            throw new InvalidOperationException($"Address already exists: '{address}' \nAssetPath:\n     '{existed}'\n     '{assetPath}'.");
                    }
                }
            }

            // 返回结果
            return result.Values.ToList();
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        /// <returns>所有资源标签列表</returns>
        public List<string> GetAllTags()
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var group in Groups)
            {
                var groupTags = EditorStringUtility.SplitToList(group.AssetTags, ';');
                foreach (var tag in groupTags)
                {
                    if (result.Contains(tag) == false)
                        result.Add(tag);
                }

                foreach (var collector in group.Collectors)
                {
                    var collectorTags = EditorStringUtility.SplitToList(collector.AssetTags, ';');
                    foreach (var tag in collectorTags)
                    {
                        if (result.Contains(tag) == false)
                            result.Add(tag);
                    }
                }
            }
            return result.ToList();
        }
    }
}