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
				case ELanguageKey.ABB_CopyBuildinFileParam: return "内置文件拷贝参数";
				case ELanguageKey.ABB_ClickBuild: return "开始构建";

				// AssetBundleCollector
				case ELanguageKey.ABC_WindowTitle: return "资源包收集工具";
				case ELanguageKey.ABC_GlobalSettings: return "全局设置";
				case ELanguageKey.ABC_PackageSettings: return "包裹设置";
				case ELanguageKey.ABC_ShowPackages: return "显示包裹栏";
				case ELanguageKey.ABC_ShowRuleAlias: return "显示规则昵称";
				case ELanguageKey.ABC_UniqueBundleName: return "资源包名唯一化";
				case ELanguageKey.ABC_EnableAddressable: return "启用可寻址模式";
				case ELanguageKey.ABC_LocationToLower: return "资源定位地址大小写不敏感";
				case ELanguageKey.ABC_IncludeAssetGUID: return "包含资源GUID";
				case ELanguageKey.ABC_IgnoreDefaultType: return "忽略引擎无法识别的类型";
				case ELanguageKey.ABC_AutoCollectShaders: return "自动收集着色器";

				case ELanguageKey.ABC_Fix: return "修复";
				case ELanguageKey.ABC_Import: return "导入";
				case ELanguageKey.ABC_Export: return "导出";
				case ELanguageKey.ABC_Save: return "保存";

				case ELanguageKey.ABC_Packages: return "包裹列表";
				case ELanguageKey.ABC_PackageName: return "包裹名称";
				case ELanguageKey.ABC_PackageDesc: return "包裹介绍";
				case ELanguageKey.ABC_Groups: return "分组列表";
				case ELanguageKey.ABC_ActiveRule: return "激活规则";
				case ELanguageKey.ABC_GroupName: return "分组名称";
				case ELanguageKey.ABC_GroupDesc: return "分组介绍";
				case ELanguageKey.ABC_GroupTags: return "资源标签";
				case ELanguageKey.ABC_Collectors: return "收集器列表";
				case ELanguageKey.ABC_Collector: return "收集器";		
				case ELanguageKey.ABC_UserData: return "用户数据";
				case ELanguageKey.ABC_Tags: return "资源标签";

				case ELanguageKey.ABC_HelpBox1: return "无法同时开启[Enable Addressable]选项和[Location To Lower]选项";
				case ELanguageKey.ABC_HelpBox2: return "检测到当前配置存在多个Package，建议开启[Unique Bundle Name]选项";

				default:
					{
						Debug.LogError($"Not found language : {key}");
						return string.Empty;
					}
			}
		}
	}
}