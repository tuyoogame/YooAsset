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

		private Button _saveButton;
		private TextField _buildOutputField;
		private IntegerField _buildVersionField;
		private EnumField _buildPipelineField;
		private EnumField _buildModeField;
		private TextField _buildinTagsField;
		private PopupField<string> _encryptionField;
		private EnumField _compressionField;
		private EnumField _outputNameStyleField;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = this.rootVisualElement;

				// 加载布局文件
				var visualAsset = EditorHelper.LoadWindowUXML<AssetBundleBuilderWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 配置保存按钮
				_saveButton = root.Q<Button>("SaveButton");
				_saveButton.clicked += SaveBtn_clicked;

				// 构建平台
				_buildTarget = EditorUserBuildSettings.activeBuildTarget;

				// 加密服务类
				_encryptionServicesClassTypes = GetEncryptionServicesClassTypes();
				_encryptionServicesClassNames = _encryptionServicesClassTypes.Select(t => t.FullName).ToList();

				// 输出目录
				string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, _buildTarget);
				_buildOutputField = root.Q<TextField>("BuildOutput");
				_buildOutputField.SetValueWithoutNotify(pipelineOutputDirectory);
				_buildOutputField.SetEnabled(false);

				// 构建版本
				_buildVersionField = root.Q<IntegerField>("BuildVersion");
				_buildVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildVersion);
				_buildVersionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildVersion = _buildVersionField.value;
				});

				// 构建管线
				_buildPipelineField = root.Q<EnumField>("BuildPipeline");
				_buildPipelineField.Init(AssetBundleBuilderSettingData.Setting.BuildPipeline);
				_buildPipelineField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildPipeline);
				_buildPipelineField.style.width = 350;
				_buildPipelineField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildPipeline = (EBuildPipeline)_buildPipelineField.value;
					RefreshWindow();
				});

				// 构建模式
				_buildModeField = root.Q<EnumField>("BuildMode");
				_buildModeField.Init(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.style.width = 350;
				_buildModeField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildMode = (EBuildMode)_buildModeField.value;
					RefreshWindow();
				});

				// 内置资源标签
				_buildinTagsField = root.Q<TextField>("BuildinTags");
				_buildinTagsField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildTags);
				_buildinTagsField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildTags = _buildinTagsField.value;
				});

				// 加密方法
				var encryptionContainer = root.Q("EncryptionContainer");
				if (_encryptionServicesClassNames.Count > 0)
				{
					int defaultIndex = GetEncryptionDefaultIndex(AssetBundleBuilderSettingData.Setting.EncyptionClassName);
					_encryptionField = new PopupField<string>(_encryptionServicesClassNames, defaultIndex);
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 350;
					_encryptionField.RegisterValueChangedCallback(evt =>
					{
						AssetBundleBuilderSettingData.IsDirty = true;
						AssetBundleBuilderSettingData.Setting.EncyptionClassName = _encryptionField.value;
					});
					encryptionContainer.Add(_encryptionField);
				}
				else
				{
					_encryptionField = new PopupField<string>();
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 350;
					encryptionContainer.Add(_encryptionField);
				}

				// 压缩方式
				_compressionField = root.Q<EnumField>("Compression");
				_compressionField.Init(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.style.width = 350;
				_compressionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.CompressOption = (ECompressOption)_compressionField.value;
				});

				// 输出文件名称样式
				_outputNameStyleField = root.Q<EnumField>("OutputNameStyle");
				_outputNameStyleField.Init(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
				_outputNameStyleField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
				_outputNameStyleField.style.width = 350;
				_outputNameStyleField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.OutputNameStyle = (EOutputNameStyle)_outputNameStyleField.value;
				});

				// 构建按钮
				var buildButton = root.Q<Button>("Build");
				buildButton.clicked += BuildButton_clicked; ;

				RefreshWindow();
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			if(AssetBundleBuilderSettingData.IsDirty)
				AssetBundleBuilderSettingData.SaveFile();
		}
		public void Update()
		{
			if(_saveButton != null)
			{
				if(AssetBundleBuilderSettingData.IsDirty)
				{
					if (_saveButton.enabledSelf == false)
						_saveButton.SetEnabled(true);
				}
				else
				{
					if(_saveButton.enabledSelf)
						_saveButton.SetEnabled(false);
				}
			}
		}

		private void RefreshWindow()
		{
			var buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			bool enableElement = buildMode == EBuildMode.ForceRebuild;
			_buildinTagsField.SetEnabled(enableElement);
			_encryptionField.SetEnabled(enableElement);
			_compressionField.SetEnabled(enableElement);
			_outputNameStyleField.SetEnabled(enableElement);
		}
		private void SaveBtn_clicked()
		{
			AssetBundleBuilderSettingData.SaveFile();
		}
		private void BuildButton_clicked()
		{
			var buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			if (EditorUtility.DisplayDialog("提示", $"通过构建模式【{buildMode}】来构建！", "Yes", "No"))
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
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = _buildTarget;
			buildParameters.BuildPipeline = AssetBundleBuilderSettingData.Setting.BuildPipeline;
			buildParameters.BuildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			buildParameters.BuildVersion = AssetBundleBuilderSettingData.Setting.BuildVersion;
			buildParameters.BuildinTags = AssetBundleBuilderSettingData.Setting.BuildTags;
			buildParameters.VerifyBuildingResult = true;
			buildParameters.EnableAddressable = AssetBundleCollectorSettingData.Setting.EnableAddressable;
			buildParameters.CopyBuildinTagFiles = AssetBundleBuilderSettingData.Setting.BuildMode == EBuildMode.ForceRebuild;
			buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
			buildParameters.CompressOption = AssetBundleBuilderSettingData.Setting.CompressOption;
			buildParameters.OutputNameStyle = AssetBundleBuilderSettingData.Setting.OutputNameStyle;

			if (AssetBundleBuilderSettingData.Setting.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
				buildParameters.SBPParameters.WriteLinkXML = true;
			}
			
			var builder = new AssetBundleBuilder();
			var buildResult = builder.Run(buildParameters);
			if (buildResult.Success)
			{
				EditorUtility.RevealInFinder($"{buildParameters.OutputRoot}/{buildParameters.BuildTarget}/{buildParameters.BuildVersion}");
			}
		}

		// 加密类相关
		private int GetEncryptionDefaultIndex(string className)
		{
			for (int index = 0; index < _encryptionServicesClassNames.Count; index++)
			{
				if (_encryptionServicesClassNames[index] == className)
				{
					return index;
				}
			}
			return 0;
		}
		private List<Type> GetEncryptionServicesClassTypes()
		{
			return EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
		}
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