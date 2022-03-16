using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class CollectAssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public List<string> AssetTags { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { private set; get; }

		public CollectAssetInfo(string assetPath, List<string> assetTags, bool isRawAsset)
		{
			AssetPath = assetPath;
			AssetTags = assetTags;
			IsRawAsset = isRawAsset;
		}
	}
}