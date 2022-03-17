using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class BuildAssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源包完整名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset = false;

		/// <summary>
		/// 是否为主动收集资源
		/// </summary>
		public bool IsCollectAsset = false;

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public readonly List<string> AssetTags = new List<string>();

		/// <summary>
		/// 依赖的所有资源
		/// 注意：包括零依赖资源和冗余资源（资源包名无效）
		/// </summary>
		public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


		public BuildAssetInfo(string assetPath)
		{
			AssetPath = assetPath;
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
		/// 设置资源包的标签和文件格式
		/// </summary>
		public void SetBundleLabelAndVariant(string bundleLabel, string bundleVariant)
		{
			if (string.IsNullOrEmpty(BundleName) == false)
				throw new System.Exception("Should never get here !");

			BundleName = AssetBundleBuilderHelper.MakeBundleName(bundleLabel, bundleVariant);
		}

		/// <summary>
		/// 添加资源标记
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			foreach (var tag in tags)
			{
				AddAssetTag(tag);
			}
		}

		/// <summary>
		/// 添加资源标记
		/// </summary>
		public void  AddAssetTag(string tag)
		{
			if (AssetTags.Contains(tag) == false)
			{
				AssetTags.Add(tag);
			}
		}

		/// <summary>
		/// 资源包名是否有效
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