using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器的 XML 配置
    /// </summary>
    public class BundleCollectorConfig
    {
        private const string ConfigVersion = "1";

        private const string XmlVersion = "Version";
        private const string XmlCommon = "Common";
        private const string XmlShowPackageView = "ShowPackageView";
        private const string XmlShowEditorAlias = "ShowEditorAlias";
        private const string XmlUniqueBundleName = "UniqueBundleName";

        private const string XmlPackage = "Package";
        private const string XmlPackageName = "PackageName";
        private const string XmlPackageDesc = "PackageDesc";
        private const string XmlEnableAddressable = "AutoAddressable";
        private const string XmlSupportExtensionless = "SupportExtensionless";
        private const string XmlLocationToLower = "LocationToLower";
        private const string XmlIncludeAssetGUID = "IncludeAssetGUID";
        private const string XmlAutoCollectShaders = "AutoCollectShaders";
        private const string XmlIgnoreRuleName = "IgnoreRuleName";

        private const string XmlGroup = "Group";
        private const string XmlGroupActiveRule = "GroupActiveRule";
        private const string XmlGroupName = "GroupName";
        private const string XmlGroupDesc = "GroupDesc";

        private const string XmlCollector = "Collector";
        private const string XmlCollectPath = "CollectPath";
        private const string XmlCollectorGUID = "CollectGUID";
        private const string XmlCollectorType = "CollectType";
        private const string XmlAddressRule = "AddressRule";
        private const string XmlPackRule = "PackRule";
        private const string XmlFilterRule = "FilterRule";
        private const string XmlUserData = "UserData";
        private const string XmlAssetTags = "AssetTags";

        /// <summary>
        /// 导入XML配置表
        /// </summary>
        /// <param name="filePath">XML 配置文件路径</param>
        public static void ImportXmlConfig(string filePath)
        {
            if (File.Exists(filePath) == false)
                throw new FileNotFoundException(filePath);

            if (Path.GetExtension(filePath) != ".xml")
                throw new ArgumentException($"Only XML files are supported: '{filePath}'.", nameof(filePath));

            // 加载配置文件
            XDocument xdoc = XDocument.Load(filePath);
            XElement root = xdoc.Root;

            // 读取配置版本
            string configVersion = (string)root.Attribute(XmlVersion);
            if (configVersion != ConfigVersion)
            {
                if (UpdateXmlConfig(xdoc) == false)
                    throw new InvalidOperationException($"Config version is not compatible: {configVersion} -> {ConfigVersion}.");
                else
                    Debug.Log($"Config version update succeeded: {configVersion} -> {ConfigVersion}.");
            }

            // 读取公共配置
            bool uniqueBundleName = false;
            bool showPackageView = false;
            bool showEditorAlias = false;
            var commonElement = root.Element(XmlCommon);
            if (commonElement != null)
            {
                showPackageView = ParseBoolAttribute(commonElement, XmlShowPackageView);
                showEditorAlias = ParseBoolAttribute(commonElement, XmlShowEditorAlias);
                uniqueBundleName = ParseBoolAttribute(commonElement, XmlUniqueBundleName);
            }

            // 读取包裹配置
            List<BundleCollectorPackage> packages = new List<BundleCollectorPackage>();
            foreach (var packageElement in root.Elements(XmlPackage))
            {
                BundleCollectorPackage package = new BundleCollectorPackage();
                package.PackageName = ReadStringAttribute(packageElement, XmlPackageName, XmlPackage);
                package.PackageDesc = ReadStringAttribute(packageElement, XmlPackageDesc, XmlPackage);
                package.EnableAddressable = ParseBoolAttribute(packageElement, XmlEnableAddressable);
                package.SupportExtensionless = ParseBoolAttribute(packageElement, XmlSupportExtensionless);
                package.LocationToLower = ParseBoolAttribute(packageElement, XmlLocationToLower);
                package.IncludeAssetGUID = ParseBoolAttribute(packageElement, XmlIncludeAssetGUID);
                package.AutoCollectShaders = ReadBoolAttribute(packageElement, XmlAutoCollectShaders, XmlPackage);
                package.IgnoreRuleName = ReadStringAttribute(packageElement, XmlIgnoreRuleName, XmlPackage);
                packages.Add(package);

                // 读取分组配置
                foreach (var groupElement in packageElement.Elements(XmlGroup))
                {
                    BundleCollectorGroup group = new BundleCollectorGroup();
                    group.ActiveRuleName = ReadStringAttribute(groupElement, XmlGroupActiveRule, XmlGroup);
                    group.GroupName = ReadStringAttribute(groupElement, XmlGroupName, XmlGroup);
                    group.GroupDesc = ReadStringAttribute(groupElement, XmlGroupDesc, XmlGroup);
                    group.AssetTags = ReadStringAttribute(groupElement, XmlAssetTags, XmlGroup);
                    package.Groups.Add(group);

                    // 读取收集器配置
                    foreach (var collectorElement in groupElement.Elements(XmlCollector))
                    {
                        BundleCollector collector = new BundleCollector();
                        collector.CollectPath = ReadStringAttribute(collectorElement, XmlCollectPath, XmlCollector);
                        collector.CollectorGUID = ReadStringAttribute(collectorElement, XmlCollectorGUID, XmlCollector);
                        collector.CollectorType = ReadEnumAttribute<ECollectorType>(collectorElement, XmlCollectorType, XmlCollector);
                        collector.AddressRuleName = ReadStringAttribute(collectorElement, XmlAddressRule, XmlCollector);
                        collector.PackRuleName = ReadStringAttribute(collectorElement, XmlPackRule, XmlCollector);
                        collector.FilterRuleName = ReadStringAttribute(collectorElement, XmlFilterRule, XmlCollector);
                        collector.UserData = ReadStringAttribute(collectorElement, XmlUserData, XmlCollector);
                        collector.AssetTags = ReadStringAttribute(collectorElement, XmlAssetTags, XmlCollector);
                        group.Collectors.Add(collector);
                    }
                }
            }

            // 检测包裹名称唯一性
            var packageNames = new HashSet<string>();
            foreach (var package in packages)
            {
                if (packageNames.Add(package.PackageName) == false)
                    throw new InvalidOperationException($"Duplicate package name found: '{package.PackageName}'.");
            }

            // 检测配置错误
            foreach (var package in packages)
            {
                package.CheckConfigError();
            }

            // 保存配置数据
            BundleCollectorSettingData.ClearAll();
            BundleCollectorSettingData.Setting.ShowPackageView = showPackageView;
            BundleCollectorSettingData.Setting.ShowEditorAlias = showEditorAlias;
            BundleCollectorSettingData.Setting.UniqueBundleName = uniqueBundleName;
            BundleCollectorSettingData.Setting.Packages.AddRange(packages);
            BundleCollectorSettingData.SaveFile();
            Debug.Log("Bundle collector config import completed.");
        }

        /// <summary>
        /// 导出XML配置表
        /// </summary>
        /// <param name="savePath">XML 配置保存路径</param>
        public static void ExportXmlConfig(string savePath)
        {
            if (string.IsNullOrEmpty(savePath))
                throw new ArgumentNullException(nameof(savePath));

            if (File.Exists(savePath))
                File.Delete(savePath);

            var setting = BundleCollectorSettingData.Setting;

            var root = new XElement("root",
                new XAttribute(XmlVersion, ConfigVersion),
                new XElement(XmlCommon,
                    new XAttribute(XmlShowPackageView, setting.ShowPackageView),
                    new XAttribute(XmlShowEditorAlias, setting.ShowEditorAlias),
                    new XAttribute(XmlUniqueBundleName, setting.UniqueBundleName)
                )
            );

            // 设置Package配置
            foreach (var package in setting.Packages)
            {
                var packageElement = new XElement(XmlPackage,
                    new XAttribute(XmlPackageName, package.PackageName),
                    new XAttribute(XmlPackageDesc, package.PackageDesc),
                    new XAttribute(XmlEnableAddressable, package.EnableAddressable),
                    new XAttribute(XmlSupportExtensionless, package.SupportExtensionless),
                    new XAttribute(XmlLocationToLower, package.LocationToLower),
                    new XAttribute(XmlIncludeAssetGUID, package.IncludeAssetGUID),
                    new XAttribute(XmlAutoCollectShaders, package.AutoCollectShaders),
                    new XAttribute(XmlIgnoreRuleName, package.IgnoreRuleName)
                );

                // 设置分组配置
                foreach (var group in package.Groups)
                {
                    var groupElement = new XElement(XmlGroup,
                        new XAttribute(XmlGroupActiveRule, group.ActiveRuleName),
                        new XAttribute(XmlGroupName, group.GroupName),
                        new XAttribute(XmlGroupDesc, group.GroupDesc),
                        new XAttribute(XmlAssetTags, group.AssetTags)
                    );

                    // 设置收集器配置
                    foreach (var collector in group.Collectors)
                    {
                        groupElement.Add(new XElement(XmlCollector,
                            new XAttribute(XmlCollectPath, collector.CollectPath),
                            new XAttribute(XmlCollectorGUID, collector.CollectorGUID),
                            new XAttribute(XmlCollectorType, collector.CollectorType),
                            new XAttribute(XmlAddressRule, collector.AddressRuleName),
                            new XAttribute(XmlPackRule, collector.PackRuleName),
                            new XAttribute(XmlFilterRule, collector.FilterRuleName),
                            new XAttribute(XmlUserData, collector.UserData),
                            new XAttribute(XmlAssetTags, collector.AssetTags)
                        ));
                    }
                    packageElement.Add(groupElement);
                }
                root.Add(packageElement);
            }

            // 生成配置文件
            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            xdoc.Save(savePath);
            Debug.Log("Bundle collector config export completed.");
        }

        private static T ReadEnumAttribute<T>(XElement element, string attributeName, string elementName) where T : struct, Enum
        {
            string value = ReadStringAttribute(element, attributeName, elementName);
            if (Enum.IsDefined(typeof(T), value) == false)
                throw new InvalidOperationException($"Attribute '{attributeName}' in '{elementName}' has invalid enum value: '{value}'.");
            return (T)Enum.Parse(typeof(T), value);
        }
        private static string ReadStringAttribute(XElement element, string attributeName, string elementName)
        {
            var attr = element.Attribute(attributeName);
            if (attr == null)
                throw new InvalidOperationException($"Attribute not found: '{attributeName}' in '{elementName}'.");
            return (string)attr;
        }
        private static bool ReadBoolAttribute(XElement element, string attributeName, string elementName)
        {
            var attr = element.Attribute(attributeName);
            if (attr == null)
                throw new InvalidOperationException($"Attribute not found: '{attributeName}' in '{elementName}'.");
            return string.Equals((string)attr, "True", StringComparison.OrdinalIgnoreCase);
        }
        private static bool ParseBoolAttribute(XElement element, string attributeName)
        {
            var value = (string)element.Attribute(attributeName);
            return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 升级XML配置表
        /// </summary>
        private static bool UpdateXmlConfig(XDocument xdoc)
        {
            XElement root = xdoc.Root;
            string configVersion = (string)root.Attribute(XmlVersion);
            if (configVersion == ConfigVersion)
                return true;

            Debug.LogWarning($"No migration path from config version '{configVersion}' to '{ConfigVersion}'.");
            return false;
        }
    }
}