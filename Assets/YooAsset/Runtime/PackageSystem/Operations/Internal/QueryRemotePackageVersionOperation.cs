
using UnityEngine;

namespace YooAsset
{
	internal class QueryRemotePackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageVersion,
			TryAgain,
			Done,
		}

		private int _requestCount = 0;
		private float _tryAgainTimer;
		private readonly IRemoteServices _remoteServices;
		private readonly string _packageName;
		private readonly bool _appendTimeTicks;
		private readonly int _timeout;
		private readonly int _failedTryAgain;
		private UnityWebDataRequester _downloader;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }
		

		public QueryRemotePackageVersionOperation(IRemoteServices remoteServices, string packageName, bool appendTimeTicks, int timeout, int downloadFailedTryAgain)
		{
			_remoteServices = remoteServices;
			_packageName = packageName;
			_appendTimeTicks = appendTimeTicks;
			_timeout = timeout;
			_failedTryAgain = downloadFailedTryAgain;
		}
		internal override void Start()
		{
			_steps = ESteps.DownloadPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageVersion)
			{
				if (_downloader == null)
				{
					string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
					string webURL = GetPackageVersionRequestURL(fileName);
					YooLogger.Log($"Beginning to request package version : {webURL}");
					_downloader = new UnityWebDataRequester();
					_downloader.SendRequest(webURL, _timeout);
				}

				Progress = _downloader.Progress();
				_downloader.CheckTimeout();
				if (_downloader.IsDone() == false)
				{
					return;
				}

				if (_downloader.HasError())
				{
					_steps = ESteps.TryAgain;
					_requestCount++;
					YooLogger.Warning($"Request package version failed : {_downloader.URL}, {_downloader.GetError()}");
				}
				else
				{
					PackageVersion = _downloader.GetText();
					if (string.IsNullOrEmpty(PackageVersion))
					{
						_steps = ESteps.TryAgain;
						_requestCount++;
						YooLogger.Warning($"Remote package version is empty : {_downloader.URL}");
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
				
				_downloader.Dispose();
				_downloader = null;
			}

			if (_steps == ESteps.TryAgain)
			{
				if (_requestCount <= _failedTryAgain)
				{
					_tryAgainTimer += Time.unscaledDeltaTime;
					if (_tryAgainTimer > 1f)
					{
						_steps = ESteps.DownloadPackageVersion;
						Error = string.Empty;
						_tryAgainTimer = 0f;
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Request package version failed : reach max try again count : {_requestCount}";
				}
			}
		}

		private string GetPackageVersionRequestURL(string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (_requestCount % 2 == 0)
				url = _remoteServices.GetRemoteMainURL(fileName);
			else
				url = _remoteServices.GetRemoteFallbackURL(fileName);

			// 在URL末尾添加时间戳
			if (_appendTimeTicks)
				return $"{url}?{System.DateTime.UtcNow.Ticks}";
			else
				return url;
		}
	}
}