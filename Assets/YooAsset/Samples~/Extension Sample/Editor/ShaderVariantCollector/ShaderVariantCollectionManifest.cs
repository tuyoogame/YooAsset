using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[Serializable]
public class ShaderVariantCollectionManifest
{
    [Serializable]
    public class ShaderVariantElement : IComparable<ShaderVariantElement>
    {
        public string SortValue { private set; get; }

        /// <summary>
        ///  Pass type to use in this variant.
        /// </summary>
        public PassType PassType;

        /// <summary>
        /// Array of shader keywords to use in this variant.
        /// </summary>
        public string[] Keywords;

        public void MakeSortValue()
        {
            string combineKeyword = string.Empty;
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (i == 0)
                    combineKeyword = Keywords[0];
                else
                    combineKeyword = $"{combineKeyword}+{Keywords[0]}";
            }

            SortValue = $"{PassType}+{combineKeyword}";
        }
        public int CompareTo(ShaderVariantElement other)
        {
            return SortValue.CompareTo(other.SortValue);
        }
    }

    [Serializable]
    public class ShaderVariantInfo : IComparable<ShaderVariantInfo>
    {
        public string SortValue { private set; get; }

        /// <summary>
        /// 着色器资源路径.
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

        public void MakeSortValue()
        {
            SortValue = AssetPath + "+" + ShaderName;
        }
        public int CompareTo(ShaderVariantInfo other)
        {
            return SortValue.CompareTo(other.SortValue);
        }
    }


    /// <summary>
    /// Number of shaders in this collection
    /// </summary>
    public int ShaderTotalCount;

    /// <summary>
    /// Number of total varians in this collection
    /// </summary>
    public int VariantTotalCount;

    /// <summary>
    /// Shader variants info list.
    /// </summary>
    public List<ShaderVariantInfo> ShaderVariantInfos = new List<ShaderVariantInfo>(1000);

    /// <summary>
    /// 添加着色器变种信息
    /// </summary>
    public void AddShaderVariant(string assetPath, string shaderName, PassType passType, string[] keywords)
    {
        // 排序Keyword列表
        List<string> temper = new List<string>(keywords);
        temper.Sort();

        var info = GetOrCreateShaderVariantInfo(assetPath, shaderName);
        ShaderVariantElement element = new ShaderVariantElement();
        element.PassType = passType;
        element.Keywords = temper.ToArray();
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
            throw new Exception("Should never get here !");

        return selectList[0];
    }


    /// <summary>
    /// 解析SVC文件并将数据写入到清单
    /// </summary>
    public static ShaderVariantCollectionManifest Extract(ShaderVariantCollection svc)
    {
        var manifest = new ShaderVariantCollectionManifest();
        manifest.ShaderTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
        manifest.VariantTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();

        using (var so = new SerializedObject(svc))
        {
            var shaderArray = so.FindProperty("m_Shaders.Array");
            if (shaderArray != null && shaderArray.isArray)
            {
                for (int i = 0; i < shaderArray.arraySize; ++i)
                {
                    var shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
                    var shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
                    if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference && shaderVariantsArray != null && shaderVariantsArray.isArray)
                    {
                        var shader = shaderRef.objectReferenceValue as Shader;
                        if (shader == null)
                        {
                            throw new Exception("Invalid shader in ShaderVariantCollection file.");
                        }

                        string shaderAssetPath = AssetDatabase.GetAssetPath(shader);
                        string shaderName = shader.name;

                        // 添加变种信息
                        for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
                        {
                            var propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
                            var propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");
                            if (propKeywords != null && propPassType != null && propKeywords.propertyType == SerializedPropertyType.String)
                            {
                                string[] keywords = propKeywords.stringValue.Split(' ');
                                PassType pathType = (PassType)propPassType.intValue;
                                manifest.AddShaderVariant(shaderAssetPath, shaderName, pathType, keywords);
                            }
                        }
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

        return manifest;
    }
}