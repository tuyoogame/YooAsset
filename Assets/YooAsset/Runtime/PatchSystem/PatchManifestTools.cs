using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal static class PatchManifestTools
	{

#if UNITY_EDITOR
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
					buffer.WriteInt32Array(patchBundle.ReferenceIDs);
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

			// 读取文件版本
			string fileVersion = buffer.ReadUTF8();
			if (fileVersion != YooAssetSettings.PatchManifestFileVersion)
				throw new Exception($"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.PatchManifestFileVersion}");

			PatchManifest manifest = new PatchManifest();
			{
				// 读取文件头信息
				manifest.FileVersion = fileVersion;
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
					patchBundle.ReferenceIDs = buffer.ReadInt32Array();
					manifest.BundleList.Add(patchBundle);
				}
			}

			// BundleDic
			manifest.BundleDic = new Dictionary<string, PatchBundle>(manifest.BundleList.Count);
			foreach (var patchBundle in manifest.BundleList)
			{
				patchBundle.ParseBundle(manifest.PackageName, manifest.OutputNameStyle);
				manifest.BundleDic.Add(patchBundle.BundleName, patchBundle);
			}

			// AssetDic
			manifest.AssetDic = new Dictionary<string, PatchAsset>(manifest.AssetList.Count);
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
#endif

		public static string GetRemoteBundleFileExtension(string bundleName)
		{
			string fileExtension = Path.GetExtension(bundleName);
			return fileExtension;
		}
		public static string GetRemoteBundleFileName(int nameStyle, string bundleName, string fileExtension, string fileHash)
		{
			if (nameStyle == 1) //HashName
			{
				return StringUtility.Format("{0}{1}", fileHash, fileExtension);
			}
			else if (nameStyle == 4) //BundleName_HashName
			{
				string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
				return StringUtility.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
			}
			else
			{
				throw new NotImplementedException($"Invalid name style : {nameStyle}");
			}
		}

		/// <summary>
		/// 获取解压BundleInfo
		/// </summary>
		public static BundleInfo GetUnpackInfo(PatchBundle patchBundle)
		{
			// 注意：我们把流加载路径指定为远端下载地址
			string streamingPath = PathHelper.ConvertToWWWPath(patchBundle.StreamingFilePath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
			return bundleInfo;
		}
	}
}