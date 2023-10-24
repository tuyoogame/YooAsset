using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace YooAsset
{
	internal abstract class ProviderBase
	{
		public enum EStatus
		{
			None = 0,
			CheckBundle,
			Loading,
			Checking,
			Succeed,
			Failed,
		}

		/// <summary>
		/// 资源提供者唯一标识符
		/// </summary>
		public string ProviderGUID { private set; get; }

		/// <summary>
		/// 所属资源系统
		/// </summary>
		public ResourceManager Impl { private set; get; }

		/// <summary>
		/// 资源信息
		/// </summary>
		public AssetInfo MainAssetInfo { private set; get; }

		/// <summary>
		/// 获取的资源对象
		/// </summary>
		public UnityEngine.Object AssetObject { protected set; get; }

		/// <summary>
		/// 获取的资源对象集合
		/// </summary>
		public UnityEngine.Object[] AllAssetObjects { protected set; get; }

		/// <summary>
		/// 获取的场景对象
		/// </summary>
		public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

		/// <summary>
		/// 原生文件路径
		/// </summary>
		public string RawFilePath { protected set; get; }


		/// <summary>
		/// 当前的加载状态
		/// </summary>
		public EStatus Status { protected set; get; } = EStatus.None;

		/// <summary>
		/// 最近的错误信息
		/// </summary>
		public string LastError { protected set; get; } = string.Empty;

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress { protected set; get; } = 0f;

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { private set; get; } = 0;

		/// <summary>
		/// 是否已经销毁
		/// </summary>
		public bool IsDestroyed { private set; get; } = false;

		/// <summary>
		/// 是否完毕（成功或失败）
		/// </summary>
		public bool IsDone
		{
			get
			{
				return Status == EStatus.Succeed || Status == EStatus.Failed;
			}
		}


		protected BundleLoaderBase OwnerBundle { private set; get; }
		protected DependAssetBundles DependBundles { private set; get; }
		protected bool IsWaitForAsyncComplete { private set; get; } = false;
		protected bool IsForceDestroyComplete { private set; get; } = false;
		private readonly List<HandleBase> _handles = new List<HandleBase>();


		public ProviderBase(ResourceManager impl, string providerGUID, AssetInfo assetInfo)
		{
			Impl = impl;
			ProviderGUID = providerGUID;
			MainAssetInfo = assetInfo;

			// 创建资源包加载器
			if (impl != null)
			{
				OwnerBundle = impl.CreateOwnerAssetBundleLoader(assetInfo);
				OwnerBundle.Reference();
				OwnerBundle.AddProvider(this);

				var dependList = impl.CreateDependAssetBundleLoaders(assetInfo);
				DependBundles = new DependAssetBundles(dependList);
				DependBundles.Reference();
			}
		}

		/// <summary>
		/// 轮询更新方法
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// 销毁资源提供者
		/// </summary>
		public void Destroy()
		{
			IsDestroyed = true;

			// 释放资源包加载器
			if (OwnerBundle != null)
			{
				OwnerBundle.Release();
				OwnerBundle = null;
			}
			if (DependBundles != null)
			{
				DependBundles.Release();
				DependBundles = null;
			}
		}

		/// <summary>
		/// 是否可以销毁
		/// </summary>
		public bool CanDestroy()
		{
			// 注意：在进行资源加载过程时不可以销毁
			if (Status == EStatus.Loading || Status == EStatus.Checking)
				return false;

			return RefCount <= 0;
		}

		/// <summary>
		/// 创建资源句柄
		/// </summary>
		public T CreateHandle<T>() where T : HandleBase
		{
			// 引用计数增加
			RefCount++;

			HandleBase handle;
			if (typeof(T) == typeof(AssetHandle))
				handle = new AssetHandle(this);
			else if (typeof(T) == typeof(SceneHandle))
				handle = new SceneHandle(this);
			else if (typeof(T) == typeof(SubAssetsHandle))
				handle = new SubAssetsHandle(this);
			else if (typeof(T) == typeof(AllAssetsHandle))
				handle = new AllAssetsHandle(this);
			else if (typeof(T) == typeof(RawFileHandle))
				handle = new RawFileHandle(this);
			else
				throw new System.NotImplementedException();

			_handles.Add(handle);
			return handle as T;
		}

		/// <summary>
		/// 释放资源句柄
		/// </summary>
		public void ReleaseHandle(HandleBase handle)
		{
			if (RefCount <= 0)
				throw new System.Exception("Should never get here !");

			if (_handles.Remove(handle) == false)
				throw new System.Exception("Should never get here !");

			// 引用计数减少
			RefCount--;
		}

		/// <summary>
		/// 释放所有资源句柄
		/// </summary>
		public void ReleaseAllHandles()
		{
			for (int i = _handles.Count - 1; i >= 0; i--)
			{
				var handle = _handles[i];
				handle.ReleaseInternal();
			}
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			IsWaitForAsyncComplete = true;

			// 注意：主动轮询更新完成同步加载
			Update();

			// 验证结果
			if (IsDone == false)
			{
				YooLogger.Warning($"{nameof(WaitForAsyncComplete)} failed to loading : {MainAssetInfo.AssetPath}");
			}
		}

		/// <summary>
		/// 强制销毁资源提供者
		/// </summary>
		public void ForceDestroyComplete()
		{
			IsForceDestroyComplete = true;

			// 注意：主动轮询更新完成同步加载
			// 说明：如果资源包未准备完毕也可以放心销毁。
			Update();
		}

		/// <summary>
		/// 处理特殊异常
		/// </summary>
		protected void ProcessCacheBundleException()
		{
			if (OwnerBundle.IsDestroyed)
				throw new System.Exception("Should never get here !");

			Status = EStatus.Failed;
			LastError = $"The bundle {OwnerBundle.MainBundleInfo.Bundle.BundleName} has been destroyed by unity bugs !";
			YooLogger.Error(LastError);
			InvokeCompletion();
		}

		/// <summary>
		/// 异步操作任务
		/// </summary>
		public Task Task
		{
			get
			{
				if (_taskCompletionSource == null)
				{
					_taskCompletionSource = new TaskCompletionSource<object>();
					if (IsDone)
						_taskCompletionSource.SetResult(null);
				}
				return _taskCompletionSource.Task;
			}
		}

		#region 异步编程相关
		private TaskCompletionSource<object> _taskCompletionSource;
		protected void InvokeCompletion()
		{
			DebugEndRecording();

			// 进度百分百完成
			Progress = 1f;

			// 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
			// 注意：回调方法如果发生异常，会阻断列表里的后续回调方法！
			List<HandleBase> tempers = new List<HandleBase>(_handles);
			foreach (var hande in tempers)
			{
				if (hande.IsValid)
				{
					hande.InvokeCallback();
				}
			}

			if (_taskCompletionSource != null)
				_taskCompletionSource.TrySetResult(null);
		}
		#endregion

		#region 调试信息相关
		/// <summary>
		/// 出生的场景
		/// </summary>
		public string SpawnScene = string.Empty;

		/// <summary>
		/// 出生的时间
		/// </summary>
		public string SpawnTime = string.Empty;

		/// <summary>
		/// 加载耗时（单位：毫秒）
		/// </summary>
		public long LoadingTime { protected set; get; }

		// 加载耗时统计
		private Stopwatch _watch = null;

		[Conditional("DEBUG")]
		public void InitSpawnDebugInfo()
		{
			SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; ;
			SpawnTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
		}
		private string SpawnTimeToString(float spawnTime)
		{
			float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
			float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
			float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
			return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
		}

		[Conditional("DEBUG")]
		protected void DebugBeginRecording()
		{
			if (_watch == null)
			{
				_watch = Stopwatch.StartNew();
			}
		}

		[Conditional("DEBUG")]
		private void DebugEndRecording()
		{
			if (_watch != null)
			{
				LoadingTime = _watch.ElapsedMilliseconds;
				_watch = null;
			}
		}

		/// <summary>
		/// 获取下载报告
		/// </summary>
		internal DownloadStatus GetDownloadStatus()
		{
			DownloadStatus status = new DownloadStatus();
			status.TotalBytes = (ulong)OwnerBundle.MainBundleInfo.Bundle.FileSize;
			status.DownloadedBytes = OwnerBundle.DownloadedBytes;
			foreach (var dependBundle in DependBundles.DependList)
			{
				status.TotalBytes += (ulong)dependBundle.MainBundleInfo.Bundle.FileSize;
				status.DownloadedBytes += dependBundle.DownloadedBytes;
			}

			if (status.TotalBytes == 0)
				throw new System.Exception("Should never get here !");

			status.IsDone = status.DownloadedBytes == status.TotalBytes;
			status.Progress = (float)status.DownloadedBytes / status.TotalBytes;
			return status;
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
		{
			var bundleInfo = new DebugBundleInfo();
			bundleInfo.BundleName = OwnerBundle.MainBundleInfo.Bundle.BundleName;
			bundleInfo.RefCount = OwnerBundle.RefCount;
			bundleInfo.Status = OwnerBundle.Status.ToString();
			output.Add(bundleInfo);

			DependBundles.GetBundleDebugInfos(output);
		}
		#endregion
	}
}