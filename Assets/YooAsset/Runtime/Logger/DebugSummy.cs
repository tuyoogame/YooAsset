using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	public class DebugSummy
	{
		/// <summary>
		/// 资源包调试信息
		/// </summary>
		public class DebugBundleInfo
		{
			/// <summary>
			/// 资源包名称
			/// </summary>
			public string BundleName { set; get; }

			/// <summary>
			/// 资源版本
			/// </summary>
			public int Version { set; get; }

			/// <summary>
			/// 引用计数
			/// </summary>
			public int RefCount { set; get; }

			/// <summary>
			/// 加载状态
			/// </summary>
			public ELoaderStates States { set; get; }
		}

		/// <summary>
		/// 资源加载对象调试信息
		/// </summary>
		public class DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
		{
			/// <summary>
			/// 资源对象路径
			/// </summary>
			public string AssetPath { set; get; }

			/// <summary>
			/// 引用计数
			/// </summary>
			public int RefCount { set; get; }

			/// <summary>
			/// 加载状态
			/// </summary>
			public EAssetStates States { set; get; }

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


		public readonly List<DebugProviderInfo> ProviderInfos = new List<DebugProviderInfo>(1000);
		public int BundleCount { set; get; }
		public int AssetCount { set; get; }

		public void ClearAll()
		{
			ProviderInfos.Clear();
		}
	}
}