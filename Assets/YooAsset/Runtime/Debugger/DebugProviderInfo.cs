using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
	{
		/// <summary>
		/// 资源对象路径
		/// </summary>
		public string AssetPath { set; get; }

		/// <summary>
		/// 资源出生的场景
		/// </summary>
		public string SpawnScene { set; get; }

		/// <summary>
		/// 资源出生的时间
		/// </summary>
		public string SpawnTime { set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { set; get; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public ProviderBase.EStatus Status { set; get; }

		/// <summary>
		/// 依赖的资源包列表
		/// </summary>
		public readonly List<DebugBundleInfo> BundleInfos = new List<DebugBundleInfo>();

		public int CompareTo(DebugProviderInfo other)
		{
			return Compare(this, other);
		}
		public int Compare(DebugProviderInfo a, DebugProviderInfo b)
		{
			return string.CompareOrdinal(a.AssetPath, b.AssetPath);
		}
	}
}