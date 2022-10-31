using UnityEngine;

namespace YooAsset
{
	internal static class YooAssetSettingsData
	{
		private static YooAssetSettings _setting = null;
		public static YooAssetSettings Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			_setting = Resources.Load<YooAssetSettings>("YooAssetSettings");
			if (_setting == null)
			{
				YooLogger.Log("YooAsset use default settings.");
				_setting = ScriptableObject.CreateInstance<YooAssetSettings>();
			}
			else
			{
				YooLogger.Log("YooAsset use user settings.");
			}
		}

		/// <summary>
		/// 获取构建报告文件名
		/// </summary>
		public static string GetReportFileName(string packageName, string packageVersion)
		{
			return $"{YooAssetSettings.ReportFileName}_{packageName}_{packageVersion}.json";
		}

		/// <summary>
		/// 获取补丁清单文件不带版本号的名称
		/// </summary>
		public static string GetPatchManifestFileNameWithoutVersion(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.bytes";
		}

		/// <summary>
		/// 获取补丁清单文件完整名称
		/// </summary>
		public static string GetPatchManifestFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.bytes";
		}

		/// <summary>
		/// 获取补丁清单哈希文件完整名称
		/// </summary>
		public static string GetPatchManifestHashFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.hash";
		}

		/// <summary>
		/// 获取补丁清单版本文件完整名称
		/// </summary>
		public static string GetPatchManifestVersionFileName(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.version";
		}

		/// <summary>
		/// 获取着色器资源包全名称（包含后缀名）
		/// </summary>
		public static string GetUnityShadersBundleFullName()
		{
			return $"{YooAssetSettings.UnityShadersBundleName}.{Setting.AssetBundleFileVariant}";
		}
	}
}