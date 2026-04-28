using UnityEngine;
using UnityEditor;

/// <summary>
/// 保存着色器变种收集工具的编辑器偏好设置
/// </summary>
public class ShaderVariantCollectorSetting : ScriptableObject
{
    private const string DefaultSavePath = "Assets/MyShaderVariants.shadervariants";

    /// <summary>
    /// 查询收集结果保存路径
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <returns>收集结果保存路径</returns>
    public static string GetFileSavePath(string packageName)
    {
        string key = $"{Application.productName}_{packageName}_GetFileSavePath";
        return EditorPrefs.GetString(key, DefaultSavePath);
    }

    /// <summary>
    /// 设置收集结果保存路径
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <param name="savePath">收集结果保存路径</param>
    public static void SetFileSavePath(string packageName, string savePath)
    {
        string key = $"{Application.productName}_{packageName}_GetFileSavePath";
        EditorPrefs.SetString(key, savePath);
    }

    /// <summary>
    /// 查询单批处理容量
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <returns>单批处理容量</returns>
    public static int GetProcessCapacity(string packageName)
    {
        string key = $"{Application.productName}_{packageName}_GetProcessCapacity";
        return EditorPrefs.GetInt(key, 1000);
    }

    /// <summary>
    /// 设置单批处理容量
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <param name="capacity">单批处理容量</param>
    public static void SetProcessCapacity(string packageName, int capacity)
    {
        string key = $"{Application.productName}_{packageName}_GetProcessCapacity";
        EditorPrefs.SetInt(key, capacity);
    }
}