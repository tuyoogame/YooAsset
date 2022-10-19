using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class CollectResult
	{
		/// <summary>
		/// 包裹名称
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable { private set; get; }

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName { private set; get; }

		/// <summary>
		/// 收集的资源信息列表
		/// </summary>
		public List<CollectAssetInfo> CollectAssets { private set; get; }


		public CollectResult(string packageName, bool enableAddressable, bool uniqueBundleName)
		{
			PackageName = packageName;
			EnableAddressable = enableAddressable;
			UniqueBundleName = uniqueBundleName;
		}

		public void SetCollectAssets(List<CollectAssetInfo> collectAssets)
		{
			CollectAssets = collectAssets;

			if (UniqueBundleName)
			{
				foreach (var collectAsset in CollectAssets)
				{
					collectAsset.BundleNameAppendPackageName(PackageName);
				}
			}
		}
	}
}