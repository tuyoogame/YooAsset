
namespace YooAsset
{
	internal class DownloadManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHash,
			DownloadManifest,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly int _timeout;
		private UnityWebFileRequester _downloader1;
		private UnityWebFileRequester _downloader2;
		private ESteps _steps = ESteps.None;

		internal DownloadManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.DownloadPackageHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageHash)
			{
				if (_downloader1 == null)
				{
					string savePath = PersistentHelper.GetCachePackageHashFilePath(_packageName, _packageVersion);
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
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					_steps = ESteps.DownloadManifest;
				}

				_downloader1.Dispose();
			}

			if (_steps == ESteps.DownloadManifest)
			{
				if (_downloader2 == null)
				{
					string savePath = PersistentHelper.GetCacheManifestFilePath(_packageName, _packageVersion);
					string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download manifest file : {webURL}");
					_downloader2 = new UnityWebFileRequester();
					_downloader2.SendRequest(webURL, savePath, _timeout);
				}

				_downloader2.CheckTimeout();
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				_downloader2.Dispose();
			}
		}

		private string GetDownloadRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _impl.GetPatchDownloadFallbackURL(fileName);
			else
				return _impl.GetPatchDownloadMainURL(fileName);
		}
	}
}