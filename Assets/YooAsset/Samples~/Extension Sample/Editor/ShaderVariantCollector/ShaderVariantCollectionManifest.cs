using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

/// <summary>
/// 着色器变种集合的可序列化清单
/// </summary>
[Serializable]
public class ShaderVariantCollectionManifest
{
    /// <summary>
    /// 单个着色器变种的序列化信息
    /// </summary>
    [Serializable]
    public class ShaderVariantElement : IComparable<ShaderVariantElement>
    {
        /// <summary>
        /// 用于稳定排序的组合键
        /// </summary>
        public string SortValue { private set; get; }

        /// <summary>
        /// 变种使用的渲染通道类型
        /// </summary>
        public PassType PassType;

        /// <summary>
        /// 变种使用的着色器关键字数组
        /// </summary>
        public string[] Keywords;

        /// <summary>
        /// 生成排序键
        /// </summary>
        public void MakeSortValue()
        {
            string combineKeyword = string.Empty;
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (i == 0)
                    combineKeyword = Keywords[i];
                else
                    combineKeyword = $"{combineKeyword}+{Keywords[i]}";
            }

            SortValue = $"{PassType}+{combineKeyword}";
        }

        /// <inheritdoc/>
        public int CompareTo(ShaderVariantElement other)
        {
            return SortValue.CompareTo(other.SortValue);
        }
    }

    /// <summary>
    /// 单个着色器及其变种列表的序列化信息
    /// </summary>
    [Serializable]
    public class ShaderVariantInfo : IComparable<ShaderVariantInfo>
    {
        /// <summary>
        /// 用于稳定排序的组合键
        /// </summary>
        public string SortValue { private set; get; }

        /// <summary>
        /// 着色器资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 着色器名称
        /// </summary>
        public string ShaderName;

        /// <summary>
        /// 着色器变种总数
        /// </summary>
        public int ShaderVariantCount = 0;

        /// <summary>
        /// 着色器变种列表
        /// </summary>
        public List<ShaderVariantElement> ShaderVariantElements = new List<ShaderVariantElement>(1000);

        /// <summary>
        /// 生成排序键
        /// </summary>
        public void MakeSortValue()
        {
            SortValue = AssetPath + "+" + ShaderName;
        }

        /// <inheritdoc/>
        public int CompareTo(ShaderVariantInfo other)
        {
            return SortValue.CompareTo(other.SortValue);
        }
    }


    /// <summary>
    /// 清单中的着色器总数
    /// </summary>
    public int ShaderTotalCount;

    /// <summary>
    /// 清单中的变种总数
    /// </summary>
    public int VariantTotalCount;

    /// <summary>
    /// 着色器变种信息列表
    /// </summary>
    public List<ShaderVariantInfo> ShaderVariantInfos = new List<ShaderVariantInfo>(1000);

    /// <summary>
    /// 添加着色器变种信息
    /// </summary>
    /// <param name="assetPath">着色器资源路径</param>
    /// <param name="shaderName">着色器名称</param>
    /// <param name="passType">渲染通道类型</param>
    /// <param name="keywords">着色器关键字数组</param>
    public void AddShaderVariant(string assetPath, string shaderName, PassType passType, string[] keywords)
    {
        List<string> sortedKeywords = new List<string>(keywords);
        sortedKeywords.Sort();

        var info = GetOrCreateShaderVariantInfo(assetPath, shaderName);
        ShaderVariantElement element = new ShaderVariantElement();
        element.PassType = passType;
        element.Keywords = sortedKeywords.ToArray();
        element.MakeSortValue();
        info.ShaderVariantElements.Add(element);
        info.ShaderVariantCount++;
    }
    private ShaderVariantInfo GetOrCreateShaderVariantInfo(string assetPath, string shaderName)
    {
        var selectList = ShaderVariantInfos.Where(t => t.ShaderName == shaderName && t.AssetPath == assetPath).ToList();
        if (selectList.Count == 0)
        {
            ShaderVariantInfo newInfo = new ShaderVariantInfo();
            newInfo.AssetPath = assetPath;
            newInfo.ShaderName = shaderName;
            newInfo.MakeSortValue();
            ShaderVariantInfos.Add(newInfo);
            return newInfo;
        }

        if (selectList.Count != 1)
            throw new InvalidOperationException($"Unexpected duplicate ShaderVariantInfo entries (count={selectList.Count}).");

        return selectList[0];
    }

    /// <summary>
    /// 从 ShaderVariantCollection 提取清单数据
    /// </summary>
    /// <param name="svc">待解析的着色器变种集合</param>
    /// <returns>提取后的着色器变种清单</returns>
    public static ShaderVariantCollectionManifest Extract(ShaderVariantCollection svc)
    {
        if (svc == null)
            throw new ArgumentNullException(nameof(svc));

        var manifest = new ShaderVariantCollectionManifest();

        using (var so = new SerializedObject(svc))
        {
            var shaderArray = so.FindProperty("m_Shaders.Array");
            if (shaderArray != null && shaderArray.isArray)
            {
                for (int i = 0; i < shaderArray.arraySize; ++i)
                {
                    var shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
                    var shader = shaderRef.objectReferenceValue as Shader;
                    if (shader == null)
                        throw new InvalidOperationException("Invalid shader in ShaderVariantCollection file.");

                    string shaderAssetPath = AssetDatabase.GetAssetPath(shader);
                    string shaderName = shader.name;

                    // 添加变种信息
                    var shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
                    for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
                    {
                        var propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
                        var propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");

                        string[] keywords = propKeywords.stringValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        PassType passType = (PassType)propPassType.intValue;
                        manifest.AddShaderVariant(shaderAssetPath, shaderName, passType, keywords);
                    }
                }
            }
        }

        // 重新排序
        manifest.ShaderVariantInfos.Sort();
        foreach (var shaderVariantInfo in manifest.ShaderVariantInfos)
        {
            shaderVariantInfo.ShaderVariantElements.Sort();
        }

        // 统计数量
        foreach (var shaderVariantInfo in manifest.ShaderVariantInfos)
        {
            manifest.VariantTotalCount += shaderVariantInfo.ShaderVariantElements.Count;
        }
        manifest.ShaderTotalCount = manifest.ShaderVariantInfos.Count;

        return manifest;
    }
}