using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 编辑器下运行时支持
	/// </summary>
	public static class AssetBundleGrouperRuntimeSupport
	{
		private static readonly Dictionary<string, CollectAssetInfo> _locationDic = new Dictionary<string, CollectAssetInfo>(1000);

		public static void InitEditorPlayMode(bool enableAddressable)
		{
			_locationDic.Clear();

			if (enableAddressable)
			{
				var collectAssetList = AssetBundleGrouperSettingData.Setting.GetAllCollectAssets();
				foreach (var collectAsset in collectAssetList)
				{
					if(collectAsset.CollectorType == ECollectorType.MainAssetCollector)
					{
						string address = collectAsset.Address;
						if (_locationDic.ContainsKey(address))
							throw new Exception($"The address is existed : {address} in grouper setting.");
						else
							_locationDic.Add(address, collectAsset);
					}
				}
			}
			else
			{
				var collectAssetList = AssetBundleGrouperSettingData.Setting.GetAllCollectAssets();
				foreach (var collectAsset in collectAssetList)
				{
					if (collectAsset.CollectorType == ECollectorType.MainAssetCollector)
					{
						// 添加原始路径
						// 注意：我们不允许原始路径存在重名
						string assetPath = collectAsset.AssetPath;
						if (_locationDic.ContainsKey(assetPath))
							throw new Exception($"Asset path have existed : {assetPath}");
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
	}
}