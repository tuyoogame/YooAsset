using System.Diagnostics;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 资源系统的驱动器
    /// </summary>
    internal class YooAssetsDriver : MonoBehaviour
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            s_latestUpdateFrame = 0;
        }
#endif

        private static int s_latestUpdateFrame = 0;

        void Update()
        {
            DebugCheckDuplicateDriver();
            YooAssets.Update();
        }

#if UNITY_EDITOR
        void OnApplicationQuit()
        {
            // 注意：在编辑器下确保播放被停止时IO类操作被终止。
            YooAssets.Destroy();
        }
#endif

        [Conditional("DEBUG")]
        private void DebugCheckDuplicateDriver()
        {
            if (s_latestUpdateFrame > 0)
            {
                if (s_latestUpdateFrame == Time.frameCount)
                    YooLogger.LogWarning($"Duplicate {nameof(YooAssetsDriver)} detected in the scene. Ensure there is always exactly one driver.");
            }

            s_latestUpdateFrame = Time.frameCount;
        }
    }
}