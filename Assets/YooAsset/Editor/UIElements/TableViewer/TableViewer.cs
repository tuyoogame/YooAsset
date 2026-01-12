using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 多列表格视图
    /// </summary>
    /// <remarks>
    /// Unity 2022 及以上版本推荐使用官方 MultiColumnListView 组件替代。
    /// </remarks>
    public class TableViewer : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TableViewer, UxmlTraits>
        {
        }

        private readonly Toolbar _toolbar;
        private readonly ListView _listView;

        private readonly List<TableColumn> _columns = new List<TableColumn>(10);
        private IList<ITableData> _itemsSource;
        private IList<ITableData> _sortingDatas;

        // 排序相关
        private string _sortingHeader;
        private bool _descendingSort = true;

        /// <summary>
        /// 数据源
        /// </summary>
        public IList<ITableData> ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                if (CheckItemsSource(value))
                {
                    _itemsSource = value;
                    _sortingDatas = value;
                }
            }
        }

        /// <summary>
        /// 选中的数据列表
        /// </summary>
        public IReadOnlyList<ITableData> SelectedItems
        {
            get
            {
#if UNITY_2020_3_OR_NEWER
                return _listView.selectedItems.Cast<ITableData>().ToList();
#else
                List<ITableData> result = new List<ITableData>();
                result.Add(_listView.selectedItem as ITableData);
                return result;
#endif
            }
        }

        /// <summary>
        /// 当点击表格数据行时触发
        /// </summary>
        public event Action<PointerDownEvent, ITableData> TableDataClicked;

        /// <summary>
        /// 当点击表格标题列时触发
        /// </summary>
        public event Action<TableColumn> TableHeadClicked;

        /// <summary>
        /// 当选中项变化时触发
        /// </summary>
        public event Action<ITableData> SelectionChanged;


        /// <summary>
        /// 创建表格视图实例
        /// </summary>
        public TableViewer()
        {
            this.style.flexShrink = 1f;
            this.style.flexGrow = 1f;

            // 定义标题栏
            _toolbar = new Toolbar();

            // 定义列表视图
            _listView = new ListView();
            _listView.style.flexShrink = 1f;
            _listView.style.flexGrow = 1f;
            _listView.makeItem = MakeListViewElement;
            _listView.bindItem = BindListViewElement;
            _listView.selectionType = SelectionType.Multiple;
            _listView.RegisterCallback<PointerDownEvent>(OnClickListItem);

#if UNITY_2022_3_OR_NEWER
            _listView.selectionChanged += OnSelectionChanged;
#elif UNITY_2020_1_OR_NEWER
            _listView.onSelectionChange += OnSelectionChanged;
#else
            _listView.onSelectionChanged += OnSelectionChanged;
#endif

            this.Add(_toolbar);
            this.Add(_listView);
        }

        /// <summary>
        /// 获取指定名称的标题栏元素
        /// </summary>
        /// <param name="elementName">标题栏元素名称</param>
        /// <returns>匹配的 ToolbarButton 元素，未找到时返回 null。</returns>
        public VisualElement GetHeaderElement(string elementName)
        {
            return _toolbar.Q<ToolbarButton>(elementName);
        }

        /// <summary>
        /// 添加单元列
        /// </summary>
        /// <param name="column">要添加的列定义</param>
        public void AddColumn(TableColumn column)
        {
            var toolbarBtn = new ToolbarButton();
            toolbarBtn.userData = column;
            toolbarBtn.name = column.ElementName;
            toolbarBtn.text = column.HeaderTitle;
            toolbarBtn.style.flexGrow = 0;
            toolbarBtn.style.width = column.ColumnStyle.Width;
            toolbarBtn.style.minWidth = column.ColumnStyle.Width;
            toolbarBtn.style.maxWidth = column.ColumnStyle.Width;
            toolbarBtn.clickable.clickedWithEventInfo += OnClickTableHead;
            SetCellElementStyle(toolbarBtn);
            _toolbar.Add(toolbarBtn);
            _columns.Add(column);

            // 可伸缩控制柄
            if (column.ColumnStyle.Stretchable)
            {
                int handleWidth = 3;
                int minWidth = (int)column.ColumnStyle.MinWidth.value;
                int maxWidth = (int)column.ColumnStyle.MaxWidth.value;
                var resizeHandle = new ResizeHandle(handleWidth, toolbarBtn, minWidth, maxWidth);
                resizeHandle.ResizeChanged += (float value) =>
                {
                    float width = Mathf.Clamp(value, column.ColumnStyle.MinWidth.value, column.ColumnStyle.MaxWidth.value);
                    column.ColumnStyle.Width = width;

                    foreach (var element in column.CellElements)
                    {
                        element.style.width = width;
                        element.style.minWidth = width;
                        element.style.maxWidth = width;
                    }
                };
                _toolbar.Add(resizeHandle);
            }

            // 计算索引值
            column.ColumnIndex = _columns.Count - 1;

            if (column.ColumnStyle.Sortable == false)
                toolbarBtn.SetEnabled(false);
        }

        /// <summary>
        /// 批量添加单元列
        /// </summary>
        /// <param name="columns">要添加的列定义集合</param>
        public void AddColumns(IList<TableColumn> columns)
        {
            foreach (var column in columns)
            {
                AddColumn(column);
            }
        }

        /// <summary>
        /// 重建表格视图
        /// </summary>
        public void RebuildView()
        {
            if (_itemsSource == null)
                return;

            var itemsSource = _sortingDatas.Where(row => row.Visible);

            _listView.Clear();
            _listView.ClearSelection();
            _listView.itemsSource = itemsSource.ToList();
            _listView.Rebuild();

            RefreshToolbar();
        }
        private void RefreshToolbar()
        {
            // 设置为原始标题
            foreach (var column in _columns)
            {
                var toolbarButton = _toolbar.Q<ToolbarButton>(column.ElementName);
                toolbarButton.text = column.HeaderTitle;
            }

            // 设置元素数量
            foreach (var column in _columns)
            {
                if (column.ColumnStyle.Counter)
                {
                    var toolbarButton = GetHeaderElement(column.ElementName) as ToolbarButton;
                    int visibleCount = _listView.itemsSource.Count;
                    int totalCount = ItemsSource.Count;
                    toolbarButton.text = $"{toolbarButton.text} ({visibleCount}/{totalCount})";
                }
            }

            // 设置展示单位
            foreach (var column in _columns)
            {
                if (string.IsNullOrEmpty(column.ColumnStyle.Units) == false)
                {
                    var toolbarButton = GetHeaderElement(column.ElementName) as ToolbarButton;
                    toolbarButton.text = $"{toolbarButton.text} ({column.ColumnStyle.Units})";
                }
            }

            // 设置升降符号
            if (string.IsNullOrEmpty(_sortingHeader) == false)
            {
                var _toolbarButton = _toolbar.Q<ToolbarButton>(_sortingHeader);
                if (_descendingSort)
                    _toolbarButton.text = $"{_toolbarButton.text} ↓";
                else
                    _toolbarButton.text = $"{_toolbarButton.text} ↑";
            }
        }

        /// <summary>
        /// 清空表格数据
        /// </summary>
        /// <param name="clearColumns">是否同时清空列定义</param>
        /// <param name="clearSource">是否同时清空数据源</param>
        public void ClearAll(bool clearColumns, bool clearSource)
        {
            if (clearColumns)
            {
                _columns.Clear();
                _toolbar.Clear();
            }

            if (clearSource)
            {
                if (_itemsSource != null)
                    _itemsSource.Clear();
                if (_sortingDatas != null)
                    _sortingDatas.Clear();
                _listView.Clear();
                _listView.ClearSelection();
            }
        }

        private void OnClickListItem(PointerDownEvent evt)
        {
            var selectData = _listView.selectedItem as ITableData;
            if (selectData == null)
                return;

            TableDataClicked?.Invoke(evt, selectData);
        }
        private void OnClickTableHead(EventBase eventBase)
        {
            if (_itemsSource == null)
                return;

            ToolbarButton toolbarBtn = eventBase.target as ToolbarButton;
            var clickedColumn = toolbarBtn.userData as TableColumn;
            if (clickedColumn == null)
                return;

            TableHeadClicked?.Invoke(clickedColumn);
            if (clickedColumn.ColumnStyle.Sortable == false)
                return;

            if (_sortingHeader != clickedColumn.ElementName)
            {
                _sortingHeader = clickedColumn.ElementName;
                _descendingSort = false;
            }
            else
            {
                _descendingSort = !_descendingSort;
            }

            // 升降排序
            if (_descendingSort)
                _sortingDatas = _itemsSource.OrderByDescending(tableData => tableData.Cells[clickedColumn.ColumnIndex]).ToList();
            else
                _sortingDatas = _itemsSource.OrderBy(tableData => tableData.Cells[clickedColumn.ColumnIndex]).ToList();

            RebuildView();
        }
        private void OnSelectionChanged(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var tableData = item as ITableData;
                SelectionChanged?.Invoke(tableData);
                break;
            }
        }

        private bool CheckItemsSource(IList<ITableData> itemsSource)
        {
            if (itemsSource == null)
                return false;

            if (itemsSource.Count > 0)
            {
                int cellCount = itemsSource[0].Cells.Count;
                for (int i = 0; i < itemsSource.Count; i++)
                {
                    var tableData = itemsSource[i];
                    if (tableData == null)
                    {
                        Debug.LogWarning("Items source contains a null instance.");
                        return false;
                    }
                    if (tableData.Cells == null || tableData.Cells.Count == 0)
                    {
                        Debug.LogWarning("Items source data has empty cells.");
                        return false;
                    }
                    if (tableData.Cells.Count != cellCount)
                    {
                        Debug.LogWarning($"Inconsistent cell count in items source. Item index: {i}.");
                        return false;
                    }
                }
            }

            return true;
        }
        private VisualElement MakeListViewElement()
        {
            VisualElement listViewElement = new VisualElement();
            listViewElement.style.flexDirection = FlexDirection.Row;
            foreach (var column in _columns)
            {
                var cellElement = column.MakeCell.Invoke();
                cellElement.name = column.ElementName;
                cellElement.style.flexGrow = 0f;
                cellElement.style.width = column.ColumnStyle.Width;
                cellElement.style.minWidth = column.ColumnStyle.Width;
                cellElement.style.maxWidth = column.ColumnStyle.Width;
                SetCellElementStyle(cellElement);
                listViewElement.Add(cellElement);
                column.CellElements.Add(cellElement);
            }
            return listViewElement;
        }
        private void BindListViewElement(VisualElement listViewElement, int index)
        {
            var sourceDatas = _listView.itemsSource as List<ITableData>;
            var tableData = sourceDatas[index];
            foreach (var column in _columns)
            {
                var cellElement = listViewElement.Q(column.ElementName);
                var tableCell = tableData.Cells[column.ColumnIndex];
                column.BindCell.Invoke(cellElement, tableData, tableCell);
            }
        }
        private void SetCellElementStyle(VisualElement element)
        {
            StyleLength defaultStyle = new StyleLength(1f);
            element.style.paddingTop = defaultStyle;
            element.style.paddingBottom = defaultStyle;
            element.style.marginTop = defaultStyle;
            element.style.marginBottom = defaultStyle;

            element.style.paddingLeft = 1;
            element.style.paddingRight = 1;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
        }
    }
}