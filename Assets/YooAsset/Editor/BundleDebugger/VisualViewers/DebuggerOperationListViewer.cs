using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 异步操作列表调试视图，展示运行时所有异步操作的状态与子操作树
    /// </summary>
    internal class DebuggerOperationListViewer
    {
        private class OperationTableData : DefaultTableData
        {
            public DiagnosticPackageData PackageData;
            public DiagnosticOperationInfo OperationInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableViewer _operationTableView;
        private Toolbar _bottomToolbar;
        private TreeViewer _childTreeView;

        private List<ITableData> _sourceData;


        /// <summary>
        /// 初始化界面布局、表格列定义和子操作树
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUxml<DebuggerOperationListViewer>();
            if (_visualAsset == null)
                throw new Exception($"Failed to load UXML for {nameof(DebuggerOperationListViewer)}.");

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 任务列表
            _operationTableView = _root.Q<TableViewer>("TopTableView");
            _operationTableView.SelectionChanged += OnOperationTableViewSelectionChanged;
            CreateOperationTableViewColumns();

            // 底部标题栏
            _bottomToolbar = _root.Q<Toolbar>("BottomToolbar");
            CreateBottomToolbarHeaders();

            // 子列表
            _childTreeView = _root.Q<TreeViewer>("BottomTreeView");
            _childTreeView.MakeItem = MakeTreeViewItem;
            _childTreeView.BindItem = BindTreeViewItem;

            // 面板分屏
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            UIElementsTools.SplitVerticalPanel(_root, topGroup, bottomGroup);
        }
        private void CreateOperationTableViewColumns()
        {
            // PackageName
            {
                var columnStyle = new ColumnStyle(200);
                columnStyle.Searchable = true;
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
                _operationTableView.AddColumn(column);
            }

            // OperationName
            {
                var columnStyle = new ColumnStyle(300, 300, 600);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var column = new TableColumn("OperationName", "Operation Name", columnStyle);
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
                _operationTableView.AddColumn(column);
            }

            // Priority
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Priority", "Priority", columnStyle);
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
                _operationTableView.AddColumn(column);
            }

            // Progress
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = false;
                var column = new TableColumn("Progress", "Progress", columnStyle);
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
                _operationTableView.AddColumn(column);
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
                _operationTableView.AddColumn(column);
            }

            // ElapsedMS
            {
                var columnStyle = new ColumnStyle(130);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                columnStyle.Units = "ms";
                var column = new TableColumn("ElapsedMS", "Elapsed MS", columnStyle);
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
                _operationTableView.AddColumn(column);
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
                    var operationTableData = data as OperationTableData;
                    if (operationTableData.OperationInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _operationTableView.AddColumn(column);
            }

            // Desc
            {
                var columnStyle = new ColumnStyle(500, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                var column = new TableColumn("Desc", "Desc", columnStyle);
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
                _operationTableView.AddColumn(column);
            }
        }
        private void CreateBottomToolbarHeaders()
        {
            // OperationName
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "OperationName";
                button.style.flexGrow = 0;
                button.style.width = 315;
                _bottomToolbar.Add(button);
            }

            // Progress
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "Progress";
                button.style.flexGrow = 0;
                button.style.width = 100;
                _bottomToolbar.Add(button);
            }

            // StartTime
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "StartTime";
                button.style.flexGrow = 0;
                button.style.width = 100;
                _bottomToolbar.Add(button);
            }

            // ElapsedMS
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "ElapsedMS";
                button.style.flexGrow = 0;
                button.style.width = 130;
                _bottomToolbar.Add(button);
            }

            // Status
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "Status";
                button.style.flexGrow = 0;
                button.style.width = 100;
                _bottomToolbar.Add(button);
            }

            // Desc
            {
                ToolbarButton button = new ToolbarButton();
                button.text = "Desc";
                button.style.flexGrow = 0;
                button.style.width = 500;
                _bottomToolbar.Add(button);
            }
        }

        /// <summary>
        /// 填充诊断报告中的操作数据到视图
        /// </summary>
        /// <param name="debugReport">待展示的诊断报告</param>
        public void FillViewData(DiagnosticReport debugReport)
        {
            // 清空旧数据
            _operationTableView.ClearAll(false, true);
            _childTreeView.ClearAll();
            _childTreeView.RebuildView();

            // 填充数据源
            _sourceData = new List<ITableData>(1000);
            foreach (var packageData in debugReport.PackageDataList)
            {
                foreach (var operationInfo in packageData.OperationInfos)
                {
                    var rowData = new OperationTableData();
                    rowData.PackageData = packageData;
                    rowData.OperationInfo = operationInfo;
                    rowData.AddStringValueCell("PackageName", packageData.PackageName);
                    rowData.AddStringValueCell("OperationName", operationInfo.OperationName);
                    rowData.AddLongValueCell("Priority", operationInfo.Priority);
                    rowData.AddDoubleValueCell("Progress", operationInfo.Progress);
                    rowData.AddStringValueCell("StartTime", operationInfo.StartTime);
                    rowData.AddLongValueCell("ElapsedMS", operationInfo.ElapsedMilliseconds);
                    rowData.AddStringValueCell("Status", operationInfo.Status.ToString());
                    rowData.AddStringValueCell("Desc", operationInfo.OperationDescription);
                    _sourceData.Add(rowData);
                }
            }
            _operationTableView.ItemsSource = _sourceData;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 清空所有表格和树视图数据并刷新
        /// </summary>
        public void ClearView()
        {
            _operationTableView.ClearAll(false, true);
            _operationTableView.RebuildView();

            _childTreeView.ClearAll();
            _childTreeView.RebuildView();
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
            _operationTableView.RebuildView();
            _childTreeView.RebuildView();
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

        private void OnOperationTableViewSelectionChanged(ITableData data)
        {
            if (!(data is OperationTableData operationTableData))
                throw new YooInternalException($"Unexpected table data type: {data.GetType()}");

            DiagnosticPackageData packageData = operationTableData.PackageData;
            DiagnosticOperationInfo operationInfo = operationTableData.OperationInfo;

            TreeNode rootNode = new TreeNode(operationInfo);
            FillTreeData(operationInfo, rootNode);
            _childTreeView.ClearAll();
            _childTreeView.AddRootItem(rootNode);
            _childTreeView.RebuildView();
        }
        private void MakeTreeViewItem(VisualElement container)
        {
            // OperationName
            {
                Label label = new Label();
                label.name = "OperationName";
                label.style.flexGrow = 0f;
                label.style.width = 300;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }

            // Progress
            {
                var label = new Label();
                label.name = "Progress";
                label.style.flexGrow = 0f;
                label.style.width = 100;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }

            // StartTime
            {
                var label = new Label();
                label.name = "StartTime";
                label.style.flexGrow = 0f;
                label.style.width = 100;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }

            // ElapsedMS
            {
                var label = new Label();
                label.name = "ElapsedMS";
                label.style.flexGrow = 0f;
                label.style.width = 130;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }

            // Status
            {
                var label = new Label();
                label.name = "Status";
                label.style.flexGrow = 0f;
                label.style.width = 100;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }

            // Desc
            {
                Label label = new Label();
                label.name = "Desc";
                label.style.flexGrow = 1f;
                label.style.width = 500;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                container.Add(label);
            }
        }
        private void BindTreeViewItem(VisualElement container, object userData)
        {
            var operationInfo = (DiagnosticOperationInfo)userData;

            // OperationName
            {
                var label = container.Q<Label>("OperationName");
                label.text = operationInfo.OperationName;
            }

            // Progress
            {
                var label = container.Q<Label>("Progress");
                label.text = operationInfo.Progress.ToString();
            }

            // StartTime
            {
                var label = container.Q<Label>("StartTime");
                label.text = operationInfo.StartTime;
            }

            // ElapsedMS
            {
                var label = container.Q<Label>("ElapsedMS");
                label.text = operationInfo.ElapsedMilliseconds.ToString();
            }

            // Status
            {
                StyleColor textColor;
                if (operationInfo.Status == EOperationStatus.Failed.ToString())
                    textColor = new StyleColor(Color.yellow);
                else
                    textColor = new StyleColor(Color.white);

                var label = container.Q<Label>("Status");
                label.text = operationInfo.Status;
                label.style.color = textColor;
            }

            // Desc
            {
                var label = container.Q<Label>("Desc");
                label.text = operationInfo.OperationDescription;
            }
        }
        private void FillTreeData(DiagnosticOperationInfo parentOperation, TreeNode rootNode)
        {
            foreach (var childOperation in parentOperation.Children)
            {
                var childNode = new TreeNode(childOperation);
                rootNode.AddChild(childNode);
                FillTreeData(childOperation, childNode);
            }
        }
    }
}