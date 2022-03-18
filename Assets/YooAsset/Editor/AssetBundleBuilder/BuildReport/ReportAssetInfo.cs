using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportAssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 所属资源包名称
		/// </summary>
		public string MainBundle;
		
		/// <summary>
		/// 依赖的资源包名称列表
		/// </summary>
		public List<string> DependBundles = new List<string>();

		/// <summary>
		/// 依赖的资源路径列表
		/// </summary>
		public List<string> DependAssets = new List<string>();
	}
}