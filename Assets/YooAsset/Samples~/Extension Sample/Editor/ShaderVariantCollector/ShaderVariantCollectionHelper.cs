using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using YooAsset.Editor;

/// <summary>
/// 封装 Unity 编辑器中的 ShaderVariantCollection 反射调用
/// </summary>
public static class ShaderVariantCollectionHelper
{
    /// <summary>
    /// 清空当前编辑器记录的着色器变种集合
    /// </summary>
    public static void ClearCurrentShaderVariantCollection()
    {
        EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
    }

    /// <summary>
    /// 保存当前编辑器记录的着色器变种集合
    /// </summary>
    /// <param name="savePath">保存目标路径</param>
    public static void SaveCurrentShaderVariantCollection(string savePath)
    {
        EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
    }

    /// <summary>
    /// 当前着色器变种集合中的着色器数量
    /// </summary>
    /// <returns>着色器数量</returns>
    public static int GetCurrentShaderVariantCollectionShaderCount()
    {
        return (int)EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
    }

    /// <summary>
    /// 当前着色器变种集合中的变种数量
    /// </summary>
    /// <returns>变种数量</returns>
    public static int GetCurrentShaderVariantCollectionVariantCount()
    {
        return (int)EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
    }

    /// <summary>
    /// 获取指定着色器资源的变种总数量
    /// </summary>
    /// <param name="assetPath">着色器资源路径</param>
    /// <returns>变种总数量的字符串表示</returns>
    public static string GetShaderVariantCount(string assetPath)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
        var variantCount = EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
        return variantCount.ToString();
    }
}