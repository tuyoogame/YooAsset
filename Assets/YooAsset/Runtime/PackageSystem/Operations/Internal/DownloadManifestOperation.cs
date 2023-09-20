
using UnityEngine;

namespace YooAsset
{
	internal class DownloadManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHashFile,
			DownloadManifestFile,
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
		private UnityWebFileRequester _downloader1;
		private UnityWebFileRequester _downloader2;
		private ESteps _steps = ESteps.None;

		internal DownloadManifestOperation(IRemoteServices remoteServices, string packageName, string packageVersion, bool appendTimeTicks, int timeout, int downloadFailedTryAgain)
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
			_steps = ESteps.DownloadPackageHashFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageHashFile)
			{
				if (_downloader1 == null)
				{
					string savePath = PersistentTools.GetPersistent(_packageName).GetSandboxPackageHashFilePath(_packageVersion);
					string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download package hash file : {webURL}");
					_downloader1 = new UnityWebFileRequester();
					_downloader1.SendRequest(webURL, savePath, _timeout);
				}

				_downloader1.CheckTimeout();
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					_steps = ESteps.TryAgain;
					_requestCount++;
					YooLogger.Warning($"Request download package hash failed : {_downloader1.GetError()}");
				}
				else
				{
					_steps = ESteps.DownloadManifestFile;
					_requestCount = 0;
				}

				_downloader1.Dispose();
				_downloader1 = null;
			}
			
			if (_steps == ESteps.TryAgain)
			{
				if (_requestCount <= _failedTryAgain)
				{
					_tryAgainTimer += Time.unscaledDeltaTime;
					if (_tryAgainTimer > 1f)
					{
						_steps = ESteps.DownloadPackageHashFile;
						Error = string.Empty;
						_tryAgainTimer = 0f;
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Request download package hash failed : reach max try again count : {_requestCount}";
				}
			}

			if (_steps == ESteps.DownloadManifestFile)
			{
				if (_downloader2 == null)
				{
					string savePath = PersistentTools.GetPersistent(_packageName).GetSandboxPackageManifestFilePath(_packageVersion);
					string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download package manifest file : {webURL}");
					_downloader2 = new UnityWebFileRequester();
					_downloader2.SendRequest(webURL, savePath, _timeout);
				}

				_downloader2.CheckTimeout();
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					_steps = ESteps.TryAgain;
					_requestCount++;
					YooLogger.Warning($"Request download package manifest file failed : {_downloader2.GetError()}");
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				_downloader2.Dispose();
				_downloader2 = null;
			}
			
			if (_steps == ESteps.TryAgain)
			{
				if (_requestCount <= _failedTryAgain)
				{
					_tryAgainTimer += Time.unscaledDeltaTime;
					if (_tryAgainTimer > 1f)
					{
						_steps = ESteps.DownloadManifestFile;
						Error = string.Empty;
						_tryAgainTimer = 0;
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Request download package manifest failed : reach max try again count : {_requestCount}";
				}
			}
		}

		private string GetDownloadRequestURL(string fileName)
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