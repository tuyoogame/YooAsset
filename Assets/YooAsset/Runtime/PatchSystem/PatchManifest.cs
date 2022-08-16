﻿using System;
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
		/// 文件版本
		/// </summary>
		public string FileVersion;

		/// <summary>
		/// 资源版本号
		/// </summary>
		public int ResourceVersion;

		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 文件名称样式
		/// </summary>
		public int OutputNameStyle;

		/// <summary>
		/// 内置资源的标签列表（首包资源）
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
		public readonly Dictionary<string, PatchBundle> BundleDic = new Dictionary<string, PatchBundle>();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PatchAsset）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchAsset> AssetDic = new Dictionary<string, PatchAsset>();

		/// <summary>
		/// 资源路径映射集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, string> AssetPathMapping = new Dictionary<string, string>();

		// 资源路径映射相关
		private bool _isInitAssetPathMapping = false;
		private bool _locationToLower = false;


		/// <summary>
		/// 初始化资源路径映射
		/// </summary>
		public void InitAssetPathMapping(bool locationToLower)
		{
			if (_isInitAssetPathMapping)
				return;
			_isInitAssetPathMapping = true;

			if (EnableAddressable)
			{
				if (locationToLower)
					YooLogger.Error("Addressable not support location to lower !");
			}
			else
			{
				_locationToLower = locationToLower;
			}

            foreach(var patchAsset in AssetList)
            {
                string location = patchAsset.AssetPath;
                if(locationToLower)
                    location = location.ToLower();
                // 添加原生路径的映射
                if(AssetPathMapping.ContainsKey(location))
                    throw new Exception($"AssetPath have existed : {location}");
                else
                    AssetPathMapping.Add(location, patchAsset.AssetPath);

                // 添加无后缀名路径的映射
                if(Path.HasExtension(location))
                {
                    string locationWithoutExtension = StringUtility.RemoveExtension(location);
                    if(AssetPathMapping.ContainsKey(locationWithoutExtension))
                        YooLogger.Warning($"AssetPath have existed : {locationWithoutExtension}");
                    else
                        AssetPathMapping.Add(locationWithoutExtension, patchAsset.AssetPath);
                }
                // 可寻址路径映射
                if(EnableAddressable)
                {
                    // 可寻址路径
                    location = patchAsset.Address;
                    if(AssetPathMapping.ContainsKey(location))
                        throw new Exception($"Address have existed : {location}");
                    else
                        AssetPathMapping.Add(location, patchAsset.AssetPath);
                }
            }

        }

		/// <summary>
		/// 映射为资源路径
		/// </summary>
		public string MappingToAssetPath(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				YooLogger.Error("Failed to mapping location to asset path, The location is null or empty.");
				return string.Empty;
			}

			if (_locationToLower)
				location = location.ToLower();

			if (AssetPathMapping.TryGetValue(location, out string assetPath))
			{
				return assetPath;
			}
			else
			{
				YooLogger.Warning($"Failed to mapping location to asset path : {location}");
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取主资源包
		/// 注意：传入的资源路径一定合法有效！
		/// </summary>
		public PatchBundle GetMainPatchBundle(string assetPath)
		{
			if (AssetDic.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				int bundleID = patchAsset.BundleID;
				if (bundleID >= 0 && bundleID < BundleList.Count)
				{
					var patchBundle = BundleList[bundleID];
					return patchBundle;
				}
				else
				{
					throw new Exception($"Invalid bundle id : {bundleID} Asset path : {assetPath}");
				}
			}
			else
			{
				throw new Exception("Should never get here !");
			}
		}

		/// <summary>
		/// 获取资源依赖列表
		/// 注意：传入的资源路径一定合法有效！
		/// </summary>
		public PatchBundle[] GetAllDependencies(string assetPath)
		{
			if (AssetDic.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				List<PatchBundle> result = new List<PatchBundle>(patchAsset.DependIDs.Length);
				foreach (var dependID in patchAsset.DependIDs)
				{
					if (dependID >= 0 && dependID < BundleList.Count)
					{
						var dependPatchBundle = BundleList[dependID];
						result.Add(dependPatchBundle);
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
				throw new Exception("Should never get here !");
			}
		}

		/// <summary>
		/// 尝试获取补丁资源
		/// </summary>
		public bool TryGetPatchAsset(string assetPath, out PatchAsset result)
		{
			return AssetDic.TryGetValue(assetPath, out result);
		}

		/// <summary>
		/// 尝试获取补丁资源包
		/// </summary>
		public bool TryGetPatchBundle(string bundleName, out PatchBundle result)
		{
			return BundleDic.TryGetValue(bundleName, out result);
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

			// 检测文件版本
			if (patchManifest.FileVersion != YooAssetSettings.PatchManifestFileVersion)
				throw new Exception($"The manifest file version are not compatible : {patchManifest.FileVersion} != {YooAssetSettings.PatchManifestFileVersion}");

			// BundleList
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.ParseFlagsValue();
				patchBundle.ParseFileName(patchManifest.OutputNameStyle);
				patchManifest.BundleDic.Add(patchBundle.BundleName, patchBundle);
			}

			// AssetList
			foreach (var patchAsset in patchManifest.AssetList)
			{
				// 注意：我们不允许原始路径存在重名
				string assetPath = patchAsset.AssetPath;
				if (patchManifest.AssetDic.ContainsKey(assetPath))
					throw new Exception($"AssetPath have existed : {assetPath}");
				else
					patchManifest.AssetDic.Add(assetPath, patchAsset);
			}

			return patchManifest;
		}
	}
}