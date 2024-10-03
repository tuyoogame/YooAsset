﻿using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YooAsset
{
    public abstract class AsyncOperationBase : IEnumerator, IComparable<AsyncOperationBase>
    {
        private Action<AsyncOperationBase> _callback;
        private string _packageName = null;
        private int _whileFrame = 1000;

        /// <summary>
        /// 是否已经完成
        /// </summary>
        internal bool IsFinish = false;

        /// <summary>
        /// 优先级
        /// </summary>
        public uint Priority { set; get; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Status == EOperationStatus.Failed || Status == EOperationStatus.Succeed;
            }
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                if (IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                _callback -= value;
            }
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

        internal abstract void InternalOnStart();
        internal abstract void InternalOnUpdate();
        internal virtual void InternalOnAbort()
        {
        }
        internal virtual void InternalWaitForAsyncComplete()
        {
            throw new System.NotImplementedException(this.GetType().Name);
        }

        public string GetPackageName()
        {
            return _packageName;
        }
        internal void SetPackageName(string packageName)
        {
            _packageName = packageName;
        }
        internal void SetStart()
        {
            Status = EOperationStatus.Processing;
            InternalOnStart();
        }
        internal void SetFinish()
        {
            IsFinish = true;

            // 进度百分百完成
            Progress = 1f;

            //注意：如果完成回调内发生异常，会导致Task无限期等待
            _callback?.Invoke(this);

            if (_taskCompletionSource != null)
                _taskCompletionSource.TrySetResult(null);
        }
        internal void SetAbort()
        {
            if (IsDone == false)
            {
                Status = EOperationStatus.Failed;
                Error = "user abort";
                YooLogger.Warning($"Async operaiton {this.GetType().Name} has been abort !");
                InternalOnAbort();
            }
        }

        /// <summary>
        /// 执行While循环
        /// </summary>
        protected bool ExecuteWhileDone()
        {
            if (IsDone == false)
            {
                // 执行更新逻辑
                InternalOnUpdate();

                // 当执行次数用完时
                _whileFrame--;
                if (_whileFrame == 0)
                {
                    Status = EOperationStatus.Failed;
                    Error = $"Operation {this.GetType().Name} failed to wait for async complete !";
                    YooLogger.Error(Error);
                }
            }
            return IsDone;
        }

        /// <summary>
        /// 清空完成回调
        /// </summary>
        protected void ClearCompletedCallback()
        {
            _callback = null;
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsDone)
                return;

            InternalWaitForAsyncComplete();
        }

        #region 排序接口实现
        public int CompareTo(AsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }
        #endregion

        #region 异步编程相关
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        void IEnumerator.Reset()
        {
        }
        object IEnumerator.Current => null;

        private TaskCompletionSource<object> _taskCompletionSource;
        #endregion
    }
}
