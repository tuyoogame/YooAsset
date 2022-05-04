#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	public class AssetBundleCollectorWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Collector", false, 101)]
		public static void ShowExample()
		{
			AssetBundleCollectorWindow window = GetWindow<AssetBundleCollectorWindow>("资源包收集工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private List<string> _collectorTypeList;
		private List<string> _addressRuleList;
		private List<string> _packRuleList;
		private List<string> _filterRuleList;
		private ListView _groupListView;
		private ScrollView _collectorScrollView;
		private Toggle _enableAddressableToogle;
		private Toggle _autoCollectShaderToogle;
		private TextField _shaderBundleNameTxt;
		private TextField _groupNameTxt;
		private TextField _groupDescTxt;
		private TextField _groupAssetTagsTxt;
		private VisualElement _groupContainer;

		public void CreateGUI()
		{
			Undo.undoRedoPerformed -= RefreshWindow;
			Undo.undoRedoPerformed += RefreshWindow;

			VisualElement root = this.rootVisualElement;

			_collectorTypeList = new List<string>()
			{
				$"{nameof(ECollectorType.MainAssetCollector)}",
				$"{nameof(ECollectorType.StaticAssetCollector)}",
				$"{nameof(ECollectorType.DependAssetCollector)}"
			};
			_addressRuleList = AssetBundleCollectorSettingData.GetAddressRuleNames();
			_packRuleList = AssetBundleCollectorSettingData.GetPackRuleNames();
			_filterRuleList = AssetBundleCollectorSettingData.GetFilterRuleNames();

			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetSourcePath();
			string uxml = $"{rootPath}/Editor/AssetBundleCollector/{nameof(AssetBundleCollectorWindow)}.uxml";
			var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetBundleCollectorWindow)}.uxml : {uxml}");
				return;
			}
			visualAsset.CloneTree(root);

			try
			{
				// 导入导出按钮
				var exportBtn = root.Q<Button>("ExportButton");
				exportBtn.clicked += ExportBtn_clicked;
				var importBtn = root.Q<Button>("ImportButton");
				importBtn.clicked += ImportBtn_clicked;

				// 公共设置相关
				_enableAddressableToogle = root.Q<Toggle>("EnableAddressable");
				_enableAddressableToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyAddressable(evt.newValue);
				});
				_autoCollectShaderToogle = root.Q<Toggle>("AutoCollectShader");
				_autoCollectShaderToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyShader(evt.newValue, _shaderBundleNameTxt.value);
					_shaderBundleNameTxt.SetEnabled(evt.newValue);
				});
				_shaderBundleNameTxt = root.Q<TextField>("ShaderBundleName");
				_shaderBundleNameTxt.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyShader(_autoCollectShaderToogle.value, evt.newValue);
				});

				// 分组列表相关
				_groupListView = root.Q<ListView>("GroupListView");
				_groupListView.makeItem = MakeGroupListViewItem;
				_groupListView.bindItem = BindGroupListViewItem;
#if UNITY_2020_1_OR_NEWER
				_groupListView.onSelectionChange += GroupListView_onSelectionChange;
#else
				_groupListView.onSelectionChanged += GroupListView_onSelectionChange;
#endif

				// 分组添加删除按钮
				var groupAddContainer = root.Q("GroupAddContainer");
				{
					var addBtn = groupAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddGroupBtn_clicked;
					var removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemoveGroupBtn_clicked;
				}

				// 分组容器
				_groupContainer = root.Q("GroupContainer");

				// 分组名称
				_groupNameTxt = root.Q<TextField>("GroupName");
				_groupNameTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectGroup != null)
					{
						selectGroup.GroupName = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectGroup);
					}
				});

				// 分组备注
				_groupDescTxt = root.Q<TextField>("GroupDesc");
				_groupDescTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectGroup != null)
					{
						selectGroup.GroupDesc = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectGroup);
					}
				});

				// 分组的资源标签
				_groupAssetTagsTxt = root.Q<TextField>("GroupAssetTags");
				_groupAssetTagsTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectGroup != null)
					{
						selectGroup.AssetTags = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectGroup);
					}
				});

				// 收集列表相关
				_collectorScrollView = root.Q<ScrollView>("CollectorScrollView");
				_collectorScrollView.style.height = new Length(100, LengthUnit.Percent);
				_collectorScrollView.viewDataKey = "scrollView";

				// 收集器创建按钮
				var collectorAddContainer = root.Q("CollectorAddContainer");
				{
					var addBtn = collectorAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddCollectorBtn_clicked;
				}

				// 刷新窗体
				RefreshWindow();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			if (AssetBundleCollectorSettingData.IsDirty)
				AssetBundleCollectorSettingData.SaveFile();
		}

		private void RefreshWindow()
		{
			_enableAddressableToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.EnableAddressable);
			_autoCollectShaderToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.AutoCollectShaders);
			_shaderBundleNameTxt.SetEnabled(AssetBundleCollectorSettingData.Setting.AutoCollectShaders);
			_shaderBundleNameTxt.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.ShadersBundleName);
			_groupContainer.visible = false;

			FillGroupViewData();
		}
		private void ExportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFolderPanel("Export XML", "Assets/");
			if (resultPath != null)
			{
				AssetBundleCollectorConfig.ExportXmlConfig($"{resultPath}/{nameof(AssetBundleCollectorConfig)}.xml");
			}
		}
		private void ImportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFilePath("Import XML", "Assets/", "xml");
			if (resultPath != null)
			{
				AssetBundleCollectorConfig.ImportXmlConfig(resultPath);
				RefreshWindow();
			}
		}

		// 分组列表相关
		private void FillGroupViewData()
		{
			_groupListView.Clear();
			_groupListView.ClearSelection();
			_groupListView.itemsSource = AssetBundleCollectorSettingData.Setting.Groups;
			_groupListView.Rebuild();
		}
		private VisualElement MakeGroupListViewItem()
		{
			VisualElement element = new VisualElement();

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.flexGrow = 1f;
				label.style.height = 20f;
				element.Add(label);
			}

			return element;
		}
		private void BindGroupListViewItem(VisualElement element, int index)
		{
			var group = AssetBundleCollectorSettingData.Setting.Groups[index];

			// Group Name
			var textField1 = element.Q<Label>("Label1");
			if (string.IsNullOrEmpty(group.GroupDesc))
				textField1.text = group.GroupName;
			else
				textField1.text = $"{group.GroupName} ({group.GroupDesc})";
		}
		private void GroupListView_onSelectionChange(IEnumerable<object> objs)
		{
			FillCollectorViewData();
		}
		private void AddGroupBtn_clicked()
		{
			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset AddGroup");
			AssetBundleCollectorSettingData.CreateGroup("Default Group");
			FillGroupViewData();
		}
		private void RemoveGroupBtn_clicked()
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset RemoveGroup");

			AssetBundleCollectorSettingData.RemoveGroup(selectGroup);
			FillGroupViewData();
		}

		// 收集列表相关
		private void FillCollectorViewData()
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
			{
				_groupContainer.visible = false;
				return;
			}

			_groupContainer.visible = true;
			_groupNameTxt.SetValueWithoutNotify(selectGroup.GroupName);
			_groupDescTxt.SetValueWithoutNotify(selectGroup.GroupDesc);
			_groupAssetTagsTxt.SetValueWithoutNotify(selectGroup.AssetTags);

			// 填充数据
			_collectorScrollView.Clear();
			for (int i = 0; i < selectGroup.Collectors.Count; i++)
			{
				VisualElement element = MakeCollectorListViewItem();
				BindCollectorListViewItem(element, i);
				_collectorScrollView.Add(element);
			}
		}
		private VisualElement MakeCollectorListViewItem()
		{
			VisualElement element = new VisualElement();

			VisualElement elementTop = new VisualElement();
			elementTop.style.flexDirection = FlexDirection.Row;
			element.Add(elementTop);

			VisualElement elementBottom = new VisualElement();
			elementBottom.style.flexDirection = FlexDirection.Row;
			element.Add(elementBottom);

			VisualElement elementFoldout = new VisualElement();
			elementFoldout.style.flexDirection = FlexDirection.Row;
			element.Add(elementFoldout);

			VisualElement elementSpace = new VisualElement();
			elementSpace.style.flexDirection = FlexDirection.Column;
			element.Add(elementSpace);

			// Top VisualElement
			{
				var button = new Button();
				button.name = "Button1";
				button.text = "-";
				button.style.unityTextAlign = TextAnchor.MiddleCenter;
				button.style.flexGrow = 0f;
				elementTop.Add(button);
			}
			{
				var objectField = new ObjectField();
				objectField.name = "ObjectField1";
				objectField.label = "Collector";
				objectField.objectType = typeof(UnityEngine.Object);
				objectField.style.unityTextAlign = TextAnchor.MiddleLeft;
				objectField.style.flexGrow = 1f;
				elementTop.Add(objectField);
				var label = objectField.Q<Label>();
				label.style.minWidth = 63;
			}

			// Bottom VisualElement
			{
				var label = new Label();
				label.style.width = 90;
				elementBottom.Add(label);
			}
			{
				var popupField = new PopupField<string>(_collectorTypeList, 0);
				popupField.name = "PopupField0";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 150;
				elementBottom.Add(popupField);
			}
			if (_enableAddressableToogle.value)
			{
				var popupField = new PopupField<string>(_addressRuleList, 0);
				popupField.name = "PopupField1";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 200;
				elementBottom.Add(popupField);
			}
			{
				var popupField = new PopupField<string>(_packRuleList, 0);
				popupField.name = "PopupField2";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 150;
				elementBottom.Add(popupField);
			}
			{
				var popupField = new PopupField<string>(_filterRuleList, 0);
				popupField.name = "PopupField3";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 150;
				elementBottom.Add(popupField);
			}
			{
				var textField = new TextField();
				textField.name = "TextField1";
				textField.label = "Tags";
				textField.style.width = 100;
				textField.style.marginLeft = 20;
				textField.style.flexGrow = 1;
				elementBottom.Add(textField);
				var label = textField.Q<Label>();
				label.style.minWidth = 40;
			}

			// Foldout VisualElement
			{
				var label = new Label();
				label.style.width = 90;
				elementFoldout.Add(label);
			}
			{
				var foldout = new Foldout();
				foldout.name = "Foldout1";
				foldout.value = false;
				foldout.text = "Main Assets";
				elementFoldout.Add(foldout);
			}

			// Space VisualElement
			{
				var label = new Label();
				label.style.height = 10;
				elementSpace.Add(label);
			}

			return element;
		}
		private void BindCollectorListViewItem(VisualElement element, int index)
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			var collector = selectGroup.Collectors[index];
			var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);
			if (collectObject != null)
				collectObject.name = collector.CollectPath;

			// Foldout
			var foldout = element.Q<Foldout>("Foldout1");
			foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
					RefreshFoldout(foldout, selectGroup, collector);
				else
					foldout.Clear();
			});

			// Remove Button
			var removeBtn = element.Q<Button>("Button1");
			removeBtn.clicked += () =>
			{
				RemoveCollectorBtn_clicked(collector);
			};

			// Collector Path
			var objectField1 = element.Q<ObjectField>("ObjectField1");
			objectField1.SetValueWithoutNotify(collectObject);
			objectField1.RegisterValueChangedCallback(evt =>
			{
				collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
				objectField1.value.name = collector.CollectPath;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Collector Type
			var popupField0 = element.Q<PopupField<string>>("PopupField0");
			popupField0.index = GetCollectorTypeIndex(collector.CollectorType.ToString());
			popupField0.RegisterValueChangedCallback(evt =>
			{
				collector.CollectorType = StringUtility.NameToEnum<ECollectorType>(evt.newValue);
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Address Rule
			var popupField1 = element.Q<PopupField<string>>("PopupField1");
			if (popupField1 != null)
			{
				popupField1.index = GetAddressRuleIndex(collector.AddressRuleName);
				popupField1.RegisterValueChangedCallback(evt =>
				{
					collector.AddressRuleName = evt.newValue;
					AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
					if (foldout.value)
					{
						RefreshFoldout(foldout, selectGroup, collector);
					}
				});
			}

			// Pack Rule
			var popupField2 = element.Q<PopupField<string>>("PopupField2");
			popupField2.index = GetPackRuleIndex(collector.PackRuleName);
			popupField2.RegisterValueChangedCallback(evt =>
			{
				collector.PackRuleName = evt.newValue;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Filter Rule
			var popupField3 = element.Q<PopupField<string>>("PopupField3");
			popupField3.index = GetFilterRuleIndex(collector.FilterRuleName);
			popupField3.RegisterValueChangedCallback(evt =>
			{
				collector.FilterRuleName = evt.newValue;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Tags
			var textFiled1 = element.Q<TextField>("TextField1");
			textFiled1.SetValueWithoutNotify(collector.AssetTags);
			textFiled1.RegisterValueChangedCallback(evt =>
			{
				collector.AssetTags = evt.newValue;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
			});
		}
		private void RefreshFoldout(Foldout foldout, AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			// 清空旧元素
			foldout.Clear();

			if (collector.IsValid() == false)
			{
				Debug.LogWarning($"The collector is invalid : {collector.CollectPath} in group : {group.GroupName}");
				return;
			}

			if (collector.CollectorType == ECollectorType.MainAssetCollector || collector.CollectorType == ECollectorType.StaticAssetCollector)
			{
				List<CollectAssetInfo> collectAssetInfos = null;

				try
				{
					collectAssetInfos = collector.GetAllCollectAssets(group);
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.ToString());
				}

				if (collectAssetInfos != null)
				{
					foreach (var collectAssetInfo in collectAssetInfos)
					{
						VisualElement elementRow = new VisualElement();
						elementRow.style.flexDirection = FlexDirection.Row;
						foldout.Add(elementRow);

						string showInfo = collectAssetInfo.AssetPath;
						if (_enableAddressableToogle.value)
						{
							IAddressRule instance = AssetBundleCollectorSettingData.GetAddressRuleInstance(collector.AddressRuleName);
							AddressRuleData ruleData = new AddressRuleData(collectAssetInfo.AssetPath, collector.CollectPath, group.GroupName);
							string addressValue = instance.GetAssetAddress(ruleData);
							showInfo = $"[{addressValue}] {showInfo}";
						}

						var label = new Label();
						label.text = showInfo;
						label.style.width = 300;
						label.style.marginLeft = 0;
						label.style.flexGrow = 1;
						elementRow.Add(label);
					}
				}
			}
		}
		private void AddCollectorBtn_clicked()
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			AssetBundleCollectorSettingData.CreateCollector(selectGroup, string.Empty);
			FillCollectorViewData();
		}
		private void RemoveCollectorBtn_clicked(AssetBundleCollector selectCollector)
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;
			if (selectCollector == null)
				return;
			AssetBundleCollectorSettingData.RemoveCollector(selectGroup, selectCollector);
			FillCollectorViewData();
		}

		private int GetCollectorTypeIndex(string typeName)
		{
			for (int i = 0; i < _collectorTypeList.Count; i++)
			{
				if (_collectorTypeList[i] == typeName)
					return i;
			}
			return 0;
		}
		private int GetAddressRuleIndex(string ruleName)
		{
			for (int i = 0; i < _addressRuleList.Count; i++)
			{
				if (_addressRuleList[i] == ruleName)
					return i;
			}
			return 0;
		}
		private int GetPackRuleIndex(string ruleName)
		{
			for (int i = 0; i < _packRuleList.Count; i++)
			{
				if (_packRuleList[i] == ruleName)
					return i;
			}
			return 0;
		}
		private int GetFilterRuleIndex(string ruleName)
		{
			for (int i = 0; i < _filterRuleList.Count; i++)
			{
				if (_filterRuleList[i] == ruleName)
					return i;
			}
			return 0;
		}
	}
}
#endif