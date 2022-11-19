using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public static partial class YooAssets
	{
		private static AssetsPackage _defaultPackage;

		/// <summary>
		/// 设置默认的资源包
		/// </summary>
		public static void SetDefaultAssetsPackage(AssetsPackage assetsPackage)
		{
			_defaultPackage = assetsPackage;
		}

		#region 资源信息
		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool IsNeedDownloadFromRemote(string location)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.IsNeedDownloadFromRemote(location);
		}

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.IsNeedDownloadFromRemote(assetInfo);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		public static AssetInfo[] GetAssetInfos(string tag)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.GetAssetInfos(tag);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		public static AssetInfo[] GetAssetInfos(string[] tags)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static AssetInfo GetAssetInfo(string location)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.GetAssetInfo(location);
		}

		/// <summary>
		/// 检查资源定位地址是否有效
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool CheckLocationValid(string location)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CheckLocationValid(location);
		}
		#endregion

		#region 原生文件
		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadRawFileSync(assetInfo);
		}

		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static RawFileOperationHandle LoadRawFileSync(string location)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadRawFileSync(location);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadRawFileAsync(assetInfo);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static RawFileOperationHandle LoadRawFileAsync(string location)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadRawFileAsync(location);
		}
		#endregion

		#region 场景加载
		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetSync(assetInfo);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetSync<TObject>(location);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetSync(location, type);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetAsync(assetInfo);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetAsync<TObject>(location);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadAssetAsync(location, type);
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsSync(assetInfo);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsSync<TObject>(location);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsSync(location, type);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsAsync(assetInfo);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsAsync<TObject>(location);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.LoadSubAssetsAsync(location, type);
		}
		#endregion

		#region 资源下载
		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchDownloader(tags, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchDownloader(downloadingMaxNumber, failedTryAgain);
		}


		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源定位列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreateBundleDownloader(locations, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchUnpacker(tag, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchUnpacker(tags, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.CreatePatchUnpacker(unpackingMaxNumber, failedTryAgain);
		}
		#endregion

		#region 包裹更新
		/// <summary>
		/// 资源包裹更新，用于下载更新指定资源版本所有的资源包文件
		/// </summary>
		/// <param name="packageVersion">指定更新的包裹版本</param>
		/// <param name="timeout">超时时间</param>
		public static DownloadPackageOperation DownloadPackageAsync(string packageVersion, int timeout = 60)
		{
			DebugCheckDefaultPackageValid();
			return _defaultPackage.DownloadPackageAsync(packageVersion, timeout);
		}
		#endregion

		#region 调试方法
		[Conditional("DEBUG")]
		private static void DebugCheckDefaultPackageValid()
		{
			if (_defaultPackage == null)
				throw new Exception($"Default package is null. Please use {nameof(YooAssets.SetDefaultAssetsPackage)} !");
		}
		#endregion
	}
}