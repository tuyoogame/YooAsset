using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

#if YOOASSET_MACRO_SUPPORT
namespace YooAsset.Editor.Experiment
{
    /// <summary>
    /// 通过 csc.rsp 文件注入 YooAsset 版本宏定义
    /// </summary>
    [InitializeOnLoad]
    public class RspGenerator
    {
        // Unity 编译器响应文件路径
        private static string RspFilePath => Path.Combine(Application.dataPath, "csc.rsp");

        static RspGenerator()
        {
            UpdateRspFile(MacroDefine.Macros, null);
        }

        /// <summary>
        /// 更新 csc.rsp 文件
        /// </summary>
        private static void UpdateRspFile(IReadOnlyList<string> addMacros, IReadOnlyList<string> removeMacros)
        {
            var existingDefines = new HashSet<string>();
            var otherLines = new List<string>();

            // 1. 读取现有内容
            ReadRspFile(existingDefines, otherLines);

            // 2. 添加新宏
            if (addMacros != null && addMacros.Count > 0)
            {
                foreach (var x in addMacros)
                {
                    if (existingDefines.Contains(x) == false)
                        existingDefines.Add(x);
                }
            }

            // 3. 移除指定宏
            if (removeMacros != null && removeMacros.Count > 0)
            {
                foreach (var x in removeMacros)
                {
                    existingDefines.Remove(x);
                }
            }

            // 4. 重新生成内容
            bool changed = WriteRspFile(existingDefines, otherLines);
            if (changed == false)
                return;

            // 5. 刷新AssetDatabase
            AssetDatabase.Refresh();
            EditorUtility.RequestScriptReload();
        }

        /// <summary>
        /// 读取 csc.rsp 文件中的宏定义和其他行
        /// </summary>
        private static void ReadRspFile(HashSet<string> defines, List<string> others)
        {
            if (defines == null)
                defines = new HashSet<string>();

            if (others == null)
                others = new List<string>();

            if (File.Exists(RspFilePath) == false)
                return;

            foreach (string line in File.ReadAllLines(RspFilePath))
            {
                if (line.StartsWith("-define:"))
                {
                    string[] parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        defines.Add(parts[1].Trim());
                    }
                }
                else
                {
                    others.Add(line);
                }
            }
        }

        /// <summary>
        /// 重新写入 csc.rsp 文件
        /// </summary>
        /// <returns>文件内容发生变化时返回 true</returns>
        private static bool WriteRspFile(HashSet<string> defines, List<string> others)
        {
            StringBuilder sb = new StringBuilder();
            if (others != null && others.Count > 0)
            {
                others.ForEach(o => sb.AppendLine(o));
            }

            if (defines != null && defines.Count > 0)
            {
                foreach (string define in defines)
                {
                    sb.AppendLine($"-define:{define}");
                }
            }

            string newContent = sb.ToString();
            if (File.Exists(RspFilePath))
            {
                string oldContent = File.ReadAllText(RspFilePath);
                if (oldContent == newContent)
                    return false;
            }

            File.WriteAllText(RspFilePath, newContent);
            return true;
        }
    }
}
#endif