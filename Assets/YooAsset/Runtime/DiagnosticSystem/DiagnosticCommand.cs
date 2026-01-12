using System;
using System.Text;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 诊断命令
    /// </summary>
    /// <remarks>
    /// 用于 Editor 向 Player 发送诊断指令
    /// </remarks>
    [Serializable]
    internal class DiagnosticCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public EDiagnosticCommandType CommandType;

        /// <summary>
        /// 命令附加参数
        /// </summary>
        public string Parameter;

        /// <summary>
        /// 序列化命令为字节数组
        /// </summary>
        /// <param name="command">要序列化的诊断命令</param>
        /// <returns>UTF-8 编码的 JSON 字节数组</returns>
        public static byte[] Serialize(DiagnosticCommand command)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(command));
        }

        /// <summary>
        /// 从字节数组反序列化命令
        /// </summary>
        /// <param name="data">UTF-8 编码的 JSON 字节数组</param>
        /// <returns>反序列化后的诊断命令</returns>
        public static DiagnosticCommand Deserialize(byte[] data)
        {
            return JsonUtility.FromJson<DiagnosticCommand>(Encoding.UTF8.GetString(data));
        }
    }
}
