using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

/// <summary>
/// 按照文件名的首字母来划分资源
/// </summary>
public class PackEffectTexture : IPackRule
{
	private const string PackDirectory = "Assets/Effect/Textures/";

	string IPackRule.GetBundleName(PackRuleData data)
	{
		string assetPath = data.AssetPath;
		if (assetPath.StartsWith(PackDirectory) == false)
			throw new Exception($"Only support folder : {PackDirectory}");
	
		string assetName = Path.GetFileName(assetPath).ToLower();
		string firstChar = assetName.Substring(0, 1);
		return $"{PackDirectory}effect_texture_{firstChar}";
	}
}