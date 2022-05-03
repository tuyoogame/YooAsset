using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 补丁清单文件
	/// </summary>
	[Serializable]
	internal class PatchManifest
	{
		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 资源版本号
		/// </summary>
		public int ResourceVersion;

		/// <summary>
		/// 内置资源的标记列表
		/// </summary>
		public string BuildinTags;

		/// <summary>
		/// 资源列表（主动收集的资源列表）
		/// </summary>
		public List<PatchAsset> AssetList = new List<PatchAsset>();

		/// <summary>
		/// 资源包列表
		/// </summary>
		public List<PatchBundle> BundleList = new List<PatchBundle>();


		/// <summary>
		/// 资源包集合（提供BundleName获取PatchBundle）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchBundle> Bundles = new Dictionary<string, PatchBundle>();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PatchAsset）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchAsset> Assets = new Dictionary<string, PatchAsset>();

		/// <summary>
		/// 资源路径映射集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, string> AssetPathMapping = new Dictionary<string, string>();


		/// <summary>
		/// 获取内置资源标签列表
		/// </summary>
		public string[] GetBuildinTags()
		{
			return StringUtility.StringToStringList(BuildinTags, ';').ToArray();
		}

		/// <summary>
		/// 获取资源依赖列表
		/// </summary>
		public string[] GetAllDependencies(string assetPath)
		{
			if (Assets.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				List<string> result = new List<string>(patchAsset.DependIDs.Length);
				foreach (var dependID in patchAsset.DependIDs)
				{
					if (dependID >= 0 && dependID < BundleList.Count)
					{
						var dependPatchBundle = BundleList[dependID];
						result.Add(dependPatchBundle.BundleName);
					}
					else
					{
						throw new Exception($"Invalid bundle id : {dependID} Asset path : {assetPath}");
					}
				}
				return result.ToArray();
			}
			else
			{
				YooLogger.Warning($"Not found asset path in patch manifest : {assetPath}");
				return new string[] { };
			}
		}

		/// <summary>
		/// 获取资源包名称
		/// </summary>
		public string GetBundleName(string assetPath)
		{
			if (Assets.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				int bundleID = patchAsset.BundleID;
				if (bundleID >= 0 && bundleID < BundleList.Count)
				{
					var patchBundle = BundleList[bundleID];
					return patchBundle.BundleName;
				}
				else
				{
					throw new Exception($"Invalid bundle id : {bundleID} Asset path : {assetPath}");
				}
			}
			else
			{
				YooLogger.Warning($"Not found asset path in patch manifest : {assetPath}");
				return string.Empty;
			}
		}

		/// <summary>
		/// 尝试获取资源包的主资源路径
		/// </summary>
		public string TryGetBundleMainAssetPath(string bundleName)
		{
			foreach (var patchAsset in AssetList)
			{
				int bundleID = patchAsset.BundleID;
				if (bundleID >= 0 && bundleID < BundleList.Count)
				{
					var patchBundle = BundleList[bundleID];
					if (patchBundle.BundleName == bundleName)
						return patchAsset.AssetPath;
				}
				else
				{
					throw new Exception($"Invalid bundle id : {bundleID} Asset path : {patchAsset.AssetPath}");
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// 映射为资源路径
		/// </summary>
		public string MappingToAssetPath(string location)
		{
			if (AssetPathMapping.TryGetValue(location, out string assetPath))
			{
				return assetPath;
			}
			else
			{
				YooLogger.Warning($"Failed to mapping location to asset path  : {location}");
				return string.Empty;
			}
		}


		/// <summary>
		/// 序列化
		/// </summary>
		public static void Serialize(string savePath, PatchManifest patchManifest)
		{
			string json = JsonUtility.ToJson(patchManifest);
			FileUtility.CreateFile(savePath, json);
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static PatchManifest Deserialize(string jsonData)
		{
			PatchManifest patchManifest = JsonUtility.FromJson<PatchManifest>(jsonData);

			// BundleList
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.ParseFlagsValue();
				patchManifest.Bundles.Add(patchBundle.BundleName, patchBundle);
			}

			// AssetList
			foreach (var patchAsset in patchManifest.AssetList)
			{
				// 注意：我们不允许原始路径存在重名
				string assetPath = patchAsset.AssetPath;
				if (patchManifest.Assets.ContainsKey(assetPath))
					throw new Exception($"AssetPath have existed : {assetPath}");
				else
					patchManifest.Assets.Add(assetPath, patchAsset);
			}

			// AssetPathMapping
			if (patchManifest.EnableAddressable)
			{
				foreach (var patchAsset in patchManifest.AssetList)
				{
					string address = patchAsset.Address;
					if (patchManifest.AssetPathMapping.ContainsKey(address))
						throw new Exception($"Address have existed : {address}");
					else
						patchManifest.AssetPathMapping.Add(address, patchAsset.AssetPath);
				}
			}
			else
			{
				foreach (var patchAsset in patchManifest.AssetList)
				{
					string assetPath = patchAsset.AssetPath;

					// 添加原生路径的映射
					if (patchManifest.AssetPathMapping.ContainsKey(assetPath))
						throw new Exception($"AssetPath have existed : {assetPath}");
					else
						patchManifest.AssetPathMapping.Add(assetPath, assetPath);

					// 添加无后缀名路径的映射
					if (Path.HasExtension(assetPath))
					{
						string assetPathWithoutExtension = StringUtility.RemoveExtension(assetPath);
						if (patchManifest.AssetPathMapping.ContainsKey(assetPathWithoutExtension))
							YooLogger.Warning($"AssetPath have existed : {assetPathWithoutExtension}");
						else
							patchManifest.AssetPathMapping.Add(assetPathWithoutExtension, assetPath);
					}
				}
			}

			return patchManifest;
		}
	}
}