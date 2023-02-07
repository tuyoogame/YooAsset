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
		public string RawBundleFileVariant = "rawfile";

		/// <summary>
		/// 清单文件名称
		/// </summary>
		public string PatchManifestFileName = "PatchManifest";

		/// <summary>
		/// 资源包名正规化（移除路径分隔符）
		/// </summary>
		public bool RegularBundleName = true;


		/// <summary>
		/// 清单文件头标记
		/// </summary>
		public const uint PatchManifestFileSign = 0x594F4F;

		/// <summary>
		/// 清单文件极限大小（100MB）
		/// </summary>
		public const int PatchManifestFileMaxSize = 104857600;

		/// <summary>
		/// 清单文件格式版本
		/// </summary>
		public const string PatchManifestFileVersion = "1.4.0";

		
		/// <summary>
		/// 缓存的数据文件名称
		/// </summary>
		public const string CacheBundleDataFileName = "__data";

		/// <summary>
		/// 缓存的信息文件名称
		/// </summary>
		public const string CacheBundleInfoFileName = "__info";


		/// <summary>
		/// 构建输出文件夹名称
		/// </summary>
		public const string OutputFolderName = "OutputCache";

		/// <summary>
		/// 构建输出的报告文件
		/// </summary>
		public const string ReportFileName = "BuildReport";

		/// <summary>
		/// Unity着色器资源包名称
		/// </summary>
		public const string UnityShadersBundleName = "unityshaders";

		/// <summary>
		/// 内置资源目录名称
		/// </summary>
		public const string StreamingAssetsBuildinFolder = "BuildinFiles";


		/// <summary>
		/// 忽略的文件类型
		/// </summary>
		public static readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };
	}
}