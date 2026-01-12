using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// AssetBundle 文件的加载缓存管理器
    /// </summary>
    public static class AssetBundleRecorder
    {
        private static readonly Dictionary<string, AssetBundle> s_loadedAssetBundles = new Dictionary<string, AssetBundle>(1000);

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            UnloadAll();
        }

        /// <summary>
        /// 获取 AssetBundle 对象，如果没有被缓存就重新加载。
        /// </summary>
        /// <param name="filePath">AssetBundle 文件的完整路径</param>
        /// <returns>加载到的 AssetBundle 对象，文件不存在或无效时返回 null。</returns>
        public static AssetBundle GetAssetBundle(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (s_loadedAssetBundles.TryGetValue(filePath, out AssetBundle bundle))
            {
                return bundle;
            }

            if (File.Exists(filePath) == false)
            {
                Debug.LogWarning($"Asset bundle file not found: '{filePath}'.");
                return null;
            }

            // 验证文件有效性（可能文件被加密）
            byte[] fileData = File.ReadAllBytes(filePath);
            if (EditorFileUtility.CheckBundleFileValid(fileData) == false)
            {
                Debug.LogWarning($"Asset bundle file is invalid and may be encrypted: '{filePath}'.");
                return null;
            }

            AssetBundle newBundle = AssetBundle.LoadFromFile(filePath);
            if (newBundle != null)
            {
                s_loadedAssetBundles.Add(filePath, newBundle);
            }
            return newBundle;
        }

        /// <summary>
        /// 卸载所有已经加载的AssetBundle文件
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var valuePair in s_loadedAssetBundles)
            {
                if (valuePair.Value != null)
                    valuePair.Value.Unload(true);
            }
            s_loadedAssetBundles.Clear();
        }
    }
}
