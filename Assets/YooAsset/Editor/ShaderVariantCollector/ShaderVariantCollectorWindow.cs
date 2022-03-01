using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class ShaderVariantCollectionWindow : EditorWindow
	{
		static ShaderVariantCollectionWindow _thisInstance;

		[MenuItem("YooAsset/ShaderVariant Collector", false, 203)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(ShaderVariantCollectionWindow), false, "着色器变种收集工具", true) as ShaderVariantCollectionWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _saveFilePath = "Assets/MyShaderVariants.shadervariants";
		private ShaderVariantCollection _selectSVC;

		private void OnGUI()
		{
			EditorGUILayout.Space();
			_saveFilePath = EditorGUILayout.TextField("收集文件保存路径", _saveFilePath);

			int currentShaderCount = ShaderVariantCollector.GetCurrentShaderVariantCollectionShaderCount();
			int currentVariantCount = ShaderVariantCollector.GetCurrentShaderVariantCollectionVariantCount();
			EditorGUILayout.LabelField($"CurrentShaderCount : {currentShaderCount}");
			EditorGUILayout.LabelField($"CurrentVariantCount : {currentVariantCount}");

			// 搜集变种
			EditorGUILayout.Space();
			if (GUILayout.Button("搜集变种", GUILayout.MaxWidth(80)))
			{
				ShaderVariantCollector.Run(_saveFilePath);
			}

			// 查询
			EditorGUILayout.Space();
			if (GUILayout.Button("查询", GUILayout.MaxWidth(80)))
			{
				string resultPath = EditorTools.OpenFilePath("Select File", "Assets/", "shadervariants");
				if (string.IsNullOrEmpty(resultPath))
					return;
				string assetPath = EditorTools.AbsolutePathToAssetPath(resultPath);
				_selectSVC = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(assetPath);
			}
			if (_selectSVC != null)
			{
				EditorGUILayout.LabelField($"ShaderCount : {_selectSVC.shaderCount}");
				EditorGUILayout.LabelField($"VariantCount : {_selectSVC.variantCount}");
			}
		}
	}
}