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
    public class AssetArtScannerWindow : EditorWindow
    {
        [MenuItem("YooAsset/AssetArt Scanner", false, 301)]
        public static void OpenWindow()
        {
            AssetArtScannerWindow window = GetWindow<AssetArtScannerWindow>("AssetArt Scanner", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(800, 600);
        }

        private Button _saveButton;
        private ListView _scannerListView;
        private ToolbarSearchField _scannerSearchField;
        private VisualElement _scannerContentContainer;
        private VisualElement _inspectorContainer;
        private Label _schemaGuideTxt;
        private TextField _scannerNameTxt;
        private TextField _scannerDescTxt;
        private ObjectField _scannerSchemaField;
        private ObjectField _outputFolderField;
        private ScrollView _collectorScrollView;

        private int _lastModifyScannerIndex = 0;


        public void CreateGUI()
        {
            Undo.undoRedoPerformed -= RefreshWindow;
            Undo.undoRedoPerformed += RefreshWindow;

            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<AssetArtScannerWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 导入导出按钮
                var exportBtn = root.Q<Button>("ExportButton");
                exportBtn.clicked += ExportBtn_clicked;
                var importBtn = root.Q<Button>("ImportButton");
                importBtn.clicked += ImportBtn_clicked;

                // 配置保存按钮
                _saveButton = root.Q<Button>("SaveButton");
                _saveButton.clicked += SaveBtn_clicked;

                // 扫描按钮
                var scanAllBtn = root.Q<Button>("ScanAllButton");
                scanAllBtn.clicked += ScanAllBtn_clicked;
                var scanBtn = root.Q<Button>("ScanBtn");
                scanBtn.clicked += ScanBtn_clicked;

                // 扫描列表相关
                _scannerListView = root.Q<ListView>("ScannerListView");
                _scannerListView.makeItem = MakeScannerListViewItem;
                _scannerListView.bindItem = BindScannerListViewItem;
#if UNITY_2022_3_OR_NEWER
                _scannerListView.selectionChanged += ScannerListView_onSelectionChange;
#elif  UNITY_2020_1_OR_NEWER
                _scannerListView.onSelectionChange += ScannerListView_onSelectionChange;
#else
                _scannerListView.onSelectionChanged += ScannerListView_onSelectionChange;
#endif

                // 扫描列表过滤
                _scannerSearchField = root.Q<ToolbarSearchField>("ScannerSearchField");
                _scannerSearchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

                // 扫描器添加删除按钮
                var scannerAddContainer = root.Q("ScannerAddContainer");
                {
                    var addBtn = scannerAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += AddScannerBtn_clicked;
                    var removeBtn = scannerAddContainer.Q<Button>("RemoveBtn");
                    removeBtn.clicked += RemoveScannerBtn_clicked;
                }

                // 扫描器容器
                _scannerContentContainer = root.Q("ScannerContentContainer");

                // 检视界面容器
                _inspectorContainer = root.Q("InspectorContainer");

                // 扫描器指南
                _schemaGuideTxt = root.Q<Label>("SchemaUserGuide");

                // 扫描器名称
                _scannerNameTxt = root.Q<TextField>("ScannerName");
                _scannerNameTxt.RegisterValueChangedCallback(evt =>
                {
                    var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
                    if (selectScanner != null)
                    {
                        selectScanner.ScannerName = evt.newValue;
                        AssetArtScannerSettingData.ModifyScanner(selectScanner);
                        FillScannerListViewData();
                    }
                });

                // 扫描器备注
                _scannerDescTxt = root.Q<TextField>("ScannerDesc");
                _scannerDescTxt.RegisterValueChangedCallback(evt =>
                {
                    var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
                    if (selectScanner != null)
                    {
                        selectScanner.ScannerDesc = evt.newValue;
                        AssetArtScannerSettingData.ModifyScanner(selectScanner);
                        FillScannerListViewData();
                    }
                });

                // 扫描模式
                _scannerSchemaField = root.Q<ObjectField>("ScanSchema");
                _scannerSchemaField.RegisterValueChangedCallback(evt =>
                {
                    var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
                    if (selectScanner != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                        selectScanner.ScannerSchema = AssetDatabase.AssetPathToGUID(assetPath);
                        AssetArtScannerSettingData.ModifyScanner(selectScanner);
                    }
                });

                // 存储目录
                _outputFolderField = root.Q<ObjectField>("OutputFolder");
                _outputFolderField.RegisterValueChangedCallback(evt =>
                {
                    var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
                    if (selectScanner != null)
                    {
                        if (evt.newValue == null)
                        {
                            selectScanner.SaveDirectory = string.Empty;
                            AssetArtScannerSettingData.ModifyScanner(selectScanner);
                        }
                        else
                        {
                            string assetPath = AssetDatabase.GetAssetPath(evt.newValue);
                            if (AssetDatabase.IsValidFolder(assetPath))
                            {
                                selectScanner.SaveDirectory = assetPath;
                                AssetArtScannerSettingData.ModifyScanner(selectScanner);
                            }
                            else
                            {
                                Debug.LogWarning($"Select asset object not folder ! {assetPath}");
                            }
                        }
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
            // 注意：清空所有撤销操作
            Undo.ClearAll();

            if (AssetArtScannerSettingData.IsDirty)
                AssetArtScannerSettingData.SaveFile();
        }
        public void Update()
        {
            if (_saveButton != null)
            {
                if (AssetArtScannerSettingData.IsDirty)
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
            _scannerContentContainer.visible = false;

            FillScannerListViewData();
        }
        private void ExportBtn_clicked()
        {
            string resultPath = EditorTools.OpenFolderPanel("Export JSON", "Assets/");
            if (resultPath != null)
            {
                AssetArtScannerConfig.ExportJsonConfig($"{resultPath}/AssetArtScannerConfig.json");
            }
        }
        private void ImportBtn_clicked()
        {
            string resultPath = EditorTools.OpenFilePath("Import JSON", "Assets/", "json");
            if (resultPath != null)
            {
                AssetArtScannerConfig.ImportJsonConfig(resultPath);
                RefreshWindow();
            }
        }
        private void SaveBtn_clicked()
        {
            AssetArtScannerSettingData.SaveFile();
        }
        private void ScanAllBtn_clicked()
        {
            if (EditorUtility.DisplayDialog("提示", $"开始全面扫描！", "Yes", "No"))
            {
                string searchKeyWord = _scannerSearchField.value;
                AssetArtScannerSettingData.ScanAll(searchKeyWord);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("全面扫描已经取消");
            }
        }
        private void ScanBtn_clicked()
        {
            var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectScanner == null)
                return;

            ScannerResult scannerResult = AssetArtScannerSettingData.Scan(selectScanner.ScannerGUID);
            if (scannerResult.Succeed)
            {
                // 自动打开报告界面
                scannerResult.OpenReportWindow();
                AssetDatabase.Refresh();
            }
        }
        private void OnSearchKeyWordChange(ChangeEvent<string> e)
        {
            _lastModifyScannerIndex = 0;
            RefreshWindow();
        }

        // 分组列表相关
        private void FillScannerListViewData()
        {
            _scannerListView.Clear();
            _scannerListView.ClearSelection();

            var filterItems = FilterScanners();
            if (AssetArtScannerSettingData.Setting.Scanners.Count == filterItems.Count)
            {
#if UNITY_2020_3_OR_NEWER
                _scannerListView.reorderable = true;
#endif
                _scannerListView.itemsSource = AssetArtScannerSettingData.Setting.Scanners;
                _scannerListView.Rebuild();
            }
            else
            {
#if UNITY_2020_3_OR_NEWER
                _scannerListView.reorderable = false;
#endif
                _scannerListView.itemsSource = filterItems;
                _scannerListView.Rebuild();
            }

            if (_lastModifyScannerIndex >= 0 && _lastModifyScannerIndex < _scannerListView.itemsSource.Count)
            {
                _scannerListView.selectedIndex = _lastModifyScannerIndex;
            }
        }
        private List<AssetArtScanner> FilterScanners()
        {
            string searchKeyWord = _scannerSearchField.value;
            List<AssetArtScanner> result = new List<AssetArtScanner>(AssetArtScannerSettingData.Setting.Scanners.Count);

            // 过滤列表
            foreach (var scanner in AssetArtScannerSettingData.Setting.Scanners)
            {
                if (string.IsNullOrEmpty(searchKeyWord) == false)
                {
                    if (scanner.CheckKeyword(searchKeyWord) == false)
                        continue;
                }
                result.Add(scanner);
            }

            return result;
        }
        private VisualElement MakeScannerListViewItem()
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
        private void BindScannerListViewItem(VisualElement element, int index)
        {
            List<AssetArtScanner> sourceList = _scannerListView.itemsSource as List<AssetArtScanner>;
            var scanner = sourceList[index];
            var textField1 = element.Q<Label>("Label1");
            if (string.IsNullOrEmpty(scanner.ScannerDesc))
                textField1.text = scanner.ScannerName;
            else
                textField1.text = $"{scanner.ScannerName} ({scanner.ScannerDesc})";
        }
        private void ScannerListView_onSelectionChange(IEnumerable<object> objs)
        {
            var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectScanner == null)
            {
                _scannerContentContainer.visible = false;
                return;
            }

            _scannerContentContainer.visible = true;
            _lastModifyScannerIndex = _scannerListView.selectedIndex;
            _scannerNameTxt.SetValueWithoutNotify(selectScanner.ScannerName);
            _scannerDescTxt.SetValueWithoutNotify(selectScanner.ScannerDesc);

            // 显示检视面板
            var scanSchema = selectScanner.LoadSchema();
            RefreshInspector(scanSchema);

            // 设置Schema对象
            if (scanSchema == null)
            {
                _scannerSchemaField.SetValueWithoutNotify(null);
                _schemaGuideTxt.text = string.Empty;
            }
            else
            {
                _scannerSchemaField.SetValueWithoutNotify(scanSchema);
                _schemaGuideTxt.text = scanSchema.GetUserGuide();
            }

            // 显示存储目录
            DefaultAsset saveFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(selectScanner.SaveDirectory);
            if (saveFolder == null)
            {
                _outputFolderField.SetValueWithoutNotify(null);
            }
            else
            {
                _outputFolderField.SetValueWithoutNotify(saveFolder);
            }

            FillCollectorViewData();
        }
        private void AddScannerBtn_clicked()
        {
            Undo.RecordObject(AssetArtScannerSettingData.Setting, "YooAsset.AssetArtScannerWindow AddScanner");
            AssetArtScannerSettingData.CreateScanner("Default Scanner", string.Empty);
            FillScannerListViewData();
        }
        private void RemoveScannerBtn_clicked()
        {
            var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectScanner == null)
                return;

            Undo.RecordObject(AssetArtScannerSettingData.Setting, "YooAsset.AssetArtScannerWindow RemoveScanner");
            AssetArtScannerSettingData.RemoveScanner(selectScanner);
            FillScannerListViewData();
        }

        // 收集列表相关
        private void FillCollectorViewData()
        {
            var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectScanner == null)
                return;

            // 填充数据
            _collectorScrollView.Clear();
            for (int i = 0; i < selectScanner.Collectors.Count; i++)
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
            var selectScanner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectScanner == null)
                return;

            var collector = selectScanner.Collectors[index];
            var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);
            if (collectObject != null)
                collectObject.name = collector.CollectPath;

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
                AssetArtScannerSettingData.ModifyCollector(selectScanner, collector);
            });
        }
        private void AddCollectorBtn_clicked()
        {
            var selectSacnner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectSacnner == null)
                return;

            Undo.RecordObject(AssetArtScannerSettingData.Setting, "YooAsset.AssetArtScannerWindow AddCollector");
            AssetArtCollector collector = new AssetArtCollector();
            AssetArtScannerSettingData.CreateCollector(selectSacnner, collector);
            FillCollectorViewData();
        }
        private void RemoveCollectorBtn_clicked(AssetArtCollector selectCollector)
        {
            var selectSacnner = _scannerListView.selectedItem as AssetArtScanner;
            if (selectSacnner == null)
                return;
            if (selectCollector == null)
                return;

            Undo.RecordObject(AssetArtScannerSettingData.Setting, "YooAsset.AssetArtScannerWindow RemoveCollector");
            AssetArtScannerSettingData.RemoveCollector(selectSacnner, selectCollector);
            FillCollectorViewData();
        }

        // 属性面板相关
        private void RefreshInspector(ScannerSchema scanSchema)
        {
            if (scanSchema == null)
            {
                UIElementsTools.SetElementVisible(_inspectorContainer, false);
                return;
            }

            var inspector = scanSchema.CreateInspector();
            if (inspector == null)
            {
                UIElementsTools.SetElementVisible(_inspectorContainer, false);
                return;
            }

            if (inspector.Containner is VisualElement container)
            {
                UIElementsTools.SetElementVisible(_inspectorContainer, true);
                _inspectorContainer.Clear();
                _inspectorContainer.Add(container);
                _inspectorContainer.style.width = inspector.Width;
                _inspectorContainer.style.minWidth = inspector.MinWidth;
                _inspectorContainer.style.maxWidth = inspector.MaxWidth;
            }
            else
            {
                Debug.LogWarning($"{nameof(ScannerSchema)} inspector container is invalid !");
                UIElementsTools.SetElementVisible(_inspectorContainer, false);
            }
        }
    }
}
#endif