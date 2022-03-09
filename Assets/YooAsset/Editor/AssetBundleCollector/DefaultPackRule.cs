using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 以文件路径作为AssetBundle标签名
	/// </summary>
	public class PackExplicit : IPackRule
	{
		string IPackRule.GetAssetBundleLabel(string assetPath)
		{
			return StringUtility.RemoveExtension(assetPath); //"Assets/Config/test.txt" --> "Assets/Config/test"
		}
	}

	/// <summary>
	/// 以父文件夹路径作为AssetBundle标签名
	/// 注意：该文件夹下所有资源被打到一个AssetBundle文件里
	/// </summary>
	public class PackDirectory : IPackRule
	{
		string IPackRule.GetAssetBundleLabel(string assetPath)
		{
			return Path.GetDirectoryName(assetPath); //"Assets/Config/test.txt" --> "Assets/Config"
		}
	}

	/// <summary>
	/// 原生文件打包模式
	/// 注意：原生文件打包支持：图片，音频，视频，文本
	/// </summary>
	public class PackRawFile : IPackRule
	{
		string IPackRule.GetAssetBundleLabel(string assetPath)
		{
			string extension = StringUtility.RemoveFirstChar(Path.GetExtension(assetPath));
			if (extension == EAssetFileExtension.unity.ToString() || extension == EAssetFileExtension.prefab.ToString() ||
				extension == EAssetFileExtension.mat.ToString() || extension == EAssetFileExtension.controller.ToString() ||
				extension == EAssetFileExtension.fbx.ToString() || extension == EAssetFileExtension.anim.ToString() ||
				extension == EAssetFileExtension.shader.ToString())
			{
				throw new Exception($"{nameof(PackRawFile)} is not support file estension : {extension}");
			}

			// 注意：原生文件只支持无依赖关系的资源
			string[] depends = AssetDatabase.GetDependencies(assetPath, true);
			if (depends.Length != 1)
				throw new Exception($"{nameof(PackRawFile)} is not support estension : {extension}");

			return StringUtility.RemoveExtension(assetPath);
		}
	}
}