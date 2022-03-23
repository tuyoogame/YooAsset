using UnityEngine;

namespace YooAsset
{
	[CreateAssetMenu(fileName = "YooAssetSettings", menuName = "YooAsset/Create Settings")]
	internal class YooAssetSettings : ScriptableObject
	{
		/// <summary>
		/// AssetBundle文件的后缀名
		/// </summary>
		public string AssetBundleFileVariant = "bundle";

		/// <summary>
		/// 原生文件的后缀名
		/// </summary>
		public string RawFileVariant = "rawfile";

		/// <summary>
		/// 构建输出的补丁清单文件名称
		/// </summary>
		public string PatchManifestFileName = "PatchManifest.bytes";

		/// <summary>
		/// 构建输出的补丁清单哈希文件名称
		/// </summary>
		public string PatchManifestHashFileName = "PatchManifestHash.bytes";

		/// <summary>
		/// 构建输出的Unity清单文件名称
		/// </summary>
		public string UnityManifestFileName = "UnityManifest";

		/// <summary>
		/// 构建输出的报告文件
		/// </summary>
		public const string ReportFileName = "BuildReport.json";
	}
}