using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class DatabaseAssetProvider : AssetProviderBase
	{
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

		public DatabaseAssetProvider(string assetPath, System.Type assetType)
			: base(assetPath, assetType)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(AssetPath);
				if (string.IsNullOrEmpty(guid))
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
					return;
				}
				else
				{
					States = EAssetStates.Loading;
				}
			
				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				AssetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(AssetPath, AssetType);
				States = EAssetStates.Checking;
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				States = AssetObject == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					Logger.Warning($"Failed to load asset object : {AssetPath}");
				InvokeCompletion();
			}
#endif
		}
	}
}