using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class YooAssetEditorSettingsData
	{
		private static YooAssetEditorSettings _setting = null;
		public static YooAssetEditorSettings Setting
		{
			get
			{
				if (_setting == null)
					LoadEditorSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadEditorSettingData()
		{
			var guids = AssetDatabase.FindAssets($"t:{nameof(YooAssetEditorSettings)}", new[] { "Assets", "Packages" });
			if (guids.Length == 0)
				throw new System.Exception($"Not found {nameof(YooAssetEditorSettings)} file !");
			if (guids.Length != 1)
				throw new System.Exception($"Found multiple {nameof(YooAssetEditorSettings)} files !");

			string settingFilePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			_setting = AssetDatabase.LoadAssetAtPath<YooAssetEditorSettings>(settingFilePath);
			Debug.Log($"Load {nameof(YooAssetEditorSettings)}.asset ok !");
		}
	}
}