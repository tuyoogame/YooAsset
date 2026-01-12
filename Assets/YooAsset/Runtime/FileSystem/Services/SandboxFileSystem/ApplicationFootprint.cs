using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 应用程序水印
    /// </summary>
    internal class ApplicationFootprint
    {
        private readonly string _filePath;
        private string _footprint;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="filePath">足迹文件路径</param>
        public ApplicationFootprint(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// 读取应用程序水印
        /// </summary>
        public void Load(string packageName)
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    _footprint = FileUtility.ReadAllText(_filePath);
                }
                catch (System.Exception ex)
                {
                    _footprint = string.Empty;
                    YooLogger.LogError($"Failed to read application footprint file: {ex.Message}.");
                }
            }
            else
            {
                Overwrite(packageName);
            }
        }

        /// <summary>
        /// 检测水印是否发生变化
        /// </summary>
        public bool IsDirty()
        {
            return _footprint != GetApplicationIdentifier();
        }

        /// <summary>
        /// 覆盖掉水印
        /// </summary>
        public void Overwrite(string packageName)
        {
            _footprint = GetApplicationIdentifier();
            try
            {
                FileUtility.WriteAllText(_filePath, _footprint);
                YooLogger.Log($"Saved application footprint: '{_footprint}'.");
            }
            catch (System.Exception ex)
            {
                YooLogger.LogWarning($"Failed to save application footprint file: {ex.Message}.");
            }
        }

        private static string GetApplicationIdentifier()
        {
#if UNITY_EDITOR
            return Application.version;
#else
            return Application.buildGUID;
#endif
        }
    }
}