using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class DatabaseSubAssetsProvider : AssetProviderBase
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

		public DatabaseSubAssetsProvider(string assetPath, System.Type assetType)
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
					InvokeCompletion();
					return;
				}
				else
				{
					Status = EStatus.Loading;
				}		

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				var findAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(AssetPath);
				List<UnityEngine.Object> result = new List<Object>(findAssets.Length);
				foreach (var findObj in findAssets)
				{
					if (findObj.GetType() == AssetType)
						result.Add(findObj);
				}
				AllAssets = result.ToArray();
				Status = EStatus.Checking;
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Status = AllAssets == null ? EStatus.Fail : EStatus.Success;
				if (Status == EStatus.Fail)
					YooLogger.Warning($"Failed to load all asset object : {AssetPath}");
				InvokeCompletion();
			}
#endif
		}
	}
}