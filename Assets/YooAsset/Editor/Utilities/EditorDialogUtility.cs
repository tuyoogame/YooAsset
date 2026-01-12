using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器对话框与进度条工具类
    /// </summary>
    public static class EditorDialogUtility
    {
        /// <summary>
        /// 打开文件夹选择面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <param name="defaultName">默认文件夹名称</param>
        /// <returns>选择的文件夹绝对路径，无效时为 null</returns>
        public static string OpenFolderPanel(string title, string defaultPath, string defaultName = "")
        {
            string openPath = EditorUtility.OpenFolderPanel(title, defaultPath, defaultName);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets/") == false && openPath.EndsWith("/Assets") == false)
            {
                Debug.LogWarning("Please select a Unity assets folder.");
                return null;
            }
            return openPath;
        }

        /// <summary>
        /// 打开文件选择面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <param name="extension">文件扩展名过滤</param>
        /// <returns>选择的文件绝对路径，无效时为 null</returns>
        public static string OpenFilePath(string title, string defaultPath, string extension = "")
        {
            string openPath = EditorUtility.OpenFilePanel(title, defaultPath, extension);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets/") == false && openPath.EndsWith("/Assets") == false)
            {
                Debug.LogWarning("Please select a Unity assets file.");
                return null;
            }
            return openPath;
        }

        /// <summary>
        /// 显示进度框
        /// </summary>
        /// <param name="tips">提示信息</param>
        /// <param name="progressValue">当前进度值</param>
        /// <param name="totalValue">总进度值</param>
        public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
        {
            float progress = totalValue > 0 ? (float)progressValue / totalValue : 0f;
            EditorUtility.DisplayProgressBar("Progress", $"{tips} : {progressValue}/{totalValue}", progress);
        }

        /// <summary>
        /// 隐藏进度框
        /// </summary>
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
