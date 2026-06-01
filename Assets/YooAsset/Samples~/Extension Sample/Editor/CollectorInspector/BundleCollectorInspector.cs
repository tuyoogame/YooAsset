using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器的 Inspector 扩展
    /// </summary>
    [InitializeOnLoad]
    internal static class BundleCollectorInspector
    {
        private struct CollectorContext
        {
            public BundleCollectorPackage Package;
            public BundleCollectorGroup Group;
            public BundleCollector Collector;
        }

        private static readonly string[] CollectorTypeNames =
        {
            nameof(ECollectorType.MainAssetCollector),
            nameof(ECollectorType.StaticAssetCollector),
            nameof(ECollectorType.DependAssetCollector),
        };

        private static int _createPackageIndex;
        private static int _createGroupIndex;

        static BundleCollectorInspector()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        /// <summary>
        /// Inspector 默认头部绘制完成后的回调
        /// </summary>
        /// <param name="editor">当前正在绘制的 Inspector 编辑器实例</param>
        private static void OnPostHeaderGUI(UnityEditor.Editor editor)
        {
            // 注意：多目标选择的时候不绘制
            if (editor.targets != null && editor.targets.Length > 1)
                return;

            UnityEngine.Object target = editor.target;
            if (target == null)
                return;

            // 检测选择的路径是否合法
            string assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath))
                return;
            if (assetPath == "Assets")
                return;
            if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) == false)
                return;
            if (AssetDatabase.IsValidFolder(assetPath) == false)
                return;

            // 检测配置文件是否存在
            if (BundleCollectorSettingData.HasSettingAsset() == false)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("YooAsset", "BundleCollectorSetting.asset not found.");
                return;
            }

            // 根据当前文件夹是否已经配置，动态切换展示模式。
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(BundleCollectorGUIStyle.SectionStyle))
            {
                bool isFindCollector = TryGetCollector(assetPath, out CollectorContext collectorContext);
                if (isFindCollector)
                {
                    DrawOpenCollectorHeader(collectorContext);
                    DrawTargetCollectorContent(collectorContext);
                }
                else
                {
                    DrawCreateCollectorHeader(assetPath);
                    DrawCreateCollectorSelector();
                }
            }
        }
        private static void DrawCreateCollectorHeader(string assetPath)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("YooAsset", EditorStyles.boldLabel, GUILayout.Width(95));
                GUILayout.FlexibleSpace();

                bool hasCreateTarget = TryGetCreateCollectorTarget(out var package, out var group);
                using (new EditorGUI.DisabledScope(hasCreateTarget == false))
                {
                    if (GUILayout.Button("Create Collector", GUILayout.Width(120)))
                    {
                        TryAddCollector(assetPath, package, group, out _);
                    }
                }
            }
        }
        private static void DrawCreateCollectorSelector()
        {
            using (new EditorGUILayout.VerticalScope(BundleCollectorGUIStyle.SectionStyle))
            {
                BundleCollectorGUIDraw.DrawSectionTitle("Package & Group");

                var setting = BundleCollectorSettingData.Setting;
                if (setting == null || setting.Packages.Count == 0)
                {
                    EditorGUILayout.HelpBox("Please create a Package in the Bundle Collector window first.", MessageType.Info);
                    return;
                }

                ClampCreateTargetIndices();
                string[] packageNames = setting.Packages.Select(item => item.PackageName).ToArray();
                int newPackageIndex = BundleCollectorGUIDraw.DrawPopupField("Package", _createPackageIndex, packageNames);
                if (newPackageIndex != _createPackageIndex)
                {
                    _createPackageIndex = newPackageIndex;
                    _createGroupIndex = 0;
                }

                var package = setting.Packages[_createPackageIndex];
                if (package.Groups.Count == 0)
                {
                    EditorGUILayout.HelpBox("Please create a Group in the Bundle Collector window first.", MessageType.Info);
                    return;
                }

                string[] groupNames = package.Groups.Select(item => item.GroupName).ToArray();
                _createGroupIndex = BundleCollectorGUIDraw.DrawPopupField("Group", _createGroupIndex, groupNames);
            }
        }
        private static void DrawOpenCollectorHeader(CollectorContext collectorContext)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("YooAsset", EditorStyles.boldLabel, GUILayout.Width(95));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open Collector", GUILayout.Width(120)))
                {
                    BundleCollectorWindow.OpenWindow(collectorContext.Package.PackageName, collectorContext.Group.GroupName, collectorContext.Collector.CollectPath);
                }
            }
        }
        private static void DrawTargetCollectorContent(CollectorContext collectorContext)
        {
            var package = collectorContext.Package;
            var group = collectorContext.Group;
            var collector = collectorContext.Collector;
            var setting = BundleCollectorSettingData.Setting;
            bool showAlias = setting.ShowEditorAlias;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            try
            {
                // 区块：包裹与分组
                using (new EditorGUILayout.VerticalScope(BundleCollectorGUIStyle.SectionStyle))
                {
                    BundleCollectorGUIDraw.DrawSectionTitle("Package & Group");

                    // 包裹
                    var packageNames = setting.Packages.Select(p => p.PackageName).ToArray();
                    int packageIndex = Mathf.Max(0, Array.IndexOf(packageNames, package.PackageName));
                    int newPackageIndex = BundleCollectorGUIDraw.DrawPopupField("Package", packageIndex, packageNames);
                    if (newPackageIndex != packageIndex && MoveCollectorToPackage(collector, group, setting.Packages[newPackageIndex]))
                        return;

                    // 分组
                    var groupNames = package.Groups.Select(g => g.GroupName).ToArray();
                    int groupIndex = Mathf.Max(0, Array.IndexOf(groupNames, group.GroupName));
                    int newGroupIndex = BundleCollectorGUIDraw.DrawPopupField("Group", groupIndex, groupNames);
                    if (newGroupIndex != groupIndex)
                    {
                        MoveCollectorToGroup(collector, group, package.Groups[newGroupIndex]);
                        return;
                    }
                }

                // 区块：收集器配置
                using (new EditorGUILayout.VerticalScope(BundleCollectorGUIStyle.SectionStyle))
                {
                    BundleCollectorGUIDraw.DrawSectionTitle("Collector Settings");
                    BundleCollectorGUIDraw.DrawLabelField("Collect Path", collector.CollectPath);

                    // 收集器类型
                    int typeIndex = Mathf.Max(0, Array.IndexOf(CollectorTypeNames, collector.CollectorType.ToString()));
                    int newTypeIndex = BundleCollectorGUIDraw.DrawPopupField("Collector Type", typeIndex, CollectorTypeNames);
                    if (newTypeIndex != typeIndex)
                    {
                        RecordUndo("YooAsset.Inspector Modify CollectorType");
                        collector.CollectorType = EditorStringUtility.ParseEnum<ECollectorType>(CollectorTypeNames[newTypeIndex]);
                        CommitModify(group, collector);
                    }

                    // 地址规则（仅对主资源收集器生效）
                    bool isMainCollector = collector.CollectorType == ECollectorType.MainAssetCollector;
                    using (new EditorGUI.DisabledScope(isMainCollector == false))
                    {
                        var addressRules = BundleCollectorSettingData.GetAddressRuleNames();
                        if (BundleCollectorGUIDraw.TryDrawRuleSelection("Address Rule", GetRuleDisplayNames(addressRules, showAlias), GetRuleIndex(addressRules, collector.AddressRuleName), out int newAddressRuleIndex))
                        {
                            RecordUndo("YooAsset.Inspector Modify AddressRule");
                            collector.AddressRuleName = addressRules[newAddressRuleIndex].ClassName;
                            CommitModify(group, collector);
                        }
                    }

                    // 打包规则
                    var packRules = BundleCollectorSettingData.GetBundlePackRuleNames();
                    if (BundleCollectorGUIDraw.TryDrawRuleSelection("Pack Rule", GetRuleDisplayNames(packRules, showAlias), GetRuleIndex(packRules, collector.PackRuleName), out int newPackRuleIndex))
                    {
                        RecordUndo("YooAsset.Inspector Modify PackRule");
                        collector.PackRuleName = packRules[newPackRuleIndex].ClassName;
                        CommitModify(group, collector);
                    }

                    // 过滤规则
                    var filterRules = BundleCollectorSettingData.GetAssetFilterRuleNames();
                    if (BundleCollectorGUIDraw.TryDrawRuleSelection("Filter Rule", GetRuleDisplayNames(filterRules, showAlias), GetRuleIndex(filterRules, collector.FilterRuleName), out int newFilterRuleIndex))
                    {
                        RecordUndo("YooAsset.Inspector Modify FilterRule");
                        collector.FilterRuleName = filterRules[newFilterRuleIndex].ClassName;
                        CommitModify(group, collector);
                    }

                    // 用户数据（延迟提交，避免逐键写盘）
                    string newUserData = BundleCollectorGUIDraw.DrawTextField("UserData", collector.UserData);
                    if (newUserData != collector.UserData)
                    {
                        RecordUndo("YooAsset.Inspector Modify UserData");
                        collector.UserData = newUserData;
                        CommitModify(group, collector);
                    }

                    // 资源标签（仅对主资源收集器生效）
                    using (new EditorGUI.DisabledScope(isMainCollector == false))
                    {
                        string newTags = BundleCollectorGUIDraw.DrawTextField("Tags", collector.AssetTags);
                        if (newTags != collector.AssetTags)
                        {
                            RecordUndo("YooAsset.Inspector Modify AssetTags");
                            collector.AssetTags = newTags;
                            CommitModify(group, collector);
                        }
                    }
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
        }

        /// <summary>
        /// 获取规则列表用于下拉框展示的名称数组
        /// </summary>
        private static string[] GetRuleDisplayNames(List<RuleDisplayName> rules, bool showAlias)
        {
            if (rules == null || rules.Count == 0)
                return Array.Empty<string>();

            return rules.Select(rule => showAlias ? rule.DisplayName : rule.ClassName).ToArray();
        }

        /// <summary>
        /// 根据规则类名查找规则在列表中的索引
        /// </summary>
        private static int GetRuleIndex(List<RuleDisplayName> rules, string className)
        {
            if (rules == null || rules.Count == 0)
                return -1;

            return Mathf.Max(0, rules.FindIndex(rule => rule.ClassName == className));
        }

        #region 数据查找与编辑
        /// <summary>
        /// 获取当前创建收集器所选择的目标包裹和分组
        /// </summary>
        private static bool TryGetCreateCollectorTarget(out BundleCollectorPackage package, out BundleCollectorGroup group)
        {
            package = null;
            group = null;

            var setting = BundleCollectorSettingData.Setting;
            if (setting == null || setting.Packages.Count == 0)
                return false;

            ClampCreateTargetIndices();
            package = setting.Packages[_createPackageIndex];
            if (package.Groups.Count == 0)
                return false;

            group = package.Groups[_createGroupIndex];
            return true;
        }

        /// <summary>
        /// 将创建目标的包裹/分组索引收敛到当前配置的合法范围
        /// </summary>
        private static void ClampCreateTargetIndices()
        {
            var setting = BundleCollectorSettingData.Setting;
            if (setting == null || setting.Packages.Count == 0)
            {
                _createPackageIndex = 0;
                _createGroupIndex = 0;
                return;
            }

            _createPackageIndex = Mathf.Clamp(_createPackageIndex, 0, setting.Packages.Count - 1);
            var package = setting.Packages[_createPackageIndex];
            if (package.Groups.Count == 0)
            {
                _createGroupIndex = 0;
                return;
            }

            _createGroupIndex = Mathf.Clamp(_createGroupIndex, 0, package.Groups.Count - 1);
        }

        /// <summary>
        /// 按文件夹路径精确获取已配置的收集器信息
        /// </summary>
        private static bool TryGetCollector(string folderPath, out CollectorContext result)
        {
            result = default;

            var setting = BundleCollectorSettingData.Setting;
            if (setting == null)
                return false;

            foreach (var package in setting.Packages)
            {
                foreach (var group in package.Groups)
                {
                    foreach (var collector in group.Collectors)
                    {
                        if (string.Equals(collector.CollectPath, folderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            result = new CollectorContext
                            {
                                Package = package,
                                Group = group,
                                Collector = collector,
                            };
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 为指定文件夹在目标包裹分组下创建收集器
        /// </summary>
        private static bool TryAddCollector(string folderPath, BundleCollectorPackage package, BundleCollectorGroup group, out CollectorContext result)
        {
            result = default;
            if (package == null || group == null)
                return false;

            RecordUndo("YooAsset.Inspector Add Collector");
            var collector = new BundleCollector
            {
                CollectPath = folderPath,
                CollectorGUID = AssetDatabase.AssetPathToGUID(folderPath),
            };
            BundleCollectorSettingData.CreateCollector(group, collector);
            BundleCollectorSettingData.SaveFile();

            result = new CollectorContext
            {
                Package = package,
                Group = group,
                Collector = collector,
            };
            return true;
        }

        /// <summary>
        /// 将收集器移动到目标包裹的第一个分组
        /// </summary>
        /// <param name="collector">要移动的收集器实例</param>
        /// <param name="fromGroup">原分组</param>
        /// <param name="toPackage">目标包裹</param>
        /// <returns>目标包裹无分组或无需移动时返回 false</returns>
        private static bool MoveCollectorToPackage(BundleCollector collector, BundleCollectorGroup fromGroup, BundleCollectorPackage toPackage)
        {
            if (toPackage.Groups.Count == 0)
            {
                Debug.LogWarning($"Package '{toPackage.PackageName}' has no group. Please create a group first.");
                return false;
            }

            var toGroup = toPackage.Groups[0];
            if (toGroup == fromGroup)
                return false;

            RecordUndo("YooAsset.Inspector Move Collector Package");
            fromGroup.Collectors.Remove(collector);
            toGroup.Collectors.Add(collector);
            BundleCollectorSettingData.ModifyCollector(toGroup, collector);
            BundleCollectorSettingData.SaveFile();
            return true;
        }

        /// <summary>
        /// 将收集器在同一包裹内移动到目标分组
        /// </summary>
        /// <param name="collector">要移动的收集器实例</param>
        /// <param name="fromGroup">原分组</param>
        /// <param name="toGroup">目标分组</param>
        private static void MoveCollectorToGroup(BundleCollector collector, BundleCollectorGroup fromGroup, BundleCollectorGroup toGroup)
        {
            if (toGroup == fromGroup)
                return;

            RecordUndo("YooAsset.Inspector Move Collector Group");
            fromGroup.Collectors.Remove(collector);
            toGroup.Collectors.Add(collector);
            BundleCollectorSettingData.ModifyCollector(toGroup, collector);
            BundleCollectorSettingData.SaveFile();
        }

        /// <summary>
        /// 标记收集器已修改并持久化配置文件
        /// </summary>
        /// <param name="group">收集器所属分组</param>
        /// <param name="collector">被修改的收集器</param>
        private static void CommitModify(BundleCollectorGroup group, BundleCollector collector)
        {
            BundleCollectorSettingData.ModifyCollector(group, collector);
            BundleCollectorSettingData.SaveFile();
        }

        /// <summary>
        /// 在修改配置前登记 Undo，使操作可撤销。
        /// </summary>
        /// <param name="name">Undo 操作名称</param>
        private static void RecordUndo(string name)
        {
            Undo.RecordObject(BundleCollectorSettingData.Setting, name);
        }
        #endregion
    }
}
