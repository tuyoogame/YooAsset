using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源对象列表调试视图，展示运行时所有已加载资源的状态信息
    /// </summary>
    internal class DebuggerAssetListViewer
    {
        private class ProviderTableData : DefaultTableData
        {
            public DiagnosticPackageData PackageData;
            public DiagnosticProviderInfo ProviderInfo;
        }
        private class DependTableData : DefaultTableData
        {
            public DiagnosticBundleInfo BundleInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableViewer _providerTableView;
        private TableViewer _dependTableView;

        private List<ITableData> _sourceData;


        /// <summary>
        /// 初始化界面布局和表格列定义
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUxml<DebuggerAssetListViewer>();
            if (_visualAsset == null)
                throw new Exception($"Failed to load UXML for {nameof(DebuggerAssetListViewer)}.");

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源列表
            _providerTableView = _root.Q<TableViewer>("TopTableView");
            _providerTableView.SelectionChanged += OnProviderTableViewSelectionChanged;
            CreateAssetTableViewColumns();

            // 依赖列表
            _dependTableView = _root.Q<TableViewer>("BottomTableView");
            CreateDependTableViewColumns();

            // 面板分屏
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            UIElementsTools.SplitVerticalPanel(_root, topGroup, bottomGroup);
        }
        private void CreateAssetTableViewColumns()
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
                _providerTableView.AddColumn(column);
            }

            // AssetPath
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("AssetPath", "Asset Path", columnStyle);
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
                _providerTableView.AddColumn(column);
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
                _providerTableView.AddColumn(column);
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
                _providerTableView.AddColumn(column);
            }

            // LoadingTime
            {
                var columnStyle = new ColumnStyle(130);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                columnStyle.Units = "ms";
                var column = new TableColumn("LoadingTime", "Loading Time", columnStyle);
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
                _providerTableView.AddColumn(column);
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
                _providerTableView.AddColumn(column);
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
                    var providerTableData = data as ProviderTableData;
                    if (providerTableData.ProviderInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _providerTableView.AddColumn(column);
            }
        }
        private void CreateDependTableViewColumns()
        {
            // DependBundles
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("DependBundles", "Depend Bundles", columnStyle);
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
                _dependTableView.AddColumn(column);
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
                _dependTableView.AddColumn(column);
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
                    var dependTableData = data as DependTableData;
                    if (dependTableData.BundleInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _dependTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充诊断报告中的资源数据到视图
        /// </summary>
        /// <param name="debugReport">待展示的诊断报告</param>
        public void FillViewData(DiagnosticReport debugReport)
        {
            // 清空旧数据
            _providerTableView.ClearAll(false, true);
            _dependTableView.ClearAll(false, true);

            // 填充数据源
            _sourceData = new List<ITableData>(1000);
            foreach (var packageData in debugReport.PackageDataList)
            {
                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    var rowData = new ProviderTableData();
                    rowData.PackageData = packageData;
                    rowData.ProviderInfo = providerInfo;
                    rowData.AddAssetPathCell("PackageName", packageData.PackageName);
                    rowData.AddStringValueCell("AssetPath", providerInfo.AssetPath);
                    rowData.AddStringValueCell("SceneName", providerInfo.SceneName);
                    rowData.AddStringValueCell("StartTime", providerInfo.StartTime);
                    rowData.AddLongValueCell("LoadingTime", providerInfo.ElapsedMilliseconds);
                    rowData.AddLongValueCell("RefCount", providerInfo.ReferenceCount);
                    rowData.AddStringValueCell("Status", providerInfo.Status.ToString());
                    _sourceData.Add(rowData);
                }
            }
            _providerTableView.ItemsSource = _sourceData;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 清空所有表格数据并刷新视图
        /// </summary>
        public void ClearView()
        {
            _providerTableView.ClearAll(false, true);
            _providerTableView.RebuildView();

            _dependTableView.ClearAll(false, true);
            _dependTableView.RebuildView();
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
            _providerTableView.RebuildView();
            _dependTableView.RebuildView();
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

        private void OnProviderTableViewSelectionChanged(ITableData data)
        {
            if (!(data is ProviderTableData providerTableData))
                throw new YooInternalException($"Unexpected table data type: {data.GetType()}");

            DiagnosticPackageData packageData = providerTableData.PackageData;
            DiagnosticProviderInfo providerInfo = providerTableData.ProviderInfo;

            // 填充依赖数据
            var sourceDatas = new List<ITableData>(providerInfo.Dependencies.Count);
            foreach (var bundleName in providerInfo.Dependencies)
            {
                if (packageData.TryGetBundleInfo(bundleName, out var dependBundleInfo) == false)
                    continue;

                var rowData = new DependTableData();
                rowData.BundleInfo = dependBundleInfo;
                rowData.AddStringValueCell("DependBundles", dependBundleInfo.BundleName);
                rowData.AddLongValueCell("RefCount", dependBundleInfo.ReferenceCount);
                rowData.AddStringValueCell("Status", dependBundleInfo.Status.ToString());
                sourceDatas.Add(rowData);
            }
            _dependTableView.ItemsSource = sourceDatas;
            _dependTableView.RebuildView();
        }
    }
}