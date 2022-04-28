using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	public class AssetBundleGrouperConfig
	{
		public const string XmlShader = "Shader";
		public const string XmlAutoCollectShader = "AutoCollectShader";
		public const string XmlShaderBundleName = "ShaderBundleName";
		public const string XmlGrouper = "Grouper";
		public const string XmlGrouperName = "GrouperName";
		public const string XmlGrouperDesc = "GrouperDesc";
		public const string XmlCollector = "Collector";
		public const string XmlDirectory = "CollectPath";
		public const string XmlAddressRule = "AddressRule";
		public const string XmlPackRule = "PackRule";
		public const string XmlFilterRule = "FilterRule";
		public const string XmlNotWriteToAssetList = "NotWriteToAssetList";
		public const string XmlAssetTags = "AssetTags";


		/// <summary>
		/// 导入XML配置表
		/// </summary>
		public static void ImportXmlConfig(string filePath)
		{
			if (File.Exists(filePath) == false)
				throw new FileNotFoundException(filePath);

			if (Path.GetExtension(filePath) != ".xml")
				throw new Exception($"Only support xml : {filePath}");

			// 加载配置文件
			XmlDocument xml = new XmlDocument();
			xml.Load(filePath);
			XmlElement root = xml.DocumentElement;

			// 读取着色器配置
			bool autoCollectShaders = false;
			string shaderBundleName = string.Empty;
			var shaderNodeList = root.GetElementsByTagName(XmlShader);
			if (shaderNodeList.Count > 0)
			{
				XmlElement shaderElement = shaderNodeList[0] as XmlElement;
				if (shaderElement.HasAttribute(XmlAutoCollectShader) == false)
					throw new Exception($"Not found attribute {XmlAutoCollectShader} in {XmlShader}");
				if (shaderElement.HasAttribute(XmlShaderBundleName) == false)
					throw new Exception($"Not found attribute {XmlShaderBundleName} in {XmlShader}");

				autoCollectShaders = shaderElement.GetAttribute(XmlAutoCollectShader) == "True" ? true : false;
				shaderBundleName = shaderElement.GetAttribute(XmlShaderBundleName);
			}

			// 读取分组配置
			List<AssetBundleGrouper> grouperTemper = new List<AssetBundleGrouper>();
			var grouperNodeList = root.GetElementsByTagName(XmlGrouper);
			foreach (var grouperNode in grouperNodeList)
			{
				XmlElement grouperElement = grouperNode as XmlElement;
				if (grouperElement.HasAttribute(XmlGrouperName) == false)
					throw new Exception($"Not found attribute {XmlGrouperName} in {XmlGrouper}");
				if (grouperElement.HasAttribute(XmlGrouperDesc) == false)
					throw new Exception($"Not found attribute {XmlGrouperDesc} in {XmlGrouper}");
				if (grouperElement.HasAttribute(XmlAssetTags) == false)
					throw new Exception($"Not found attribute {XmlAssetTags} in {XmlGrouper}");

				AssetBundleGrouper grouper = new AssetBundleGrouper();
				grouper.GrouperName = grouperElement.GetAttribute(XmlGrouperName);
				grouper.GrouperDesc = grouperElement.GetAttribute(XmlGrouperDesc);
				grouper.AssetTags = grouperElement.GetAttribute(XmlAssetTags);
				grouperTemper.Add(grouper);

				// 读取收集器配置
				var collectorNodeList = grouperElement.GetElementsByTagName(XmlCollector);
				foreach (var collectorNode in collectorNodeList)
				{
					XmlElement collectorElement = collectorNode as XmlElement;
					if (collectorElement.HasAttribute(XmlDirectory) == false)
						throw new Exception($"Not found attribute {XmlDirectory} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlAddressRule) == false)
						throw new Exception($"Not found attribute {XmlAddressRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlPackRule) == false)
						throw new Exception($"Not found attribute {XmlPackRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlFilterRule) == false)
						throw new Exception($"Not found attribute {XmlFilterRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlNotWriteToAssetList) == false)
						throw new Exception($"Not found attribute {XmlNotWriteToAssetList} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlAssetTags) == false)
						throw new Exception($"Not found attribute {XmlAssetTags} in {XmlCollector}");

					AssetBundleCollector collector = new AssetBundleCollector();
					collector.CollectPath = collectorElement.GetAttribute(XmlDirectory);
					collector.AddressRuleName = collectorElement.GetAttribute(XmlAddressRule);
					collector.PackRuleName = collectorElement.GetAttribute(XmlPackRule);
					collector.FilterRuleName = collectorElement.GetAttribute(XmlFilterRule);
					collector.NotWriteToAssetList = collectorElement.GetAttribute(XmlNotWriteToAssetList) == "True" ? true : false;
					collector.AssetTags = collectorElement.GetAttribute(XmlAssetTags); ;
					grouper.Collectors.Add(collector);
				}
			}

			// 保存配置数据
			AssetBundleGrouperSettingData.ClearAll();
			AssetBundleGrouperSettingData.Setting.AutoCollectShaders = autoCollectShaders;
			AssetBundleGrouperSettingData.Setting.ShadersBundleName = shaderBundleName;
			AssetBundleGrouperSettingData.Setting.Groupers.AddRange(grouperTemper);
			AssetBundleGrouperSettingData.SaveFile();
			Debug.Log($"导入配置完毕！");
		}

		/// <summary>
		/// 导出XML配置表
		/// </summary>
		public static void ExportXmlConfig(string savePath)
		{
			if (File.Exists(savePath))
				File.Delete(savePath);

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sb.AppendLine("<root>");
			sb.AppendLine("</root>");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(sb.ToString());
			XmlElement root = xmlDoc.DocumentElement;

			// 设置着色器配置
			var shaderElement = xmlDoc.CreateElement(XmlShader);
			shaderElement.SetAttribute(XmlAutoCollectShader, AssetBundleGrouperSettingData.Setting.AutoCollectShaders.ToString());
			shaderElement.SetAttribute(XmlShaderBundleName, AssetBundleGrouperSettingData.Setting.ShadersBundleName);

			// 设置分组配置
			foreach (var grouper in AssetBundleGrouperSettingData.Setting.Groupers)
			{
				var grouperElement = xmlDoc.CreateElement(XmlGrouper);
				grouperElement.SetAttribute(XmlGrouperName, grouper.GrouperName);
				grouperElement.SetAttribute(XmlGrouperDesc, grouper.GrouperDesc);
				grouperElement.SetAttribute(XmlAssetTags, grouper.AssetTags);
				root.AppendChild(grouperElement);

				// 设置收集器配置
				foreach (var collector in grouper.Collectors)
				{
					var collectorElement = xmlDoc.CreateElement(XmlCollector);
					collectorElement.SetAttribute(XmlDirectory, collector.CollectPath);
					collectorElement.SetAttribute(XmlAddressRule, collector.AddressRuleName);
					collectorElement.SetAttribute(XmlPackRule, collector.PackRuleName);
					collectorElement.SetAttribute(XmlFilterRule, collector.FilterRuleName);
					collectorElement.SetAttribute(XmlNotWriteToAssetList, collector.NotWriteToAssetList.ToString());
					collectorElement.SetAttribute(XmlAssetTags, collector.AssetTags);
					grouperElement.AppendChild(collectorElement);
				}
			}

			// 生成配置文件
			xmlDoc.Save(savePath);
			Debug.Log($"导出配置完毕！");
		}
	}
}