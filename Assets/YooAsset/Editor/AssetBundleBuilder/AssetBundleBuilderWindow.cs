using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderWindow : EditorWindow
	{
		static AssetBundleBuilderWindow _thisInstance;

		[MenuItem("YooAsset/AssetBundle Builder", false, 102)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetBundleBuilderWindow), false, "资源包构建工具", true) as AssetBundleBuilderWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		// 构建器
		private readonly AssetBundleBuilder _assetBuilder = new AssetBundleBuilder();

		// 构建参数
		private int _buildVersion;
		private BuildTarget _buildTarget;
		private ECompressOption _compressOption;
		private bool _appendExtension = false;
		private bool _forceRebuild = false;
		private string _buildinTags = string.Empty;

		// 加密类相关
		private List<Type> _encryptionServicesClassTypes;
		private string[] _encryptionServicesClassNames;
		private int _encryptionServicesSelectIndex = -1;

		// 冗余类相关
		private List<Type> _redundancyServicesClassTypes;
		private string[] _redundancyServicesClassNames;
		private int _redundancyServicesSelectIndex = -1;

		// GUI相关
		private bool _isInit = false;
		private GUIStyle _centerStyle;
		private GUIStyle _leftStyle;


		private void OnGUI()
		{
			InitInternal();

			// 标题
			EditorGUILayout.LabelField("Build setup", _centerStyle);
			EditorGUILayout.Space();

			// 输出路径
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, _buildTarget);
			EditorGUILayout.LabelField("Build Output", pipelineOutputDirectory);

			// 构建参数
			_buildVersion = EditorGUILayout.IntField("Build Version", _buildVersion, GUILayout.MaxWidth(300));
			_compressOption = (ECompressOption)EditorGUILayout.EnumPopup("Compression", _compressOption, GUILayout.MaxWidth(300));
			if (_encryptionServicesClassNames.Length > 0)
				_encryptionServicesSelectIndex = EditorGUILayout.Popup("Encryption Services", _encryptionServicesSelectIndex, _encryptionServicesClassNames, GUILayout.MaxWidth(300));
			if (_redundancyServicesClassNames.Length > 0)
				_redundancyServicesSelectIndex = EditorGUILayout.Popup("Redundancy Services", _redundancyServicesSelectIndex, _redundancyServicesClassNames, GUILayout.MaxWidth(300));
			_appendExtension = GUILayout.Toggle(_appendExtension, "Append Extension", GUILayout.MaxWidth(120));
			_forceRebuild = GUILayout.Toggle(_forceRebuild, "Force Rebuild", GUILayout.MaxWidth(120));
			if (_forceRebuild)
				_buildinTags = EditorGUILayout.TextField("Buildin Tags", _buildinTags);

			// 构建按钮
			EditorGUILayout.Space();
			if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
			{
				string title;
				string content;
				if (_forceRebuild)
				{
					title = "警告";
					content = "确定开始强制构建吗，这样会删除所有已有构建的文件";
				}
				else
				{
					title = "提示";
					content = "确定开始增量构建吗";
				}
				if (EditorUtility.DisplayDialog(title, content, "Yes", "No"))
				{
					SaveSettingsToPlayerPrefs();
					EditorTools.ClearUnityConsole();
					EditorApplication.delayCall += ExecuteBuild;
				}
				else
				{
					Debug.LogWarning("[Build] 打包已经取消");
				}
			}
		}
		private void InitInternal()
		{
			if (_isInit)
				return;
			_isInit = true;

			// GUI相关
			_centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
			_centerStyle.alignment = TextAnchor.UpperCenter;
			_leftStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
			_leftStyle.alignment = TextAnchor.MiddleLeft;

			// 构建参数
			var appVersion = new Version(Application.version);
			_buildVersion = appVersion.Revision;
			_buildTarget = EditorUserBuildSettings.activeBuildTarget;

			_encryptionServicesClassTypes = GetEncryptionServicesClassTypes();
			_encryptionServicesClassNames = _encryptionServicesClassTypes.Select(t => t.FullName).ToArray();

			_redundancyServicesClassTypes = GetRedundancyServicesClassTypes();
			_redundancyServicesClassNames = _redundancyServicesClassTypes.Select(t => t.FullName).ToArray();

			// 读取配置
			LoadSettingsFromPlayerPrefs();
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		private void ExecuteBuild()
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.VerifyBuildingResult = true;
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = _buildTarget;
			buildParameters.BuildVersion = _buildVersion;
			buildParameters.CompressOption = _compressOption;
			buildParameters.AppendFileExtension = _appendExtension;
			buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
			buildParameters.RedundancyServices = CreateRedundancyServicesInstance();
			buildParameters.ForceRebuild = _forceRebuild;
			buildParameters.BuildinTags = _buildinTags;
			_assetBuilder.Run(buildParameters);
		}

		private List<Type> GetEncryptionServicesClassTypes()
		{
			List<Type> classTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IEncryptionServices));
			return classTypes;
		}
		private IEncryptionServices CreateEncryptionServicesInstance()
		{
			if (_encryptionServicesSelectIndex < 0)
				return null;
			var classType = _encryptionServicesClassTypes[_encryptionServicesSelectIndex];
			return (IEncryptionServices)Activator.CreateInstance(classType);
		}

		private List<Type> GetRedundancyServicesClassTypes()
		{
			List<Type> classTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IRedundancyServices));
			return classTypes;
		}
		private IRedundancyServices CreateRedundancyServicesInstance()
		{
			if (_redundancyServicesSelectIndex < 0)
				return null;
			var classType = _redundancyServicesClassTypes[_redundancyServicesSelectIndex];
			return (IRedundancyServices)Activator.CreateInstance(classType);
		}

		#region 配置相关
		private const string StrEditorCompressOption = "StrEditorCompressOption";
		private const string StrEditorAppendExtension = "StrEditorAppendExtension";
		private const string StrEditorForceRebuild = "StrEditorForceRebuild";
		private const string StrEditorBuildinTags = "StrEditorBuildinTags";

		/// <summary>
		/// 存储配置
		/// </summary>
		private void SaveSettingsToPlayerPrefs()
		{
			EditorTools.PlayerSetEnum<ECompressOption>(StrEditorCompressOption, _compressOption);
			EditorPrefs.SetBool(StrEditorAppendExtension, _appendExtension);
			EditorPrefs.SetBool(StrEditorForceRebuild, _forceRebuild);
			EditorPrefs.SetString(StrEditorBuildinTags, _buildinTags);
		}

		/// <summary>
		/// 读取配置
		/// </summary>
		private void LoadSettingsFromPlayerPrefs()
		{
			_compressOption = EditorTools.PlayerGetEnum<ECompressOption>(StrEditorCompressOption, ECompressOption.Uncompressed);
			_appendExtension = EditorPrefs.GetBool(StrEditorAppendExtension, false);
			_forceRebuild = EditorPrefs.GetBool(StrEditorForceRebuild, false);
			_buildinTags = EditorPrefs.GetString(StrEditorBuildinTags, string.Empty);
		}
		#endregion
	}
}