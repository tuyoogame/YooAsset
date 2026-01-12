using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包列表调试视图，展示运行时所有已加载资源包的状态信息
    /// </summary>
    internal class DebuggerBundleListViewer
    {
        private class BundleTableData : DefaultTableData
        {
            public DiagnosticPackageData PackageData;
            public DiagnosticBundleInfo BundleInfo;
        }
        private class UsingTableData : DefaultTableData
        {
            public DiagnosticProviderInfo ProviderInfo;
        }
        private class ReferenceTableData : DefaultTableData
        {
            public DiagnosticBundleInfo BundleInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableViewer _bundleTableView;
        private TableViewer _usingTableView;
        private TableViewer _referenceTableView;

        private List<ITableData> _sourceData;

        /// <summary>
        /// 初始化界面布局和表格列定义
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUxml<DebuggerBundleListViewer>();
            if (_visualAsset == null)
                throw new Exception($"Failed to load UXML for {nameof(DebuggerBundleListViewer)}.");

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源包列表
            _bundleTableView = _root.Q<TableViewer>("BundleTableView");
            _bundleTableView.SelectionChanged += OnBundleTableViewSelectionChanged;
            CreateBundleTableViewColumns();

            // 使用列表
            _usingTableView = _root.Q<TableViewer>("UsingTableView");
            CreateUsingTableViewColumns();

            // 引用列表
            _referenceTableView = _root.Q<TableViewer>("ReferenceTableView");
            CreateReferenceTableViewColumns();

            // 面板分屏
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            UIElementsTools.SplitVerticalPanel(_root, topGroup, bottomGroup);
            UIElementsTools.SplitVerticalPanel(bottomGroup, _usingTableView, _referenceTableView);
        }
        private void CreateBundleTableViewColumns()
        {
            // PackageName
            {
                var columnStyle = new ColumnStyle(200);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("PackageName", "Package Name", columnStyle);
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

            // BundleName
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

            // RefCount
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("RefCount", "Ref Count", columnStyle);
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

            // Status
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Status", "Status", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    StyleColor textColor;
                    var bundleTableData = data as BundleTableData;
                    if (bundleTableData.BundleInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _bundleTableView.AddColumn(column);
            }
        }
        private void CreateUsingTableViewColumns()
        {
            // UsingAssets
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("UsingAssets", "Using Assets", columnStyle);
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
                _usingTableView.AddColumn(column);
            }

            // SceneName
            {
                var columnStyle = new ColumnStyle(150);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("SceneName", "Scene Name", columnStyle);
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
                _usingTableView.AddColumn(column);
            }

            // StartTime
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("StartTime", "Start Time", columnStyle);
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
                _usingTableView.AddColumn(column);
            }

            // RefCount
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("RefCount", "Ref Count", columnStyle);
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
                _usingTableView.AddColumn(column);
            }

            // Status
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Status", "Status", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    StyleColor textColor;
                    var usingTableData = data as UsingTableData;
                    if (usingTableData.ProviderInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _usingTableView.AddColumn(column);
            }
        }
        private void CreateReferenceTableViewColumns()
        {
            // BundleName
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("ReferenceBundle", "Reference Bundle", columnStyle);
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
                _referenceTableView.AddColumn(column);
            }

            // RefCount
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("RefCount", "Ref Count", columnStyle);
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
                _referenceTableView.AddColumn(column);
            }

            // Status
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Status", "Status", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    StyleColor textColor;
                    var referenceTableData = data as ReferenceTableData;
                    if (referenceTableData.BundleInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _referenceTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充诊断报告中的资源包数据到视图
        /// </summary>
        /// <param name="debugReport">待展示的诊断报告</param>
        public void FillViewData(DiagnosticReport debugReport)
        {
            // 清空旧数据
            _bundleTableView.ClearAll(false, true);
            _usingTableView.ClearAll(false, true);
            _referenceTableView.ClearAll(false, true);

            // 填充数据源
            _sourceData = new List<ITableData>(1000);
            foreach (var packageData in debugReport.PackageDataList)
            {
                foreach (var bundleInfo in packageData.BundleInfos)
                {
                    var rowData = new BundleTableData();
                    rowData.PackageData = packageData;
                    rowData.BundleInfo = bundleInfo;
                    rowData.AddAssetPathCell("PackageName", packageData.PackageName);
                    rowData.AddStringValueCell("BundleName", bundleInfo.BundleName);
                    rowData.AddLongValueCell("RefCount", bundleInfo.ReferenceCount);
                    rowData.AddStringValueCell("Status", bundleInfo.Status.ToString());
                    _sourceData.Add(rowData);
                }
            }
            _bundleTableView.ItemsSource = _sourceData;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 清空所有表格数据并刷新视图
        /// </summary>
        public void ClearView()
        {
            _bundleTableView.ClearAll(false, true);
            _bundleTableView.RebuildView();

            _usingTableView.ClearAll(false, true);
            _usingTableView.RebuildView();

            _referenceTableView.ClearAll(false, true);
            _referenceTableView.RebuildView();
        }

        /// <summary>
        /// 按关键字过滤并重建视图
        /// </summary>
        /// <param name="searchKeyword">搜索关键字，为 null 时显示全部</param>
        public void RebuildView(string searchKeyword)
        {
            // 搜索匹配
            if (_sourceData != null)
                DefaultSearchSystem.Search(_sourceData, searchKeyword, ESearchLogic.AND);

            // 重建视图
            _bundleTableView.RebuildView();
            _usingTableView.RebuildView();
            _referenceTableView.RebuildView();
        }

        /// <summary>
        /// 将视图挂接到指定的父级容器
        /// </summary>
        /// <param name="parent">目标父级</param>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 将视图从父级容器中移除
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        private void OnBundleTableViewSelectionChanged(ITableData data)
        {
            if (!(data is BundleTableData bundleTableData))
                throw new YooInternalException($"Unexpected table data type: {data.GetType()}");

            var packageData = bundleTableData.PackageData;
            var selectBundleInfo = bundleTableData.BundleInfo;

            // 填充UsingTableView
            {
                var sourceDatas = new List<ITableData>(1000);
                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    foreach (var dependBundleName in providerInfo.Dependencies)
                    {
                        if (dependBundleName == selectBundleInfo.BundleName)
                        {
                            var rowData = new UsingTableData();
                            rowData.ProviderInfo = providerInfo;
                            rowData.AddStringValueCell("UsingAssets", providerInfo.AssetPath);
                            rowData.AddStringValueCell("SceneName", providerInfo.SceneName);
                            rowData.AddStringValueCell("StartTime", providerInfo.StartTime);
                            rowData.AddLongValueCell("RefCount", providerInfo.ReferenceCount);
                            rowData.AddStringValueCell("Status", providerInfo.Status);
                            sourceDatas.Add(rowData);
                            break;
                        }
                    }
                }
                _usingTableView.ItemsSource = sourceDatas;
                _usingTableView.RebuildView();
            }

            // 填充ReferenceTableView
            {
                var sourceDatas = new List<ITableData>(1000);
                foreach (string referenceBundleName in selectBundleInfo.Referencers)
                {
                    if (packageData.TryGetBundleInfo(referenceBundleName, out var bundleInfo) == false)
                        continue;

                    var rowData = new ReferenceTableData();
                    rowData.BundleInfo = bundleInfo;
                    rowData.AddStringValueCell("BundleName", bundleInfo.BundleName);
                    rowData.AddLongValueCell("RefCount", bundleInfo.ReferenceCount);
                    rowData.AddStringValueCell("Status", bundleInfo.Status.ToString());
                    sourceDatas.Add(rowData);
                }
                _referenceTableView.ItemsSource = sourceDatas;
                _referenceTableView.RebuildView();
            }
        }
    }
}