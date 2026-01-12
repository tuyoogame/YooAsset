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
    internal class ReporterBundleListViewer
    {
        private class BundleTableData : DefaultTableData
        {
            public ReportBundleInfo BundleInfo;
        }
        private class IncludeTableData : DefaultTableData
        {
            public ReportAssetInfo AssetInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableViewer _bundleTableView;
        private TableViewer _includeTableView;

        private BuildReport _buildReport;
        private string _reportFilePath;
        private List<ITableData> _sourceDatas;


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUxml<ReporterBundleListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源包列表
            _bundleTableView = _root.Q<TableViewer>("TopTableView");
            _bundleTableView.TableDataClicked += OnClickBundleTableView;
            _bundleTableView.SelectionChanged += OnBundleTableViewSelectionChanged;
            CreateBundleTableViewColumns();

            // 包含列表
            _includeTableView = _root.Q<TableViewer>("BottomTableView");
            _includeTableView.TableDataClicked += OnClickIncludeTableView;
            CreateIncludeTableViewColumns();

#if UNITY_2020_3_OR_NEWER
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            UIElementsTools.SplitVerticalPanel(_root, topGroup, bottomGroup);
#endif
        }
        private void CreateBundleTableViewColumns()
        {
            //BundleName
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("BundleName", "Bundle Name", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _bundleTableView.AddColumn(column);
            }

            // FileSize
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("FileSize", "File Size", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    long fileSize = (long)cell.CellValue;
                    infoLabel.text = EditorUtility.FormatBytes(fileSize);
                };
                _bundleTableView.AddColumn(column);
            }

            // FileHash
            {
                var columnStyle = new ColumnStyle(250);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("FileHash", "File Hash", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _bundleTableView.AddColumn(column);
            }

            //Encrypted
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Encrypted", "Encrypted", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    bool encrypted = (bool)cell.CellValue;
                    infoLabel.text = encrypted.ToString();
                };
                _bundleTableView.AddColumn(column);
            }

            //Tags
            {
                var columnStyle = new ColumnStyle(150, 100, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("Tags", "Tags", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _bundleTableView.AddColumn(column);
            }
        }
        private void CreateIncludeTableViewColumns()
        {
            //IncludeAssets
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("IncludeAssets", "Include Assets", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _includeTableView.AddColumn(column);
            }

            //AssetSource
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("AssetSource", "Asset Source", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _includeTableView.AddColumn(column);
            }

            //AssetGUID
            {
                var columnStyle = new ColumnStyle(250);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("AssetGUID", "Asset GUID", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _includeTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        /// <param name="buildReport">构建报告数据</param>
        /// <param name="reportFilePath">报告文件路径</param>
        public void FillViewData(BuildReport buildReport, string reportFilePath)
        {
            _buildReport = buildReport;
            _reportFilePath = reportFilePath;

            // 清空旧数据
            _bundleTableView.ClearAll(false, true);
            _includeTableView.ClearAll(false, true);

            // 填充数据源
            _sourceDatas = new List<ITableData>(_buildReport.BundleInfos.Count);
            foreach (var bundleInfo in _buildReport.BundleInfos)
            {
                var rowData = new BundleTableData();
                rowData.BundleInfo = bundleInfo;
                rowData.AddStringValueCell("BundleName", bundleInfo.BundleName);
                rowData.AddLongValueCell("FileSize", bundleInfo.FileSize);
                rowData.AddStringValueCell("FileHash", bundleInfo.FileHash);
                rowData.AddBoolValueCell("Encrypted", bundleInfo.Encrypted);
                rowData.AddStringValueCell("Tags", bundleInfo.GetTagsString());
                _sourceDatas.Add(rowData);
            }
            _bundleTableView.ItemsSource = _sourceDatas;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 重建视图
        /// </summary>
        /// <param name="searchKeyWord">搜索关键字，传入 null 表示不过滤</param>
        public void RebuildView(string searchKeyWord)
        {
            // 搜索匹配
            DefaultSearchSystem.Search(_sourceDatas, searchKeyWord, ESearchLogic.AND);

            // 重建视图
            _bundleTableView.RebuildView();
        }

        /// <summary>
        /// 挂接到父级页面上
        /// </summary>
        /// <param name="parent">父级视觉元素</param>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父级页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        private void OnBundleTableViewSelectionChanged(ITableData data)
        {
            var bundleTableData = data as BundleTableData;
            var bundleInfo = bundleTableData.BundleInfo;

            // 填充包含数据
            var sourceDatas = new List<ITableData>();
            var mainAssetDic = new HashSet<string>();
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == bundleInfo.BundleName)
                {
                    mainAssetDic.Add(assetInfo.AssetPath);

                    var rowData = new IncludeTableData();
                    rowData.AssetInfo = assetInfo;
                    rowData.AddAssetPathCell("IncludeAssets", assetInfo.AssetPath);
                    rowData.AddStringValueCell("AssetSource", "MainAsset");
                    rowData.AddStringValueCell("AssetGUID", assetInfo.AssetGuid);
                    sourceDatas.Add(rowData);
                }
            }
            foreach (var assetInfo in bundleInfo.BundleContents)
            {
                if (mainAssetDic.Contains(assetInfo.AssetPath) == false)
                {
                    var rowData = new IncludeTableData();
                    rowData.AssetInfo = null;
                    rowData.AddAssetPathCell("IncludeAssets", assetInfo.AssetPath);
                    rowData.AddStringValueCell("AssetSource", "BuiltinAsset");
                    rowData.AddStringValueCell("AssetGUID", assetInfo.AssetGUID);
                    sourceDatas.Add(rowData);
                }
            }

            _includeTableView.ItemsSource = sourceDatas;
            _includeTableView.RebuildView();
        }
        private void OnClickBundleTableView(PointerDownEvent evt, ITableData data)
        {
            // 鼠标双击后检视
            if (evt.clickCount != 2)
                return;

            var bundleTableData = data as BundleTableData;
            if (bundleTableData.BundleInfo.Encrypted)
                return;

            if (_buildReport.Summary.BuildBundleType == (int)EBundleType.AssetBundle)
            {
                string rootDirectory = Path.GetDirectoryName(_reportFilePath);
                string filePath = Path.Combine(rootDirectory, bundleTableData.BundleInfo.FileName);
                if (File.Exists(filePath))
                    Selection.activeObject = AssetBundleRecorder.GetAssetBundle(filePath);
                else
                    Selection.activeObject = null;
            }
        }
        private void OnClickIncludeTableView(PointerDownEvent evt, ITableData data)
        {
            // 鼠标双击后检视
            if (evt.clickCount != 2)
                return;

            foreach (var cell in data.Cells)
            {
                if (cell is AssetPathCell assetPathCell)
                {
                    if (assetPathCell.PingAssetObject())
                        break;
                }
            }
        }
    }
}