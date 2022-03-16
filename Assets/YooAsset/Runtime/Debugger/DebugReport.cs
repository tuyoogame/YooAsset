using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 资源系统调试信息
	/// </summary>
	internal class DebugReport
	{
		public readonly List<DebugProviderInfo> ProviderInfos = new List<DebugProviderInfo>(1000);
		public int BundleCount { set; get; }
		public int AssetCount { set; get; }

		public void ClearAll()
		{
			ProviderInfos.Clear();
		}
	}
}