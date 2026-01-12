using System;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器配置文件加载器
    /// </summary>
    public static class SettingLoader
    {
        /// <summary>
        /// 加载指定类型的配置文件，如果不存在则自动创建
        /// </summary>
        /// <typeparam name="TSetting">配置文件类型，必须继承自 ScriptableObject</typeparam>
        /// <returns>加载或新创建的配置文件实例</returns>
        public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
        {
            var settingType = typeof(TSetting);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Creating new '{settingType.Name}.asset' file.");
                var setting = ScriptableObject.CreateInstance<TSetting>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple files: '{path}'.");
                    }
                    throw new InvalidOperationException($"Found multiple {settingType.Name} files.");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
                if (setting == null)
                    throw new InvalidOperationException($"Failed to load {settingType.Name} at path: '{filePath}'.");
                return setting;
            }
        }
    }
}