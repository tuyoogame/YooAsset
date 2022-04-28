using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class ShaderVariantCollectorSettingData
	{
		private static ShaderVariantCollectorSetting _setting = null;
		public static ShaderVariantCollectorSetting Setting
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
			// 加载配置文件
			_setting = AssetDatabase.LoadAssetAtPath<ShaderVariantCollectorSetting>(EditorDefine.ShaderVariantCollectorSettingFilePath);
			if (_setting == null)
			{
				Debug.LogWarning($"Create new {nameof(ShaderVariantCollectorSetting)}.asset : {EditorDefine.ShaderVariantCollectorSettingFilePath}");
				_setting = ScriptableObject.CreateInstance<ShaderVariantCollectorSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.ShaderVariantCollectorSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.ShaderVariantCollectorSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(ShaderVariantCollectorSetting)}.asset ok");
			}
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(ShaderVariantCollectorSetting)}.asset is saved!");
			}
		}
	}
}