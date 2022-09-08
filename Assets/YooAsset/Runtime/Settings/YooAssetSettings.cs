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
		/// 补丁清单文件名称
		/// </summary>
		public string PatchManifestFileName = "PatchManifest";


		/// <summary>
		/// 补丁清单文件版本
		/// </summary>
		public const string PatchManifestFileVersion = "1.2.2";

		/// <summary>
		/// 构建输出文件夹名称
		/// </summary>
		public const string OutputFolderName = "OutputCache";

		/// <summary>
		/// 构建输出的报告文件
		/// </summary>
		public const string ReportFileName = "BuildReport";

		/// <summary>
		/// 静态版本文件
		/// </summary>
		public const string VersionFileName = "StaticVersion.bytes";

		/// <summary>
		/// Unity着色器资源包名称
		/// </summary>
		public const string UnityShadersBundleName = "unityshaders";

		/// <summary>
		/// 忽略的文件类型
		/// </summary>
		public static readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc" };
	}
}