using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 收集所有资源的过滤规则
    /// </summary>
    [DisplayName("收集所有资源")]
    public class CollectAll : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.All.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
        {
            return true;
        }
    }

    /// <summary>
    /// 收集场景文件的过滤规则
    /// </summary>
    [DisplayName("收集场景")]
    public class CollectScene : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.Scene.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
        {
            string extension = Path.GetExtension(data.AssetPath);
            return extension == ".unity" || extension == ".scene";
        }
    }

    /// <summary>
    /// 收集预制体的过滤规则
    /// </summary>
    [DisplayName("收集预制体")]
    public class CollectPrefab : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.Prefab.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".prefab";
        }
    }

    /// <summary>
    /// 收集精灵纹理的过滤规则
    /// </summary>
    [DisplayName("收集精灵类型的纹理")]
    public class CollectSprite : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.Sprite.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
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

    /// <summary>
    /// 收集着色器的过滤规则
    /// </summary>
    [DisplayName("收集着色器")]
    public class CollectShader : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.Shader.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shader";
        }
    }

    /// <summary>
    /// 收集着色器变种集合的过滤规则
    /// </summary>
    [DisplayName("收集着色器变种集合")]
    public class CollectShaderVariants : IAssetFilterRule
    {
        /// <inheritdoc/>
        public string FindAssetType
        {
            get { return EAssetFilterType.All.ToString(); }
        }

        /// <inheritdoc/>
        public bool IsCollectAsset(AssetFilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shadervariants";
        }
    }
}
