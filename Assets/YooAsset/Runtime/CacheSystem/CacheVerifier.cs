using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
	/// <summary>
	/// 本地缓存文件验证器
	/// </summary>
	internal abstract class CacheVerifier
	{
		public abstract void InitVerifier(List<VerifyInfo> verifyInfos);
		public abstract bool UpdateVerifier();
		public abstract float GetVerifierProgress();

		public List<VerifyInfo> VerifySuccessList { protected set; get; }
		public List<VerifyInfo> VerifyFailList { protected set; get; }
	}

	/// <summary>
	/// 本地缓存文件验证器（线程版）
	/// </summary>
	internal class CacheVerifierWithThread : CacheVerifier
	{
		private readonly ThreadSyncContext _syncContext = new ThreadSyncContext();
		private readonly List<VerifyInfo> _waitingList = new List<VerifyInfo>(1000);
		private readonly List<VerifyInfo> _verifyingList = new List<VerifyInfo>(100);
		private int _verifyMaxNum;
		private int _verifyTotalCount;

		public override void InitVerifier(List<VerifyInfo> verifyInfos)
		{
			_waitingList.AddRange(verifyInfos);
			VerifySuccessList = new List<VerifyInfo>(verifyInfos.Count);
			VerifyFailList = new List<VerifyInfo>(verifyInfos.Count);

			// 设置同时验证的最大数
			ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
			YooLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
			_verifyMaxNum = Math.Min(workerThreads, ioThreads);
			_verifyTotalCount = _waitingList.Count;
			if (_verifyMaxNum < 1)
				_verifyMaxNum = 1;
		}
		public override bool UpdateVerifier()
		{
			_syncContext.Update();

			if (_waitingList.Count == 0 && _verifyingList.Count == 0)
				return true;

			if (_verifyingList.Count >= _verifyMaxNum)
				return false;

			for (int i = _waitingList.Count - 1; i >= 0; i--)
			{
				if (_verifyingList.Count >= _verifyMaxNum)
					break;

				var verifyIno = _waitingList[i];
				if (VerifyFileWithThread(verifyIno))
				{
					_waitingList.RemoveAt(i);
					_verifyingList.Add(verifyIno);
				}
				else
				{
					YooLogger.Warning("The thread pool is failed queued.");
					break;
				}
			}

			return false;
		}
		public override float GetVerifierProgress()
		{
			if (_verifyTotalCount == 0)
				return 1f;
			return (float)(VerifySuccessList.Count + VerifyFailList.Count) / _verifyTotalCount;
		}

		private bool VerifyFileWithThread(VerifyInfo verifyInfo)
		{
			return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), verifyInfo);
		}
		private void VerifyInThread(object infoObj)
		{
			VerifyInfo verifyInfo = (VerifyInfo)infoObj;
			verifyInfo.Result = CacheSystem.VerifyBundle(verifyInfo.VerifyBundle, CacheSystem.InitVerifyLevel);
			_syncContext.Post(VerifyCallback, verifyInfo);
		}
		private void VerifyCallback(object obj)
		{
			VerifyInfo verifyIno = (VerifyInfo)obj;
			if (verifyIno.Result == EVerifyResult.Succeed)
			{
				VerifySuccessList.Add(verifyIno);
				CacheSystem.CacheBundle(verifyIno.VerifyBundle);
			}
			else
			{
				VerifyFailList.Add(verifyIno);

				// NOTE：不期望删除断点续传的资源文件
				/*
				if (File.Exists(patchBundle.CachedBundleFilePath))
					File.Delete(patchBundle.CachedBundleFilePath);
				*/
			}
			_verifyingList.Remove(verifyIno);
		}
	}

	/// <summary>
	/// 本地缓存文件验证器（非线程版）
	/// </summary>
	internal class CacheVerifierWithoutThread : CacheVerifier
	{
		private readonly List<VerifyInfo> _waitingList = new List<VerifyInfo>(1000);
		private readonly List<VerifyInfo> _verifyingList = new List<VerifyInfo>(100);
		private int _verifyMaxNum;
		private int _verifyTotalCount;

		public override void InitVerifier(List<VerifyInfo> verifyInfos)
		{
			_waitingList.AddRange(verifyInfos);
			VerifySuccessList = new List<VerifyInfo>(verifyInfos.Count);
			VerifyFailList = new List<VerifyInfo>(verifyInfos.Count);

			// 设置同时验证的最大数
			_verifyMaxNum = 32;
			_verifyTotalCount = _waitingList.Count;
		}
		public override bool UpdateVerifier()
		{
			if (_waitingList.Count == 0 && _verifyingList.Count == 0)
				return true;

			for (int i = _waitingList.Count - 1; i >= 0; i--)
			{
				if (_verifyingList.Count >= _verifyMaxNum)
					break;

				var verifyIno = _waitingList[i];
				VerifyFileWithoutThread(verifyIno);
				_waitingList.RemoveAt(i);
				_verifyingList.Add(verifyIno);
			}

			_verifyingList.Clear();
			return false;
		}
		public override float GetVerifierProgress()
		{
			if (_verifyTotalCount == 0)
				return 1f;
			return (float)(VerifySuccessList.Count + VerifyFailList.Count) / _verifyTotalCount;
		}

		private void VerifyFileWithoutThread(VerifyInfo verifyIno)
		{
			var verifyResult = CacheSystem.VerifyAndCacheBundle(verifyIno.VerifyBundle, CacheSystem.InitVerifyLevel);
			if (verifyResult == EVerifyResult.Succeed)
			{
				VerifySuccessList.Add(verifyIno);
			}
			else
			{
				VerifyFailList.Add(verifyIno);

				// NOTE：不期望删除断点续传的资源文件
				/*			
				if (File.Exists(patchBundle.CachedBundleFilePath))
					File.Delete(patchBundle.CachedBundleFilePath);
				*/
			}
		}
	}
}