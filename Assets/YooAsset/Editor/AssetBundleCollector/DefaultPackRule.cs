using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 以文件路径作为资源包名
	/// 注意：每个文件独自打资源包
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image_backgroud.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view_main.bundle"
	/// </summary>
	[DisplayName("以文件路径作为资源包名")]
	public class PackSeparately : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return StringUtility.RemoveExtension(data.AssetPath);
		}
	}

	/// <summary>
	/// 以父类文件夹路径作为资源包名
	/// 注意：文件夹下所有文件打进一个资源包
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view.bundle"
	/// </summary>
	[DisplayName("以父类文件夹路径作为资源包名")]
	public class PackDirectory : IPackRule
	{
		public static PackDirectory StaticPackRule = new PackDirectory();

		string IPackRule.GetBundleName(PackRuleData data)
		{
			return Path.GetDirectoryName(data.AssetPath);
		}
	}

	/// <summary>
	/// 以收集器路径下顶级文件夹为资源包名
	/// 注意：文件夹下所有文件打进一个资源包
	/// 例如：收集器路径为 "Assets/UIPanel"
	/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
	/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
	/// </summary>
	[DisplayName("以收集器路径下顶级文件夹为资源包名")]
	public class PackTopDirectory : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			string assetPath = data.AssetPath.Replace(data.CollectPath, string.Empty);
			assetPath = assetPath.TrimStart('/');
			string[] splits = assetPath.Split('/');
			if (splits.Length > 0)
			{
				if (Path.HasExtension(splits[0]))
					throw new Exception($"Not found root directory : {assetPath}");
				string bundleName = $"{data.CollectPath}/{splits[0]}";
				return bundleName;
			}
			else
			{
				throw new Exception($"Not found root directory : {assetPath}");
			}
		}
	}

	/// <summary>
	/// 以收集器路径作为资源包名
	/// 注意：收集的所有文件打进一个资源包
	/// </summary>
	[DisplayName("以收集器路径作为资源包名")]
	public class PackCollector : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			string collectPath = data.CollectPath;
			if (AssetDatabase.IsValidFolder(collectPath))
			{
				return collectPath;
			}
			else
			{
				return StringUtility.RemoveExtension(collectPath);
			}
		}
	}

	/// <summary>
	/// 以分组名称作为资源包名
	/// 注意：收集的所有文件打进一个资源包
	/// </summary>
	[DisplayName("以分组名称作为资源包名")]
	public class PackGroup : IPackRule
	{
		string IPackRule.GetBundleName(PackRuleData data)
		{
			return data.GroupName;
		}
	}

	/// <summary>
	/// 打包原生文件
	/// 注意：原生文件打包支持：图片，音频，视频，文本
	/// </summary>
	[DisplayName("打包原生文件")]
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

			return data.AssetPath;
		}
	}

	/// <summary>
	/// 打包着色器变种集合
	/// </summary>
	[DisplayName("打包着色器变种集合")]
	public class PackShaderVariants : IPackRule
	{
		public string GetBundleName(PackRuleData data)
		{
			return YooAssetSettings.UnityShadersBundleName;
		}
	}
}