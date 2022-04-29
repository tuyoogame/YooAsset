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
	public class AssetBundleGrouperWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Grouper", false, 101)]
		public static void ShowExample()
		{
			AssetBundleGrouperWindow window = GetWindow<AssetBundleGrouperWindow>("资源包分组工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private List<string> _addressRuleList;
		private List<string> _packRuleList;
		private List<string> _filterRuleList;
		private ListView _grouperListView;
		private ScrollView _collectorScrollView;
		private Toggle _enableAddressableToogle;
		private Toggle _autoCollectShaderToogle;
		private TextField _shaderBundleNameTxt;
		private TextField _grouperNameTxt;
		private TextField _grouperDescTxt;
		private TextField _grouperAssetTagsTxt;
		private VisualElement _grouperContainer;

		public void CreateGUI()
		{
			Undo.undoRedoPerformed -= RefreshWindow;
			Undo.undoRedoPerformed += RefreshWindow;

			VisualElement root = this.rootVisualElement;

			_addressRuleList = AssetBundleGrouperSettingData.GetAddressRuleNames();
			_packRuleList = AssetBundleGrouperSettingData.GetPackRuleNames();
			_filterRuleList = AssetBundleGrouperSettingData.GetFilterRuleNames();

			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetSourcePath();
			string uxml = $"{rootPath}/Editor/AssetBundleGrouper/{nameof(AssetBundleGrouperWindow)}.uxml";
			var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetBundleGrouperWindow)}.uxml : {uxml}");
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
					AssetBundleGrouperSettingData.ModifyAddressable(evt.newValue);
				});
				_autoCollectShaderToogle = root.Q<Toggle>("AutoCollectShader");
				_autoCollectShaderToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleGrouperSettingData.ModifyShader(evt.newValue, _shaderBundleNameTxt.value);
					_shaderBundleNameTxt.SetEnabled(evt.newValue);
				});
				_shaderBundleNameTxt = root.Q<TextField>("ShaderBundleName");
				_shaderBundleNameTxt.RegisterValueChangedCallback(evt =>
				{
					AssetBundleGrouperSettingData.ModifyShader(_autoCollectShaderToogle.value, evt.newValue);
				});

				// 分组列表相关
				_grouperListView = root.Q<ListView>("GrouperListView");
				_grouperListView.makeItem = MakeGrouperListViewItem;
				_grouperListView.bindItem = BindGrouperListViewItem;
#if UNITY_2020_1_OR_NEWER
				_grouperListView.onSelectionChange += GrouperListView_onSelectionChange;
#else
				_grouperListView.onSelectionChanged += GrouperListView_onSelectionChange;
#endif

				// 分组添加删除按钮
				var grouperAddContainer = root.Q("GrouperAddContainer");
				{
					var addBtn = grouperAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddGrouperBtn_clicked;
					var removeBtn = grouperAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemoveGrouperBtn_clicked;
				}

				// 分组容器
				_grouperContainer = root.Q("GrouperContainer");

				// 分组名称
				_grouperNameTxt = root.Q<TextField>("GrouperName");
				_grouperNameTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
					if (selectGrouper != null)
					{
						selectGrouper.GrouperName = evt.newValue;
						AssetBundleGrouperSettingData.ModifyGrouper(selectGrouper);
					}
				});

				// 分组备注
				_grouperDescTxt = root.Q<TextField>("GrouperDesc");
				_grouperDescTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
					if (selectGrouper != null)
					{
						selectGrouper.GrouperDesc = evt.newValue;
						AssetBundleGrouperSettingData.ModifyGrouper(selectGrouper);
					}
				});

				// 分组的资源标签
				_grouperAssetTagsTxt = root.Q<TextField>("GrouperAssetTags");
				_grouperAssetTagsTxt.RegisterValueChangedCallback(evt =>
				{
					var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
					if (selectGrouper != null)
					{
						selectGrouper.AssetTags = evt.newValue;
						AssetBundleGrouperSettingData.ModifyGrouper(selectGrouper);
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
			if (AssetBundleGrouperSettingData.IsDirty)
				AssetBundleGrouperSettingData.SaveFile();
		}

		private void RefreshWindow()
		{
			_enableAddressableToogle.SetValueWithoutNotify(AssetBundleGrouperSettingData.Setting.EnableAddressable);
			_autoCollectShaderToogle.SetValueWithoutNotify(AssetBundleGrouperSettingData.Setting.AutoCollectShaders);
			_shaderBundleNameTxt.SetEnabled(AssetBundleGrouperSettingData.Setting.AutoCollectShaders);
			_shaderBundleNameTxt.SetValueWithoutNotify(AssetBundleGrouperSettingData.Setting.ShadersBundleName);
			_grouperContainer.visible = false;

			FillGrouperViewData();
		}
		private void ExportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFolderPanel("Export XML", "Assets/");
			if (resultPath != null)
			{
				AssetBundleGrouperConfig.ExportXmlConfig($"{resultPath}/{nameof(AssetBundleGrouperConfig)}.xml");
			}
		}
		private void ImportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFilePath("Import XML", "Assets/", "xml");
			if (resultPath != null)
			{
				AssetBundleGrouperConfig.ImportXmlConfig(resultPath);
				RefreshWindow();
			}
		}

		// 分组列表相关
		private void FillGrouperViewData()
		{
			_grouperListView.Clear();
			_grouperListView.ClearSelection();
			_grouperListView.itemsSource = AssetBundleGrouperSettingData.Setting.Groupers;
			_grouperListView.Rebuild();
		}
		private VisualElement MakeGrouperListViewItem()
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
		private void BindGrouperListViewItem(VisualElement element, int index)
		{
			var grouper = AssetBundleGrouperSettingData.Setting.Groupers[index];

			// Grouper Name
			var textField1 = element.Q<Label>("Label1");
			if (string.IsNullOrEmpty(grouper.GrouperDesc))
				textField1.text = grouper.GrouperName;
			else
				textField1.text = $"{grouper.GrouperName} ({grouper.GrouperDesc})";
		}
		private void GrouperListView_onSelectionChange(IEnumerable<object> objs)
		{
			FillCollectorViewData();
		}
		private void AddGrouperBtn_clicked()
		{
			Undo.RecordObject(AssetBundleGrouperSettingData.Setting, "YooAsset AddGrouper");
			AssetBundleGrouperSettingData.CreateGrouper("Default Grouper");
			FillGrouperViewData();
		}
		private void RemoveGrouperBtn_clicked()
		{
			var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
			if (selectGrouper == null)
				return;

			Undo.RecordObject(AssetBundleGrouperSettingData.Setting, "YooAsset RemoveGrouper");

			AssetBundleGrouperSettingData.RemoveGrouper(selectGrouper);
			FillGrouperViewData();
		}

		// 收集列表相关
		private void FillCollectorViewData()
		{
			var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
			if (selectGrouper == null)
			{
				_grouperContainer.visible = false;
				return;
			}

			_grouperContainer.visible = true;
			_grouperNameTxt.SetValueWithoutNotify(selectGrouper.GrouperName);
			_grouperDescTxt.SetValueWithoutNotify(selectGrouper.GrouperDesc);
			_grouperAssetTagsTxt.SetValueWithoutNotify(selectGrouper.AssetTags);

			// 填充数据
			_collectorScrollView.Clear();
			for (int i = 0; i < selectGrouper.Collectors.Count; i++)
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
				objectField.label = "Collecter";
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
				var toggle = new Toggle();
				toggle.name = "Toggle1";
				toggle.label = "NotWriteToAssetList";
				toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
				toggle.style.width = 150;
				toggle.style.marginLeft = 20;
				elementBottom.Add(toggle);
				var label = toggle.Q<Label>();
				label.style.minWidth = 130;
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
				foldout.text = "Assets";
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
			var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
			if (selectGrouper == null)
				return;

			var collector = selectGrouper.Collectors[index];
			var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);
			if (collectObject != null)
				collectObject.name = collector.CollectPath;

			// Foldout
			var foldout = element.Q<Foldout>("Foldout1");
			RefreshFoldout(foldout, selectGrouper, collector);

			// Remove Button
			var removeBtn = element.Q<Button>("Button1");
			removeBtn.clicked += () =>
			{
				RemoveCollectorBtn_clicked(collector);
			};

			// Collect Path
			var objectField1 = element.Q<ObjectField>("ObjectField1");
			objectField1.SetValueWithoutNotify(collectObject);
			objectField1.RegisterValueChangedCallback(evt =>
			{
				collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
				objectField1.value.name = collector.CollectPath;
				AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
				RefreshFoldout(foldout, selectGrouper, collector);
			});

			// Address Rule
			var popupField1 = element.Q<PopupField<string>>("PopupField1");
			if (popupField1 != null)
			{
				popupField1.index = GetAddressRuleIndex(collector.AddressRuleName);
				popupField1.RegisterValueChangedCallback(evt =>
				{
					collector.AddressRuleName = evt.newValue;
					AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
					RefreshFoldout(foldout, selectGrouper, collector);
				});
			}

			// Pack Rule
			var popupField2 = element.Q<PopupField<string>>("PopupField2");
			popupField2.index = GetPackRuleIndex(collector.PackRuleName);
			popupField2.RegisterValueChangedCallback(evt =>
			{
				collector.PackRuleName = evt.newValue;
				AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
				RefreshFoldout(foldout, selectGrouper, collector);
			});

			// Filter Rule
			var popupField3 = element.Q<PopupField<string>>("PopupField3");
			popupField3.index = GetFilterRuleIndex(collector.FilterRuleName);
			popupField3.RegisterValueChangedCallback(evt =>
			{
				collector.FilterRuleName = evt.newValue;
				AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
				RefreshFoldout(foldout, selectGrouper, collector);
			});

			// NotWriteToAssetList
			var toggle1 = element.Q<Toggle>("Toggle1");
			toggle1.SetValueWithoutNotify(collector.NotWriteToAssetList);
			toggle1.RegisterValueChangedCallback(evt =>
			{
				collector.NotWriteToAssetList = evt.newValue;
				AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
				RefreshFoldout(foldout, selectGrouper, collector);
			});

			// Tags
			var textFiled1 = element.Q<TextField>("TextField1");
			textFiled1.SetValueWithoutNotify(collector.AssetTags);
			textFiled1.RegisterValueChangedCallback(evt =>
			{
				collector.AssetTags = evt.newValue;
				AssetBundleGrouperSettingData.ModifyCollector(selectGrouper, collector);
			});
		}
		private void RefreshFoldout(Foldout foldout, AssetBundleGrouper grouper, AssetBundleCollector collector)
		{
			// 清空旧元素
			foldout.Clear();

			if (collector.IsValid() && collector.NotWriteToAssetList == false)
			{
				List<CollectAssetInfo> collectAssetInfos = null;

				try
				{
					collectAssetInfos = collector.GetAllCollectAssets(grouper);
				}
				catch(System.Exception e)
				{
					Debug.LogError(e.ToString());
				}

				if(collectAssetInfos != null)
				{
					foreach (var collectAssetInfo in collectAssetInfos)
					{
						VisualElement elementRow = new VisualElement();
						elementRow.style.flexDirection = FlexDirection.Row;
						foldout.Add(elementRow);

						string showInfo = collectAssetInfo.AssetPath;
						if (_enableAddressableToogle.value)
						{
							IAddressRule instance = AssetBundleGrouperSettingData.GetAddressRuleInstance(collector.AddressRuleName);
							AddressRuleData ruleData = new AddressRuleData(collectAssetInfo.AssetPath, collector.CollectPath, grouper.GrouperName);
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
			var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
			if (selectGrouper == null)
				return;

			AssetBundleGrouperSettingData.CreateCollector(selectGrouper, string.Empty);
			FillCollectorViewData();
		}
		private void RemoveCollectorBtn_clicked(AssetBundleCollector selectCollector)
		{
			var selectGrouper = _grouperListView.selectedItem as AssetBundleGrouper;
			if (selectGrouper == null)
				return;
			if (selectCollector == null)
				return;
			AssetBundleGrouperSettingData.RemoveCollector(selectGrouper, selectCollector);
			FillCollectorViewData();
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