using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	/// <summary>
	/// 普通的下载器
	/// </summary>
	internal sealed class FileGeneralDownloader : DownloaderBase
	{
		private enum ESteps
		{
			None,
			PrepareDownload,
			CreateDownloader,
			CheckDownload,
			VerifyTempFile,
			WaitingVerifyTempFile,
			CachingFile,
			TryAgain,
			Done,
		}

		private readonly string _tempFilePath;
		private VerifyTempFileOperation _verifyFileOp = null;
		private ESteps _steps = ESteps.None;


		public FileGeneralDownloader(BundleInfo bundleInfo, int failedTryAgain, int timeout) : base(bundleInfo, failedTryAgain, timeout)
		{
			_tempFilePath = bundleInfo.Bundle.TempDataFilePath;
		}
		public override void SendRequest(params object[] param)
		{
			if (_steps == ESteps.None)
			{
				_steps = ESteps.PrepareDownload;
			}
		}
		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			// 准备下载
			if (_steps == ESteps.PrepareDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;

				// 重置变量
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;
				_tryAgainTimer = 0f;

				// 删除临时文件
				if (File.Exists(_tempFilePath))
					File.Delete(_tempFilePath);

				// 获取请求地址
				_requestURL = GetRequestURL();
				_steps = ESteps.CreateDownloader;
			}

			// 创建下载器
			if (_steps == ESteps.CreateDownloader)
			{
				_webRequest = DownloadSystem.NewRequest(_requestURL);
				DownloadHandlerFile handler = new DownloadHandlerFile(_tempFilePath);
				handler.removeFileOnAbort = true;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				_webRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _webRequest.downloadProgress;
				_downloadedBytes = _webRequest.downloadedBytes;
				if (_webRequest.isDone == false)
				{
					CheckTimeout();
					return;
				}

				bool hasError = false;

				// 检查网络错误
#if UNITY_2020_3_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
				{
					hasError = true;
					_lastError = _webRequest.error;
					_lastCode = _webRequest.responseCode;
				}
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
				{
					hasError = true;
					_lastError = _webRequest.error;
					_lastCode = _webRequest.responseCode;
				}
#endif

				// 如果网络异常
				if (hasError)
				{
					// 下载失败之后删除文件
					if (File.Exists(_tempFilePath))
						File.Delete(_tempFilePath);

					_steps = ESteps.TryAgain;
				}
				else
				{
					_steps = ESteps.VerifyTempFile;
				}

				// 最终释放下载器
				DisposeWebRequest();
			}

			// 验证下载文件
			if (_steps == ESteps.VerifyTempFile)
			{
				VerifyTempFileElement element = new VerifyTempFileElement(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileCRC, _bundleInfo.Bundle.FileSize);
				_verifyFileOp = VerifyTempFileOperation.CreateOperation(element);
				OperationSystem.StartOperation(_verifyFileOp);
				_steps = ESteps.WaitingVerifyTempFile;
			}

			// 等待验证完成
			if (_steps == ESteps.WaitingVerifyTempFile)
			{
				if (WaitForAsyncComplete)
					_verifyFileOp.Update();

				if (_verifyFileOp.IsDone == false)
					return;

				if (_verifyFileOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.CachingFile;
				}
				else
				{
					if (File.Exists(_tempFilePath))
						File.Delete(_tempFilePath);

					_lastError = _verifyFileOp.Error;
					_steps = ESteps.TryAgain;
				}
			}

			// 缓存下载文件
			if (_steps == ESteps.CachingFile)
			{
				try
				{
					CachingFile(_tempFilePath);
					_status = EStatus.Succeed;
					_steps = ESteps.Done;
					_lastError = string.Empty;
					_lastCode = 0;
				}
				catch (Exception e)
				{
					_lastError = e.Message;
					_steps = ESteps.TryAgain;
				}
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				if (_failedTryAgain <= 0)
				{
					ReportError();
					_status = EStatus.Failed;
					_steps = ESteps.Done;
					return;
				}

				_tryAgainTimer += Time.unscaledDeltaTime;
				if (_tryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.PrepareDownload;
					ReportWarning();
					YooLogger.Warning($"Try again download : {_requestURL}");
				}
			}
		}
		public override void Abort()
		{
			if (IsDone() == false)
			{
				_status = EStatus.Failed;
				_steps = ESteps.Done;
				_lastError = "user abort";
				_lastCode = 0;
				DisposeWebRequest();
			}
		}

		private void DisposeWebRequest()
		{
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
			}
		}
	}
}