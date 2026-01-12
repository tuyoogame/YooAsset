using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器窗口与控制台工具类
    /// </summary>
    public static class EditorWindowUtility
    {
        private static MethodInfo s_clearConsoleMethod;


        /// <summary>
        /// 聚焦 Unity Scene 窗口
        /// </summary>
        public static void FocusUnitySceneWindow()
        {
            EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }

        /// <summary>
        /// 关闭 Unity Game 窗口
        /// </summary>
        public static void CloseUnityGameWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(windowType, false, "GameView", true).Close();
        }

        /// <summary>
        /// 聚焦 Unity Game 窗口
        /// </summary>
        public static void FocusUnityGameWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(windowType, false, "GameView", true);
        }

        /// <summary>
        /// 聚焦 Unity Project 窗口
        /// </summary>
        public static void FocusUnityProjectWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.ProjectBrowser");
            EditorWindow.GetWindow(windowType, false, "Project", true);
        }

        /// <summary>
        /// 聚焦 Unity Hierarchy 窗口
        /// </summary>
        public static void FocusUnityHierarchyWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow.GetWindow(windowType, false, "Hierarchy", true);
        }

        /// <summary>
        /// 聚焦 Unity Inspector 窗口
        /// </summary>
        public static void FocusUnityInspectorWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
            EditorWindow.GetWindow(windowType, false, "Inspector", true);
        }

        /// <summary>
        /// 聚焦 Unity Console 窗口
        /// </summary>
        public static void FocusUnityConsoleWindow()
        {
            System.Type windowType = Assembly.Load("UnityEditor").GetType("UnityEditor.ConsoleWindow");
            EditorWindow.GetWindow(windowType, false, "Console", true);
        }

        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void ClearUnityConsole()
        {
            GetClearConsoleMethod().Invoke(null, null);
        }

        /// <summary>
        /// 通过反射获取 Console 窗口的 Clear 方法并缓存
        /// </summary>
        private static MethodInfo GetClearConsoleMethod()
        {
            if (s_clearConsoleMethod == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                System.Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                s_clearConsoleMethod = logEntries.GetMethod("Clear");
            }
            return s_clearConsoleMethod;
        }
    }
}
