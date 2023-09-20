
using UnityEngine;

namespace YooAsset
{
	internal class QueryRemotePackageHashOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHash,
			TryAgain,
			Done,
		}
		
		private int _requestCount = 0;
		private float _tryAgainTimer;
		private readonly IRemoteServices _remoteServices;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly bool _appendTimeTicks;
		private readonly int _timeout;
		private readonly int _failedTryAgain;
		private UnityWebDataRequester _downloader;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹哈希值
		/// </summary>
		public string PackageHash { private set; get; }


		public QueryRemotePackageHashOperation(IRemoteServices remoteServices, string packageName, string packageVersion, bool appendTimeTicks, int timeout, int downloadFailedTryAgain)
		{
			_remoteServices = remoteServices;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_appendTimeTicks = appendTimeTicks;
			_timeout = timeout;
			_failedTryAgain = downloadFailedTryAgain;
		}
		internal override void Start()
		{
			_steps = ESteps.DownloadPackageHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageHash)
			{
				if (_downloader == null)
				{
					string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, _packageVersion);
					string webURL = GetPackageHashRequestURL(fileName);
					YooLogger.Log($"Beginning to request package hash : {webURL}");
					_downloader = new UnityWebDataRequester();
					_downloader.SendRequest(webURL, _timeout);
				}

				Progress = _downloader.Progress();
				_downloader.CheckTimeout();
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.TryAgain;
					_requestCount++;
					YooLogger.Warning($"Request package hash failed : {_downloader.URL}, {_downloader.GetError()}");
				}
				else
				{
					PackageHash = _downloader.GetText();
					if (string.IsNullOrEmpty(PackageHash))
					{
						_steps = ESteps.TryAgain;
						_requestCount++;
						YooLogger.Warning($"Remote package hash is empty : {_downloader.URL}");
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
						_steps = ESteps.DownloadPackageHash;
						Error = string.Empty;
						_tryAgainTimer = 0f;
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Request package hash failed : reach max try again count : {_requestCount}";
				}
			}
		}

		private string GetPackageHashRequestURL(string fileName)
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