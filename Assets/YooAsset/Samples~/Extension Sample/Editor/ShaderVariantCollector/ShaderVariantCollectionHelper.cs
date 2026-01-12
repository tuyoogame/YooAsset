using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using YooAsset.Editor;

public static class ShaderVariantCollectionHelper
{
    public static void ClearCurrentShaderVariantCollection()
    {
        EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
    }
    public static void SaveCurrentShaderVariantCollection(string savePath)
    {
        EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
    }
    public static int GetCurrentShaderVariantCollectionShaderCount()
    {
        return (int)EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
    }
    public static int GetCurrentShaderVariantCollectionVariantCount()
    {
        return (int)EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
    }

    /// <summary>
    /// 获取着色器的变种总数量
    /// </summary>
    public static string GetShaderVariantCount(string assetPath)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
        var variantCount = EditorAssemblyUtility.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
        return variantCount.ToString();
    }
}