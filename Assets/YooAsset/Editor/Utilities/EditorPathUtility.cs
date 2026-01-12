using System.IO;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 路径工具类
    /// </summary>
    public static class EditorPathUtility
    {
        /// <summary>
        /// 获取规范的路径
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <returns>使用正斜杠分隔的规范路径</returns>
        public static string GetRegularPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// 移除路径里的后缀名
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <returns>去除扩展名后的路径</returns>
        public static string RemoveExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // 只在最后一个路径分隔符之后查找点号，避免误截含点号的目录名（如 com.unity.package）
            int separatorIndex = path.LastIndexOf('/');
            int dotIndex = path.LastIndexOf('.');
            if (dotIndex == -1 || dotIndex < separatorIndex)
                return path;
            else
                return path.Remove(dotIndex);
        }

        /// <summary>
        /// 获取项目工程路径
        /// </summary>
        /// <returns>项目根目录的规范路径</returns>
        public static string GetProjectPath()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return GetRegularPath(projectPath);
        }

        /// <summary>
        /// 转换文件的绝对路径为 Unity 资源路径
        /// </summary>
        /// <example>
        /// <code>D:\\YourProject\\Assets\\Works\\file.txt → Assets/Works/file.txt</code>
        /// </example>
        /// <param name="absolutePath">文件的绝对路径</param>
        /// <returns>以 Assets/ 开头的 Unity 资源路径</returns>
        public static string AbsolutePathToAssetPath(string absolutePath)
        {
            string content = GetRegularPath(absolutePath);
            return EditorStringUtility.Substring(content, "Assets/", true);
        }

        /// <summary>
        /// 转换 Unity 资源路径为文件的绝对路径
        /// </summary>
        /// <example>
        /// <code>Assets/Works/file.txt → D:\\YourProject/Assets/Works/file.txt</code>
        /// </example>
        /// <param name="assetPath">Unity 资源路径</param>
        /// <returns>文件的绝对路径</returns>
        public static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectPath = GetProjectPath();
            return $"{projectPath}/{assetPath}";
        }
    }
}
