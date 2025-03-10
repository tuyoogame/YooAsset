using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace YooAsset.Editor
{
    [InitializeOnLoad]
    public class MacrosProcessor : AssetPostprocessor
    {
        // csc.rsp 文件路径
        private static string RspFilePath => Path.Combine(Application.dataPath, "csc.rsp");
        /// <summary>
        /// YooAsset版本宏定义
        /// </summary>
        private static readonly List<string> YooAssetMacros = new List<string>()
        {
            "YOO_ASSET_2",
            "YOO_ASSET_2_3",
            "YOO_ASSET_2_3_OR_NEWER",
        };

        static MacrosProcessor()
        {
            //添加到全局宏定义
            UpdateRspFile(YooAssetMacros);
        }

        static string OnGeneratedCSProject(string path, string content)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);

            if (IsCSProjectReferenced(xmlDoc.DocumentElement) == false)
                return content;

            if (ProcessDefineConstants(xmlDoc.DocumentElement) == false)
                return content;

            // 将修改后的XML结构重新输出为文本
            var stringWriter = new StringWriter();
            var writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            var xmlWriter = XmlWriter.Create(stringWriter, writerSettings);
            xmlDoc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        /// <summary>
        /// 处理宏定义
        /// </summary>
        private static bool ProcessDefineConstants(XmlElement element)
        {
            if (element == null)
                return false;

            bool processed = false;
            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.Name != "PropertyGroup")
                    continue;

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name != "DefineConstants")
                        continue;

                    string[] defines = childNode.InnerText.Split(';');
                    HashSet<string> hashSets = new HashSet<string>(defines);
                    foreach (string yooMacro in YooAssetMacros)
                    {
                        string tmpMacro = yooMacro.Trim();
                        if (hashSets.Contains(tmpMacro) == false)
                            hashSets.Add(tmpMacro);
                    }
                    childNode.InnerText = string.Join(";", hashSets.ToArray());
                    processed = true;
                }
            }

            return processed;
        }

        /// <summary>
        /// 检测工程是否引用了YooAsset
        /// </summary>
        private static bool IsCSProjectReferenced(XmlElement element)
        {
            if (element == null)
                return false;

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.Name != "ItemGroup")
                    continue;

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name != "Reference" && childNode.Name != "ProjectReference")
                        continue;

                    string include = childNode.Attributes["Include"].Value;
                    if (include.Contains("YooAsset"))
                        return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 更新csc.rsp文件
        /// </summary>
        /// <param name="macrosToAdd"></param>
        /// <param name="macrosToRemove"></param>
        private static void UpdateRspFile(List<string> macrosToAdd = null, List<string> macrosToRemove = null)
        {
            HashSet<string> existingDefines = new HashSet<string>();
            List<string> otherLines = new List<string>();
            // 1. 读取现有内容
            ReadRspFile(ref existingDefines, ref otherLines);
            // 2. 添加新宏
            if (macrosToAdd != null && macrosToAdd.Count > 0)
            {
                macrosToAdd.ForEach(x =>
                {
                    if (existingDefines.Contains(x) == false)
                        existingDefines.Add(x);
                });
            }
            // 3. 移除指定宏
            if (macrosToRemove != null && macrosToRemove.Count > 0)
            {
                macrosToRemove.ForEach(x =>
                {
                    existingDefines.Remove(x);
                });
            }
            // 4. 重新生成内容
            WriteRspFile(existingDefines, otherLines);
            // 5. 刷新AssetDatabase
            AssetDatabase.Refresh();
            EditorUtility.RequestScriptReload();
        }
        /// <summary>
        /// 读取csc.rsp文件,返回宏定义和其他行
        /// </summary>
        /// <param name="defines"></param>
        /// <param name="others"></param>
        private static void ReadRspFile(ref HashSet<string> defines, ref List<string> others)
        {
            if (defines == null)
            {
                defines = new HashSet<string>();
            }

            if (others == null)
            {
                others = new List<string>();
            }

            if (File.Exists(RspFilePath) == false)
            {
                return;
            }

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
        /// 重写入出csc.rsp文件
        /// </summary>
        /// <param name="defines"></param>
        /// <param name="others"></param>
        private static void WriteRspFile(HashSet<string> defines, List<string> others)
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

            File.WriteAllText(RspFilePath, sb.ToString());
        }

    }
}
