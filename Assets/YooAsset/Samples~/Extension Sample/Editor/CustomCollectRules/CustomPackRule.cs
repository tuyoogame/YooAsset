using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("打包特效纹理（自定义）")]
public class PackEffectTexture : IBundlePackRule
{
    private const string PackDirectory = "Assets/Effect/Textures/";

    BundlePackRuleResult IBundlePackRule.GetPackRuleResult(BundlePackRuleData data)
    {
        string assetPath = data.AssetPath;
        if (assetPath.StartsWith(PackDirectory) == false)
            throw new Exception($"Only support folder : {PackDirectory}");
    
        string assetName = Path.GetFileName(assetPath).ToLower();
        string firstChar = assetName.Substring(0, 1);
        string bundleName = $"{PackDirectory}effect_texture_{firstChar}";
        var packRuleResult = new BundlePackRuleResult(bundleName, DefaultBundlePackRule.AssetBundleFileExtension);
        return packRuleResult;
    }
}

[DisplayName("打包视频（自定义）")]
public class PackVideo : IBundlePackRule
{
    public BundlePackRuleResult GetPackRuleResult(BundlePackRuleData data)
    {
        string bundleName = RemoveExtension(data.AssetPath);
        string fileExtension = Path.GetExtension(data.AssetPath);
        fileExtension = fileExtension.Remove(0, 1);
        BundlePackRuleResult result = new BundlePackRuleResult(bundleName, fileExtension);
        return result;
    }

    private string RemoveExtension(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        int index = str.LastIndexOf(".");
        if (index == -1)
            return str;
        else
            return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
    }
}