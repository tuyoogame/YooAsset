using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	public static class CollectorConfigImporter
	{
		private class CollectWrapper
		{
			public string CollectDirectory;
			public string PackRuleName;
			public string FilterRuleName;
			public bool DontWriteAssetPath;
			public string AssetTags;

			public CollectWrapper(string directory, string packRuleName, string filterRuleName, bool dontWriteAssetPath, string assetTags)
			{
				CollectDirectory = directory;
				PackRuleName = packRuleName;
				FilterRuleName = filterRuleName;
				DontWriteAssetPath = dontWriteAssetPath;
				AssetTags = assetTags;
			}
		}

		public const string XmlCollector = "Collector";
		public const string XmlDirectory = "Directory";
		public const string XmlPackRule = "PackRule";
		public const string XmlFilterRule = "FilterRule";
		public const string XmlDontWriteAssetPath = "DontWriteAssetPath";
		public const string XmlAssetTags = "AssetTags";
		
		public static void ImportXmlConfig(string filePath)
		{
			if (File.Exists(filePath) == false)
				throw new FileNotFoundException(filePath);

			if (Path.GetExtension(filePath) != ".xml")
				throw new Exception($"Only support xml : {filePath}");

			List<CollectWrapper> wrappers = new List<CollectWrapper>();

			// 加载文件
			XmlDocument xml = new XmlDocument();
			xml.Load(filePath);

			// 解析文件
			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlCollector);
			if (nodeList.Count == 0)
				throw new Exception($"Not found any {XmlCollector}");
			foreach (XmlNode node in nodeList)
			{
				XmlElement collect = node as XmlElement;
				string directory = collect.GetAttribute(XmlDirectory);
				string packRuleName = collect.GetAttribute(XmlPackRule);
				string filterRuleName = collect.GetAttribute(XmlFilterRule);
				string dontWriteAssetPath = collect.GetAttribute(XmlDontWriteAssetPath);
				string assetTags = collect.GetAttribute(XmlAssetTags);

				if (Directory.Exists(directory) == false)
					throw new Exception($"Not found directory : {directory}");

				if (collect.HasAttribute(XmlPackRule) == false)
					throw new Exception($"Not found attribute {XmlPackRule} in collector : {directory}");
				if (collect.HasAttribute(XmlFilterRule) == false)
					throw new Exception($"Not found attribute {XmlFilterRule} in collector : {directory}");
				if (collect.HasAttribute(XmlDontWriteAssetPath) == false)
					throw new Exception($"Not found attribute {XmlDontWriteAssetPath} in collector : {directory}");
				if (collect.HasAttribute(XmlAssetTags) == false)
					throw new Exception($"Not found attribute {XmlAssetTags} in collector : {directory}");

				if (AssetBundleCollectorSettingData.HasPackRuleName(packRuleName) == false)
					throw new Exception($"Invalid {nameof(IPackRule)} class type : {packRuleName}");
				if (AssetBundleCollectorSettingData.HasFilterRuleName(filterRuleName) == false)
					throw new Exception($"Invalid {nameof(IFilterRule)} class type : {filterRuleName}");

				bool dontWriteAssetPathValue = StringUtility.StringToBool(dontWriteAssetPath);
				CollectWrapper collectWrapper = new CollectWrapper(directory, packRuleName, filterRuleName, dontWriteAssetPathValue, assetTags);
				wrappers.Add(collectWrapper);
			}

			// 导入配置数据
			AssetBundleCollectorSettingData.ClearAllCollector();
			foreach (var wrapper in wrappers)
			{
				AssetBundleCollectorSettingData.AddCollector(wrapper.CollectDirectory, wrapper.PackRuleName, wrapper.FilterRuleName, wrapper.DontWriteAssetPath, wrapper.AssetTags, false);
			}

			// 保存配置数据
			AssetBundleCollectorSettingData.SaveFile();
			Debug.Log($"导入配置完毕，一共导入{wrappers.Count}个收集器。");
		}
	}
}