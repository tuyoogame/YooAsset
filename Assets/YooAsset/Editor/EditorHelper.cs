using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class EditorHelper
	{
#if UNITY_2019_4_OR_NEWER
		private readonly static Dictionary<System.Type, string> _uxmlDic = new Dictionary<System.Type, string>();

		/// <summary>
		/// 加载窗口的布局文件
		/// </summary>
		public static UnityEngine.UIElements.VisualTreeAsset LoadWindowUXML<TWindow>() where TWindow : class
		{
			var windowType = typeof(TWindow);

			// 缓存里查询并加载
			if (_uxmlDic.TryGetValue(windowType, out string uxmlGUID))
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGUID);
				if (string.IsNullOrEmpty(assetPath))
				{
					_uxmlDic.Clear();
					throw new System.Exception($"Invalid UXML GUID : {uxmlGUID} ! Please close the window and open it again !");
				}
				var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
				return treeAsset;
			}

			// 全局搜索并加载
			string[] guids = AssetDatabase.FindAssets(windowType.Name);
			if (guids.Length == 0)
				throw new System.Exception($"Not found any assets : {windowType.Name}");

			foreach (string assetGUID in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
				var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.UIElements.VisualTreeAsset))
				{
					_uxmlDic.Add(windowType, assetGUID);
					var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
					return treeAsset;
				}
			}
			throw new System.Exception($"Not found UXML file : {windowType.Name}");
		}
#endif

		/// <summary>
		/// 加载相关的配置文件
		/// </summary>
		public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
		{
			var settingType = typeof(TSetting);
			var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
			if (guids.Length == 0)
			{
				Debug.LogWarning($"Create new {settingType.Name}.asset");
				var setting = ScriptableObject.CreateInstance<TSetting>();
				string filePath = $"Assets/{settingType.Name}.asset";
				AssetDatabase.CreateAsset(setting, filePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				return setting;
			}
			else
			{
				if (guids.Length != 1)
				{
					foreach (var guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						Debug.LogWarning($"Found multiple file : {path}");
					}
					throw new System.Exception($"Found multiple {settingType.Name} files !");
				}

				string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
				var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
				return setting;
			}
		}
	}
}