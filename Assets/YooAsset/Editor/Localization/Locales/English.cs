using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	public class English : ILocalization
	{
		public string GetLanguage(ELanguageKey key)
		{
			switch (key)
			{
				case ELanguageKey.None: return "Invalid language";

				// AssetBundleBuilder
				case ELanguageKey.ABB_WindowTitle: return "Asset Bundle Builder";
				case ELanguageKey.AAB_NoPackageTips: return "Not found nny package";
				case ELanguageKey.ABB_BuildOutput: return "Build Output";
				case ELanguageKey.ABB_BuildVersion: return "Build Version";
				case ELanguageKey.ABB_BuildMode: return "Build Mode";
				case ELanguageKey.ABB_Encryption: return "Encryption";
				case ELanguageKey.ABB_Compression: return "Compression";
				case ELanguageKey.ABB_FileNameStyle: return "File Name Style";
				case ELanguageKey.ABB_CopyBuildinFileOption: return "Copy Buildin File Option";
				case ELanguageKey.ABB_CopyBuildinFileParam: return "Copy Buildin File Param";
				case ELanguageKey.ABB_ClickBuild: return "Build";

				// AssetBundleCollector
				case ELanguageKey.ABC_WindowTitle: return "Asset Bundle Collector";
				case ELanguageKey.ABC_GlobalSettings: return "Global Settings";
				case ELanguageKey.ABC_PackageSettings: return "Package Settings";
				case ELanguageKey.ABC_ShowPackages: return "Show Packages";
				case ELanguageKey.ABC_ShowRuleAlias: return "Show Rule Alias";
				case ELanguageKey.ABC_UniqueBundleName: return "Unique Bundle Name";
				case ELanguageKey.ABC_EnableAddressable: return "Enable Addressable";
				case ELanguageKey.ABC_LocationToLower: return "Location To Lower";
				case ELanguageKey.ABC_IncludeAssetGUID: return "Include Asset GUID";
				case ELanguageKey.ABC_IgnoreDefaultType: return "Ignore Default Type";
				case ELanguageKey.ABC_AutoCollectShaders: return "Auto Collect Shaders";

				case ELanguageKey.ABC_Fix: return "Fix";
				case ELanguageKey.ABC_Import: return "Import";
				case ELanguageKey.ABC_Export: return "Export";
				case ELanguageKey.ABC_Save: return "Save";

				case ELanguageKey.ABC_Packages: return "Packages";
				case ELanguageKey.ABC_PackageName: return "Package Name";
				case ELanguageKey.ABC_PackageDesc: return "Package Desc";
				case ELanguageKey.ABC_Groups: return "Groups";
				case ELanguageKey.ABC_ActiveRule: return "Active Rule";
				case ELanguageKey.ABC_GroupName: return "Group Name";
				case ELanguageKey.ABC_GroupDesc: return "Group Desc";
				case ELanguageKey.ABC_GroupTags: return "Asset Tags";
				case ELanguageKey.ABC_Collectors: return "Collectors";
				case ELanguageKey.ABC_Collector: return "Collector";
				case ELanguageKey.ABC_UserData: return "User Data";
				case ELanguageKey.ABC_Tags: return "Asset Tags";

				case ELanguageKey.ABC_HelpBox1: return "The [Enable Addressable] option and [Location To Lower] option cannot be enabled at the same time.";
				case ELanguageKey.ABC_HelpBox2: return "There are multiple Packages in the current config, Recommended to enable the [Unique Bundle Name] option.";

				default:
					{
						Debug.LogError($"Not found language : {key}");
						return string.Empty;
					}
			}
		}
	}
}