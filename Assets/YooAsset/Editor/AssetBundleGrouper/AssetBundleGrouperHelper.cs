using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleGrouperHelper
	{
		/// <summary>
		/// 收集着色器的资源包名称
		/// </summary>
		public static string CollectShaderBundleName(string assetPath)
		{
			// 如果自动收集所有的着色器
			if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
				{
					string bundleName = AssetBundleGrouperSettingData.Setting.ShadersBundleName;
					return CorrectBundleName(bundleName, false);
				}
			}
			return null;
		}

		/// <summary>
		/// 修正资源包名称
		/// </summary>
		public static string CorrectBundleName(string bundleName, bool isRawBundle)
		{
			if (isRawBundle)
			{
				string fullName = $"{bundleName}.{YooAssetSettingsData.Setting.RawFileVariant}";
				return EditorTools.GetRegularPath(fullName).ToLower();
			}
			else
			{
				string fullName = $"{bundleName}.{YooAssetSettingsData.Setting.AssetBundleFileVariant}";
				return EditorTools.GetRegularPath(fullName).ToLower(); ;
			}
		}


		#region 编辑器下运行时支持
		private static readonly Dictionary<string, CollectAssetInfo> _locationDic = new Dictionary<string, CollectAssetInfo>(1000);

		public static void InitEditorPlayMode(bool enableAddressable)
		{
			_locationDic.Clear();

			if (enableAddressable)
			{
				var collectAssetList = AssetBundleGrouperSettingData.Setting.GetAllCollectAssets();
				foreach (var collectAsset in collectAssetList)
				{
					if (collectAsset.NotWriteToAssetList)
						continue;

					string address = collectAsset.Address;
					if (_locationDic.ContainsKey(address))
						UnityEngine.Debug.LogWarning($"Address have existed : {address}");
					else
						_locationDic.Add(address, collectAsset);
				}
			}
			else
			{
				var collectAssetList = AssetBundleGrouperSettingData.Setting.GetAllCollectAssets();
				foreach (var collectAsset in collectAssetList)
				{
					if (collectAsset.NotWriteToAssetList)
						continue;

					// 添加原始路径
					string assetPath = collectAsset.AssetPath;
					if (_locationDic.ContainsKey(assetPath))
						UnityEngine.Debug.LogWarning($"Asset path have existed : {assetPath}");
					else
						_locationDic.Add(assetPath, collectAsset);

					// 添加去掉后缀名的路径
					if (Path.HasExtension(assetPath))
					{
						string assetPathWithoutExtension = StringUtility.RemoveExtension(assetPath);
						if (_locationDic.ContainsKey(assetPathWithoutExtension))
							UnityEngine.Debug.LogWarning($"Asset path have existed : {assetPathWithoutExtension}");
						else
							_locationDic.Add(assetPathWithoutExtension, collectAsset);
					}
				}
			}
		}
		public static string ConvertLocationToAssetPath(string location)
		{
			// 检测地址合法性
			CheckLocation(location);

			if (_locationDic.ContainsKey(location))
			{
				return _locationDic[location].AssetPath;
			}
			else
			{
				UnityEngine.Debug.LogWarning($"Not found asset in grouper setting : {location}");
				return string.Empty;
			}
		}
		private static void CheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				UnityEngine.Debug.LogError("location param is null or empty!");
			}
			else
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ");
				if (index != -1)
				{
					if (location.Length == index + 1)
						UnityEngine.Debug.LogWarning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
					UnityEngine.Debug.LogWarning($"Found illegal character in location : \"{location}\"");
			}
		}
		#endregion
	}
}