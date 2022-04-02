using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class CollectAssetInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public List<string> AssetTags { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { private set; get; }

		/// <summary>
		/// 不写入资源列表
		/// </summary>
		public bool NotWriteToAssetList { private set; get; }

		/// <summary>
		/// 依赖的资源列表
		/// </summary>
		public List<string> DependAssets = new List<string>();


		public CollectAssetInfo(string bundleName, string assetPath, List<string> assetTags, bool isRawAsset, bool notWriteToAssetList)
		{
			BundleName = bundleName;
			AssetPath = assetPath;
			AssetTags = assetTags;
			IsRawAsset = isRawAsset;
			NotWriteToAssetList = notWriteToAssetList;
		}
		public CollectAssetInfo(string assetPath, List<string> assetTags, bool isRawAsset, bool notWriteToAssetList)
		{
			BundleName = string.Empty;
			AssetPath = assetPath;
			AssetTags = assetTags;
			IsRawAsset = isRawAsset;
			NotWriteToAssetList = notWriteToAssetList;
		}
	}
}