using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 更新静态版本操作
	/// </summary>
	public abstract class UpdateStaticVersionOperation : AsyncOperationBase
	{
		/// <summary>
		/// 包裹文件的哈希值
		/// </summary>
		public string PackageCRC { protected set; get; } = string.Empty;
	}

	/// <summary>
	/// 编辑器下模拟运行的更新静态版本操作
	/// </summary>
	internal sealed class EditorPlayModeUpdateStaticVersionOperation : UpdateStaticVersionOperation
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
	/// 离线模式的更新静态版本操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdateStaticVersionOperation : UpdateStaticVersionOperation
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
	/// 联机模式的更新静态版本操作
	/// </summary>
	internal sealed class HostPlayModeUpdateStaticVersionOperation : UpdateStaticVersionOperation
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

		internal HostPlayModeUpdateStaticVersionOperation(HostPlayModeImpl impl, string packageName, int timeout)
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
				string versionFileName = YooAssetSettingsData.GetStaticVersionFileName(_packageName);
				string webURL = GetStaticVersionRequestURL(versionFileName);
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
					string packageCRC = _downloader.GetText();
					if(string.IsNullOrEmpty(packageCRC))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"URL : {_downloader.URL} Error : static version content is empty.";
					}
					else
					{
						PackageCRC = packageCRC;
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