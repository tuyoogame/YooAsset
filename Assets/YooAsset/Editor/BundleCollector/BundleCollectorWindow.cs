using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集器编辑器窗口
    /// </summary>
    public class BundleCollectorWindow : EditorWindow
    {
        /// <summary>
        /// 打开资源收集器窗口
        /// </summary>
        [MenuItem("YooAsset/Bundle Collector", false, 101)]
        public static void OpenWindow()
        {
            Type[] dockedTypes = EditorWindowDefine.GetDockedWindowTypes();
            BundleCollectorWindow window = GetWindow<BundleCollectorWindow>("Bundle Collector", true, dockedTypes);
            window.minSize = new Vector2(800, 600);
        }

        private const string PlaceholderClass = "search-placeholder";

        private Button _saveButton;
        private List<string> _collectorTypeList;
        private List<RuleDisplayName> _groupActiveRuleList;
        private List<RuleDisplayName> _addressRuleList;
        private List<RuleDisplayName> _bundlePackRuleList;
        private List<RuleDisplayName> _assetFilterRuleList;
        private List<RuleDisplayName> _assetIgnoreRuleList;

        private VisualElement _helpBoxContainer;

        private ToolbarSearchField _searchField;
        private TextField _searchTextField;
        private Button _searchButton;
        private Label _searchResultLabel;

        private Button _globalSettingsButton;
        private Button _packageSettingsButton;

        private VisualElement _setting1Container;
        private Toggle _showPackageToggle;
        private Toggle _showEditorAliasToggle;
        private Toggle _uniqueBundleNameToggle;

        private VisualElement _setting2Container;
        private Toggle _enableAddressableToggle;
        private Toggle _supportExtensionlessToggle;
        private Toggle _locationToLowerToggle;
        private Toggle _includeAssetGUIDToggle;
        private Toggle _autoCollectShadersToggle;
        private PopupField<RuleDisplayName> _ignoreRulePopupField;

        private VisualElement _packageContainer;
        private ListView _packageListView;
        private TextField _packageNameTextField;
        private TextField _packageDescTextField;

        private VisualElement _groupContainer;
        private ListView _groupListView;
        private TextField _groupNameTextField;
        private TextField _groupDescTextField;
        private TextField _groupTagsTextField;

        private VisualElement _collectorContainer;
        private ScrollView _collectorScrollView;
        private PopupField<RuleDisplayName> _activeRulePopupField;

        private string _highlightAssetPath;
        private int _highlightCollectorIndex = -1;
        private int _lastModifyPackageIndex = 0;
        private int _lastModifyGroupIndex = 0;
        private bool _showGlobalSettings = false;
        private bool _showPackageSettings = false;

        /// <summary>
        /// 创建编辑器界面
        /// </summary>
        public void CreateGUI()
        {
            try
            {
                _collectorTypeList = new List<string>()
                {
                    $"{nameof(ECollectorType.MainAssetCollector)}",
                    $"{nameof(ECollectorType.StaticAssetCollector)}",
                    $"{nameof(ECollectorType.DependAssetCollector)}"
                };
                _groupActiveRuleList = BundleCollectorSettingData.GetGroupActiveRuleNames();
                _addressRuleList = BundleCollectorSettingData.GetAddressRuleNames();
                _bundlePackRuleList = BundleCollectorSettingData.GetBundlePackRuleNames();
                _assetFilterRuleList = BundleCollectorSettingData.GetAssetFilterRuleNames();
                _assetIgnoreRuleList = BundleCollectorSettingData.GetAssetIgnoreRuleNames();

                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUxml<BundleCollectorWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 警示栏
                _helpBoxContainer = root.Q("HelpBoxContainer");

                _globalSettingsButton = root.Q<Button>("GlobalSettingsButton");
                _globalSettingsButton.clicked += OnGlobalSettingsButtonClicked;
                _packageSettingsButton = root.Q<Button>("PackageSettingsButton");
                _packageSettingsButton.clicked += OnPackageSettingsButtonClicked;

                // 公共设置相关
                _setting1Container = root.Q("PublicContainer1");
                _showPackageToggle = root.Q<Toggle>("ShowPackages");
                _showPackageToggle.RegisterValueChangedCallback(evt =>
                {
                    BundleCollectorSettingData.ModifyShowPackageView(evt.newValue);
                    RefreshWindow();
                });
                _showEditorAliasToggle = root.Q<Toggle>("ShowRuleAlias");
                _showEditorAliasToggle.RegisterValueChangedCallback(evt =>
                {
                    BundleCollectorSettingData.ModifyShowEditorAlias(evt.newValue);
                    RefreshWindow();
                });
                _uniqueBundleNameToggle = root.Q<Toggle>("UniqueBundleName");
                _uniqueBundleNameToggle.RegisterValueChangedCallback(evt =>
                {
                    BundleCollectorSettingData.ModifyUniqueBundleName(evt.newValue);
                    RefreshWindow();
                });

                // 包裹设置相关
                _setting2Container = root.Q("PublicContainer2");
                _enableAddressableToggle = root.Q<Toggle>("EnableAddressable");
                _enableAddressableToggle.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.EnableAddressable = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        RefreshWindow();
                    }
                });
                _supportExtensionlessToggle = root.Q<Toggle>("SupportExtensionless");
                _supportExtensionlessToggle.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.SupportExtensionless = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        RefreshWindow();
                    }
                });
                _locationToLowerToggle = root.Q<Toggle>("LocationToLower");
                _locationToLowerToggle.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.LocationToLower = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        RefreshWindow();
                    }
                });
                _includeAssetGUIDToggle = root.Q<Toggle>("IncludeAssetGUID");
                _includeAssetGUIDToggle.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.IncludeAssetGUID = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        RefreshWindow();
                    }
                });
                _autoCollectShadersToggle = root.Q<Toggle>("AutoCollectShaders");
                _autoCollectShadersToggle.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.AutoCollectShaders = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        RefreshWindow();
                    }
                });

                // 忽略规则
                _ignoreRulePopupField = new PopupField<RuleDisplayName>(_assetIgnoreRuleList, 0);
                _ignoreRulePopupField.label = "File Ignore Rule";
                _ignoreRulePopupField.name = "IgnoreRulePopupField";
                _ignoreRulePopupField.style.unityTextAlign = TextAnchor.MiddleLeft;
                _ignoreRulePopupField.style.width = 300;
                _ignoreRulePopupField.formatListItemCallback = FormatListItemCallback;
                _ignoreRulePopupField.formatSelectedValueCallback = FormatSelectedValueCallback;
                _ignoreRulePopupField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.IgnoreRuleName = evt.newValue.ClassName;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                    }
                });
                _setting2Container.Add(_ignoreRulePopupField);

                // 配置修复按钮
                var fixBtn = root.Q<Button>("FixButton");
                fixBtn.clicked += OnFixButtonClicked;

                // 导入导出按钮
                var exportBtn = root.Q<Button>("ExportButton");
                exportBtn.clicked += OnExportButtonClicked;
                var importBtn = root.Q<Button>("ImportButton");
                importBtn.clicked += OnImportButtonClicked;

                // 配置保存按钮
                _saveButton = root.Q<Button>("SaveButton");
                _saveButton.clicked += OnSaveButtonClicked;

                // 搜索相关
                _searchField = root.Q<ToolbarSearchField>("SearchField");
                _searchTextField = _searchField.Q<TextField>();
                _searchTextField.RegisterCallback<FocusInEvent>(evt =>
                {
                    if (_searchTextField.ClassListContains(PlaceholderClass))
                    {
                        _searchField.value = string.Empty;
                        ClearSearchPlaceholder();
                    }
                });
                _searchTextField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    if (string.IsNullOrEmpty(_searchField.value))
                        ApplySearchPlaceholder();
                });
                _searchField.RegisterCallback<DragUpdatedEvent>(evt =>
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                });
                _searchField.RegisterCallback<DragPerformEvent>(evt =>
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
                        if (string.IsNullOrEmpty(assetPath) == false)
                        {
                            _searchField.value = assetPath;
                            ClearSearchPlaceholder();
                        }
                    }
                });
                ApplySearchPlaceholder();
                _searchButton = root.Q<Button>("SearchButton");
                _searchButton.clicked += OnSearchButtonClicked;
                _searchResultLabel = root.Q<Label>("SearchResultLabel");

                // 包裹容器
                _packageContainer = root.Q("PackageContainer");

                // 包裹列表相关
                _packageListView = root.Q<ListView>("PackageListView");
                _packageListView.makeItem = MakePackageListViewItem;
                _packageListView.bindItem = BindPackageListViewItem;
#if UNITY_2022_3_OR_NEWER
                _packageListView.selectionChanged += OnPackageListViewSelectionChange;
#elif UNITY_2020_1_OR_NEWER
                _packageListView.onSelectionChange += OnPackageListViewSelectionChange;
#else
                _packageListView.onSelectionChanged += OnPackageListViewSelectionChange;
#endif

                // 包裹添加删除按钮
                var packageAddContainer = root.Q("PackageAddContainer");
                {
                    var addBtn = packageAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += OnAddPackageButtonClicked;
                    var removeBtn = packageAddContainer.Q<Button>("RemoveBtn");
                    removeBtn.clicked += OnRemovePackageButtonClicked;
                }

                // 包裹名称
                _packageNameTextField = root.Q<TextField>("PackageName");
                _packageNameTextField.isDelayed = true;
                _packageNameTextField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.PackageName = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        FillPackageViewData();
                    }
                });

                // 包裹备注
                _packageDescTextField = root.Q<TextField>("PackageDesc");
                _packageDescTextField.isDelayed = true;
                _packageDescTextField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.PackageDesc = evt.newValue;
                        BundleCollectorSettingData.ModifyPackage(selectPackage);
                        FillPackageViewData();
                    }
                });

                // 分组列表相关
                _groupListView = root.Q<ListView>("GroupListView");
                _groupListView.makeItem = MakeGroupListViewItem;
                _groupListView.bindItem = BindGroupListViewItem;
#if UNITY_2022_3_OR_NEWER
                _groupListView.selectionChanged += OnGroupListViewSelectionChange;
#elif UNITY_2020_1_OR_NEWER
                _groupListView.onSelectionChange += OnGroupListViewSelectionChange;
#else
                _groupListView.onSelectionChanged += OnGroupListViewSelectionChange;
#endif

                // 分组添加删除按钮
                var groupAddContainer = root.Q("GroupAddContainer");
                {
                    var addBtn = groupAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += OnAddGroupButtonClicked;
                    var removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
                    removeBtn.clicked += OnRemoveGroupButtonClicked;
                }

                // 分组容器
                _groupContainer = root.Q("GroupContainer");

                // 分组名称
                _groupNameTextField = root.Q<TextField>("GroupName");
                _groupNameTextField.isDelayed = true;
                _groupNameTextField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.GroupName = evt.newValue;
                        BundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                        FillGroupViewData();
                    }
                });

                // 分组备注
                _groupDescTextField = root.Q<TextField>("GroupDesc");
                _groupDescTextField.isDelayed = true;
                _groupDescTextField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.GroupDesc = evt.newValue;
                        BundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                        FillGroupViewData();
                    }
                });

                // 分组的资源标签
                _groupTagsTextField = root.Q<TextField>("GroupTags");
                _groupTagsTextField.isDelayed = true;
                _groupTagsTextField.RegisterValueChangedCallback(evt =>
                {
                    var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                    var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.AssetTags = evt.newValue;
                        BundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                    }
                });

                // 收集列表容器
                _collectorContainer = root.Q("CollectorContainer");

                // 收集列表相关
                _collectorScrollView = root.Q<ScrollView>("CollectorScrollView");
                _collectorScrollView.style.height = new Length(100, LengthUnit.Percent);
                _collectorScrollView.viewDataKey = "scrollView";

                // 收集器创建按钮
                var collectorAddContainer = root.Q("CollectorAddContainer");
                {
                    var addBtn = collectorAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += OnAddCollectorButtonClicked;
                }

                // 分组激活规则
                var activeRuleContainer = root.Q("ActiveRuleContainer");
                {
                    _activeRulePopupField = new PopupField<RuleDisplayName>("Group Active", _groupActiveRuleList, 0);
                    _activeRulePopupField.name = "ActiveRuleMaskField";
                    _activeRulePopupField.style.unityTextAlign = TextAnchor.MiddleLeft;
                    _activeRulePopupField.formatListItemCallback = FormatListItemCallback;
                    _activeRulePopupField.formatSelectedValueCallback = FormatSelectedValueCallback;
                    _activeRulePopupField.RegisterValueChangedCallback(evt =>
                    {
                        var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
                        var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
                        if (selectPackage != null && selectGroup != null)
                        {
                            selectGroup.ActiveRuleName = evt.newValue.ClassName;
                            BundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                            FillGroupViewData();
                        }
                    });
                    activeRuleContainer.Add(_activeRulePopupField);
                }

                // 注册 Undo 回调
                Undo.undoRedoPerformed += RefreshWindow;

                // 刷新窗体
                RefreshWindow();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 窗口销毁时清理
        /// </summary>
        public void OnDestroy()
        {
            if (BundleCollectorSettingData.IsDirty)
                BundleCollectorSettingData.SaveFile();

            // 注销 Undo 回调
            Undo.undoRedoPerformed -= RefreshWindow;
            Undo.ClearUndo(BundleCollectorSettingData.Setting);
        }

        /// <summary>
        /// 编辑器窗口更新
        /// </summary>
        public void Update()
        {
            if (_saveButton != null)
            {
                if (BundleCollectorSettingData.IsDirty)
                {
                    if (_saveButton.enabledSelf == false)
                        _saveButton.SetEnabled(true);
                }
                else
                {
                    if (_saveButton.enabledSelf)
                        _saveButton.SetEnabled(false);
                }
            }
        }

        private void RefreshWindow()
        {
            _highlightAssetPath = null;
            _highlightCollectorIndex = -1;
            _groupContainer.visible = false;
            _collectorContainer.visible = false;

            FillPackageViewData();
            RefreshSettings();
        }
        private void OnFixButtonClicked()
        {
            BundleCollectorSettingData.FixFile();
            RefreshWindow();
        }
        private void OnExportButtonClicked()
        {
            string resultPath = EditorDialogUtility.OpenFolderPanel("Export XML", "Assets/");
            if (resultPath != null)
            {
                BundleCollectorConfig.ExportXmlConfig($"{resultPath}/{nameof(BundleCollectorConfig)}.xml");
            }
        }
        private void OnImportButtonClicked()
        {
            string resultPath = EditorDialogUtility.OpenFilePath("Import XML", "Assets/", "xml");
            if (resultPath != null)
            {
                BundleCollectorConfig.ImportXmlConfig(resultPath);
                RefreshWindow();
            }
        }
        private void OnSaveButtonClicked()
        {
            BundleCollectorSettingData.SaveFile();
        }
        private void OnSearchButtonClicked()
        {
            _highlightAssetPath = null;
            _highlightCollectorIndex = -1;
            FillCollectorViewData();

            string searchInput = GetSearchInput();
            var pathError = CollectAssetSearchUtility.ValidateSearchPath(searchInput);
            if (pathError != ECollectAssetSearchError.None)
            {
                string message = CollectAssetSearchUtility.GetSearchPathErrorMessage(pathError, searchInput);
                ShowSearchResult(message, new Color(1f, 0.4f, 0.4f));
                return;
            }

            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
            {
                string message = "No package selected. Please select a package first.";
                ShowSearchResult(message, new Color(1f, 0.8f, 0.3f));
                return;
            }

            var searchResult = CollectAssetSearchUtility.SearchAssetPath(selectPackage, searchInput);
            if (searchResult == null)
            {
                string message = $"No results found in package '{selectPackage.PackageName}'.";
                ShowSearchResult(message, new Color(1f, 0.8f, 0.3f));
                return;
            }

            string resultMessage = $"Found in group '{searchResult.Group.GroupName}', collector '{searchResult.Collector.CollectPath}'";
            ShowSearchResult(resultMessage, Color.white);

            _searchResultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _searchResultLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _highlightAssetPath = searchResult.AssetPath;
            _highlightCollectorIndex = searchResult.CollectorIndex;

            _groupContainer.visible = true;
            _lastModifyGroupIndex = searchResult.GroupIndex;

            if (_groupListView.selectedIndex == searchResult.GroupIndex)
                FillCollectorViewData();
            else
                _groupListView.selectedIndex = searchResult.GroupIndex;
        }
        private void OnGlobalSettingsButtonClicked()
        {
            _showGlobalSettings = !_showGlobalSettings;
            RefreshGlobalSetting();
        }
        private void OnPackageSettingsButtonClicked()
        {
            _showPackageSettings = !_showPackageSettings;
            RefreshPackageSetting();
        }
        private string FormatListItemCallback(RuleDisplayName ruleDisplayName)
        {
            if (_showEditorAliasToggle.value)
                return ruleDisplayName.DisplayName;
            else
                return ruleDisplayName.ClassName;
        }
        private string FormatSelectedValueCallback(RuleDisplayName ruleDisplayName)
        {
            if (_showEditorAliasToggle.value)
                return ruleDisplayName.DisplayName;
            else
                return ruleDisplayName.ClassName;
        }

        // 搜索栏相关
        private void ShowSearchResult(string message, Color color)
        {
            _searchResultLabel.text = message;
            _searchResultLabel.style.color = color;
            _searchResultLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            _searchResultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _searchResultLabel.style.display = DisplayStyle.Flex;
        }
        private void ClearSearchResult()
        {
            _searchResultLabel.text = string.Empty;
            _searchResultLabel.style.display = DisplayStyle.None;
        }
        private void ApplySearchPlaceholder()
        {
            _searchField.value = "Drag or enter asset path here (e.g. Assets/Res/icon.png)";
            if (_searchTextField.ClassListContains(PlaceholderClass) == false)
                _searchTextField.AddToClassList(PlaceholderClass);

            var inputElement = _searchTextField.Q("unity-text-input");
            inputElement.style.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        }
        private void ClearSearchPlaceholder()
        {
            if (_searchTextField.ClassListContains(PlaceholderClass))
            {
                _searchTextField.RemoveFromClassList(PlaceholderClass);
                var inputElement = _searchTextField.Q("unity-text-input");
                inputElement.style.color = StyleKeyword.Null;
            }
        }
        private string GetSearchInput()
        {
            if (_searchTextField.ClassListContains(PlaceholderClass))
                return string.Empty;
            return _searchField.value;
        }

        // 设置栏相关
        private void RefreshSettings()
        {
            RefreshGlobalSetting();
            RefreshPackageSetting();
            RefreshHelpBoxTips();
        }
        private void RefreshGlobalSetting()
        {
            _showPackageToggle.SetValueWithoutNotify(BundleCollectorSettingData.Setting.ShowPackageView);
            _showEditorAliasToggle.SetValueWithoutNotify(BundleCollectorSettingData.Setting.ShowEditorAlias);
            _uniqueBundleNameToggle.SetValueWithoutNotify(BundleCollectorSettingData.Setting.UniqueBundleName);

            if (_showGlobalSettings)
            {
                _setting1Container.style.display = DisplayStyle.Flex;
            }
            else
            {
                _setting1Container.style.display = DisplayStyle.None;
            }

            if (_showPackageToggle.value)
                _packageContainer.style.display = DisplayStyle.Flex;
            else
                _packageContainer.style.display = DisplayStyle.None;
        }
        private void RefreshPackageSetting()
        {
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage != null)
            {
                string packageSettingName = "Package Settings";
                _packageSettingsButton.SetEnabled(true);
                _packageSettingsButton.text = $"{packageSettingName} ({selectPackage.PackageName})";
                _enableAddressableToggle.SetValueWithoutNotify(selectPackage.EnableAddressable);
                _supportExtensionlessToggle.SetValueWithoutNotify(selectPackage.SupportExtensionless);
                _locationToLowerToggle.SetValueWithoutNotify(selectPackage.LocationToLower);
                _includeAssetGUIDToggle.SetValueWithoutNotify(selectPackage.IncludeAssetGUID);
                _autoCollectShadersToggle.SetValueWithoutNotify(selectPackage.AutoCollectShaders);
                _ignoreRulePopupField.SetValueWithoutNotify(GetAssetIgnoreRuleIndex(selectPackage.IgnoreRuleName));
            }
            else
            {
                _showPackageSettings = false;
                _packageSettingsButton.SetEnabled(false);
                if (_packageListView.itemsSource.Count == 0)
                    _packageSettingsButton.text = $"Not Found Any Package !";
                else
                    _packageSettingsButton.text = $"Package Setting";
            }

            if (_showPackageSettings)
            {
                _setting2Container.style.display = DisplayStyle.Flex;
            }
            else
            {
                _setting2Container.style.display = DisplayStyle.None;
            }
        }
        private void RefreshHelpBoxTips()
        {
#if UNITY_2020_3_OR_NEWER
            _helpBoxContainer.Clear();

            if (_enableAddressableToggle.value && _locationToLowerToggle.value)
            {
                string tips = "The [Enable Addressable] option and [Location To Lower] option cannot be enabled at the same time.";
                var helpBox = new HelpBox(tips, HelpBoxMessageType.Error);
                _helpBoxContainer.Add(helpBox);
            }

            if (BundleCollectorSettingData.Setting.Packages.Count > 1 && _uniqueBundleNameToggle.value == false)
            {
                string tips = "There are multiple Packages in the current config, Recommended to enable the [Unique Bundle Name] option.";
                var helpBox = new HelpBox(tips, HelpBoxMessageType.Warning);
                _helpBoxContainer.Add(helpBox);
            }

            if (_helpBoxContainer.childCount > 0)
                _helpBoxContainer.style.display = DisplayStyle.Flex;
            else
                _helpBoxContainer.style.display = DisplayStyle.None;
#endif
        }

        // 包裹列表相关
        private void FillPackageViewData()
        {
            _packageListView.Clear();
            _packageListView.ClearSelection();
            _packageListView.itemsSource = BundleCollectorSettingData.Setting.Packages;
            _packageListView.Rebuild();

            if (_lastModifyPackageIndex >= 0 && _lastModifyPackageIndex < _packageListView.itemsSource.Count)
            {
                _packageListView.selectedIndex = _lastModifyPackageIndex;
            }
        }
        private VisualElement MakePackageListViewItem()
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
        private void BindPackageListViewItem(VisualElement element, int index)
        {
            var package = BundleCollectorSettingData.Setting.Packages[index];

            var textField1 = element.Q<Label>("Label1");
            if (string.IsNullOrEmpty(package.PackageDesc))
                textField1.text = package.PackageName;
            else
                textField1.text = $"{package.PackageName} ({package.PackageDesc})";
        }
        private void OnPackageListViewSelectionChange(IEnumerable<object> objs)
        {
            ClearSearchResult();

            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
            {
                _groupContainer.visible = false;
                _collectorContainer.visible = false;
            }
            else
            {
                _groupContainer.visible = true;
                _lastModifyPackageIndex = _packageListView.selectedIndex;
                _packageNameTextField.SetValueWithoutNotify(selectPackage.PackageName);
                _packageDescTextField.SetValueWithoutNotify(selectPackage.PackageDesc);

                RefreshSettings();
                FillGroupViewData();
            }
        }
        private void OnAddPackageButtonClicked()
        {
            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow AddPackage");
            BundleCollectorSettingData.CreatePackage("DefaultPackage");
            FillPackageViewData();
            RefreshSettings();
        }
        private void OnRemovePackageButtonClicked()
        {
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
                return;

            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow RemovePackage");
            BundleCollectorSettingData.RemovePackage(selectPackage);
            FillPackageViewData();
            RefreshSettings();
        }

        // 分组列表相关
        private void FillGroupViewData()
        {
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
                return;

            _groupListView.Clear();
            _groupListView.ClearSelection();
            _groupListView.itemsSource = selectPackage.Groups;
            _groupListView.Rebuild();

            if (_lastModifyGroupIndex >= 0 && _lastModifyGroupIndex < _groupListView.itemsSource.Count)
            {
                _groupListView.selectedIndex = _lastModifyGroupIndex;
            }
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
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
                return;

            var group = selectPackage.Groups[index];

            var textField1 = element.Q<Label>("Label1");
            if (string.IsNullOrEmpty(group.GroupDesc))
                textField1.text = group.GroupName;
            else
                textField1.text = $"{group.GroupName} ({group.GroupDesc})";

            // 激活状态
            IGroupActiveRule activeRule = BundleCollectorSettingData.GetGroupActiveRuleInstance(group.ActiveRuleName);
            bool isActive = activeRule.IsActiveGroup(new GroupActiveRuleData(group.GroupName));
            textField1.SetEnabled(isActive);
        }
        private void OnGroupListViewSelectionChange(IEnumerable<object> objs)
        {
            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
            {
                _collectorContainer.visible = false;
                return;
            }

            _collectorContainer.visible = true;
            _lastModifyGroupIndex = _groupListView.selectedIndex;
            _activeRulePopupField.SetValueWithoutNotify(GetGroupActiveRuleIndex(selectGroup.ActiveRuleName));
            _groupNameTextField.SetValueWithoutNotify(selectGroup.GroupName);
            _groupDescTextField.SetValueWithoutNotify(selectGroup.GroupDesc);
            _groupTagsTextField.SetValueWithoutNotify(selectGroup.AssetTags);

            FillCollectorViewData();
        }
        private void OnAddGroupButtonClicked()
        {
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
                return;

            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow AddGroup");
            BundleCollectorSettingData.CreateGroup(selectPackage, "Default Group");
            FillGroupViewData();
        }
        private void OnRemoveGroupButtonClicked()
        {
            var selectPackage = _packageListView.selectedItem as BundleCollectorPackage;
            if (selectPackage == null)
                return;

            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
                return;

            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow RemoveGroup");
            BundleCollectorSettingData.RemoveGroup(selectPackage, selectGroup);
            FillGroupViewData();
        }

        // 收集列表相关
        private void FillCollectorViewData()
        {
            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
                return;

            // 填充数据
            _collectorScrollView.Clear();
            for (int i = 0; i < selectGroup.Collectors.Count; i++)
            {
                VisualElement element = MakeCollectorListViewItem();
                BindCollectorListViewItem(element, i);
                _collectorScrollView.Add(element);
            }

            if (_highlightCollectorIndex >= 0 && _highlightCollectorIndex < selectGroup.Collectors.Count)
            {
                var targetElement = _collectorScrollView[_highlightCollectorIndex];
                var foldout = targetElement.Q<Foldout>("Foldout1");
                if (foldout != null)
                    foldout.value = true;
                _highlightCollectorIndex = -1;
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
                UIElementsTools.SetObjectFieldShowPath(objectField);
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
            if (_enableAddressableToggle.value)
            {
                var popupField = new PopupField<RuleDisplayName>(_addressRuleList, 0);
                popupField.name = "PopupField1";
                popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
                popupField.style.width = 220;
                elementBottom.Add(popupField);
            }
            {
                var popupField = new PopupField<RuleDisplayName>(_bundlePackRuleList, 0);
                popupField.name = "PopupField2";
                popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
                popupField.style.width = 220;
                elementBottom.Add(popupField);
            }
            {
                var popupField = new PopupField<RuleDisplayName>(_assetFilterRuleList, 0);
                popupField.name = "PopupField3";
                popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
                popupField.style.width = 150;
                elementBottom.Add(popupField);
            }
            {
                var textField = new TextField();
                textField.name = "TextField0";
                textField.label = "User Data";
                textField.isDelayed = true;
                textField.style.width = 200;
                elementBottom.Add(textField);
                var label = textField.Q<Label>();
                label.style.minWidth = 63;
            }
            {
                var textField = new TextField();
                textField.name = "TextField1";
                textField.label = "Asset Tags";
                textField.isDelayed = true;
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
            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
                return;

            var collector = selectGroup.Collectors[index];
            var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);

            // 注意：非主资源收集器的标签栏需要被冻结
            var textTags = element.Q<TextField>("TextField1");
            if (collector.CollectorType == ECollectorType.MainAssetCollector)
                textTags.SetEnabled(true);
            else
                textTags.SetEnabled(false);

            // Foldout
            var foldout = element.Q<Foldout>("Foldout1");
            foldout.RegisterValueChangedCallback(evt =>
            {
                RefreshFoldoutContent(foldout, selectGroup, collector);
            });
            RefreshFoldoutName(foldout, collector.CollectorType);

            // Remove Button
            var removeBtn = element.Q<Button>("Button1");
            removeBtn.clicked += () =>
            {
                OnRemoveCollectorButtonClicked(collector);
            };

            // Collector Path
            var objectField1 = element.Q<ObjectField>("ObjectField1");
            objectField1.SetValueWithoutNotify(collectObject);
            objectField1.RegisterValueChangedCallback(evt =>
            {
                collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
                collector.CollectorGUID = AssetDatabase.AssetPathToGUID(collector.CollectPath);
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                RefreshFoldoutContent(foldout, selectGroup, collector);
            });
            UIElementsTools.RefreshObjectFieldShowPath(objectField1);

            // Collector Type
            var popupField0 = element.Q<PopupField<string>>("PopupField0");
            popupField0.index = GetCollectorTypeIndex(collector.CollectorType.ToString());
            popupField0.RegisterValueChangedCallback(evt =>
            {
                collector.CollectorType = EditorStringUtility.ParseEnum<ECollectorType>(evt.newValue);
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                RefreshFoldoutContent(foldout, selectGroup, collector);

                if (collector.CollectorType == ECollectorType.MainAssetCollector)
                    textTags.SetEnabled(true);
                else
                    textTags.SetEnabled(false);
            });

            // Address Rule
            var popupField1 = element.Q<PopupField<RuleDisplayName>>("PopupField1");
            if (popupField1 != null)
            {
                popupField1.index = GetAddressRuleIndex(collector.AddressRuleName);
                popupField1.formatListItemCallback = FormatListItemCallback;
                popupField1.formatSelectedValueCallback = FormatSelectedValueCallback;
                popupField1.RegisterValueChangedCallback(evt =>
                {
                    collector.AddressRuleName = evt.newValue.ClassName;
                    BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                    RefreshFoldoutContent(foldout, selectGroup, collector);
                });
            }

            // Pack Rule
            var popupField2 = element.Q<PopupField<RuleDisplayName>>("PopupField2");
            popupField2.index = GetBundlePackRuleIndex(collector.PackRuleName);
            popupField2.formatListItemCallback = FormatListItemCallback;
            popupField2.formatSelectedValueCallback = FormatSelectedValueCallback;
            popupField2.RegisterValueChangedCallback(evt =>
            {
                collector.PackRuleName = evt.newValue.ClassName;
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                RefreshFoldoutContent(foldout, selectGroup, collector);
            });

            // Filter Rule
            var popupField3 = element.Q<PopupField<RuleDisplayName>>("PopupField3");
            popupField3.index = GetAssetFilterRuleIndex(collector.FilterRuleName);
            popupField3.formatListItemCallback = FormatListItemCallback;
            popupField3.formatSelectedValueCallback = FormatSelectedValueCallback;
            popupField3.RegisterValueChangedCallback(evt =>
            {
                collector.FilterRuleName = evt.newValue.ClassName;
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                RefreshFoldoutContent(foldout, selectGroup, collector);
            });

            // UserData
            var textField0 = element.Q<TextField>("TextField0");
            textField0.SetValueWithoutNotify(collector.UserData);
            textField0.RegisterValueChangedCallback(evt =>
            {
                collector.UserData = evt.newValue;
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
            });

            // Tags
            var textField1 = element.Q<TextField>("TextField1");
            textField1.SetValueWithoutNotify(collector.AssetTags);
            textField1.RegisterValueChangedCallback(evt =>
            {
                collector.AssetTags = evt.newValue;
                BundleCollectorSettingData.ModifyCollector(selectGroup, collector);
            });
        }
        private void RefreshFoldoutName(Foldout foldout, ECollectorType collectorType, int elementNumber = -1)
        {
            if (collectorType == ECollectorType.MainAssetCollector)
            {
                if (elementNumber >= 0)
                    foldout.text = $"Main Assets ({elementNumber})";
                else
                    foldout.text = $"Main Assets";
            }
            else if (collectorType == ECollectorType.StaticAssetCollector)
            {
                if (elementNumber >= 0)
                    foldout.text = $"Static Assets ({elementNumber})";
                else
                    foldout.text = $"Static Assets";
            }
            else if (collectorType == ECollectorType.DependAssetCollector)
            {
                if (elementNumber >= 0)
                    foldout.text = $"Depend Assets ({elementNumber})";
                else
                    foldout.text = $"Depend Assets";
            }
            else
            {
                throw new System.NotImplementedException(collectorType.ToString());
            }
        }
        private void RefreshFoldoutContent(Foldout foldout, BundleCollectorGroup group, BundleCollector collector)
        {
            RefreshFoldoutName(foldout, collector.CollectorType);

            // 折叠栏不可见
            if (foldout.value == false)
            {
                foldout.Clear();
                return;
            }

            // 清空旧元素
            foldout.Clear();

            List<CollectAssetInfo> collectAssetInfos = null;

            try
            {
                IAssetIgnoreRule ignoreRule = BundleCollectorSettingData.GetAssetIgnoreRuleInstance(_ignoreRulePopupField.value.ClassName);
                string packageName = _packageNameTextField.value;
                var command = new CollectCommand(packageName, ignoreRule);
                command.SetFlag(ECollectFlags.IgnoreGetDependencies, true);
                command.UniqueBundleName = _uniqueBundleNameToggle.value;
                command.EnableAddressable = _enableAddressableToggle.value;
                command.SupportExtensionless = _supportExtensionlessToggle.value;
                command.LocationToLower = _locationToLowerToggle.value;
                command.IncludeAssetGUID = _includeAssetGUIDToggle.value;
                command.AutoCollectShaders = _autoCollectShadersToggle.value;

                // 检测配置是否有效
                collector.CheckConfigError();

                // 收集有效资源信息
                collectAssetInfos = collector.GetAllCollectAssets(command, group);
            }
            catch (Exception e)
            {
                Debug.LogError($"Invalid collector : {collector.CollectPath}, error: {e.Message}");
            }

            if (collectAssetInfos != null)
            {
                bool showAddress = false;
                if (_enableAddressableToggle.value && collector.CollectorType == ECollectorType.MainAssetCollector)
                    showAddress = true;

                RefreshFoldoutName(foldout, collector.CollectorType, collectAssetInfos.Count);
                foreach (var collectAsset in collectAssetInfos)
                {
                    VisualElement elementRow = new VisualElement();
                    elementRow.style.flexDirection = FlexDirection.Row;
                    foldout.Add(elementRow);

                    string showInfo = collectAsset.AssetInfo.AssetPath;
                    if (showAddress)
                        showInfo = $"[{collectAsset.Address}] {collectAsset.AssetInfo.AssetPath}";

                    var label = new Label();
                    label.text = showInfo;
                    label.style.width = 300;
                    label.style.marginLeft = 0;
                    label.style.flexGrow = 1;

                    if (string.IsNullOrEmpty(_highlightAssetPath) == false &&
                        string.Equals(collectAsset.AssetInfo.AssetPath, _highlightAssetPath, StringComparison.OrdinalIgnoreCase))
                    {
                        label.style.color = new Color(1f, 0.2f, 0.2f);
                    }

                    elementRow.Add(label);
                }
            }
        }
        private void OnAddCollectorButtonClicked()
        {
            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
                return;

            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow AddCollector");
            BundleCollector collector = new BundleCollector();
            BundleCollectorSettingData.CreateCollector(selectGroup, collector);
            FillCollectorViewData();
        }
        private void OnRemoveCollectorButtonClicked(BundleCollector selectCollector)
        {
            var selectGroup = _groupListView.selectedItem as BundleCollectorGroup;
            if (selectGroup == null)
                return;
            if (selectCollector == null)
                return;

            Undo.RecordObject(BundleCollectorSettingData.Setting, "YooAsset.BundleCollectorWindow RemoveCollector");
            BundleCollectorSettingData.RemoveCollector(selectGroup, selectCollector);
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
                if (_addressRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private int GetBundlePackRuleIndex(string ruleName)
        {
            for (int i = 0; i < _bundlePackRuleList.Count; i++)
            {
                if (_bundlePackRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private int GetAssetFilterRuleIndex(string ruleName)
        {
            for (int i = 0; i < _assetFilterRuleList.Count; i++)
            {
                if (_assetFilterRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private RuleDisplayName GetAssetIgnoreRuleIndex(string ruleName)
        {
            for (int i = 0; i < _assetIgnoreRuleList.Count; i++)
            {
                if (_assetIgnoreRuleList[i].ClassName == ruleName)
                    return _assetIgnoreRuleList[i];
            }
            if (_assetIgnoreRuleList.Count == 0)
                throw new InvalidOperationException($"No {nameof(IAssetIgnoreRule)} implementations found.");
            return _assetIgnoreRuleList[0];
        }
        private RuleDisplayName GetGroupActiveRuleIndex(string ruleName)
        {
            for (int i = 0; i < _groupActiveRuleList.Count; i++)
            {
                if (_groupActiveRuleList[i].ClassName == ruleName)
                    return _groupActiveRuleList[i];
            }
            if (_groupActiveRuleList.Count == 0)
                throw new InvalidOperationException($"No {nameof(IGroupActiveRule)} implementations found.");
            return _groupActiveRuleList[0];
        }
    }
}