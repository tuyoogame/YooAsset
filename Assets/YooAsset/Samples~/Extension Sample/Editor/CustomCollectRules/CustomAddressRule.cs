using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

/// <summary>
/// 按文件名生成资源定位地址
/// </summary>
[DisplayName("定位地址: 文件名.智能尾缀")]
public class AddressByFileNameAndExt : IAddressRule
{
    /// <inheritdoc/>
    public string GetAssetAddress(AddressRuleData data)
    {
        var extension = Path.GetExtension(data.AssetPath);
        if (extension == ".asset")
        {
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(data.AssetPath);
            if (asset == null)
                throw new InvalidOperationException($"Asset not found: '{data.AssetPath}'.");

            var assetType = asset.GetType();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(data.AssetPath);
            return fileNameWithoutExtension + $".{assetType.Name.ToLowerInvariant()}";
        }

        return Path.GetFileName(data.AssetPath);
    }
}