using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器分组
    /// </summary>
    [Serializable]
    public class BundleCollectorGroup
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDesc = string.Empty;

        /// <summary>
        /// 资源分类标签
        /// </summary>
        public string AssetTags = string.Empty;

        /// <summary>
        /// 分组激活规则
        /// </summary>
        public string ActiveRuleName = nameof(EnableGroup);

        /// <summary>
        /// 分组的收集器列表
        /// </summary>
        public List<BundleCollector> Collectors = new List<BundleCollector>();


        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            if (BundleCollectorSettingData.HasGroupActiveRuleName(ActiveRuleName) == false)
                throw new InvalidOperationException($"Invalid {nameof(IGroupActiveRule)} class type: '{ActiveRuleName}' in group: '{GroupName}'.");

            // 检测分组是否激活
            IGroupActiveRule activeRule = BundleCollectorSettingData.GetGroupActiveRuleInstance(ActiveRuleName);
            if (activeRule.IsActiveGroup(new GroupActiveRuleData(GroupName)) == false)
                return;

            foreach (var collector in Collectors)
            {
                collector.CheckConfigError();
            }
        }

        /// <summary>
        /// 修复配置错误
        /// </summary>
        /// <returns>如果修复了配置错误返回 true</returns>
        public bool FixConfigError()
        {
            bool isFixed = false;
            foreach (var collector in Collectors)
            {
                if (collector.FixConfigError())
                {
                    isFixed = true;
                }
            }
            return isFixed;
        }

        /// <summary>
        /// 获取打包收集的资源文件
        /// </summary>
        /// <param name="command">收集命令</param>
        /// <returns>收集到的资源信息列表</returns>
        public List<CollectAssetInfo> GetAllCollectAssets(CollectCommand command)
        {
            Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);

            // 检测分组是否激活
            IGroupActiveRule activeRule = BundleCollectorSettingData.GetGroupActiveRuleInstance(ActiveRuleName);
            if (activeRule.IsActiveGroup(new GroupActiveRuleData(GroupName)) == false)
            {
                return new List<CollectAssetInfo>();
            }

            // 收集打包资源
            foreach (var collector in Collectors)
            {
                var collectorAssets = collector.GetAllCollectAssets(command, this);
                foreach (var collectAsset in collectorAssets)
                {
                    if (result.ContainsKey(collectAsset.AssetInfo.AssetPath) == false)
                        result.Add(collectAsset.AssetInfo.AssetPath, collectAsset);
                    else
                        throw new InvalidOperationException($"Collecting asset file already exists: '{collectAsset.AssetInfo.AssetPath}' in group: '{GroupName}'.");
                }
            }

            // 检测资源路径合法性
            if (command.EnableAssetPathValidation)
            {
                var assetPathLookup = new HashSet<string>(StringComparer.Ordinal);
                foreach (var collectInfoPair in result)
                {
                    CollectAssetInfo collectAssetInfo = collectInfoPair.Value;
                    assetPathLookup.Add(collectAssetInfo.AssetInfo.AssetPath);
                    foreach (var dependAssetInfo in collectAssetInfo.DependAssets)
                    {
                        assetPathLookup.Add(dependAssetInfo.AssetPath);
                    }
                }

                bool hasInvalidAssetPath = false;
                foreach (string assetPath in assetPathLookup)
                {
                    if (CheckAssetPath(assetPath) == false)
                        hasInvalidAssetPath = true;
                }
                if (hasInvalidAssetPath)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.AssetPathContainsFormatCharacter,
                        "Asset paths contain Unicode format characters. Check the error logs in the Unity Console and rename the affected assets or containing folders before building.");
                    throw new InvalidOperationException(message);
                }
            }

            // 检测可寻址地址是否重复
            if (command.EnableAddressable)
            {
                var addressLookup = new Dictionary<string, string>();
                foreach (var collectAssetPair in result)
                {
                    if (collectAssetPair.Value.CollectorType == ECollectorType.MainAssetCollector)
                    {
                        string address = collectAssetPair.Value.Address;
                        string assetPath = collectAssetPair.Value.AssetInfo.AssetPath;
                        if (string.IsNullOrEmpty(address))
                            continue;

                        if (address.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"Address cannot be an asset path in group: '{GroupName}' \nAssetPath: '{assetPath}'.");

                        if (addressLookup.TryGetValue(address, out var existed) == false)
                            addressLookup.Add(address, assetPath);
                        else
                            throw new InvalidOperationException($"Address already exists: '{address}' in group: '{GroupName}' \nAssetPath:\n     '{existed}'\n     '{assetPath}'.");
                    }
                }
            }

            // 返回列表
            return result.Values.ToList();
        }

        private bool CheckAssetPath(string assetPath)
        {
            string characterEscape = EditorStringUtility.GetFormatCharacterEscape(assetPath);
            if (characterEscape == null)
                return true;

            BuildLogger.Error($"Asset path contains a Unicode format character ({characterEscape}): '{assetPath}'. Rename the asset or containing folder to remove format characters before building.");
            return false;
        }
    }
}
