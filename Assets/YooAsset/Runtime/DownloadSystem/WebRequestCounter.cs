using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 网络请求失败计数器（诊断用）
    /// </summary>
    /// <remarks>
    /// 线程安全：内部使用 Dictionary 且未加锁，约定只在 Unity 主线程调用。
    /// 如需在多线程/回调线程调用，请在外层加锁或改为并发容器实现。
    /// </remarks>
    internal class WebRequestCounter
    {
#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            _requestFailedRecorder.Clear();
        }
#endif

        /// <summary>
        /// 失败计数记录表（key = $"{packageName}_{eventName}"）
        /// </summary>
        private static readonly Dictionary<string, int> _requestFailedRecorder = new Dictionary<string, int>(1000);

        /// <summary>
        /// 记录一次失败
        /// </summary>
        public static void RecordRequestFailed(string packageName, string eventName)
        {
            string key = $"{packageName}_{eventName}";
            if (_requestFailedRecorder.ContainsKey(key) == false)
                _requestFailedRecorder.Add(key, 0);
            _requestFailedRecorder[key]++;
        }

        /// <summary>
        /// 获取失败次数
        /// </summary>
        public static int GetRequestFailedCount(string packageName, string eventName)
        {
            string key = $"{packageName}_{eventName}";
            if (_requestFailedRecorder.TryGetValue(key, out int count))
                return count;
            return 0;
        }
    }
}
