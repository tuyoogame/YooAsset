﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleCollectorSettingData
	{
		private static readonly Dictionary<string, System.Type> _cacheAddressRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IAddressRule> _cacheAddressRuleInstance = new Dictionary<string, IAddressRule>();

		private static readonly Dictionary<string, System.Type> _cachePackRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new Dictionary<string, IPackRule>();

		private static readonly Dictionary<string, System.Type> _cacheFilterRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new Dictionary<string, IFilterRule>();

		/// <summary>
		/// 配置数据是否被修改
		/// </summary>
		public static bool IsDirty { private set; get; } = false;


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

		public static List<string> GetAddressRuleNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheAddressRuleTypes)
			{
				names.Add(pair.Key);
			}
			return names;
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
		public static bool HasAddressRuleName(string ruleName)
		{
			foreach (var pair in _cacheAddressRuleTypes)
			{
				if (pair.Key == ruleName)
					return true;
			}
			return false;
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
			_setting = EditorHelper.LoadSettingData<AssetBundleCollectorSetting>();

			// IPackRule
			{
				// 清空缓存集合
				_cachePackRuleTypes.Clear();
				_cachePackRuleInstance.Clear();

				var types = TypeCache.GetTypesDerivedFrom<IPackRule>();

				foreach(var type in types)
				{
					_cachePackRuleTypes.TryAdd(type.Name, type);
				}
			}

			// IFilterRule
			{
				// 清空缓存集合
				_cacheFilterRuleTypes.Clear();
				_cacheFilterRuleInstance.Clear();

				var types = TypeCache.GetTypesDerivedFrom<IFilterRule>();
				
				foreach(var type in types)
				{
					_cacheFilterRuleTypes.TryAdd(type.Name, type);
				}
			}

			// IAddressRule
			{
				// 清空缓存集合
				_cacheAddressRuleTypes.Clear();
				_cacheAddressRuleInstance.Clear();

				var types = TypeCache.GetTypesDerivedFrom<IAddressRule>();
				
				foreach(var type in types)
				{
					_cacheAddressRuleTypes.TryAdd(type.Name, type);
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
				IsDirty = false;
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(AssetBundleCollectorSetting)}.asset is saved!");
			}
		}

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			Setting.AutoCollectShaders = false;
			Setting.ShadersBundleName = string.Empty;
			Setting.Groups.Clear();
			SaveFile();
		}

		// 实例类相关
		public static IAddressRule GetAddressRuleInstance(string ruleName)
		{
			if (_cacheAddressRuleInstance.TryGetValue(ruleName, out IAddressRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheAddressRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IAddressRule)Activator.CreateInstance(type);
				_cacheAddressRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new Exception($"{nameof(IAddressRule)}类型无效：{ruleName}");
			}
		}
		public static IPackRule GetPackRuleInstance(string ruleName)
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
		public static IFilterRule GetFilterRuleInstance(string ruleName)
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

		// 可寻址编辑相关
		public static void ModifyAddressable(bool enableAddressable)
		{
			Setting.EnableAddressable = enableAddressable;
			IsDirty = true;
		}

		// 着色器编辑相关
		public static void ModifyShader(bool isCollectAllShaders, string shadersBundleName)
		{
			Setting.AutoCollectShaders = isCollectAllShaders;
			Setting.ShadersBundleName = shadersBundleName;
			IsDirty = true;
		}

		// 资源分组编辑相关
		public static void CreateGroup(string groupName)
		{
			AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
			group.GroupName = groupName;
			Setting.Groups.Add(group);
			IsDirty = true;
		}
		public static void RemoveGroup(AssetBundleCollectorGroup group)
		{
			if (Setting.Groups.Remove(group))
			{
				IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Failed remove group : {group.GroupName}");
			}
		}
		public static void ModifyGroup(AssetBundleCollectorGroup group)
		{
			if (group != null)
			{
				IsDirty = true;
			}
		}

		// 资源收集器编辑相关
		public static void CreateCollector(AssetBundleCollectorGroup group, string collectPath)
		{
			AssetBundleCollector collector = new AssetBundleCollector();
			collector.CollectPath = collectPath;
			group.Collectors.Add(collector);
			IsDirty = true;
		}
		public static void RemoveCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group.Collectors.Remove(collector))
			{
				IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Failed remove collector : {collector.CollectPath}");
			}
		}
		public static void ModifyCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group != null && collector != null)
			{
				IsDirty = true;
			}
		}
	}
}