
namespace YooAsset
{
    /// <summary>
    /// 时间工具类
    /// </summary>
    internal static class TimeUtility
    {
        /// <summary>
        /// 游戏启动以来经过的真实时间（单位：秒）
        /// </summary>
        public static double RealtimeSinceStartup
        {
            get
            {
#if UNITY_2020_3_OR_NEWER
                return UnityEngine.Time.realtimeSinceStartupAsDouble;
#else
                return UnityEngine.Time.realtimeSinceStartup;
#endif
            }
        }
    }
}