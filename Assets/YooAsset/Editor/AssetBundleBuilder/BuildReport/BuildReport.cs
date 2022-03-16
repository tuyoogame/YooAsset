using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 构建报告
	/// </summary>
	[Serializable]
	public class BuildReport
	{
		/// <summary>
		/// 资源包列表
		/// </summary>
		public readonly List<BuildBundleInfo> BundleInfos = new List<BuildBundleInfo>(1000);

		/// <summary>
		/// 冗余的资源列表
		/// </summary>
		public readonly List<string> RedundancyList = new List<string>(1000);


		/// <summary>
		/// 检测是否包含BundleName
		/// </summary>
		public bool IsContainsBundle(string bundleFullName)
		{
			return TryGetBundleInfo(bundleFullName, out BuildBundleInfo bundleInfo);
		}

		/// <summary>
		/// 尝试获取资源包类，如果没有返回空
		/// </summary>
		public bool TryGetBundleInfo(string bundleFullName, out BuildBundleInfo result)
		{
			foreach (var bundleInfo in BundleInfos)
			{
				if (bundleInfo.BundleName == bundleFullName)
				{
					result = bundleInfo;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}