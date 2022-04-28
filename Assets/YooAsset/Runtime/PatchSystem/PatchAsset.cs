using System;

namespace YooAsset
{
	[Serializable]
	internal class PatchAsset
	{
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address;
		
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 所属资源包ID
		/// </summary>
		public int BundleID;

		/// <summary>
		/// 依赖的资源包ID列表
		/// </summary>
		public int[] DependIDs;
	}
}