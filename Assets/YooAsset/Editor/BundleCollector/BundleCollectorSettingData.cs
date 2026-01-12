using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器配置数据管理类
    /// </summary>
    public class BundleCollectorSettingData
    {
        private static readonly Dictionary<string, System.Type> s_cacheGroupActiveRuleTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, IGroupActiveRule> s_cacheGorupActiveRuleInstance = new Dictionary<string, IGroupActiveRule>();

        private static readonly Dictionary<string, System.Type> s_cacheAddressRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IAddressRule> s_cacheAddressRuleInstance = new Dictionary<string, IAddressRule>();

        private static readonly Dictionary<string, System.Type> s_cacheBundlePackRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IBundlePackRule> s_cacheBundlePackRuleInstance = new Dictionary<string, IBundlePackRule>();

        private static readonly Dictionary<string, System.Type> s_cacheAssetFilterRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IAssetFilterRule> s_cacheAssetFilterRuleInstance = new Dictionary<string, IAssetFilterRule>();

        private static readonly Dictionary<string, System.Type> s_cacheAssetIgnoreRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IAssetIgnoreRule> s_cacheAssetIgnoreRuleInstance = new Dictionary<string, IAssetIgnoreRule>();

        private static BundleCollectorSetting s_setting = null;

        /// <summary>
        /// 配置数据是否被修改
        /// </summary>
        public static bool IsDirty { private set; get; } = false;


        static BundleCollectorSettingData()
        {
            // IAddressRule
            {
                // 清空缓存集合
                s_cacheAddressRuleTypes.Clear();
                s_cacheAddressRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(AddressByFileName),
                    typeof(AddressByFolderAndFileName),
                    typeof(AddressByGroupAndFileName),
                    typeof(AddressDisable)
                };

                var customTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IAddressRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (s_cacheAddressRuleTypes.ContainsKey(type.Name) == false)
                        s_cacheAddressRuleTypes.Add(type.Name, type);
                }
            }

            // IBundlePackRule
            {
                // 清空缓存集合
                s_cacheBundlePackRuleTypes.Clear();
                s_cacheBundlePackRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(PackSeparately),
                    typeof(PackDirectory),
                    typeof(PackTopDirectory),
                    typeof(PackCollector),
                    typeof(PackGroup),
                    typeof(PackRawFile),
                    typeof(PackShaderVariants)
                };

                var customTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IBundlePackRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (s_cacheBundlePackRuleTypes.ContainsKey(type.Name) == false)
                        s_cacheBundlePackRuleTypes.Add(type.Name, type);
                }
            }

            // IAssetFilterRule
            {
                // 清空缓存集合
                s_cacheAssetFilterRuleTypes.Clear();
                s_cacheAssetFilterRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(CollectAll),
                    typeof(CollectScene),
                    typeof(CollectPrefab),
                    typeof(CollectSprite)
                };

                var customTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IAssetFilterRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (s_cacheAssetFilterRuleTypes.ContainsKey(type.Name) == false)
                        s_cacheAssetFilterRuleTypes.Add(type.Name, type);
                }
            }

            // IAssetIgnoreRule
            {
                // 清空缓存集合
                s_cacheAssetIgnoreRuleTypes.Clear();
                s_cacheAssetIgnoreRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(NormalIgnoreRule),
                    typeof(RawFileIgnoreRule),
                };

                var customTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IAssetIgnoreRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (s_cacheAssetIgnoreRuleTypes.ContainsKey(type.Name) == false)
                        s_cacheAssetIgnoreRuleTypes.Add(type.Name, type);
                }
            }

            // IGroupActiveRule
            {
                // 清空缓存集合
                s_cacheGroupActiveRuleTypes.Clear();
                s_cacheGorupActiveRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(EnableGroup),
                    typeof(DisableGroup),
                };

                var customTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IGroupActiveRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (s_cacheGroupActiveRuleTypes.ContainsKey(type.Name) == false)
                        s_cacheGroupActiveRuleTypes.Add(type.Name, type);
                }
            }
        }

        /// <summary>
        /// 配置文件实例
        /// </summary>
        public static BundleCollectorSetting Setting
        {
            get
            {
                if (s_setting == null)
                    s_setting = SettingLoader.LoadSettingData<BundleCollectorSetting>();
                return s_setting;
            }
        }

        /// <summary>
        /// 存储配置文件
        /// </summary>
        public static void SaveFile()
        {
            if (Setting != null)
            {
                IsDirty = false;
                EditorUtility.SetDirty(Setting);
                AssetDatabase.SaveAssets();
                Debug.Log($"{nameof(BundleCollectorSetting)}.asset is saved.");
            }
        }

        /// <summary>
        /// 修复配置文件
        /// </summary>
        public static void FixFile()
        {
            bool isFixed = Setting.FixAllPackageConfigError();
            if (isFixed)
            {
                IsDirty = true;
                Debug.Log("Package config errors have been fixed.");
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public static void ClearAll()
        {
            Setting.ClearAll();
        }

        private static string GetRuleDisplayName(string name, Type type)
        {
            var attribute = EditorAssemblyUtility.GetAttribute<DisplayNameAttribute>(type);
            if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
                return attribute.DisplayName;
            else
                return name;
        }

        /// <summary>
        /// 获取所有激活规则的显示名称列表
        /// </summary>
        /// <returns>激活规则的显示名称列表</returns>
        public static List<RuleDisplayName> GetGroupActiveRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in s_cacheGroupActiveRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        /// <summary>
        /// 获取所有寻址规则的显示名称列表
        /// </summary>
        /// <returns>寻址规则的显示名称列表</returns>
        public static List<RuleDisplayName> GetAddressRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in s_cacheAddressRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        /// <summary>
        /// 获取所有打包规则的显示名称列表
        /// </summary>
        /// <returns>打包规则的显示名称列表</returns>
        public static List<RuleDisplayName> GetBundlePackRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in s_cacheBundlePackRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        /// <summary>
        /// 获取所有过滤规则的显示名称列表
        /// </summary>
        /// <returns>过滤规则的显示名称列表</returns>
        public static List<RuleDisplayName> GetAssetFilterRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in s_cacheAssetFilterRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        /// <summary>
        /// 获取所有忽略规则的显示名称列表
        /// </summary>
        /// <returns>忽略规则的显示名称列表</returns>
        public static List<RuleDisplayName> GetAssetIgnoreRuleNames()
        {
            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in s_cacheAssetIgnoreRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        /// <summary>
        /// 检查是否存在指定的激活规则
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>存在则返回 true，否则返回 false</returns>
        public static bool HasGroupActiveRuleName(string ruleName)
        {
            return s_cacheGroupActiveRuleTypes.ContainsKey(ruleName);
        }

        /// <summary>
        /// 检查是否存在指定的寻址规则
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>存在则返回 true，否则返回 false</returns>
        public static bool HasAddressRuleName(string ruleName)
        {
            return s_cacheAddressRuleTypes.ContainsKey(ruleName);
        }

        /// <summary>
        /// 检查是否存在指定的打包规则
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>存在则返回 true，否则返回 false</returns>
        public static bool HasBundlePackRuleName(string ruleName)
        {
            return s_cacheBundlePackRuleTypes.ContainsKey(ruleName);
        }

        /// <summary>
        /// 检查是否存在指定的过滤规则
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>存在则返回 true，否则返回 false</returns>
        public static bool HasAssetFilterRuleName(string ruleName)
        {
            return s_cacheAssetFilterRuleTypes.ContainsKey(ruleName);
        }

        /// <summary>
        /// 检查是否存在指定的忽略规则
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>存在则返回 true，否则返回 false</returns>
        public static bool HasAssetIgnoreRuleName(string ruleName)
        {
            return s_cacheAssetIgnoreRuleTypes.ContainsKey(ruleName);
        }

        /// <summary>
        /// 获取激活规则实例
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>激活规则实例</returns>
        public static IGroupActiveRule GetGroupActiveRuleInstance(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            if (s_cacheGorupActiveRuleInstance.TryGetValue(ruleName, out IGroupActiveRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (s_cacheGroupActiveRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IGroupActiveRule)Activator.CreateInstance(type);
                s_cacheGorupActiveRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new ArgumentException($"{nameof(IGroupActiveRule)} is invalid: '{ruleName}'.", nameof(ruleName));
            }
        }

        /// <summary>
        /// 获取寻址规则实例
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>寻址规则实例</returns>
        public static IAddressRule GetAddressRuleInstance(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            if (s_cacheAddressRuleInstance.TryGetValue(ruleName, out IAddressRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (s_cacheAddressRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IAddressRule)Activator.CreateInstance(type);
                s_cacheAddressRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new ArgumentException($"{nameof(IAddressRule)} is invalid: '{ruleName}'.", nameof(ruleName));
            }
        }

        /// <summary>
        /// 获取打包规则实例
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>打包规则实例</returns>
        public static IBundlePackRule GetBundlePackRuleInstance(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            if (s_cacheBundlePackRuleInstance.TryGetValue(ruleName, out IBundlePackRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (s_cacheBundlePackRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IBundlePackRule)Activator.CreateInstance(type);
                s_cacheBundlePackRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new ArgumentException($"{nameof(IBundlePackRule)} is invalid: '{ruleName}'.", nameof(ruleName));
            }
        }

        /// <summary>
        /// 获取过滤规则实例
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>过滤规则实例</returns>
        public static IAssetFilterRule GetAssetFilterRuleInstance(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            if (s_cacheAssetFilterRuleInstance.TryGetValue(ruleName, out IAssetFilterRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (s_cacheAssetFilterRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IAssetFilterRule)Activator.CreateInstance(type);
                s_cacheAssetFilterRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new ArgumentException($"{nameof(IAssetFilterRule)} is invalid: '{ruleName}'.", nameof(ruleName));
            }
        }

        /// <summary>
        /// 获取忽略规则实例
        /// </summary>
        /// <param name="ruleName">规则类名</param>
        /// <returns>忽略规则实例</returns>
        public static IAssetIgnoreRule GetAssetIgnoreRuleInstance(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
                throw new ArgumentNullException(nameof(ruleName));

            if (s_cacheAssetIgnoreRuleInstance.TryGetValue(ruleName, out IAssetIgnoreRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (s_cacheAssetIgnoreRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IAssetIgnoreRule)Activator.CreateInstance(type);
                s_cacheAssetIgnoreRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new ArgumentException($"{nameof(IAssetIgnoreRule)} is invalid: '{ruleName}'.", nameof(ruleName));
            }
        }

        #region 公共参数编辑相关
        /// <summary>
        /// 修改是否显示资源包裹视图
        /// </summary>
        /// <param name="showPackageView">是否显示资源包裹视图</param>
        public static void ModifyShowPackageView(bool showPackageView)
        {
            Setting.ShowPackageView = showPackageView;
            IsDirty = true;
        }

        /// <summary>
        /// 修改是否显示编辑器别名
        /// </summary>
        /// <param name="showAlias">是否显示编辑器别名</param>
        public static void ModifyShowEditorAlias(bool showAlias)
        {
            Setting.ShowEditorAlias = showAlias;
            IsDirty = true;
        }

        /// <summary>
        /// 修改是否启用资源包唯一命名
        /// </summary>
        /// <param name="uniqueBundleName">是否启用资源包唯一命名</param>
        public static void ModifyUniqueBundleName(bool uniqueBundleName)
        {
            Setting.UniqueBundleName = uniqueBundleName;
            IsDirty = true;
        }
        #endregion

        #region 资源包裹编辑相关
        /// <summary>
        /// 创建资源包裹
        /// </summary>
        /// <param name="packageName">资源包裹名称</param>
        /// <returns>新创建的资源包裹</returns>
        public static BundleCollectorPackage CreatePackage(string packageName)
        {
            BundleCollectorPackage package = new BundleCollectorPackage();
            package.PackageName = packageName;
            Setting.Packages.Add(package);
            IsDirty = true;
            return package;
        }

        /// <summary>
        /// 移除资源包裹
        /// </summary>
        /// <param name="package">要移除的资源包裹</param>
        public static void RemovePackage(BundleCollectorPackage package)
        {
            if (Setting.Packages.Remove(package))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed to remove package: '{package.PackageName}'.");
            }
        }

        /// <summary>
        /// 标记资源包裹已修改
        /// </summary>
        /// <param name="package">已修改的资源包裹</param>
        public static void ModifyPackage(BundleCollectorPackage package)
        {
            if (package != null)
            {
                IsDirty = true;
            }
        }
        #endregion

        #region 资源分组编辑相关
        /// <summary>
        /// 创建资源分组
        /// </summary>
        /// <param name="package">所属资源包裹</param>
        /// <param name="groupName">资源分组名称</param>
        /// <returns>新创建的资源分组</returns>
        public static BundleCollectorGroup CreateGroup(BundleCollectorPackage package, string groupName)
        {
            BundleCollectorGroup group = new BundleCollectorGroup();
            group.GroupName = groupName;
            package.Groups.Add(group);
            IsDirty = true;
            return group;
        }

        /// <summary>
        /// 移除资源分组
        /// </summary>
        /// <param name="package">所属资源包裹</param>
        /// <param name="group">要移除的资源分组</param>
        public static void RemoveGroup(BundleCollectorPackage package, BundleCollectorGroup group)
        {
            if (package.Groups.Remove(group))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed to remove group: '{group.GroupName}'.");
            }
        }

        /// <summary>
        /// 标记资源分组已修改
        /// </summary>
        /// <param name="package">所属资源包裹</param>
        /// <param name="group">已修改的资源分组</param>
        public static void ModifyGroup(BundleCollectorPackage package, BundleCollectorGroup group)
        {
            if (package != null && group != null)
            {
                IsDirty = true;
            }
        }
        #endregion

        #region 资源收集器编辑相关
        /// <summary>
        /// 创建资源收集器条目
        /// </summary>
        /// <param name="group">所属资源分组</param>
        /// <param name="collector">资源收集器配置</param>
        public static void CreateCollector(BundleCollectorGroup group, BundleCollector collector)
        {
            group.Collectors.Add(collector);
            IsDirty = true;
        }

        /// <summary>
        /// 移除资源收集器条目
        /// </summary>
        /// <param name="group">所属资源分组</param>
        /// <param name="collector">要移除的资源收集器</param>
        public static void RemoveCollector(BundleCollectorGroup group, BundleCollector collector)
        {
            if (group.Collectors.Remove(collector))
            {
                IsDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed to remove collector: '{collector.CollectPath}'.");
            }
        }

        /// <summary>
        /// 标记资源收集器已修改
        /// </summary>
        /// <param name="group">所属资源分组</param>
        /// <param name="collector">已修改的资源收集器</param>
        public static void ModifyCollector(BundleCollectorGroup group, BundleCollector collector)
        {
            if (group != null && collector != null)
            {
                IsDirty = true;
            }
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        /// <param name="packageName">资源包裹名称</param>
        /// <returns>以分号分隔的资源标签字符串</returns>
        public static string GetPackageAllTags(string packageName)
        {
            var allTags = Setting.GetPackageAllTags(packageName);
            return string.Join(";", allTags);
        }
        #endregion
    }
}