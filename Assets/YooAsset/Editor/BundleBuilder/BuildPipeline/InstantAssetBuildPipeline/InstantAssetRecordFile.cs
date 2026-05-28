#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的分包配置文件工具类
    /// </summary>
    internal static class InstantAssetRecordFile
    {
        /// <summary>
        /// 分包配置文件名称
        /// </summary>
        public const string RecordFileName = "AssetPackerRecordFile.json";

        /// <summary>
        /// 保存分包配置文件
        /// </summary>
        public static void Save(string outputDirectory, Dictionary<string, List<string>> recordMap)
        {
            string filePath = $"{outputDirectory}/{RecordFileName}";
            string jsonData = Serialize(recordMap);

            if (File.Exists(filePath))
                File.Delete(filePath);
            FileUtility.WriteAllText(filePath, jsonData);
            BuildLogger.Log($"Create AssetPackerRecordFile: '{filePath}'.");

            // 备份文件
            string backupFilePath = $"{outputDirectory}/AssetPackerRecordFile.backup";
            FileUtility.WriteAllText(backupFilePath, jsonData);
        }

        /// <summary>
        /// 序列化为 InstantAsset 要求的单行 JSON，并在末尾追加 \0 终止符。
        /// </summary>
        private static string Serialize(Dictionary<string, List<string>> recordMap)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            int bundleIndex = 0;
            foreach (var kvp in recordMap)
            {
                if (bundleIndex > 0)
                    sb.Append(",");

                sb.Append($"\"{kvp.Key}\":[");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (i > 0)
                        sb.Append(",");
                    sb.Append($"\"{kvp.Value[i]}\"");
                }
                sb.Append("]");
                bundleIndex++;
            }
            sb.Append("}");
            sb.Append('\0');
            return sb.ToString();
        }
    }
}
#endif
