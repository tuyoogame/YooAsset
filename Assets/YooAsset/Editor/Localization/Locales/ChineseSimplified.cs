using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	public class ChineseSimplified : ILocalization
	{
		public string GetLanguage(ELanguageKey key)
		{
			switch (key)
			{
				case ELanguageKey.None: return "Invalid language";

				// AssetBundleBuilder
				case ELanguageKey.ABB_WindowTitle: return "资源包构建工具";
				case ELanguageKey.AAB_NoPackageTips: return "没有发现可构建的资源包";
				case ELanguageKey.ABB_BuildOutput: return "构建输出目录";
				case ELanguageKey.ABB_BuildVersion: return "构建版本";
				case ELanguageKey.ABB_BuildMode: return "构建模式";
				case ELanguageKey.ABB_Encryption: return "加密方法";
				case ELanguageKey.ABB_Compression: return "压缩方式";
				case ELanguageKey.ABB_FileNameStyle: return "文件命名方式";
				case ELanguageKey.ABB_CopyBuildinFileOption: return "内置文件拷贝选项";
				case ELanguageKey.ABB_CopyBuildinFileTags: return "内置文件拷贝参数";
				case ELanguageKey.ABB_ClickBuild: return "开始构建";

				// AssetBundleCollector

				default:
					{
						Debug.LogError($"Not found language : {key}");
						return string.Empty;
					}
			}
		}
	}
}