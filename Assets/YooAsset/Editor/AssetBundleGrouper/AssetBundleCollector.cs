using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class AssetBundleCollector
	{
		/// <summary>
		/// 收集路径
		/// 注意：支持文件夹或单个资源文件
		/// </summary>
		public string CollectPath = string.Empty;

		/// <summary>
		/// 收集器类型
		/// </summary>
		public ECollectorType CollectorType = ECollectorType.MainCollector;

		/// <summary>
		/// 寻址规则类名
		/// </summary>
		public string AddressRuleName = nameof(AddressByFileName);

		/// <summary>
		/// 打包规则类名
		/// </summary>
		public string PackRuleName = nameof(PackDirectory);

		/// <summary>
		/// 过滤规则类名
		/// </summary>
		public string FilterRuleName = nameof(CollectAll);

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public string AssetTags = string.Empty;


		/// <summary>
		/// 收集器是否有效
		/// </summary>
		public bool IsValid()
		{
			if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CollectPath) == null)
				return false;
			if (AssetBundleGrouperSettingData.HasPackRuleName(PackRuleName) == false)
				return false;
			if (AssetBundleGrouperSettingData.HasFilterRuleName(FilterRuleName) == false)
				return false;
			if (AssetBundleGrouperSettingData.HasAddressRuleName(AddressRuleName) == false)
				return false;
			return true;
		}

		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CollectPath) == null)
				throw new Exception($"Invalid collect path : {CollectPath}");

			if (CollectorType == ECollectorType.None)
				throw new Exception($"{nameof(ECollectorType)}.{ECollectorType.None} is invalid in collector : {CollectPath}");

			if (AssetBundleGrouperSettingData.HasPackRuleName(PackRuleName) == false)
				throw new Exception($"Invalid {nameof(IPackRule)} class type : {PackRuleName} in collector : {CollectPath}");

			if (AssetBundleGrouperSettingData.HasFilterRuleName(FilterRuleName) == false)
				throw new Exception($"Invalid {nameof(IFilterRule)} class type : {FilterRuleName} in collector : {CollectPath}");

			if (AssetBundleGrouperSettingData.HasAddressRuleName(AddressRuleName) == false)
				throw new Exception($"Invalid {nameof(IAddressRule)} class type : {AddressRuleName} in collector : {CollectPath}");
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets(AssetBundleGrouper grouper)
		{
			Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(1000);
			bool isRawAsset = PackRuleName == nameof(PackRawFile);

			// 检测原生资源包的收集器类型
			if (isRawAsset && CollectorType != ECollectorType.MainCollector)
				throw new Exception($"The raw file must be set to {nameof(ECollectorType)}.{ECollectorType.MainCollector} : {CollectPath}");

			// 收集打包资源
			if (AssetDatabase.IsValidFolder(CollectPath))
			{
				string collectDirectory = CollectPath;
				string[] findAssets = EditorTools.FindAssets(EAssetSearchType.All, collectDirectory);
				foreach (string assetPath in findAssets)
				{
					if (IsValidateAsset(assetPath) && IsCollectAsset(assetPath))
					{
						if (result.ContainsKey(assetPath) == false)
						{
							var collectAssetInfo = CreateCollectAssetInfo(grouper, assetPath, isRawAsset);
							result.Add(assetPath, collectAssetInfo);
						}
						else
						{
							throw new Exception($"The collecting asset file is existed : {assetPath} in collector : {CollectPath}");
						}
					}
				}
			}
			else
			{
				string assetPath = CollectPath;
				if (IsValidateAsset(assetPath) && IsCollectAsset(assetPath))
				{
					var collectAssetInfo = CreateCollectAssetInfo(grouper, assetPath, isRawAsset);
					result.Add(assetPath, collectAssetInfo);
				}
				else
				{
					throw new Exception($"The collecting single asset file is invalid : {assetPath} in collector : {CollectPath}");
				}
			}

			// 检测可寻址地址是否重复
			if (AssetBundleGrouperSettingData.Setting.EnableAddressable)
			{
				HashSet<string> adressTemper = new HashSet<string>();
				foreach (var collectInfoPair in result)
				{
					if (collectInfoPair.Value.CollectorType == ECollectorType.MainCollector)
					{
						string address = collectInfoPair.Value.Address;
						if (adressTemper.Contains(address) == false)
							adressTemper.Add(address);
						else
							throw new Exception($"The address is existed : {address} in collector : {CollectPath}");
					}
				}
			}

			// 返回列表
			return result.Values.ToList();
		}

		private CollectAssetInfo CreateCollectAssetInfo(AssetBundleGrouper grouper, string assetPath, bool isRawAsset)
		{
			string address = GetAddress(grouper, assetPath);
			string bundleName = GetBundleName(grouper, assetPath);
			List<string> assetTags = GetAssetTags(grouper);
			CollectAssetInfo collectAssetInfo = new CollectAssetInfo(CollectorType, bundleName, address, assetPath, assetTags, isRawAsset);
			collectAssetInfo.DependAssets = GetAllDependencies(assetPath);
			return collectAssetInfo;
		}
		private bool IsValidateAsset(string assetPath)
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
			if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta" || ext == ".cginc")
				return false;

			return true;
		}
		private bool IsCollectAsset(string assetPath)
		{
			// 如果收集全路径着色器
			if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
					return true;
			}

			// 根据规则设置过滤资源文件
			IFilterRule filterRuleInstance = AssetBundleGrouperSettingData.GetFilterRuleInstance(FilterRuleName);
			return filterRuleInstance.IsCollectAsset(new FilterRuleData(assetPath));
		}
		private string GetAddress(AssetBundleGrouper grouper, string assetPath)
		{
			if (CollectorType != ECollectorType.MainCollector)
				return string.Empty;

			IAddressRule addressRuleInstance = AssetBundleGrouperSettingData.GetAddressRuleInstance(AddressRuleName);
			string adressValue = addressRuleInstance.GetAssetAddress(new AddressRuleData(assetPath, CollectPath, grouper.GrouperName));
			return adressValue;
		}
		private string GetBundleName(AssetBundleGrouper grouper, string assetPath)
		{
			// 如果自动收集所有的着色器
			if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
				{
					string bundleName = AssetBundleGrouperSettingData.Setting.ShadersBundleName;
					return EditorTools.GetRegularPath(bundleName).ToLower();
				}
			}

			// 根据规则设置获取资源包名称
			{
				IPackRule packRuleInstance = AssetBundleGrouperSettingData.GetPackRuleInstance(PackRuleName);
				string bundleName = packRuleInstance.GetBundleName(new PackRuleData(assetPath, CollectPath, grouper.GrouperName));
				return EditorTools.GetRegularPath(bundleName).ToLower();
			}
		}
		private List<string> GetAssetTags(AssetBundleGrouper grouper)
		{
			List<string> tags = StringUtility.StringToStringList(grouper.AssetTags, ';');
			List<string> temper = StringUtility.StringToStringList(AssetTags, ';');
			tags.AddRange(temper);
			return tags;
		}
		private List<string> GetAllDependencies(string mainAssetPath)
		{
			List<string> result = new List<string>();
			string[] depends = AssetDatabase.GetDependencies(mainAssetPath, true);
			foreach (string assetPath in depends)
			{
				if (IsValidateAsset(assetPath))
				{
					// 注意：排除主资源对象
					if (assetPath != mainAssetPath)
						result.Add(assetPath);
				}
			}
			return result;
		}
	}
}