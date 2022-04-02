using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 以文件路径作为资源包名
	/// 注意：每个文件独自打资源包
	/// </summary>
	public class PackSeparately : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return StringUtility.RemoveExtension(data.AssetPath); //"Assets/Config/test.txt" --> "Assets/Config/test"
		}
	}

	/// <summary>
	/// 以父类文件夹路径作为资源包名
	/// 注意：文件夹下所有文件打进一个资源包
	/// </summary>
	public class PackDirectory : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return Path.GetDirectoryName(data.AssetPath); //"Assets/Config/test.txt" --> "Assets/Config"
		}
	}

	/// <summary>
	/// 以收集器路径作为资源包名
	/// 注意：收集器下所有文件打进一个资源包
	/// </summary>
	public class PackCollector : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return StringUtility.RemoveExtension(data.CollectPath);
		}
	}

	/// <summary>
	/// 以分组名称作为资源包名
	/// 注意：分组内所有文件打进一个资源包
	/// </summary>
	public class PackGrouper : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return data.GrouperName;
		}
	}

	/// <summary>
	/// 原生文件打包模式
	/// 注意：原生文件打包支持：图片，音频，视频，文本
	/// </summary>
	public class PackRawFile : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			string extension = StringUtility.RemoveFirstChar(Path.GetExtension(data.AssetPath));
			if (extension == EAssetFileExtension.unity.ToString() || extension == EAssetFileExtension.prefab.ToString() ||
				extension == EAssetFileExtension.mat.ToString() || extension == EAssetFileExtension.controller.ToString() ||
				extension == EAssetFileExtension.fbx.ToString() || extension == EAssetFileExtension.anim.ToString() ||
				extension == EAssetFileExtension.shader.ToString())
			{
				throw new Exception($"{nameof(PackRawFile)} is not support file estension : {extension}");
			}

			// 注意：原生文件只支持无依赖关系的资源
			string[] depends = AssetDatabase.GetDependencies(data.AssetPath, true);
			if (depends.Length != 1)
				throw new Exception($"{nameof(PackRawFile)} is not support estension : {extension}");

			return StringUtility.RemoveExtension(data.AssetPath);
		}
	}
}