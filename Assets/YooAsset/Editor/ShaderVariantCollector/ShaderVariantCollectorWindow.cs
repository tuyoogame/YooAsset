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
	public class ShaderVariantCollectorWindow : EditorWindow
	{
		[MenuItem("YooAsset/ShaderVariant Collector", false, 201)]
		public static void ShowExample()
		{
			ShaderVariantCollectorWindow window = GetWindow<ShaderVariantCollectorWindow>("着色器变种收集工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private List<string> _packageNames;

		private Button _collectButton;
		private TextField _collectOutputField;
		private Label _currentShaderCountField;
		private Label _currentVariantCountField;
		private PopupField<string> _packageField;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = this.rootVisualElement;

				// 加载布局文件
				var visualAsset = EditorHelper.LoadWindowUXML<ShaderVariantCollectorWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 包裹名称列表
				_packageNames = GetBuildPackageNames();

				// 文件输出目录
				_collectOutputField = root.Q<TextField>("CollectOutput");
				_collectOutputField.SetValueWithoutNotify(ShaderVariantCollectorSettingData.Setting.SavePath);
				_collectOutputField.RegisterValueChangedCallback(evt =>
				{
					ShaderVariantCollectorSettingData.Setting.SavePath = _collectOutputField.value;
				});

				// 收集的包裹
				var packageContainer = root.Q("PackageContainer");
				if (_packageNames.Count > 0)
				{
					int defaultIndex = GetDefaultPackageIndex(ShaderVariantCollectorSettingData.Setting.CollectPackage);
					_packageField = new PopupField<string>(_packageNames, defaultIndex);
					_packageField.label = "Package";
					_packageField.style.width = 350;
					_packageField.RegisterValueChangedCallback(evt =>
					{
						ShaderVariantCollectorSettingData.Setting.CollectPackage = _packageField.value;
					});
					packageContainer.Add(_packageField);
				}
				else
				{
					_packageField = new PopupField<string>();
					_packageField.label = "Package";
					_packageField.style.width = 350;
					packageContainer.Add(_packageField);
				}

				_currentShaderCountField = root.Q<Label>("CurrentShaderCount");
				_currentVariantCountField = root.Q<Label>("CurrentVariantCount");

				// 变种收集按钮
				_collectButton = root.Q<Button>("CollectButton");
				_collectButton.clicked += CollectButton_clicked;
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		private void Update()
		{
			if (_currentShaderCountField != null)
			{
				int currentShaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
				_currentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
			}

			if (_currentVariantCountField != null)
			{
				int currentVariantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
				_currentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
			}
		}

		private void CollectButton_clicked()
		{
			string savePath = ShaderVariantCollectorSettingData.Setting.SavePath;
			string packageName = ShaderVariantCollectorSettingData.Setting.CollectPackage;
			ShaderVariantCollector.Run(savePath, packageName, int.MaxValue, null);
		}

		// 构建包裹相关
		private int GetDefaultPackageIndex(string packageName)
		{
			for (int index = 0; index < _packageNames.Count; index++)
			{
				if (_packageNames[index] == packageName)
				{
					return index;
				}
			}

			ShaderVariantCollectorSettingData.Setting.CollectPackage = _packageNames[0];
			return 0;
		}
		private List<string> GetBuildPackageNames()
		{
			List<string> result = new List<string>();
			foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
			{
				result.Add(package.PackageName);
			}
			return result;
		}
	}
}
#endif