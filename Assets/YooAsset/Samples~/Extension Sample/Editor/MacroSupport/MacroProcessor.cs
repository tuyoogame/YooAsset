using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;

#if YOOASSET_MACRO_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// 在生成 C# 工程文件时注入 YooAsset 版本宏定义
    /// </summary>
    [InitializeOnLoad]
    public class MacroProcessor : AssetPostprocessor
    {
        static string OnGeneratedCSProject(string path, string content)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);

            if (IsCSProjectReferenced(xmlDoc.DocumentElement) == false)
                return content;

            if (ProcessDefineConstants(xmlDoc.DocumentElement) == false)
                return content;

            // 将修改后的XML结构重新输出为文本
            using (var memoryStream = new MemoryStream())
            {
                var writerSettings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(false), //无BOM
                    OmitXmlDeclaration = false
                };

                using (var xmlWriter = XmlWriter.Create(memoryStream, writerSettings))
                {
                    xmlDoc.Save(xmlWriter);
                }
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// 处理工程文件中的宏定义
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
                    foreach (string yooMacro in MacroDefine.Macros)
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
        /// 检查工程是否引用了 YooAsset
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

                    XmlAttribute includeAttribute = childNode.Attributes["Include"];
                    if (includeAttribute == null)
                        continue;

                    string include = includeAttribute.Value;
                    if (include.Contains("YooAsset"))
                        return true;
                }
            }

            return false;
        }
    }
}
#endif