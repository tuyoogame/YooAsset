using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 默认打包规则的工具类
    /// </summary>
    public class DefaultBundlePackRule
    {
        /// <summary>
        /// AssetBundle文件的后缀名
        /// </summary>
        public const string AssetBundleFileExtension = "bundle";

        /// <summary>
        /// 原生文件的后缀名
        /// </summary>
        public const string RawFileExtension = "rawfile";

        /// <summary>
        /// 默认的Unity着色器资源包名称
        /// </summary>
        public const string ShadersBundleName = "unityshaders";

        /// <summary>
        /// 默认的Unity脚本资源包名称
        /// </summary>
        public const string MonosBundleName = "unitymonos";

        /// <summary>
        /// 创建着色器资源包的打包结果
        /// </summary>
        /// <returns>着色器资源包的打包规则结果</returns>
        public static BundlePackRuleResult CreateShadersPackRuleResult()
        {
            BundlePackRuleResult result = new BundlePackRuleResult(ShadersBundleName, AssetBundleFileExtension);
            return result;
        }
        /// <summary>
        /// 创建脚本资源包的打包结果
        /// </summary>
        /// <returns>脚本资源包的打包规则结果</returns>
        public static BundlePackRuleResult CreateMonosPackRuleResult()
        {
            BundlePackRuleResult result = new BundlePackRuleResult(MonosBundleName, AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以文件路径作为资源包名
    /// 注意：每个文件独自打资源包
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image_backgroud.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view_main.bundle"
    /// </summary>
    [DisplayName("资源包名: 文件路径")]
    public class PackSeparately : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName = PathUtility.RemoveExtension(data.AssetPath);
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以父类文件夹路径作为资源包名
    /// 注意：文件夹下所有文件打进一个资源包
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view.bundle"
    /// </summary>
    [DisplayName("资源包名: 父类文件夹路径")]
    public class PackDirectory : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName = Path.GetDirectoryName(data.AssetPath);
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以收集器路径下顶级文件夹为资源包名
    /// 注意：文件夹下所有文件打进一个资源包
    /// 例如：收集器路径为 "Assets/UIPanel"
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
    /// </summary>
    [DisplayName("资源包名: 收集器下顶级文件夹路径")]
    public class PackTopDirectory : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string collectPath = data.CollectPath;
            string assetPath = data.AssetPath;
            if (AssetDatabase.IsValidFolder(collectPath) == false)
                throw new InvalidOperationException($"Collect path must be a folder: '{collectPath}'.");

            string collectPathPrefix = $"{collectPath}/";
            if (assetPath.StartsWith(collectPathPrefix, StringComparison.Ordinal) == false)
                throw new InvalidOperationException($"Asset path '{assetPath}' is not under collect path '{collectPath}'.");

            string relativePath = assetPath.Substring(collectPathPrefix.Length);
            string[] splits = relativePath.Split('/');
            string topDirectory = splits[0];
            if (Path.HasExtension(topDirectory))
                throw new InvalidOperationException($"Root directory not found: '{assetPath}'.");

            string bundleName = $"{collectPath}/{topDirectory}";
            return new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
        }
    }

    /// <summary>
    /// 以收集器路径作为资源包名
    /// 注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 收集器路径")]
    public class PackCollector : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName;
            string collectPath = data.CollectPath;
            if (AssetDatabase.IsValidFolder(collectPath))
            {
                bundleName = collectPath;
            }
            else
            {
                bundleName = PathUtility.RemoveExtension(collectPath);
            }

            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以分组名称作为资源包名
    /// 注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 分组名称")]
    public class PackGroup : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName = data.GroupName;
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包原生文件
    /// </summary>
    [DisplayName("打包原生文件")]
    public class PackRawFile : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName = data.AssetPath;
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.RawFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包视频文件
    /// </summary>
    [DisplayName("打包视频文件")]
    public class PackVideoFile : IBundlePackRule
    {
        /// <inheritdoc/>
        BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
        {
            string bundleName = data.AssetPath;
            string fileExtension = Path.GetExtension(data.AssetPath);
            if (string.IsNullOrEmpty(fileExtension))
                throw new InvalidOperationException($"Video file extension is missing: '{data.AssetPath}'.");

            fileExtension = fileExtension.Remove(0, 1);
            BundlePackRuleResult result = new BundlePackRuleResult(bundleName, fileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包着色器
    /// </summary>
    [DisplayName("打包着色器文件")]
    public class PackShader : IBundlePackRule
    {
        /// <inheritdoc/>
        public BundlePackRuleResult GetPackRuleResult(BundlePackRuleData data)
        {
            return DefaultBundlePackRule.CreateShadersPackRuleResult();
        }
    }

    /// <summary>
    /// 打包着色器变种集合
    /// </summary>
    [DisplayName("打包着色器变种集合文件")]
    public class PackShaderVariants : IBundlePackRule
    {
        /// <inheritdoc/>
        public BundlePackRuleResult GetPackRuleResult(BundlePackRuleData data)
        {
            return DefaultBundlePackRule.CreateShadersPackRuleResult();
        }
    }
}
