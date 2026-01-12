using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源搜索工具类
    /// </summary>
    public static class EditorAssetUtility
    {
        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="filterType">搜集的资源类型</param>
        /// <param name="searchInFolders">指定搜索的文件夹列表</param>
        /// <returns>搜集到的资源路径数组</returns>
        public static string[] FindAssets(EAssetFilterType filterType, string[] searchInFolders)
        {
            return FindAssets(filterType.ToString(), searchInFolders);
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="filterType">搜集的资源类型</param>
        /// <param name="searchInFolder">指定搜索的文件夹</param>
        /// <returns>搜集到的资源路径数组</returns>
        public static string[] FindAssets(EAssetFilterType filterType, string searchInFolder)
        {
            return FindAssets(filterType.ToString(), new string[] { searchInFolder });
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="filterType">搜集的资源类型</param>
        /// <param name="searchInFolders">指定搜索的文件夹列表</param>
        /// <returns>搜集到的资源路径数组</returns>
        public static string[] FindAssets(string filterType, string[] searchInFolders)
        {
            if (searchInFolders == null)
                throw new ArgumentNullException(nameof(searchInFolders));

            // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
            string[] folders = new string[searchInFolders.Length];
            for (int i = 0; i < folders.Length; i++)
            {
                folders[i] = searchInFolders[i].TrimEnd('/');
            }

            // 注意：获取指定目录下的所有资源对象（包括子文件夹）
            string[] guids;
            if (string.IsNullOrEmpty(filterType) || filterType == EAssetFilterType.All.ToString())
                guids = AssetDatabase.FindAssets(string.Empty, folders);
            else
                guids = AssetDatabase.FindAssets($"t:{filterType}", folders);

            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            HashSet<string> result = new HashSet<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (result.Contains(assetPath) == false)
                {
                    result.Add(assetPath);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="filterType">搜集的资源类型</param>
        /// <param name="searchInFolder">指定搜索的文件夹</param>
        /// <returns>搜集到的资源路径数组</returns>
        public static string[] FindAssets(string filterType, string searchInFolder)
        {
            return FindAssets(filterType, new string[] { searchInFolder });
        }
    }
}
