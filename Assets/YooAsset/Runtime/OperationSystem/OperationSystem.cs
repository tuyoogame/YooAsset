using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
    internal class OperationSystem
    {
#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            DestroyAll();
        }
#endif

        private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(1000);
        private static readonly List<AsyncOperationBase> _newList = new List<AsyncOperationBase>(1000);
        private static Action<string, AsyncOperationBase> _startCallback = null;
        private static Action<string, AsyncOperationBase> _finishCallback = null;

        // 计时器相关
        private static Stopwatch _watch;
        private static long _frameTime;

        /// <summary>
        /// 异步操作的最小时间片段
        /// </summary>
        public static long MaxTimeSlice { set; get; } = long.MaxValue;

        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public static bool IsBusy
        {
            get
            {
                if (_watch == null)
                    return false;

                // NOTE : 单次调用开销约1微秒
                return _watch.ElapsedMilliseconds - _frameTime >= MaxTimeSlice;
            }
        }


        /// <summary>
        /// 初始化异步操作系统
        /// </summary>
        public static void Initialize()
        {
            _watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// 更新异步操作系统
        /// </summary>
        public static void Update()
        {
            // 移除已经完成的异步操作
            // 注意：移除上一帧完成的异步操作，方便调试器接收到完整的信息！
            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                var operation = _operations[i];
                if (operation.IsFinish)
                {
                    _operations.RemoveAt(i);

                    if (_finishCallback != null)
                        _finishCallback.Invoke(operation.PackageName, operation);
                }
            }

            // 添加新增的异步操作
            if (_newList.Count > 0)
            {
                bool sorting = false;
                foreach (var operation in _newList)
                {
                    if (operation.Priority > 0)
                    {
                        sorting = true;
                        break;
                    }
                }

                _operations.AddRange(_newList);
                _newList.Clear();

                // 重新排序优先级
                if (sorting)
                    _operations.Sort();
            }

            // 更新进行中的异步操作
            bool checkBusy = MaxTimeSlice < long.MaxValue;
            _frameTime = _watch.ElapsedMilliseconds;
            for (int i = 0; i < _operations.Count; i++)
            {
                if (checkBusy && IsBusy)
                    break;

                var operation = _operations[i];
                if (operation.IsFinish)
                    continue;

                operation.UpdateOperation();
            }
        }

        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        public static void DestroyAll()
        {
            _operations.Clear();
            _newList.Clear();
            _startCallback = null;
            _finishCallback = null;
            _watch = null;
            _frameTime = 0;
            MaxTimeSlice = long.MaxValue;
        }

        /// <summary>
        /// 销毁包裹的所有任务
        /// </summary>
        public static void ClearPackageOperation(string packageName)
        {
            // 终止临时队列里的任务
            foreach (var operation in _newList)
            {
                if (operation.PackageName == packageName)
                {
                    operation.AbortOperation();
                }
            }

            // 终止正在进行的任务
            foreach (var operation in _operations)
            {
                if (operation.PackageName == packageName)
                {
                    operation.AbortOperation();
                }
            }
        }

        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public static void StartOperation(string packageName, AsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation.SetPackageName(packageName);
            operation.StartOperation();

            if (_startCallback != null)
                _startCallback.Invoke(packageName, operation);
        }

        /// <summary>
        /// 监听任务开始
        /// </summary>
        public static void RegisterStartCallback(Action<string, AsyncOperationBase> callback)
        {
            _startCallback = callback;
        }

        /// <summary>
        /// 监听任务结束
        /// </summary>
        public static void RegisterFinishCallback(Action<string, AsyncOperationBase> callback)
        {
            _finishCallback = callback;
        }

        #region 调试信息
        internal static List<DebugOperationInfo> GetDebugOperationInfos(string packageName)
        {
            List<DebugOperationInfo> result = new List<DebugOperationInfo>(_operations.Count);
            foreach (var operation in _operations)
            {
                if (operation.PackageName == packageName)
                {
                    var operationInfo = GetDebugOperationInfo(operation);
                    result.Add(operationInfo);
                }
            }
            return result;
        }
        internal static DebugOperationInfo GetDebugOperationInfo(AsyncOperationBase operation)
        {
            var operationInfo = new DebugOperationInfo();
            operationInfo.OperationName = operation.GetType().Name;
            operationInfo.OperationDesc = operation.GetOperationDesc();
            operationInfo.Priority = operation.Priority;
            operationInfo.Progress = operation.Progress;
            operationInfo.BeginTime = operation.BeginTime;
            operationInfo.ProcessTime = operation.ProcessTime;
            operationInfo.Status = operation.Status.ToString();
            operationInfo.Childs = new List<DebugOperationInfo>(operation.Childs.Count);
            foreach (var child in operation.Childs)
            {
                var childInfo = GetDebugOperationInfo(child);
                operationInfo.Childs.Add(childInfo);
            }
            return operationInfo;
        }
        #endregion
    }
}