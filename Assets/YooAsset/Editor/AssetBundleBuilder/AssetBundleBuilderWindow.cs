#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Builder", false, 102)]
		public static void ShowExample()
		{
			AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("资源包构建工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private BuildTarget _buildTarget;
		private List<Type> _encryptionServicesClassTypes;
		private List<string> _encryptionServicesClassNames;

		private TextField _buildOutputTxt;
		private IntegerField _buildVersionField;
		private EnumField _compressionField;
		private PopupField<string> _encryptionField;
		private Toggle _appendExtensionToggle;
		private Toggle _forceRebuildToggle;
		private Toggle _dryRunBuildToggle;
		private TextField _buildTagsTxt;


		public void CreateGUI()
		{
			VisualElement root = this.rootVisualElement;

			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetSourcePath();
			string uxml = $"{rootPath}/Editor/AssetBundleBuilder/{nameof(AssetBundleBuilderWindow)}.uxml";
			var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetBundleBuilderWindow)}.uxml : {uxml}");
				return;
			}
			visualAsset.CloneTree(root);

			try
			{
				_buildTarget = EditorUserBuildSettings.activeBuildTarget;
				_encryptionServicesClassTypes = GetEncryptionServicesClassTypes();
				_encryptionServicesClassNames = _encryptionServicesClassTypes.Select(t => t.FullName).ToList();

				// 输出目录
				string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, _buildTarget);
				_buildOutputTxt = root.Q<TextField>("BuildOutput");
				_buildOutputTxt.SetValueWithoutNotify(pipelineOutputDirectory);
				_buildOutputTxt.SetEnabled(false);

				// 构建版本
				_buildVersionField = root.Q<IntegerField>("BuildVersion");
				_buildVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildVersion);
				_buildVersionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.BuildVersion = _buildVersionField.value;
				});

				// 压缩方式
				_compressionField = root.Q<EnumField>("Compression");
				_compressionField.Init(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.style.width = 300;
				_compressionField.SetEnabled(AssetBundleBuilderSettingData.Setting.DryRunBuild == false);
				_compressionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.CompressOption = (ECompressOption)_compressionField.value;
				});

				// 加密方法
				var encryptionContainer = root.Q("EncryptionContainer");
				if (_encryptionServicesClassNames.Count > 0)
				{
					int defaultIndex = 0;
					for (int index = 0; index < _encryptionServicesClassNames.Count; index++)
					{
						if (_encryptionServicesClassNames[index] == AssetBundleBuilderSettingData.Setting.EncyptionClassName)
						{
							defaultIndex = index;
							break;
						}
					}
					_encryptionField = new PopupField<string>(_encryptionServicesClassNames, defaultIndex);
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 300;
					_encryptionField.SetEnabled(AssetBundleBuilderSettingData.Setting.DryRunBuild == false);
					_encryptionField.RegisterValueChangedCallback(evt =>
					{
						AssetBundleBuilderSettingData.Setting.EncyptionClassName = _encryptionField.value;
					});
					encryptionContainer.Add(_encryptionField);
				}
				else
				{
					_encryptionField = new PopupField<string>();
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 300;
					_encryptionField.SetEnabled(AssetBundleBuilderSettingData.Setting.DryRunBuild == false);
					encryptionContainer.Add(_encryptionField);
				}

				// 附加后缀格式
				_appendExtensionToggle = root.Q<Toggle>("AppendExtension");
				_appendExtensionToggle.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.AppendExtension);
				_appendExtensionToggle.SetEnabled(AssetBundleBuilderSettingData.Setting.DryRunBuild == false);
				_appendExtensionToggle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.AppendExtension = _appendExtensionToggle.value;
				});

				// 强制构建
				_forceRebuildToggle = root.Q<Toggle>("ForceRebuild");
				_forceRebuildToggle.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.ForceRebuild);
				_forceRebuildToggle.SetEnabled(AssetBundleBuilderSettingData.Setting.DryRunBuild == false);
				_forceRebuildToggle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.ForceRebuild = _forceRebuildToggle.value;
					_buildTagsTxt.SetEnabled(_forceRebuildToggle.value);
				});

				// 演练构建
				_dryRunBuildToggle = root.Q<Toggle>("DryRunBuild");
				_dryRunBuildToggle.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.DryRunBuild);
				_dryRunBuildToggle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.DryRunBuild = _dryRunBuildToggle.value;
					_compressionField.SetEnabled(_dryRunBuildToggle.value == false);
					_encryptionField.SetEnabled(_dryRunBuildToggle.value == false);
					_appendExtensionToggle.SetEnabled(_dryRunBuildToggle.value == false);
					_forceRebuildToggle.SetEnabled(_dryRunBuildToggle.value == false);
				});

				// 内置标签
				_buildTagsTxt = root.Q<TextField>("BuildinTags");
				_buildTagsTxt.SetEnabled(_forceRebuildToggle.value);
				_buildTagsTxt.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildTags);
				_buildTagsTxt.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.BuildTags = _buildTagsTxt.value;
				});

				// 构建按钮
				var buildButton = root.Q<Button>("Build");
				buildButton.clicked += BuildButton_clicked; ;
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			AssetBundleBuilderSettingData.SaveFile();
		}

		private void BuildButton_clicked()
		{
			string title;
			string content;
			if (_forceRebuildToggle.value)
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
				EditorTools.ClearUnityConsole();
				EditorApplication.delayCall += ExecuteBuild;
			}
			else
			{
				Debug.LogWarning("[Build] 打包已经取消");
			}
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
			buildParameters.BuildVersion = _buildVersionField.value;
			buildParameters.EnableAddressable = AssetBundleGrouperSettingData.Setting.EnableAddressable;
			buildParameters.CompressOption = (ECompressOption)_compressionField.value;
			buildParameters.AppendFileExtension = _appendExtensionToggle.value;
			buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
			buildParameters.ForceRebuild = _forceRebuildToggle.value;
			buildParameters.DryRunBuild = _dryRunBuildToggle.value;
			buildParameters.BuildinTags = _buildTagsTxt.value;

			AssetBundleBuilder builder = new AssetBundleBuilder();
			builder.Run(buildParameters);
		}

		/// <summary>
		/// 获取加密类的类型列表
		/// </summary>
		private List<Type> GetEncryptionServicesClassTypes()
		{
			TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom<IEncryptionServices>();
			List<Type> classTypes = collection.ToList();
			return classTypes;
		}

		/// <summary>
		/// 创建加密类的实例
		/// </summary>
		private IEncryptionServices CreateEncryptionServicesInstance()
		{
			if (_encryptionField.index < 0)
				return null;
			var classType = _encryptionServicesClassTypes[_encryptionField.index];
			return (IEncryptionServices)Activator.CreateInstance(classType);
		}
	}
}
#endif