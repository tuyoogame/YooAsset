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
				YooLogger.Log("YooAsset use custom settings.");
			}
		}
	}
}