using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 请求远端包裹的最新版本
	/// </summary>
	public abstract class UpdatePackageVersionOperation : AsyncOperationBase
	{
		/// <summary>
		/// 当前最新的包裹版本
		/// </summary>
		public string PackageVersion { protected set; get; }
	}

	/// <summary>
	/// 编辑器下模拟模式的请求远端包裹的最新版本
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的请求远端包裹的最新版本
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 联机模式的请求远端包裹的最新版本
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
	{
		private enum ESteps
		{
			None,
			LoadStaticVersion,
			CheckStaticVersion,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloader;

		internal HostPlayModeUpdatePackageVersionOperation(HostPlayModeImpl impl, string packageName, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadStaticVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadStaticVersion)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(_packageName);
				string webURL = GetStaticVersionRequestURL(fileName);
				YooLogger.Log($"Beginning to request static version : {webURL}");
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckStaticVersion;
			}

			if (_steps == ESteps.CheckStaticVersion)
			{
				Progress = _downloader.Progress();
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					PackageVersion = _downloader.GetText();
					if (string.IsNullOrEmpty(PackageVersion))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Static package version is empty : {_downloader.URL}";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
				_downloader.Dispose();
			}
		}

		private string GetStaticVersionRequestURL(string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				url = _impl.GetPatchDownloadFallbackURL(fileName);
			else
				url = _impl.GetPatchDownloadMainURL(fileName);

			// 注意：在URL末尾添加时间戳
			return $"{url}?{System.DateTime.UtcNow.Ticks}";
		}
	}
}