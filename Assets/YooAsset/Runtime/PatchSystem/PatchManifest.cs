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
		/// 文件版本
		/// </summary>
		public string FileVersion;

		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 文件名称样式
		/// </summary>
		public int OutputNameStyle;

		/// <summary>
		/// 资源包裹名称
		/// </summary>
		public string PackageName;

		/// <summary>
		/// 资源包裹的版本信息
		/// </summary>
		public string PackageVersion;

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

				foreach (var patchAsset in AssetList)
				{
					string location = patchAsset.Address;
					if (AssetPathMapping.ContainsKey(location))
						throw new Exception($"Address have existed : {location}");
					else
						AssetPathMapping.Add(location, patchAsset.AssetPath);
				}
			}
			else
			{
				_locationToLower = locationToLower;
				foreach (var patchAsset in AssetList)
				{
					string location = patchAsset.AssetPath;
					if (locationToLower)
						location = location.ToLower();

					// 添加原生路径的映射
					if (AssetPathMapping.ContainsKey(location))
						throw new Exception($"AssetPath have existed : {location}");
					else
						AssetPathMapping.Add(location, patchAsset.AssetPath);

					// 添加无后缀名路径的映射
					if (Path.HasExtension(location))
					{
						string locationWithoutExtension = StringUtility.RemoveExtension(location);
						if (AssetPathMapping.ContainsKey(locationWithoutExtension))
							YooLogger.Warning($"AssetPath have existed : {locationWithoutExtension}");
						else
							AssetPathMapping.Add(locationWithoutExtension, patchAsset.AssetPath);
					}
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
		/// 尝试映射为资源路径
		/// </summary>
		public string TryMappingToAssetPath(string location)
		{
			if (string.IsNullOrEmpty(location))
				return string.Empty;

			if (_locationToLower)
				location = location.ToLower();

			if (AssetPathMapping.TryGetValue(location, out string assetPath))
				return assetPath;
			else
				return string.Empty;
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
		/// 是否包含资源文件
		/// </summary>
		public bool IsIncludeBundleFile(string fileName)
		{
			foreach (var patchBundle in BundleList)
			{
				if (patchBundle.FileName == fileName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		public AssetInfo[] GetAssetsInfoByTags(string[] tags)
		{
			List<AssetInfo> result = new List<AssetInfo>(100);
			foreach (var patchAsset in AssetList)
			{
				if (patchAsset.HasTag(tags))
				{
					AssetInfo assetInfo = new AssetInfo(patchAsset);
					result.Add(assetInfo);
				}
			}
			return result.ToArray();
		}


		/// <summary>
		/// 序列化（JSON文件）
		/// </summary>
		public static void SerializeToJson(string savePath, PatchManifest manifest)
		{
			string json = JsonUtility.ToJson(manifest, true);
			FileUtility.CreateFile(savePath, json);
		}

		/// <summary>
		/// 序列化（二进制文件）
		/// </summary>
		public static void SerializeToBinary(string savePath, PatchManifest patchManifest)
		{
			using (FileStream fs = new FileStream(savePath, FileMode.Create))
			{
				// 创建缓存器
				BufferWriter buffer = new BufferWriter(YooAssetSettings.PatchManifestFileMaxSize);

				// 写入文件标记
				buffer.WriteUInt32(YooAssetSettings.PatchManifestFileSign);

				// 写入文件版本
				buffer.WriteUTF8(patchManifest.FileVersion);

				// 写入文件头信息
				buffer.WriteBool(patchManifest.EnableAddressable);
				buffer.WriteInt32(patchManifest.OutputNameStyle);
				buffer.WriteUTF8(patchManifest.PackageName);
				buffer.WriteUTF8(patchManifest.PackageVersion);

				// 写入资源列表
				buffer.WriteInt32(patchManifest.AssetList.Count);
				for (int i = 0; i < patchManifest.AssetList.Count; i++)
				{
					var patchAsset = patchManifest.AssetList[i];
					buffer.WriteUTF8(patchAsset.Address);
					buffer.WriteUTF8(patchAsset.AssetPath);
					buffer.WriteUTF8Array(patchAsset.AssetTags);
					buffer.WriteInt32(patchAsset.BundleID);
					buffer.WriteInt32Array(patchAsset.DependIDs);
				}

				// 写入资源包列表
				buffer.WriteInt32(patchManifest.BundleList.Count);
				for (int i = 0; i < patchManifest.BundleList.Count; i++)
				{
					var patchBundle = patchManifest.BundleList[i];
					buffer.WriteUTF8(patchBundle.BundleName);
					buffer.WriteUTF8(patchBundle.FileHash);
					buffer.WriteUTF8(patchBundle.FileCRC);
					buffer.WriteInt64(patchBundle.FileSize);
					buffer.WriteBool(patchBundle.IsRawFile);
					buffer.WriteByte(patchBundle.LoadMethod);
					buffer.WriteUTF8Array(patchBundle.Tags);
				}

				// 写入文件流
				buffer.WriteToStream(fs);
				fs.Flush();
			}
		}

		/// <summary>
		/// 反序列化（二进制文件）
		/// </summary>
		public static PatchManifest DeserializeFromBinary(byte[] binaryData)
		{
			// 创建缓存器
			BufferReader buffer = new BufferReader(binaryData);

			// 读取文件标记
			uint fileSign = buffer.ReadUInt32();
			if (fileSign != YooAssetSettings.PatchManifestFileSign)
				throw new Exception("Invalid manifest file !");

			PatchManifest manifest = new PatchManifest();
			{
				// 读取文件版本
				manifest.FileVersion = buffer.ReadUTF8();
				if (manifest.FileVersion != YooAssetSettings.PatchManifestFileVersion)
					throw new Exception($"The manifest file version are not compatible : {manifest.FileVersion} != {YooAssetSettings.PatchManifestFileVersion}");

				// 读取文件头信息
				manifest.EnableAddressable = buffer.ReadBool();
				manifest.OutputNameStyle = buffer.ReadInt32();
				manifest.PackageName = buffer.ReadUTF8();
				manifest.PackageVersion = buffer.ReadUTF8();

				// 读取资源列表
				int patchAssetCount = buffer.ReadInt32();
				manifest.AssetList = new List<PatchAsset>(patchAssetCount);
				for (int i = 0; i < patchAssetCount; i++)
				{
					var patchAsset = new PatchAsset();
					patchAsset.Address = buffer.ReadUTF8();
					patchAsset.AssetPath = buffer.ReadUTF8();
					patchAsset.AssetTags = buffer.ReadUTF8Array();
					patchAsset.BundleID = buffer.ReadInt32();
					patchAsset.DependIDs = buffer.ReadInt32Array();
					manifest.AssetList.Add(patchAsset);
				}

				// 读取资源包列表
				int patchBundleCount = buffer.ReadInt32();
				manifest.BundleList = new List<PatchBundle>(patchBundleCount);
				for (int i = 0; i < patchBundleCount; i++)
				{
					var patchBundle = new PatchBundle();
					patchBundle.BundleName = buffer.ReadUTF8();
					patchBundle.FileHash = buffer.ReadUTF8();
					patchBundle.FileCRC = buffer.ReadUTF8();
					patchBundle.FileSize = buffer.ReadInt64();
					patchBundle.IsRawFile = buffer.ReadBool();
					patchBundle.LoadMethod = buffer.ReadByte();
					patchBundle.Tags = buffer.ReadUTF8Array();
					manifest.BundleList.Add(patchBundle);
				}
			}

			// BundleList
			foreach (var patchBundle in manifest.BundleList)
			{
				patchBundle.ParseBundle(manifest.PackageName, manifest.OutputNameStyle);
				manifest.BundleDic.Add(patchBundle.BundleName, patchBundle);
			}

			// AssetList
			foreach (var patchAsset in manifest.AssetList)
			{
				// 注意：我们不允许原始路径存在重名
				string assetPath = patchAsset.AssetPath;
				if (manifest.AssetDic.ContainsKey(assetPath))
					throw new Exception($"AssetPath have existed : {assetPath}");
				else
					manifest.AssetDic.Add(assetPath, patchAsset);
			}

			return manifest;
		}


		/// <summary>
		/// 生成Bundle文件的正式名称
		/// </summary>
		public static string CreateBundleFileName(int nameStype, string bundleName, string fileHash)
		{
			if (nameStype == 1)
			{
				return fileHash;
			}
			else if (nameStype == 2)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				return $"{fileHash}{tempFileExtension}";
			}
			else if (nameStype == 3)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				string tempBundleName = bundleName.Replace('/', '_').Replace(tempFileExtension, "");
				return $"{tempBundleName}_{fileHash}";
			}
			else if (nameStype == 4)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				string tempBundleName = bundleName.Replace('/', '_').Replace(tempFileExtension, "");
				return $"{tempBundleName}_{fileHash}{tempFileExtension}";
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}