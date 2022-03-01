using UnityEngine;

namespace YooAsset
{
	public static class ResourceSettingData
	{
		private static ResourceSetting _setting = null;
		public static ResourceSetting Setting
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
			_setting = Resources.Load<ResourceSetting>("YooAssetSetting");
			if (_setting == null)
			{
				Debug.Log("Use YooAsset default resource setting.");
				_setting = ScriptableObject.CreateInstance<ResourceSetting>();
			}
			else
			{
				Debug.Log("Use YooAsset custom resource setting.");
			}
		}
	}
}