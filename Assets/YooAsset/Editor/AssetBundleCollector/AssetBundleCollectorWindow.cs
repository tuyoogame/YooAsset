using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleCollectorWindow : EditorWindow
	{
		static AssetBundleCollectorWindow _thisInstance;

		[MenuItem("YooAsset/AssetBundle Collector", false, 101)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetBundleCollectorWindow), false, "资源包收集工具", true) as AssetBundleCollectorWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		/// <summary>
		/// 上次打开的文件夹路径
		/// </summary>
		private string _lastOpenFolderPath = "Assets/";

		// GUI相关
		private const float GuiDirecotryMinSize = 300f;
		private const float GuiDirecotryMaxSize = 800f;
		private const float GuiPackRuleSize = 130f;
		private const float GuiFilterRuleSize = 130f;
		private const float GuiDontWriteAssetPathSize = 130f;
		private const float GuiAssetTagsMinSize = 100f;
		private const float GuiAssetTagsMaxSize = 300f;
		private const float GuiBtnSize = 40f;
		private Vector2 _scrollPos = Vector2.zero;

		// 初始化相关
		private string[] _packRuleArray = null;
		private string[] _filterRuleArray = null;
		private bool _isInit = false;


		private void Init()
		{
			List<string> packRuleNames = AssetBundleCollectorSettingData.GetPackRuleNames();
			_packRuleArray = packRuleNames.ToArray();

			List<string> filterRuleNames = AssetBundleCollectorSettingData.GetFilterRuleNames();
			_filterRuleArray = filterRuleNames.ToArray();
		}
		private int PackRuleNameToIndex(string name)
		{
			for (int i = 0; i < _packRuleArray.Length; i++)
			{
				if (_packRuleArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToPackRuleName(int index)
		{
			for (int i = 0; i < _packRuleArray.Length; i++)
			{
				if (i == index)
					return _packRuleArray[i];
			}
			return string.Empty;
		}
		private int FilterRuleNameToIndex(string name)
		{
			for (int i = 0; i < _filterRuleArray.Length; i++)
			{
				if (_filterRuleArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToFilterRuleName(int index)
		{
			for (int i = 0; i < _filterRuleArray.Length; i++)
			{
				if (i == index)
					return _filterRuleArray[i];
			}
			return string.Empty;
		}

		private void OnGUI()
		{
			if (_isInit == false)
			{
				_isInit = true;
				Init();
			}

			OnDrawShader();
			OnDrawHeadBar();
			OnDrawCollector();
		}
		private void OnDrawShader()
		{
			bool isCollectAllShader = AssetBundleCollectorSettingData.Setting.AutoCollectShaders;
			string shadersBundleName = AssetBundleCollectorSettingData.Setting.ShadersBundleName;

			EditorGUILayout.Space();

			bool newToggleValue = EditorGUILayout.Toggle("收集所有着色器", isCollectAllShader);
			if (newToggleValue != isCollectAllShader)
			{
				isCollectAllShader = newToggleValue;
				AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, shadersBundleName);
			}

			if (isCollectAllShader)
			{
				string newTextValue = EditorGUILayout.TextField("AssetBundle名称", shadersBundleName, GUILayout.MaxWidth(300));
				if (newTextValue != shadersBundleName)
				{
					shadersBundleName = newTextValue;
					AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, shadersBundleName);
				}
			}
		}
		private void OnDrawHeadBar()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Directory", GUILayout.MinWidth(GuiDirecotryMinSize), GUILayout.MaxWidth(GuiDirecotryMaxSize));
			EditorGUILayout.LabelField("PackRule", GUILayout.MinWidth(GuiPackRuleSize), GUILayout.MaxWidth(GuiPackRuleSize));
			EditorGUILayout.LabelField("FilterRule", GUILayout.MinWidth(GuiFilterRuleSize), GUILayout.MaxWidth(GuiFilterRuleSize));
			EditorGUILayout.LabelField("DontWriteAssetPath", GUILayout.MinWidth(GuiDontWriteAssetPathSize), GUILayout.MaxWidth(GuiDontWriteAssetPathSize));
			EditorGUILayout.LabelField("AssetTags", GUILayout.MinWidth(GuiAssetTagsMinSize), GUILayout.MaxWidth(GuiAssetTagsMaxSize));
			EditorGUILayout.LabelField("", GUILayout.MinWidth(GuiBtnSize), GUILayout.MaxWidth(GuiBtnSize));
			EditorGUILayout.EndHorizontal();
		}
		private void OnDrawCollector()
		{
			// 列表显示
			EditorGUILayout.Space();
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				var collector = AssetBundleCollectorSettingData.Setting.Collectors[i];
				string directory = collector.CollectDirectory;
				string packRuleName = collector.PackRuleName;
				string filterRuleName = collector.FilterRuleName;
				bool dontWriteAssetPath = collector.DontWriteAssetPath;
				string assetTags = collector.AssetTags;

				EditorGUILayout.BeginHorizontal();
				{
					// Directory
					EditorGUILayout.LabelField(directory, GUILayout.MinWidth(GuiDirecotryMinSize), GUILayout.MaxWidth(GuiDirecotryMaxSize));

					// IPackRule
					{
						int index = PackRuleNameToIndex(packRuleName);
						int newIndex = EditorGUILayout.Popup(index, _packRuleArray, GUILayout.MinWidth(GuiPackRuleSize), GUILayout.MaxWidth(GuiPackRuleSize));
						if (newIndex != index)
						{
							packRuleName = IndexToPackRuleName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleName, filterRuleName, dontWriteAssetPath, assetTags);
						}
					}

					// IFilterRule
					{
						int index = FilterRuleNameToIndex(filterRuleName);
						int newIndex = EditorGUILayout.Popup(index, _filterRuleArray, GUILayout.MinWidth(GuiFilterRuleSize), GUILayout.MaxWidth(GuiFilterRuleSize));
						if (newIndex != index)
						{
							filterRuleName = IndexToFilterRuleName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleName, filterRuleName, dontWriteAssetPath, assetTags);
						}
					}

					// DontWriteAssetPath
					{
						bool newToggleValue = EditorGUILayout.Toggle(dontWriteAssetPath, GUILayout.MinWidth(GuiDontWriteAssetPathSize), GUILayout.MaxWidth(GuiDontWriteAssetPathSize));
						if (newToggleValue != dontWriteAssetPath)
						{
							dontWriteAssetPath = newToggleValue;
							AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleName, filterRuleName, dontWriteAssetPath, assetTags);
						}
					}

					// AssetTags
					{
						if (collector.DontWriteAssetPath)
						{
							EditorGUILayout.LabelField(assetTags, GUILayout.MinWidth(GuiAssetTagsMinSize), GUILayout.MaxWidth(GuiAssetTagsMaxSize));
						}
						else
						{
							string newTextValue = EditorGUILayout.TextField(assetTags, GUILayout.MinWidth(GuiAssetTagsMinSize), GUILayout.MaxWidth(GuiAssetTagsMaxSize));
							if (newTextValue != assetTags)
							{
								assetTags = newTextValue;
								AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleName, filterRuleName, dontWriteAssetPath, assetTags);
							}
						}
					}

					if (GUILayout.Button("-", GUILayout.MinWidth(GuiBtnSize), GUILayout.MaxWidth(GuiBtnSize)))
					{
						AssetBundleCollectorSettingData.RemoveCollector(directory);
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			// 添加按钮
			if (GUILayout.Button("+"))
			{
				string resultPath = EditorTools.OpenFolderPanel("Select Folder", _lastOpenFolderPath);
				if (resultPath != null)
				{
					_lastOpenFolderPath = EditorTools.AbsolutePathToAssetPath(resultPath);
					string defaultPackRuleName = nameof(PackExplicit);
					string defaultFilterRuleName = nameof(CollectAll);
					bool defaultDontWriteAssetPathValue = false;
					string defaultAssetTag = string.Empty;
					AssetBundleCollectorSettingData.AddCollector(_lastOpenFolderPath, defaultPackRuleName, defaultFilterRuleName, defaultDontWriteAssetPathValue, defaultAssetTag);
				}
			}

			// 导入配置按钮
			if (GUILayout.Button("Import Config"))
			{
				string resultPath = EditorTools.OpenFilePath("Select File", "Assets/", "xml");
				if (resultPath != null)
				{
					CollectorConfigImporter.ImportXmlConfig(resultPath);
				}
			}
		}
	}
}