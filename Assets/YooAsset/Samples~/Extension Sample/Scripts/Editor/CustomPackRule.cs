using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("打包特效纹理（自定义）")]
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