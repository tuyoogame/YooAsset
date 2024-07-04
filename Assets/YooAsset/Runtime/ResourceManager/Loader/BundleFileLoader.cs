using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal class BundleFileLoader
    {
        public enum EStatus
        {
            None = 0,
            Succeed,
            Failed
        }

        private readonly ResourceManager _resourceManager;
        private readonly List<ProviderBase> _providers = new List<ProviderBase>(100);
        private readonly List<ProviderBase> _removeList = new List<ProviderBase>(100);
        private FSLoadBundleOperation _loadBundleOp;
        private bool _isDone = false;

        /// <summary>
        /// 资源包文件信息
        /// </summary>
        public BundleInfo MainBundleInfo { private set; get; }

        /// <summary>
        /// 加载状态
        /// </summary>
        public EStatus Status { protected set; get; }

        /// <summary>
        /// 最近的错误信息
        /// </summary>
        public string LastError { protected set; get; } = string.Empty;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 下载进度
        /// </summary>
        public float DownloadProgress { set; get; } = 0;

        /// <summary>
        /// 下载大小
        /// </summary>
        public long DownloadedBytes { set; get; } = 0;

        /// <summary>
        /// 下载结果
        /// </summary>
        public object Result { set; get; }


        public BundleFileLoader(ResourceManager resourceManager, BundleInfo bundleInfo)
        {
            _resourceManager = resourceManager;
            MainBundleInfo = bundleInfo;
            Status = EStatus.None;
        }

        /// <summary>
        /// 轮询更新
        /// </summary>
        public void Update()
        {
            if (_isDone)
                return;

            if (_loadBundleOp == null)
                _loadBundleOp = MainBundleInfo.LoadBundleFile();

            DownloadProgress = _loadBundleOp.DownloadProgress;
            DownloadedBytes = _loadBundleOp.DownloadedBytes;
            if (_loadBundleOp.IsDone == false)
                return;

            if (_loadBundleOp.Status == EOperationStatus.Succeed)
            {
                _isDone = true;
                Status = EStatus.Succeed;
                Result = _loadBundleOp.Result;
            }
            else
            {
                _isDone = true;
                Status = EStatus.Failed;
                LastError = _loadBundleOp.Error;
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            IsDestroyed = true;

            // Check fatal
            if (RefCount > 0)
                throw new Exception($"Bundle file loader ref is not zero : {MainBundleInfo.Bundle.BundleName}");
            if (IsDone() == false)
                throw new Exception($"Bundle file loader is not done : {MainBundleInfo.Bundle.BundleName}");

            MainBundleInfo.UnloadBundleFile(Result);
        }

        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 是否完毕（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            return Status == EStatus.Succeed || Status == EStatus.Failed;
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroy()
        {
            if (IsDone() == false)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 添加附属的资源提供者
        /// </summary>
        public void AddProvider(ProviderBase provider)
        {
            if (_providers.Contains(provider) == false)
                _providers.Add(provider);
        }

        /// <summary>
        /// 尝试销毁资源提供者
        /// </summary>
        public void TryDestroyProviders()
        {
            // 获取移除列表
            _removeList.Clear();
            foreach (var provider in _providers)
            {
                if (provider.CanDestroy())
                {
                    _removeList.Add(provider);
                }
            }

            // 销毁资源提供者
            foreach (var provider in _removeList)
            {
                _providers.Remove(provider);
                provider.Destroy();
            }

            // 移除资源提供者
            if (_removeList.Count > 0)
            {
                _resourceManager.RemoveBundleProviders(_removeList);
                _removeList.Clear();
            }
        }

        /// <summary>
        /// 主线程等待异步操作完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            while (true)
            {
                if (_loadBundleOp != null)
                {
                    if (_loadBundleOp.IsDone == false)
                        _loadBundleOp.WaitForAsyncComplete();
                }

                // 驱动流程
                Update();

                // 完成后退出
                if (IsDone())
                    break;
            }
        }

        /// <summary>
        /// 终止下载任务
        /// </summary>
        public void AbortDownloadOperation()
        {
            if (_loadBundleOp != null)
                _loadBundleOp.AbortDownloadOperation();
        }
    }
}