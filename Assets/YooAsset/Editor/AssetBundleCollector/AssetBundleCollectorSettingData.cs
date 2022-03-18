using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleCollectorSettingData
	{
		private static readonly Dictionary<string, System.Type> _cachePackRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new Dictionary<string, IPackRule>();

		private static readonly Dictionary<string, System.Type> _cacheFilterRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new Dictionary<string, IFilterRule>();


		private static AssetBundleCollectorSetting _setting = null;
		public static AssetBundleCollectorSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		public static List<string> GetPackRuleNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cachePackRuleTypes)
			{
				names.Add(pair.Key);
			}
			return names;
		}
		public static List<string> GetFilterRuleNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheFilterRuleTypes)
			{
				names.Add(pair.Key);
			}
			return names;
		}
		public static bool HasPackRuleName(string ruleName)
		{
			foreach (var pair in _cachePackRuleTypes)
			{
				if (pair.Key == ruleName)
					return true;
			}
			return false;
		}
		public static bool HasFilterRuleName(string ruleName)
		{
			foreach (var pair in _cacheFilterRuleTypes)
			{
				if (pair.Key == ruleName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			// 加载配置文件
			_setting = AssetDatabase.LoadAssetAtPath<AssetBundleCollectorSetting>(EditorDefine.AssetBundleCollectorSettingFilePath);
			if (_setting == null)
			{
				Debug.LogWarning($"Create new {nameof(AssetBundleCollectorSetting)}.asset : {EditorDefine.AssetBundleCollectorSettingFilePath}");
				_setting = ScriptableObject.CreateInstance<AssetBundleCollectorSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.AssetBundleCollectorSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.AssetBundleCollectorSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetBundleCollectorSetting)}.asset ok");
			}

			// IPackRule
			{
				// 清空缓存集合
				_cachePackRuleTypes.Clear();
				_cachePackRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(PackExplicit),
					typeof(PackDirectory),
					typeof(PackRawFile),
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IPackRule));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cachePackRuleTypes.ContainsKey(type.Name) == false)
						_cachePackRuleTypes.Add(type.Name, type);
				}
			}

			// IFilterRule
			{
				// 清空缓存集合
				_cacheFilterRuleTypes.Clear();
				_cacheFilterRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(CollectAll),
					typeof(CollectScene),
					typeof(CollectPrefab),
					typeof(CollectSprite)
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IFilterRule));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cacheFilterRuleTypes.ContainsKey(type.Name) == false)
						_cacheFilterRuleTypes.Add(type.Name, type);
				}
			}
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
			}
		}

		// 着色器相关
		public static void ModifyShader(bool isCollectAllShaders, string shadersBundleName)
		{
			if (string.IsNullOrEmpty(shadersBundleName))
				return;
			Setting.AutoCollectShaders = isCollectAllShaders;
			Setting.ShadersBundleName = shadersBundleName;
			SaveFile();
		}

		// 收集器相关
		public static void ClearAllCollector()
		{
			Setting.Collectors.Clear();
			SaveFile();
		}
		public static void AddCollector(string directory, string packRuleName, string filterRuleName, bool dontWriteAssetPath, string assetTags, bool saveFile = true)
		{
			// 末尾添加路径分隔符号
			if (directory.EndsWith("/") == false)
				directory = $"{directory}/";

			// 检测收集器路径冲突
			if (CheckConflict(directory))
				return;

			// 检测资源标签
			if (dontWriteAssetPath && string.IsNullOrEmpty(assetTags) == false)
				Debug.LogWarning($"Collector {directory} has asset tags : {assetTags}, It is not vliad when enable dontWriteAssetPath.");

			AssetBundleCollectorSetting.Collector element = new AssetBundleCollectorSetting.Collector();
			element.CollectDirectory = directory;
			element.PackRuleName = packRuleName;
			element.FilterRuleName = filterRuleName;
			element.DontWriteAssetPath = dontWriteAssetPath;
			element.AssetTags = assetTags;
			Setting.Collectors.Add(element);

			if (saveFile)
				SaveFile();
		}
		public static void RemoveCollector(string directory)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
				{
					Setting.Collectors.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}
		public static void ModifyCollector(string directory, string packRuleName, string filterRuleName, bool dontWriteAssetPath, string assetTags)
		{
			// 检测资源标签
			if (dontWriteAssetPath && string.IsNullOrEmpty(assetTags) == false)
				Debug.LogWarning($"Collector '{directory}' has asset tags '{assetTags}', It is invalid when enable dontWriteAssetPath.");

			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				var collector = Setting.Collectors[i];
				if (collector.CollectDirectory == directory)
				{
					collector.PackRuleName = packRuleName;
					collector.FilterRuleName = filterRuleName;
					collector.DontWriteAssetPath = dontWriteAssetPath;
					collector.AssetTags = assetTags;
					break;
				}
			}
			SaveFile();
		}
		public static bool IsContainsCollector(string directory)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
					return true;
			}
			return false;
		}
		public static bool CheckConflict(string directory)
		{
			if (IsContainsCollector(directory))
			{
				Debug.LogError($"Asset collector already existed : {directory}");
				return true;
			}

			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				var wrapper = Setting.Collectors[i];
				string compareDirectory = wrapper.CollectDirectory;
				if (directory.StartsWith(compareDirectory))
				{
					Debug.LogError($"New asset collector \"{directory}\" conflict with \"{compareDirectory}\"");
					return true;
				}
				if (compareDirectory.StartsWith(directory))
				{
					Debug.LogError($"New asset collector {directory} conflict with {compareDirectory}");
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 获取所有的DLC标记
		/// </summary>
		public static List<string> GetAllAssetTags()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				var tags = Setting.Collectors[i].GetAssetTags();
				foreach (var tag in tags)
				{
					if (result.Contains(tag) == false)
						result.Add(tag);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取收集器总数
		/// </summary>
		public static int GetCollecterCount()
		{
			return Setting.Collectors.Count;
		}

		/// <summary>
		/// 获取所有的收集路径
		/// </summary>
		public static List<string> GetAllCollectDirectory()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];
				result.Add(collector.CollectDirectory);
			}
			return result;
		}

		/// <summary>
		/// 获取所有收集的资源
		/// 注意：跳过了不写入资源路径的收集器
		/// </summary>
		public static List<CollectAssetInfo> GetAllCollectAssets()
		{
			Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];

				// 注意：跳过不需要写入资源路径的收集器
				if (collector.DontWriteAssetPath)
					continue;

				bool isRawAsset = collector.PackRuleName == nameof(PackRawFile);
				string[] findAssets = EditorTools.FindAssets(EAssetSearchType.All, collector.CollectDirectory);
				foreach (string assetPath in findAssets)
				{
					if (IsValidateAsset(assetPath) == false)
						continue;
					if (IsCollectAsset(assetPath, collector.FilterRuleName) == false)
						continue;

					if (result.ContainsKey(assetPath) == false)
					{
						var collectInfo = new CollectAssetInfo(assetPath, collector.GetAssetTags(), isRawAsset);
						result.Add(assetPath, collectInfo);
					}
				}
			}
			return result.Values.ToList();
		}
		private static bool IsCollectAsset(string assetPath, string filterRuleName)
		{
			if (Setting.AutoCollectShaders)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
					return true;
			}

			// 根据规则设置获取标签名称
			IFilterRule filterRuleInstance = GetFilterRuleInstance(filterRuleName);
			return filterRuleInstance.IsCollectAsset(assetPath);
		}

		/// <summary>
		/// 检测资源路径是否被收集器覆盖
		/// </summary>
		public static bool HasCollector(string assetPath)
		{
			// 如果收集全路径着色器
			if (Setting.AutoCollectShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
					return true;
			}

			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];
				if (assetPath.StartsWith(collector.CollectDirectory))
					return true;
			}

			return false;
		}

		/// <summary>
		/// 检测资源是否有效
		/// </summary>
		public static bool IsValidateAsset(string assetPath)
		{
			if (assetPath.StartsWith("Assets/") == false && assetPath.StartsWith("Packages/") == false)
				return false;

			if (AssetDatabase.IsValidFolder(assetPath))
				return false;

			// 注意：忽略编辑器下的类型资源
			Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (type == typeof(LightingDataAsset))
				return false;

			string ext = System.IO.Path.GetExtension(assetPath);
			if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta")
				return false;

			return true;
		}

		/// <summary>
		/// 获取资源包名
		/// </summary>
		public static string GetBundleLabel(string assetPath)
		{
			// 如果收集全路径着色器
			if (Setting.AutoCollectShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
				{
					return EditorTools.GetRegularPath(Setting.ShadersBundleName);
				}
			}

			// 获取收集器
			AssetBundleCollectorSetting.Collector findCollector = null;
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];
				if (assetPath.StartsWith(collector.CollectDirectory))
				{
					findCollector = collector;
					break;
				}
			}

			// 如果没有找到收集器
			string bundleLabel;
			if (findCollector == null)
			{
				IPackRule defaultInstance = new PackDirectory();
				bundleLabel = defaultInstance.GetAssetBundleLabel(assetPath);
			}
			else
			{
				// 根据规则设置获取标签名称
				IPackRule getInstance = GetPackRuleInstance(findCollector.PackRuleName);
				bundleLabel = getInstance.GetAssetBundleLabel(assetPath);
			}

			// 返回包名
			return EditorTools.GetRegularPath(bundleLabel);
		}

		private static IPackRule GetPackRuleInstance(string ruleName)
		{
			if (_cachePackRuleInstance.TryGetValue(ruleName, out IPackRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cachePackRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IPackRule)Activator.CreateInstance(type);
				_cachePackRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new Exception($"{nameof(IPackRule)}类型无效：{ruleName}");
			}
		}
		private static IFilterRule GetFilterRuleInstance(string ruleName)
		{
			if (_cacheFilterRuleInstance.TryGetValue(ruleName, out IFilterRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IFilterRule)Activator.CreateInstance(type);
				_cacheFilterRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new Exception($"{nameof(IFilterRule)}类型无效：{ruleName}");
			}
		}
	}
}