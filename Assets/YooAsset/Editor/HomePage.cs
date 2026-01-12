using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// YooAsset 官网主页菜单入口
    /// </summary>
    internal static class HomePageWindow
    {
        /// <summary>
        /// 在浏览器中打开 YooAsset 官网主页
        /// </summary>
        [MenuItem("YooAsset/Home Page", false, 1)]
        public static void OpenWindow()
        {
            Application.OpenURL("https://www.yooasset.com/");
        }
    }
}