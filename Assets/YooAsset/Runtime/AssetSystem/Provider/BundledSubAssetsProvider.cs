using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class BundledSubAssetsProvider : BundledProvider
	{
		private AssetBundleRequest _cacheRequest;
		public override float Progress
		{
			get
			{
				if (_cacheRequest == null)
					return 0;
				return _cacheRequest.progress;
			}
		}

		public BundledSubAssetsProvider(string assetPath, System.Type assetType)
			: base(assetPath, assetType)
		{
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					DependBundles.WaitForAsyncComplete();
					OwnerBundle.WaitForAsyncComplete();
				}

				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.CacheBundle == null)
				{
					Status = EStatus.Fail;
					InvokeCompletion();
				}
				else
				{
					Status = EStatus.Loading;
				}
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (IsWaitForAsyncComplete)
				{
					if (AssetType == null)
						AllAssets = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(AssetName);
					else
						AllAssets = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(AssetName, AssetType);
				}
				else
				{
					if (AssetType == null)
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(AssetName);
					else
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(AssetName, AssetType);
				}
				Status = EStatus.Checking;
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_cacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						YooLogger.Warning("Suspend the main thread to load unity asset.");
						AllAssets = _cacheRequest.allAssets;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						AllAssets = _cacheRequest.allAssets;
					}
				}

				Status = AllAssets == null ? EStatus.Fail : EStatus.Success;
				if (Status == EStatus.Fail)
					YooLogger.Warning($"Failed to load sub assets : {AssetName} from bundle : {OwnerBundle.BundleFileInfo.BundleName}");
				InvokeCompletion();
			}
		}
	}
}