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
		public static string GetReportFileName(string packageName, string packageCRC)
		{
			return $"{YooAssetSettings.ReportFileName}_{packageName}_{packageCRC}.json";
		}

		/// <summary>
		/// 获取补丁清单文件完整名称
		/// </summary>
		public static string GetPatchManifestFileName(string packageName, string packageCRC)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageCRC}.bytes";
		}

		/// <summary>
		/// 获取补丁清单文件临时名称
		/// </summary>
		public static string GetPatchManifestTempFileName(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.temp";
		}

		/// <summary>
		/// 获取静态版本文件名称
		/// </summary>
		public static string GetStaticVersionFileName(string packageName)
		{
			return $"{YooAssetSettings.VersionFileName}_{packageName}.bytes";
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