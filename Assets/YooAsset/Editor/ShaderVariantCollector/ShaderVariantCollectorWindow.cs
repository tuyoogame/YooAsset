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

		private Button _collectButton;
		private TextField _collectOutputField;
		private Label _currentShaderCountField;
		private Label _currentVariantCountField;

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

				// 文件输出目录
				_collectOutputField = root.Q<TextField>("CollectOutput");
				_collectOutputField.SetValueWithoutNotify(ShaderVariantCollectorSettingData.Setting.SavePath);
				_collectOutputField.RegisterValueChangedCallback(evt =>
				{
					ShaderVariantCollectorSettingData.Setting.SavePath = _collectOutputField.value;
				});

				_currentShaderCountField = root.Q<Label>("CurrentShaderCount");
				_currentVariantCountField = root.Q<Label>("CurrentVariantCount");

				// 变种收集按钮
				_collectButton = root.Q<Button>("CollectButton");
				_collectButton.clicked += CollectButton_clicked;

				//RefreshWindow();
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
			ShaderVariantCollector.Run(ShaderVariantCollectorSettingData.Setting.SavePath, null);
		}
	}
}
#endif