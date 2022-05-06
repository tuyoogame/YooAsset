using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class DatabaseAllAssetsProvider : ProviderBase
	{
		private string _bundleName;

		public override float Progress
		{
			get
			{
				if (IsDone)
					return 100f;
				else
					return 0;
			}
		}

		public DatabaseAllAssetsProvider(string assetPath, System.Type assetType)
			: base(assetPath, assetType)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(AssetPath);
				if (string.IsNullOrEmpty(guid))
				{
					Status = EStatus.Fail;
					LastError = $"Not found asset : {AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
					return;
				}

				// 获取资源包名称
				_bundleName = AssetSystem.BundleServices.GetBundleName(AssetPath);
				if (string.IsNullOrEmpty(_bundleName))
				{
					Status = EStatus.Fail;
					LastError = $"Not found bundle name : {AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				bool loadFailed = false;
				if (AssetType == null)
				{
					List<UnityEngine.Object> result = new List<Object>(100);
					AssetInfo[] allAssetInfos = AssetSystem.BundleServices.GetAssetInfos(_bundleName);
					foreach (var assetInfo in allAssetInfos)
					{
						var assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetInfo.AssetPath);
						if (assetObject != null)
						{
							result.Add(assetObject);
						}
						else
						{
							YooLogger.Warning($"Failed to load main asset : {assetInfo.AssetPath}");
							loadFailed = true;
							break;
						}
					}
					if (loadFailed == false)
						AllAssetObjects = result.ToArray();
				}
				else
				{
					List<UnityEngine.Object> result = new List<Object>(100);
					AssetInfo[] allAssetInfos = AssetSystem.BundleServices.GetAssetInfos(_bundleName);
					foreach (var assetInfo in allAssetInfos)
					{
						var assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.AssetPath, AssetType);
						if (assetObject != null)
						{
							if (AssetType.IsAssignableFrom(assetObject.GetType()))
								result.Add(assetObject);
						}
						else
						{
							YooLogger.Warning($"Failed to load asset : {assetInfo.AssetPath}");
							loadFailed = true;
							break;
						}
					}
					if (loadFailed == false)
						AllAssetObjects = result.ToArray();
				}
				Status = EStatus.Checking;
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Status = AllAssetObjects == null ? EStatus.Fail : EStatus.Success;
				if (Status == EStatus.Fail)
				{
					LastError = $"Failed to load all assets : {nameof(AssetType)} in bundle {_bundleName}";
					YooLogger.Error(LastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}