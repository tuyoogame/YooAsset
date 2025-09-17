using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    [DisplayName("收集所有资源")]
    public class CollectAll : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.All.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            return true;
        }
    }

    [DisplayName("收集场景")]
    public class CollectScene : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.Scene.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            string extension = Path.GetExtension(data.AssetPath);
            return extension == ".unity" || extension == ".scene";
        }
    }

    [DisplayName("收集预制体")]
    public class CollectPrefab : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.Prefab.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".prefab";
        }
    }

    [DisplayName("收集精灵类型的纹理")]
    public class CollectSprite : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.Sprite.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                var texImporter = AssetImporter.GetAtPath(data.AssetPath) as TextureImporter;
                if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }

    [DisplayName("收集着色器")]
    public class CollectShader : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.Shader.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shader";
        }
    }

    [DisplayName("收集着色器变种集合")]
    public class CollectShaderVariants : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.All.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shadervariants";
        }
    }
}