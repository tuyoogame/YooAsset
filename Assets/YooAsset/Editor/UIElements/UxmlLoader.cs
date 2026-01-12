using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// UXML 布局文件加载器
    /// </summary>
    public class UxmlLoader
    {
        private readonly static Dictionary<Type, string> s_uxmlDic = new Dictionary<Type, string>();

        /// <summary>
        /// 加载窗口的 UXML 布局文件
        /// </summary>
        /// <typeparam name="TWindow">窗口类型</typeparam>
        /// <returns>加载到的 VisualTreeAsset 布局资源</returns>
        public static VisualTreeAsset LoadWindowUxml<TWindow>() where TWindow : class
        {
            var windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (s_uxmlDic.TryGetValue(windowType, out string uxmlGuid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGuid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    s_uxmlDic.Clear();
                    throw new InvalidOperationException($"Invalid UXML GUID: '{uxmlGuid}'. Please close the window and reopen it.");
                }
                var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载（使用类型过滤器缩小范围）
            string[] guids = AssetDatabase.FindAssets($"t:VisualTreeAsset {windowType.Name}");
            if (guids.Length == 0)
                throw new InvalidOperationException($"Could not find any assets: '{windowType.Name}'.");

            foreach (string assetGuid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (fileName != windowType.Name)
                    continue;

                s_uxmlDic.Add(windowType, assetGuid);
                var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            throw new InvalidOperationException($"UXML file not found: '{windowType.Name}'.");
        }
    }
}