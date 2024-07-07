using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class LoadDependBundleFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckDepend,
            CheckResult,
            Done,
        }

        /// <summary>
        /// 依赖的资源包加载器列表
        /// </summary>
        internal readonly List<LoadBundleFileOperation> Depends;
        private ESteps _steps = ESteps.None;


        internal LoadDependBundleFileOperation(List<LoadBundleFileOperation> dpends)
        {
            Depends = dpends;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckDepend;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckDepend)
            {
                foreach (var loader in Depends)
                {
                    if (loader.IsDone == false)
                        return;
                }
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                LoadBundleFileOperation failedLoader = null;
                foreach (var loader in Depends)
                {
                    if (loader.Status != EOperationStatus.Succeed)
                    {
                        failedLoader = loader;
                        break;
                    }
                }

                if (failedLoader == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = failedLoader.Error;
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                foreach (var loader in Depends)
                {
                    loader.WaitForAsyncComplete();
                }

                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            foreach (var loader in Depends)
            {
                loader.Reference();
            }
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            foreach (var loader in Depends)
            {
                loader.Release();
            }
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
        {
            foreach (var loader in Depends)
            {
                var bundleInfo = new DebugBundleInfo();
                bundleInfo.BundleName = loader.BundleFileInfo.Bundle.BundleName;
                bundleInfo.RefCount = loader.RefCount;
                bundleInfo.Status = loader.Status;
                output.Add(bundleInfo);
            }
        }
    }
}