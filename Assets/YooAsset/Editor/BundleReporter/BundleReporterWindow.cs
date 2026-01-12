using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// AssetBundle 构建报告的编辑器窗口
    /// </summary>
    public class BundleReporterWindow : EditorWindow
    {
        /// <summary>
        /// 打开构建报告窗口
        /// </summary>
        [MenuItem("YooAsset/Bundle Reporter", false, 103)]
        public static void OpenWindow()
        {
            Type[] dockedTypes = EditorWindowDefine.GetDockedWindowTypes();
            BundleReporterWindow window = GetWindow<BundleReporterWindow>("Bundle Reporter", true, dockedTypes);
            window.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// 视图模式
        /// </summary>
        private enum EViewMode
        {
            /// <summary>
            /// 概览视图
            /// </summary>
            Summary,

            /// <summary>
            /// 资源对象视图
            /// </summary>
            AssetView,

            /// <summary>
            /// 资源包视图
            /// </summary>
            BundleView,
        }

        private ToolbarMenu _viewModeMenu;
        private ReporterSummaryViewer _summaryViewer;
        private ReporterAssetListViewer _assetListViewer;
        private ReporterBundleListViewer _bundleListViewer;

        private EViewMode _viewMode;
        private BuildReport _buildReport;
        private string _reportFilePath;
        private string _searchKeyword;


        /// <summary>
        /// 创建窗口的 UI 布局
        /// </summary>
        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUxml<BundleReporterWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 导入按钮
                var importBtn = root.Q<Button>("ImportButton");
                importBtn.clicked += OnImportButtonClicked;

                // 视图模式菜单
                _viewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
                _viewModeMenu.menu.AppendAction(EViewMode.Summary.ToString(), OnViewModeSummary, GetViewModeSummaryStatus);
                _viewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), OnViewModeAssetView, GetViewModeAssetViewStatus);
                _viewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), OnViewModeBundleView, GetViewModeBundleViewStatus);

                // 搜索栏
                var searchField = root.Q<ToolbarSearchField>("SearchField");
                searchField.RegisterValueChangedCallback(OnSearchKeywordChange);

                // 概览视图
                _summaryViewer = new ReporterSummaryViewer();
                _summaryViewer.InitViewer();

                // 资源列表视图
                _assetListViewer = new ReporterAssetListViewer();
                _assetListViewer.InitViewer();

                // 资源包列表视图
                _bundleListViewer = new ReporterBundleListViewer();
                _bundleListViewer.InitViewer();

                // 显示视图
                _viewMode = EViewMode.Summary;
                _viewModeMenu.text = EViewMode.Summary.ToString();
                _summaryViewer.AttachParent(root);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        public void OnDestroy()
        {
            AssetBundleRecorder.UnloadAll();
        }

        private void OnImportButtonClicked()
        {
            string selectFilePath = EditorUtility.OpenFilePanel("导入报告", EditorPathUtility.GetProjectPath(), "report");
            if (string.IsNullOrEmpty(selectFilePath))
                return;

            _reportFilePath = selectFilePath;
            string jsonData = FileUtility.ReadAllText(_reportFilePath);
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"Failed to read report file: {_reportFilePath}");
                return;
            }

            _buildReport = BuildReport.Deserialize(jsonData);
            if (_buildReport == null)
            {
                Debug.LogError($"Failed to deserialize report file: {_reportFilePath}");
                return;
            }

            _summaryViewer.FillViewData(_buildReport);
            _assetListViewer.FillViewData(_buildReport, _reportFilePath);
            _bundleListViewer.FillViewData(_buildReport, _reportFilePath);
        }
        private void OnSearchKeywordChange(ChangeEvent<string> e)
        {
            _searchKeyword = e.newValue;
            if (_buildReport != null)
            {
                _assetListViewer.RebuildView(_searchKeyword);
                _bundleListViewer.RebuildView(_searchKeyword);
            }
        }
        private void OnViewModeSummary(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.Summary)
            {
                _viewMode = EViewMode.Summary;
                VisualElement root = this.rootVisualElement;
                _viewModeMenu.text = EViewMode.Summary.ToString();
                _summaryViewer.AttachParent(root);
                _assetListViewer.DetachParent();
                _bundleListViewer.DetachParent();
            }
        }
        private void OnViewModeAssetView(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.AssetView)
            {
                _viewMode = EViewMode.AssetView;
                VisualElement root = this.rootVisualElement;
                _viewModeMenu.text = EViewMode.AssetView.ToString();
                _summaryViewer.DetachParent();
                _assetListViewer.AttachParent(root);
                _bundleListViewer.DetachParent();
            }
        }
        private void OnViewModeBundleView(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.BundleView)
            {
                _viewMode = EViewMode.BundleView;
                VisualElement root = this.rootVisualElement;
                _viewModeMenu.text = EViewMode.BundleView.ToString();
                _summaryViewer.DetachParent();
                _assetListViewer.DetachParent();
                _bundleListViewer.AttachParent(root);
            }
        }
        private DropdownMenuAction.Status GetViewModeSummaryStatus(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.Summary)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }
        private DropdownMenuAction.Status GetViewModeAssetViewStatus(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.AssetView)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }
        private DropdownMenuAction.Status GetViewModeBundleViewStatus(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.BundleView)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }
    }
}