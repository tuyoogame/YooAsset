
using UnityEngine;

namespace YooAsset
{
	internal class LoadRemoteManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHashFile,
			DownloadManifestFile,
			TryAgain,
			VerifyFileHash,
			CheckDeserializeManifest,
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
		private QueryRemotePackageHashOperation _queryRemotePackageHashOp;
		private UnityWebDataRequester _downloader;
		private DeserializeManifestOperation _deserializer;
		private byte[] _fileData;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 加载的清单实例
		/// </summary>
		public PackageManifest Manifest { private set; get; }


		internal LoadRemoteManifestOperation(IRemoteServices remoteServices, string packageName, string packageVersion, bool appendTimeTicks, int timeout, int downloadFailedTryAgain)
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
				if (_queryRemotePackageHashOp == null)
				{
					_queryRemotePackageHashOp = new QueryRemotePackageHashOperation(_remoteServices, _packageName, _packageVersion, _appendTimeTicks, _timeout, _failedTryAgain);
					OperationSystem.StartOperation(_queryRemotePackageHashOp);
				}

				if (_queryRemotePackageHashOp.IsDone == false)
					return;

				if (_queryRemotePackageHashOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.DownloadManifestFile;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _queryRemotePackageHashOp.Error;
				}
			}

			if (_steps == ESteps.DownloadManifestFile)
			{
				if (_downloader == null)
				{
					string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download manifest file : {webURL}");
					_downloader = new UnityWebDataRequester();
					_downloader.SendRequest(webURL, _timeout);
				}

				_downloader.CheckTimeout();
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.TryAgain;
					_requestCount++;
					YooLogger.Warning($"Request download manifest file failed : {_downloader.URL}, {_downloader.GetError()}");
				}
				else
				{
					_fileData = _downloader.GetData();
					_steps = ESteps.VerifyFileHash;
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
						_steps = ESteps.DownloadManifestFile;
						Error = string.Empty;
						_tryAgainTimer = 0f;
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Request download manifest file failed : reach max try again count : {_requestCount}";
				}
			}

			if (_steps == ESteps.VerifyFileHash)
			{
				string fileHash = HashUtility.BytesMD5(_fileData);
				if (fileHash != _queryRemotePackageHashOp.PackageHash)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Failed to verify remote manifest file hash !";
				}
				else
				{
					_deserializer = new DeserializeManifestOperation(_fileData);
					OperationSystem.StartOperation(_deserializer);
					_steps = ESteps.CheckDeserializeManifest;
				}
			}

			if (_steps == ESteps.CheckDeserializeManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone == false)
					return;

				if (_deserializer.Status == EOperationStatus.Succeed)
				{
					Manifest = _deserializer.Manifest;
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _deserializer.Error;
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