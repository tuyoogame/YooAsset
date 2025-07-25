using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("定位地址: 文件名.智能尾缀")]
public class AddressByFileNameAndExt : IAddressRule
{
    public string GetAssetAddress(AddressRuleData data)
    {
        var ext = Path.GetExtension(data.AssetPath);
        if (ext == ".asset")
        {
            var a = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(data.AssetPath);
            if (a == null) return ".errortype";
            var type = a.GetType();
            var dt = Path.GetFileNameWithoutExtension(data.AssetPath);
            return dt + $".{type.Name.ToLowerInvariant()}";
        }

        return Path.GetFileName(data.AssetPath);
    }
}