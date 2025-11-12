#if UNITY_2019_4_OR_NEWER
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
    public class AssetArtReporterWindow : EditorWindow
    {
        [MenuItem("YooAsset/AssetArt Reporter", false, 302)]
        public static AssetArtReporterWindow OpenWindow()
        {
            AssetArtReporterWindow window = GetWindow<AssetArtReporterWindow>("AssetArt Reporter", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(800, 600);
            return window;
        }

        private class ElementTableData : DefaultTableData
        {
            public ReportElement Element;
        }
        private class PassesBtnCell : ITableCell, IComparable
        {
            public object CellValue { set; get; }
            public string SearchTag { private set; get; }
            public ReportElement Element
            {
                get
                {
                    return (ReportElement)CellValue;
                }
            }

            public PassesBtnCell(string searchTag, ReportElement element)
            {
                SearchTag = searchTag;
                CellValue = element;
            }
            public object GetDisplayObject()
            {
                return string.Empty;
            }
            public int CompareTo(object other)
            {
                if (other is PassesBtnCell cell)
                {
                    return this.Element.Passes.CompareTo(cell.Element.Passes);
                }
                else
                {
                    return 0;
                }
            }
        }
        private class WhiteListBtnCell : ITableCell, IComparable
        {
            public object CellValue { set; get; }
            public string SearchTag { private set; get; }
            public ReportElement Element
            {
                get
                {
                    return (ReportElement)CellValue;
                }
            }

            public WhiteListBtnCell(string searchTag, ReportElement element)
            {
                SearchTag = searchTag;
                CellValue = element;
            }
            public object GetDisplayObject()
            {
                return string.Empty;
            }
            public int CompareTo(object other)
            {
                if (other is WhiteListBtnCell cell)
                {
                    return this.Element.IsWhiteList.CompareTo(cell.Element.IsWhiteList);
                }
                else
                {
                    return 0;
                }
            }
        }

        private ToolbarSearchField _searchField;
        private Button _showHiddenBtn;
        private Button _whiteListVisibleBtn;
        private Button _passesVisibleBtn;
        private Label _titleLabel;
        private Label _descLabel;
        private TableViewer _elementTableView;

        private ScanReportCombiner _reportCombiner;
        private string _lastestOpenFolder;
        private List<ITableData> _sourceDatas;
        private bool _elementVisibleState = true;
        private bool _whiteListVisibleState = true;
        private bool _passesVisibleState = true;

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<AssetArtReporterWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 导入按钮
                var importSingleBtn = root.Q<Button>("SingleImportButton");
                importSingleBtn.clicked += ImportSingleBtn_clicked;
                var importMultiBtn = root.Q<Button>("MultiImportButton");
                importMultiBtn.clicked += ImportMultiBtn_clicked;

                // 修复按钮
                var fixAllBtn = root.Q<Button>("FixAllButton");
                fixAllBtn.clicked += FixAllBtn_clicked;
                var fixSelectBtn = root.Q<Button>("FixSelectButton");
                fixSelectBtn.clicked += FixSelectBtn_clicked;

                // 可见性按钮
                _showHiddenBtn = root.Q<Button>("ShowHiddenButton");
                _showHiddenBtn.clicked += ShowHiddenBtn_clicked;
                _whiteListVisibleBtn = root.Q<Button>("WhiteListVisibleButton");
                _whiteListVisibleBtn.clicked += WhiteListVisibleBtn_clicked;
                _passesVisibleBtn = root.Q<Button>("PassesVisibleButton");
                _passesVisibleBtn.clicked += PassesVsibleBtn_clicked;

                // 文件导出按钮
                var exportFilesBtn = root.Q<Button>("ExportFilesButton");
                exportFilesBtn.clicked += ExportFilesBtn_clicked;

                // 搜索过滤
                _searchField = root.Q<ToolbarSearchField>("SearchField");
                _searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

                // 标题和备注
                _titleLabel = root.Q<Label>("ReportTitle");
                _descLabel = root.Q<Label>("ReportDesc");

                // 列表相关
                _elementTableView = root.Q<TableViewer>("TopTableView");
                _elementTableView.ClickTableDataEvent = OnClickTableViewItem;

                _lastestOpenFolder = EditorTools.GetProjectPath();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        public void OnDestroy()
        {
            if (_reportCombiner != null)
                _reportCombiner.SaveChange();
        }

        /// <summary>
        /// 导入单个报告文件
        /// </summary>
        public void ImportSingleReprotFile(string filePath)
        {
            // 记录本次打开目录
            _lastestOpenFolder = Path.GetDirectoryName(filePath);
            _reportCombiner = new ScanReportCombiner();

            try
            {
                var scanReport = ScanReportConfig.ImportJsonConfig(filePath);
                _reportCombiner.Combine(scanReport);

                // 刷新页面
                RefreshToolbar();
                FillTableView();
                RebuildView();
            }
            catch (System.Exception e)
            {
                _reportCombiner = null;
                _titleLabel.text = "导入报告失败！";
                _descLabel.text = e.Message;
                UnityEngine.Debug.LogError(e.StackTrace);
            }
        }

        /// <summary>
        /// 导入单个报告对象
        /// </summary>
        public void ImportSingleReprotFile(ScanReport report)
        {
            _reportCombiner = new ScanReportCombiner();

            try
            {
                _reportCombiner.Combine(report);

                // 刷新页面
                RefreshToolbar();
                FillTableView();
                RebuildView();
            }
            catch (System.Exception e)
            {
                _reportCombiner = null;
                _titleLabel.text = "导入报告失败！";
                _descLabel.text = e.Message;
                UnityEngine.Debug.LogError(e.StackTrace);
            }
        }


        private void ImportSingleBtn_clicked()
        {
            string selectFilePath = EditorUtility.OpenFilePanel("导入报告", _lastestOpenFolder, "json");
            if (string.IsNullOrEmpty(selectFilePath))
                return;

            ImportSingleReprotFile(selectFilePath);
        }
        private void ImportMultiBtn_clicked()
        {
            string selectFolderPath = EditorUtility.OpenFolderPanel("导入报告", _lastestOpenFolder, null);
            if (string.IsNullOrEmpty(selectFolderPath))
                return;

            // 记录本次打开目录
            _lastestOpenFolder = selectFolderPath;
            _reportCombiner = new ScanReportCombiner();

            try
            {
                string[] files = Directory.GetFiles(selectFolderPath);
                foreach (string filePath in files)
                {
                    string extension = System.IO.Path.GetExtension(filePath);
                    if (extension == ".json")
                    {
                        var scanReport = ScanReportConfig.ImportJsonConfig(filePath);
                        _reportCombiner.Combine(scanReport);
                    }
                }

                // 刷新页面
                RefreshToolbar();
                FillTableView();
                RebuildView();
            }
            catch (System.Exception e)
            {
                _reportCombiner = null;
                _titleLabel.text = "Failed to import report!";
                _descLabel.text = e.Message;
                UnityEngine.Debug.LogError(e.StackTrace);
            }
        }
        private void FixAllBtn_clicked()
        {
            if (EditorUtility.DisplayDialog("Info", "Fix all resources (excluding whitelist and hidden elements)", "Yes", "No"))
            {
                if (_reportCombiner != null)
                    _reportCombiner.FixAll();
            }
        }
        private void FixSelectBtn_clicked()
        {
            if (EditorUtility.DisplayDialog("Info", "Fix selected resources (including whitelist and hidden elements)", "Yes", "No"))
            {
                if (_reportCombiner != null)
                    _reportCombiner.FixSelect();
            }
        }
        private void ShowHiddenBtn_clicked()
        {
            _elementVisibleState = !_elementVisibleState;
            RefreshToolbar();
            RebuildView();
        }
        private void WhiteListVisibleBtn_clicked()
        {
            _whiteListVisibleState = !_whiteListVisibleState;
            RefreshToolbar();
            RebuildView();
        }
        private void PassesVsibleBtn_clicked()
        {
            _passesVisibleState = !_passesVisibleState;
            RefreshToolbar();
            RebuildView();
        }
        private void ExportFilesBtn_clicked()
        {
            string selectFolderPath = EditorUtility.OpenFolderPanel("Export all selected resources", EditorTools.GetProjectPath(), string.Empty);
            if (string.IsNullOrEmpty(selectFolderPath) == false)
            {
                if (_reportCombiner != null)
                    _reportCombiner.ExportFiles(selectFolderPath);
            }
        }

        private void RefreshToolbar()
        {
            if (_reportCombiner == null)
                return;

            _titleLabel.text = _reportCombiner.ReportTitle;
            _descLabel.text = _reportCombiner.ReportDesc;

            var enableColor = new Color32(18, 100, 18, 255);
            var disableColor = new Color32(100, 100, 100, 255);

            if (_elementVisibleState)
                _showHiddenBtn.style.backgroundColor = new StyleColor(enableColor);
            else
                _showHiddenBtn.style.backgroundColor = new StyleColor(disableColor);

            if (_whiteListVisibleState)
                _whiteListVisibleBtn.style.backgroundColor = new StyleColor(enableColor);
            else
                _whiteListVisibleBtn.style.backgroundColor = new StyleColor(disableColor);

            if (_passesVisibleState)
                _passesVisibleBtn.style.backgroundColor = new StyleColor(enableColor);
            else
                _passesVisibleBtn.style.backgroundColor = new StyleColor(disableColor);
        }
        private void FillTableView()
        {
            if (_reportCombiner == null)
                return;

            _elementTableView.ClearAll(true, true);

            // 眼睛标题
            {
                var columnStyle = new ColumnStyle(20);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("眼睛框", string.Empty, columnStyle);
                column.MakeCell = () =>
                {
                    var toggle = new ToggleDisplay();
                    toggle.text = string.Empty;
                    toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                    toggle.RegisterValueChangedCallback((evt) => { OnDisplayToggleValueChange(toggle, evt); });
                    return toggle;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var toggle = element as ToggleDisplay;
                    toggle.userData = data;
                    var tableData = data as ElementTableData;
                    toggle.SetValueWithoutNotify(tableData.Element.Hidden);
                };
                _elementTableView.AddColumn(column);
                var headerElement = _elementTableView.GetHeaderElement("眼睛框");
                headerElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            // 通过标题
            {
                var columnStyle = new ColumnStyle(70);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("通过", "通过", columnStyle);
                column.MakeCell = () =>
                {
                    var button = new Button();
                    button.text = "通过";
                    button.style.unityTextAlign = TextAnchor.MiddleCenter;
                    button.SetEnabled(false);
                    return button;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    Button button = element as Button;
                    var elementTableData = data as ElementTableData;
                    if (elementTableData.Element.Passes)
                    {
                        button.style.backgroundColor = new StyleColor(new Color32(56, 147, 58, 255));
                        button.text = "通过";
                    }
                    else
                    {
                        button.style.backgroundColor = new StyleColor(new Color32(137, 0, 0, 255));
                        button.text = "失败";
                    }
                };
                _elementTableView.AddColumn(column);
                var headerElement = _elementTableView.GetHeaderElement("通过");
                headerElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            // 白名单标题
            {
                var columnStyle = new ColumnStyle(70);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("白名单", "白名单", columnStyle);
                column.MakeCell = () =>
                {
                    Button button = new Button();
                    button.text = "白名单";
                    button.style.unityTextAlign = TextAnchor.MiddleCenter;
                    button.clickable.clickedWithEventInfo += OnClickWhitListButton;
                    return button;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    Button button = element as Button;
                    button.userData = data;
                    var elementTableData = data as ElementTableData;
                    if (elementTableData.Element.IsWhiteList)
                        button.style.backgroundColor = new StyleColor(new Color32(56, 147, 58, 255));
                    else
                        button.style.backgroundColor = new StyleColor(new Color32(100, 100, 100, 255));
                };
                _elementTableView.AddColumn(column);
                var headerElement = _elementTableView.GetHeaderElement("白名单");
                headerElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            // 选中标题
            {
                var columnStyle = new ColumnStyle(20);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("选中框", string.Empty, columnStyle);
                column.MakeCell = () =>
                {
                    var toggle = new Toggle();
                    toggle.text = string.Empty;
                    toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                    toggle.RegisterValueChangedCallback((evt) => { OnSelectToggleValueChange(toggle, evt); });
                    return toggle;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var toggle = element as Toggle;
                    toggle.userData = data;
                    var tableData = data as ElementTableData;
                    toggle.SetValueWithoutNotify(tableData.Element.IsSelected);
                };
                _elementTableView.AddColumn(column);
            }

            // 自定义标题栏
            foreach (var header in _reportCombiner.Headers)
            {
                var columnStyle = new ColumnStyle(header.Width, header.MinWidth, header.MaxWidth);
                columnStyle.Stretchable = header.Stretchable;
                columnStyle.Searchable = header.Searchable;
                columnStyle.Sortable = header.Sortable;
                columnStyle.Counter = header.Counter;
                columnStyle.Units = header.Units;
                var column = new TableColumn(header.HeaderTitle, header.HeaderTitle, columnStyle);
                column.MakeCell = () =>
                {
                    if (header.HeaderType == EHeaderType.AssetObject)
                    {
                        var objectFiled = new ObjectField();
                        return objectFiled;
                    }
                    else
                    {
                        var label = new Label();
                        label.style.marginLeft = 3f;
                        label.style.unityTextAlign = TextAnchor.MiddleLeft;
                        return label;
                    }
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    if (header.HeaderType == EHeaderType.AssetObject)
                    {
                        var objectFiled = element as ObjectField;
                        objectFiled.value = (UnityEngine.Object)cell.GetDisplayObject();
                    }
                    else
                    {
                        var infoLabel = element as Label;
                        infoLabel.text = (string)cell.GetDisplayObject();
                    }
                };
                _elementTableView.AddColumn(column);
            }

            // 填充数据源
            _sourceDatas = new List<ITableData>(_reportCombiner.Elements.Count);
            foreach (var element in _reportCombiner.Elements)
            {
                var tableData = new ElementTableData();
                tableData.Element = element;

                // 固定标题
                tableData.AddButtonCell("眼睛框");
                tableData.AddCell(new PassesBtnCell("通过", element));
                tableData.AddCell(new WhiteListBtnCell("白名单", element));
                tableData.AddButtonCell("选中框");

                // 自定义标题
                foreach (var scanInfo in element.ScanInfos)
                {
                    var header = _reportCombiner.GetHeader(scanInfo.HeaderTitle);
                    if (header.HeaderType == EHeaderType.AssetPath)
                    {
                        tableData.AddAssetPathCell(scanInfo.HeaderTitle, scanInfo.ScanInfo);
                    }
                    else if (header.HeaderType == EHeaderType.AssetObject)
                    {
                        tableData.AddAssetObjectCell(scanInfo.HeaderTitle, scanInfo.ScanInfo);
                    }
                    else if (header.HeaderType == EHeaderType.StringValue)
                    {
                        tableData.AddStringValueCell(scanInfo.HeaderTitle, scanInfo.ScanInfo);
                    }
                    else if (header.HeaderType == EHeaderType.LongValue)
                    {
                        long value = Convert.ToInt64(scanInfo.ScanInfo);
                        tableData.AddLongValueCell(scanInfo.HeaderTitle, value);
                    }
                    else if (header.HeaderType == EHeaderType.DoubleValue)
                    {
                        double value = Convert.ToDouble(scanInfo.ScanInfo);
                        tableData.AddDoubleValueCell(scanInfo.HeaderTitle, value);
                    }
                    else
                    {
                        throw new NotImplementedException(header.HeaderType.ToString());
                    }
                }

                _sourceDatas.Add(tableData);
            }
            _elementTableView.itemsSource = _sourceDatas;
        }
        private void RebuildView()
        {
            if (_reportCombiner == null)
                return;

            string searchKeyword = _searchField.value;

            // 搜索匹配
            DefaultSearchSystem.Search(_sourceDatas, searchKeyword);

            // 开关匹配
            foreach (var tableData in _sourceDatas)
            {
                var elementTableData = tableData as ElementTableData;
                if (_elementVisibleState == false && elementTableData.Element.Hidden)
                {
                    tableData.Visible = false;
                    continue;
                }
                if (_passesVisibleState == false && elementTableData.Element.Passes)
                {
                    tableData.Visible = false;
                    continue;
                }
                if (_whiteListVisibleState == false && elementTableData.Element.IsWhiteList)
                {
                    tableData.Visible = false;
                    continue;
                }
            }

            // 重建视图
            _elementTableView.RebuildView();
        }

        private void OnClickTableViewItem(PointerDownEvent evt, ITableData tableData)
        {
            // 双击后检视对应的资源
            if (evt.clickCount == 2)
            {
                foreach (var cell in tableData.Cells)
                {
                    if (cell is AssetPathCell assetPathCell)
                    {
                        if (assetPathCell.PingAssetObject())
                            break;
                    }
                }
            }
        }
        private void OnSearchKeyWordChange(ChangeEvent<string> e)
        {
            RebuildView();
        }
        private void OnSelectToggleValueChange(Toggle toggle, ChangeEvent<bool> e)
        {
            // 处理自身
            toggle.SetValueWithoutNotify(e.newValue);

            // 记录数据
            var elementTableData = toggle.userData as ElementTableData;
            elementTableData.Element.IsSelected = e.newValue;

            // 处理多选目标
            var selectedItems = _elementTableView.selectedItems;
            foreach (var selectedItem in selectedItems)
            {
                var selectElement = selectedItem as ElementTableData;
                selectElement.Element.IsSelected = e.newValue;
            }

            // 重绘视图
            RebuildView();
        }
        private void OnDisplayToggleValueChange(ToggleDisplay toggle, ChangeEvent<bool> e)
        {
            // 处理自身
            toggle.SetValueWithoutNotify(e.newValue);

            // 记录数据
            var elementTableData = toggle.userData as ElementTableData;
            elementTableData.Element.Hidden = e.newValue;

            // 处理多选目标
            var selectedItems = _elementTableView.selectedItems;
            foreach (var selectedItem in selectedItems)
            {
                var selectElement = selectedItem as ElementTableData;
                if (selectElement != null)
                    selectElement.Element.Hidden = e.newValue;
            }

            // 重绘视图
            RebuildView();
        }
        private void OnClickWhitListButton(EventBase evt)
        {
            // 刷新点击的按钮
            Button button = evt.target as Button;
            var elementTableData = button.userData as ElementTableData;
            elementTableData.Element.IsWhiteList = !elementTableData.Element.IsWhiteList;

            // 刷新框选的按钮
            var selectedItems = _elementTableView.selectedItems;
            if (selectedItems.Count() > 1)
            {
                foreach (var selectedItem in selectedItems)
                {
                    var selectElement = selectedItem as ElementTableData;
                    selectElement.Element.IsWhiteList = selectElement.Element.IsWhiteList;
                }
            }

            RebuildView();
        }
    }
}
#endif