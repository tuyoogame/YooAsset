using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class BuildAssetInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { private set; get; }

		/// <summary>
		/// 不写入资源列表
		/// </summary>
		public bool NotWriteToAssetList { private set; get; }

		/// <summary>
		/// 是否为主动收集资源
		/// </summary>
		public bool IsCollectAsset { private set; get; }

		/// <summary>
		/// 是否为着色器资源
		/// </summary>
		public bool IsShaderAsset { private set; get; }

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// 资源分类标签列表
		/// </summary>
		public readonly List<string> AssetTags = new List<string>();

		/// <summary>
		/// 依赖的所有资源
		/// 注意：包括零依赖资源和冗余资源（资源包名无效）
		/// </summary>
		public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


		public BuildAssetInfo(string address, string assetPath, bool isRawAsset, bool notWriteToAssetList)
		{
			Address = address;
			AssetPath = assetPath;
			IsRawAsset = isRawAsset;
			NotWriteToAssetList = notWriteToAssetList;
			IsCollectAsset = true;

			System.Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (assetType == typeof(UnityEngine.Shader))
				IsShaderAsset = true;
			else
				IsShaderAsset = false;
		}
		public BuildAssetInfo(string assetPath)
		{
			AssetPath = assetPath;
			IsRawAsset = false;
			NotWriteToAssetList = true;
			IsCollectAsset = false;

			System.Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (assetType == typeof(UnityEngine.Shader))
				IsShaderAsset = true;
			else
				IsShaderAsset = false;
		}

		/// <summary>
		/// 设置所有依赖的资源
		/// </summary>
		public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
		{
			if (AllDependAssetInfos != null)
				throw new System.Exception("Should never get here !");

			AllDependAssetInfos = dependAssetInfos;
		}

		/// <summary>
		/// 设置资源包名称
		/// </summary>
		public void SetBundleName(string bundleName)
		{
			if (string.IsNullOrEmpty(BundleName) == false)
				throw new System.Exception("Should never get here !");

			BundleName = bundleName;
		}

		/// <summary>
		/// 添加资源分类标签
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			foreach (var tag in tags)
			{
				AddAssetTag(tag);
			}
		}

		/// <summary>
		/// 添加资源分类标签
		/// </summary>
		public void AddAssetTag(string tag)
		{
			if (AssetTags.Contains(tag) == false)
			{
				AssetTags.Add(tag);
			}
		}

		/// <summary>
		/// 资源包名称是否有效
		/// </summary>
		public bool BundleNameIsValid()
		{
			if (string.IsNullOrEmpty(BundleName))
				return false;
			else
				return true;
		}
	}
}