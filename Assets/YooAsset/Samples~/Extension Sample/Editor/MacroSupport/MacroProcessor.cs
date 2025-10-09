using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;

#if YOO_MACRO_SUPPORT
namespace YooAsset.Editor
{
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
    }
}
#endif