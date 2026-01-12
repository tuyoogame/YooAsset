using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
    /// <summary>
    /// 异步操作系统，负责管理所有包裹的调度器。
    /// </summary>
    internal static class AsyncOperationSystem
    {
#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            Shutdown();
        }
#endif

        /// <summary>
        /// 全局调度器名称
        /// </summary>
        public const string GlobalSchedulerName = "YOOASSET_GLOBAL_SCHEDULER";

        private const long MinTimeSlice = 10; // 最小时间片（毫秒）

        private static readonly Dictionary<string, AsyncOperationScheduler> _schedulerDict = new Dictionary<string, AsyncOperationScheduler>(100);
        private static readonly List<AsyncOperationScheduler> _schedulerList = new List<AsyncOperationScheduler>(100);
        private static bool _isInitialized;
        private static int _nextCreationOrder;

        // 计时器相关
        private static Stopwatch _stopwatch;
        private static long _frameStartTime;
        private static long _maxTimeSlice = long.MaxValue;

        /// <summary>
        /// 每帧最大执行预算（毫秒）
        /// </summary>
        /// <value>最小值为 <see cref="MinTimeSlice"/> 毫秒，低于此值将被自动钳制。</value>
        /// <remarks>
        /// 设置过小会导致每帧可执行的操作极少，影响整体加载速度。
        /// </remarks>
        public static long MaxTimeSlice
        {
            get
            {
                return _maxTimeSlice;
            }
            set
            {
                if (value < MinTimeSlice)
                {
                    _maxTimeSlice = MinTimeSlice;
                    YooLogger.LogWarning($"MaxTimeSlice must be at least {MinTimeSlice} ms, clamped to {MinTimeSlice}.");
                }
                else
                {
                    _maxTimeSlice = value;
                }
            }
        }

        /// <summary>
        /// 当前帧的时间切片预算是否已用完
        /// </summary>
        public static bool IsBusy
        {
            get
            {
                if (_stopwatch == null)
                    return false;

                if (_maxTimeSlice == long.MaxValue)
                    return false;

                // 注意：单次调用开销约1微秒
                return _stopwatch.ElapsedMilliseconds - _frameStartTime >= _maxTimeSlice;
            }
        }


        /// <summary>
        /// 初始化异步操作系统
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                YooLogger.LogWarning("Operation system is already initialized.");
                return;
            }

            _isInitialized = true;
            _stopwatch = Stopwatch.StartNew();

            // 创建全局调度器
            CreatePackageScheduler(GlobalSchedulerName, uint.MaxValue);
        }

        /// <summary>
        /// 更新异步操作系统
        /// </summary>
        public static void Update()
        {
            if (_isInitialized == false)
                return;

            // 检测是否需要执行排序
            bool isDirty = false;
            foreach (var scheduler in _schedulerList)
            {
                if (scheduler.IsDirty)
                {
                    scheduler.IsDirty = false;
                    isDirty = true;
                }
            }
            if (isDirty)
            {
                _schedulerList.Sort();
            }

            // 更新帧时间
            _frameStartTime = _stopwatch.ElapsedMilliseconds;

            // 更新调度器
            for (int i = 0; i < _schedulerList.Count; i++)
            {
                if (IsBusy)
                    break;

                _schedulerList[i].Update();
            }
        }

        /// <summary>
        /// 关闭异步操作系统
        /// </summary>
        public static void Shutdown()
        {
            _isInitialized = false;

            // 清空所有调度器
            foreach (var scheduler in _schedulerList)
            {
                scheduler.AbortAll();
            }
            _schedulerDict.Clear();
            _schedulerList.Clear();
            _nextCreationOrder = 0;

            _stopwatch = null;
            _frameStartTime = 0;
            _maxTimeSlice = long.MaxValue;
        }

        /// <summary>
        /// 创建指定包裹的调度器
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="priority">初始优先级</param>
        /// <returns>新创建的调度器实例</returns>
        public static AsyncOperationScheduler CreatePackageScheduler(string packageName, uint priority)
        {
            DebugCheckInitialized(packageName);

            if (_schedulerDict.ContainsKey(packageName))
            {
                throw new YooInternalException($"Package scheduler already exists: '{packageName}'.");
            }

            var scheduler = new AsyncOperationScheduler(packageName, _nextCreationOrder++);
            _schedulerDict.Add(packageName, scheduler);
            _schedulerList.Add(scheduler);
            scheduler.Priority = priority;
            return scheduler;
        }

        /// <summary>
        /// 销毁指定包裹的调度器
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public static void DestroyPackageScheduler(string packageName)
        {
            DebugCheckInitialized(packageName);

            // 不允许销毁默认调度器
            if (packageName == GlobalSchedulerName)
            {
                throw new YooInternalException("Cannot destroy the global package scheduler.");
            }

            if (_schedulerDict.TryGetValue(packageName, out var scheduler))
            {
                scheduler.AbortAll();
                _schedulerDict.Remove(packageName);
                _schedulerList.Remove(scheduler);
            }
        }

        /// <summary>
        /// 中止并清空指定包裹中所有正在执行的操作
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public static void ClearPackageOperations(string packageName)
        {
            DebugCheckInitialized(packageName);

            var scheduler = GetScheduler(packageName);
            scheduler.AbortAll();
        }

        /// <summary>
        /// 将异步操作提交到指定包裹的调度器中执行
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="operation">要执行的异步操作</param>
        public static void StartOperation(string packageName, AsyncOperationBase operation)
        {
            DebugCheckInitialized(packageName);

            if (operation == null)
                throw new System.ArgumentNullException(nameof(operation));

            var scheduler = GetScheduler(packageName);
            scheduler.StartOperation(operation);
        }

        /// <summary>
        /// 设置指定包裹调度器的优先级
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="priority">优先级，值越大越优。</param>
        public static void SetSchedulerPriority(string packageName, uint priority)
        {
            DebugCheckInitialized(packageName);

            var scheduler = GetScheduler(packageName);
            scheduler.Priority = priority;
        }

        /// <summary>
        /// 获取指定包裹调度器的优先级
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>调度器的当前优先级</returns>
        public static uint GetSchedulerPriority(string packageName)
        {
            DebugCheckInitialized(packageName);

            var scheduler = GetScheduler(packageName);
            return scheduler.Priority;
        }

        /// <summary>
        /// 获取调度器（严格模式）
        /// </summary>
        private static AsyncOperationScheduler GetScheduler(string packageName)
        {
            if (_schedulerDict.TryGetValue(packageName, out var scheduler))
            {
                return scheduler;
            }

            // 严格模式：非默认包裹必须先创建调度器
            throw new YooInternalException($"Operation scheduler not found: '{packageName}'.");
        }

        #region 调试信息
        /// <summary>
        /// 获取指定包裹中所有操作的诊断信息
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>该包裹下所有操作的诊断信息列表</returns>
        internal static List<DiagnosticOperationInfo> GetDiagnosticInfos(string packageName)
        {
            DebugCheckInitialized(packageName);

            var scheduler = GetScheduler(packageName);
            return scheduler.GetDiagnosticInfos();
        }
        #endregion

        #region 调试方法
        [Conditional("DEBUG")]
        private static void DebugCheckInitialized(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new YooInternalException("Package name is null or empty.");

            if (_isInitialized == false)
                throw new YooInternalException($"{nameof(AsyncOperationSystem)} is not initialized.");
        }
        #endregion
    }
}
