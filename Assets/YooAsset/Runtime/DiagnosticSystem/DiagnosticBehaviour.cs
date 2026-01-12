using System;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace YooAsset
{
    /// <summary>
    /// 诊断行为组件
    /// </summary>
    /// <remarks>
    /// 负责接收 Editor 命令并发送诊断数据
    /// </remarks>
    internal class DiagnosticBehaviour : MonoBehaviour
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            s_sampleOnce = false;
            s_autoSampling = false;
        }
#endif

        private static bool s_sampleOnce = false;
        private static bool s_autoSampling = false;

        private void Awake()
        {
#if UNITY_EDITOR
            MockPlayerConnection.Instance.Initialize();
#endif
        }
        private void OnEnable()
        {
#if UNITY_EDITOR
            MockPlayerConnection.Instance.Register(DiagnosticSystemConsts.EditorToPlayerMessageId, HandleEditorMessage);
#else
            PlayerConnection.instance.Register(DiagnosticSystemConsts.EditorToPlayerMessageId, HandleEditorMessage);
#endif
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            MockPlayerConnection.Instance.Unregister(DiagnosticSystemConsts.EditorToPlayerMessageId, HandleEditorMessage);
#else
            PlayerConnection.instance.Unregister(DiagnosticSystemConsts.EditorToPlayerMessageId, HandleEditorMessage);
#endif
        }
        private void LateUpdate()
        {
            if (s_autoSampling || s_sampleOnce)
            {
                s_sampleOnce = false;
                var debugReport = YooAssets.GetDiagnosticReport();
                var data = DiagnosticReport.Serialize(debugReport);

#if UNITY_EDITOR
                MockPlayerConnection.Instance.Send(DiagnosticSystemConsts.PlayerToEditorMessageId, data);
#else
                PlayerConnection.instance.Send(DiagnosticSystemConsts.PlayerToEditorMessageId, data);
#endif
            }
        }

        private static void HandleEditorMessage(MessageEventArgs args)
        {
            var command = DiagnosticCommand.Deserialize(args.data);
            YooLogger.Log($"[{nameof(DiagnosticBehaviour)}] Received command: Type={command.CommandType}, Param='{command.Parameter}'.");
            if (command.CommandType == EDiagnosticCommandType.SampleOnce)
            {
                s_sampleOnce = true;
            }
            else if (command.CommandType == EDiagnosticCommandType.AutoSampling)
            {
                if (command.Parameter == DiagnosticSystemConsts.AutoSamplingOpen)
                    s_autoSampling = true;
                else
                    s_autoSampling = false;
            }
            else
            {
                YooLogger.LogWarning($"Unknown diagnostic command type: {command.CommandType}.");
            }
        }
    }
}
