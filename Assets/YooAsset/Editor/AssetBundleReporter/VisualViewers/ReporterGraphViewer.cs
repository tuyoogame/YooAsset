#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ReporterGraphViewer
    {
        private class DependInfo
        {
            public ReportBundleInfo BundleInfo;
            public string AssetPath;
        }

        private enum EDependSortMode
        {
            BundleName,
            BundleSize,
        }

        private EDependSortMode _dependSortMode = EDependSortMode.BundleName;
        private bool _dependDescendingSort = false;

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private ListView _includeListView;
        private ListView _dependListView;

        private BuildReport _buildReport;
        private string _reportFilePath;

        private ToolbarButton _dependToolbar1;
        private ToolbarButton _dependToolbar3;

        private ReportBundleInfo _bundleInfo;
        private List<ReportAssetInfo> _includeList = new List<ReportAssetInfo>();
        private List<DependInfo> _dependList = new List<DependInfo>();

        private TreeViewer _treeViewer;

        private readonly Dictionary<string, List<ReportBundleInfo>> _treeData =
            new Dictionary<string, List<ReportBundleInfo>>();

        private GraphViewer _graphView;

        private class TreeNodeData
        {
            public string Name;
            public bool IsBundle;
            public int BundleCount;
            public int BundleActualCount; // 当前目录下去掉冗余后的bundle数量
            public long BundleSize;
            public long BundleActualSize; // 当前目录下去掉冗余后的bundle总大小
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        public virtual void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<ReporterGraphViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 包含列表
            _includeListView = _root.Q<ListView>("MidListView");
            _includeListView.makeItem = MakeIncludeListViewItem;
            _includeListView.bindItem = BindIncludeListViewItem;

            // 依赖列表
            _dependListView = _root.Q<ListView>("BottomListView");
            _dependListView.makeItem = MakeDependListViewItem;
            _dependListView.bindItem = BindDependListViewItem;

            _dependToolbar1 = _root.Q<ToolbarButton>("BottomBar1");
            _dependToolbar1.clicked += OnClickDependToolbar1;

            _dependToolbar3 = _root.Q<ToolbarButton>("BottomBar3");
            _dependToolbar3.clicked += OnClickDependToolbar3;

            _treeViewer = _root.Q<TreeViewer>("FileExplorer");
            _treeViewer.makeItem = MakeTreeViewerItem;
            _treeViewer.bindItem = BindTreeViewerItem;
            _treeViewer.onSelectionChange = OnSelectionChange;

            // 网状视图
            _graphView = _root.Q<GraphViewer>("GraphView");

            var split1 = _root.Q<TwoPaneSplitView>("TwoPaneSplitView1");
            split1.orientation = TwoPaneSplitViewOrientation.Vertical;
            split1.fixedPaneInitialDimension = 600;

            var split2 = _root.Q<TwoPaneSplitView>("TwoPaneSplitView2");
            split2.orientation = TwoPaneSplitViewOrientation.Horizontal;
            split2.fixedPaneInitialDimension = 300;

            var split3 = _root.Q<TwoPaneSplitView>("TwoPaneSplitView3");
            split3.orientation = TwoPaneSplitViewOrientation.Horizontal;
            split3.fixedPaneInitialDimension = 800;
        }

        /// <summary>
        /// 挂接到父类页面上
        /// </summary>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父类页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        public void RebuildView(string searchKeyWord)
        {
            ReportBundleInfo bundleInfo;

            try
            {
                if (_buildReport == null)
                {
                    return;
                }

                bundleInfo = _buildReport.GetBundleInfo(searchKeyWord);
            }
            catch (Exception e)
            {
                bundleInfo = null;
            }

            if (bundleInfo == null)
            {
                return;
            }

            GraphNode root = new GraphNode(bundleInfo, this);
            _graphView.SetRootNode(root);

            FillDependListView(bundleInfo);
            FillIncludeListView(bundleInfo);
        }

        private void RebuildView(ReportBundleInfo bundleInfo)
        {
            if (bundleInfo == null)
            {
                return;
            }

            GraphNode root = new GraphNode(bundleInfo, this);
            _graphView.SetRootNode(root);

            FillDependListView(bundleInfo);
            FillIncludeListView(bundleInfo);
        }

        public void AddChildren(GraphNode parent)
        {
            List<string> dependList = new List<string>();
            List<ReportAssetInfo> assetList = new List<ReportAssetInfo>();

            // 首先获得bundle包含的asset
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == parent.BundleInfo.BundleName)
                {
                    assetList.Add(assetInfo);
                }
            }

            // 然后找到每个asset依赖的bundle，添加到dependList中
            foreach (var assetInfo in assetList)
            {
                foreach (string dependBundleName in assetInfo.DependBundles)
                {
                    if (dependList.Contains(dependBundleName) == false)
                    {
                        dependList.Add(dependBundleName);
                    }
                }
            }

            foreach (var item in dependList)
            {
                _graphView.AddChildAndEdge(parent, new GraphNode(_buildReport.GetBundleInfo(item), this));
            }

            _graphView.schedule.Execute(() => { _graphView.LayoutGraphView(); }).StartingIn(10);
        }

        public int GetDependencyCount(GraphNode node)
        {
            List<string> dependList = new List<string>();
            List<ReportAssetInfo> assetList = new List<ReportAssetInfo>();

            // 首先获得bundle包含的asset
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == node.BundleInfo.BundleName)
                {
                    assetList.Add(assetInfo);
                }
            }

            // 然后找到每个asset依赖的bundle，添加到dependList中
            foreach (var assetInfo in assetList)
            {
                foreach (string dependBundleName in assetInfo.DependBundles)
                {
                    if (dependList.Contains(dependBundleName) == false)
                    {
                        dependList.Add(dependBundleName);
                    }
                }
            }

            return dependList.Count;
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport, string reprotFilePath)
        {
            _buildReport = buildReport;
            _reportFilePath = reprotFilePath;

            InitTreeData();
            InitTreeViewer();
        }

        /// <summary>
        /// 填充包含窗口
        /// </summary>
        public void FillIncludeListView(ReportBundleInfo bundleInfo)
        {
            _includeList.Clear();
            HashSet<string> mainAssetDic = new HashSet<string>();
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == bundleInfo.BundleName)
                {
                    mainAssetDic.Add(assetInfo.AssetPath);
                    _includeList.Add(assetInfo);
                }
            }

            foreach (var item in bundleInfo.BundleContents)
            {
                if (mainAssetDic.Contains(item.AssetPath) == false)
                {
                    var assetInfo = new ReportAssetInfo();
                    assetInfo.AssetPath = item.AssetPath;
                    assetInfo.AssetGUID = "--";
                    _includeList.Add(assetInfo);
                }
            }

            _includeListView.Clear();
            _includeListView.ClearSelection();
            _includeListView.itemsSource = _includeList;
            _includeListView.Refresh();
            _root.Q<ToolbarButton>("MidBar1").text = $"Include Assets ({_includeList.Count})";
        }

        /// <summary>
        /// 填充依赖窗口
        /// </summary>
        public void FillDependListView(ReportBundleInfo bundleInfo)
        {
            bool DependListContainsBundle(string bundleName, out int i)
            {
                for (int j = 0; j < _dependList.Count; j++)
                {
                    if (_dependList[j].BundleInfo.BundleName == bundleName)
                    {
                        i = j;
                        return true;
                    }
                }

                i = 0;
                return false;
            }

            _dependList.Clear();
            List<ReportAssetInfo> assetList = new List<ReportAssetInfo>();

            // 首先获得bundle包含的asset
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == bundleInfo.BundleName)
                {
                    assetList.Add(assetInfo);
                }
            }

            // 然后找到每个asset依赖的bundle，添加到dependList中
            foreach (var assetInfo in assetList)
            {
                foreach (string dependBundleName in assetInfo.DependBundles)
                {
                    int i = 0;
                    if (DependListContainsBundle(dependBundleName, out i))
                    {
                        _dependList[i].AssetPath += $";{Path.GetFileName(assetInfo.AssetPath)}";
                    }
                    else
                    {
                        DependInfo dependInfo = new DependInfo();
                        dependInfo.BundleInfo = _buildReport.GetBundleInfo(dependBundleName);
                        dependInfo.AssetPath = Path.GetFileName(assetInfo.AssetPath);
                        _dependList.Add(dependInfo);
                    }
                }
            }

            RefreshDependListView();
        }

        /// <summary>
        /// 刷新依赖窗口
        /// </summary>
        private void RefreshDependListView()
        {
            _dependListView.Clear();
            _dependListView.ClearSelection();
            _dependListView.itemsSource = SortDependListView();
            _dependListView.Refresh();

            RefreshSortingSymbol();
        }

        /// <summary>
        /// 排序依赖窗口
        /// </summary>
        private List<DependInfo> SortDependListView()
        {
            if (_dependSortMode == EDependSortMode.BundleName)
            {
                if (_dependDescendingSort)
                    return _dependList.OrderByDescending(a => a.BundleInfo.BundleName).ToList();
                else
                    return _dependList.OrderBy(a => a.BundleInfo.BundleName).ToList();
            }
            else if (_dependSortMode == EDependSortMode.BundleSize)
            {
                if (_dependDescendingSort)
                    return _dependList.OrderByDescending(a => a.BundleInfo.FileSize).ToList();
                else
                    return _dependList.OrderBy(a => a.BundleInfo.FileSize).ToList();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// 刷新排序符号
        /// </summary>
        private void RefreshSortingSymbol()
        {
            // 刷新符号
            _dependToolbar1.text = $"Depend Bundles ({_dependList.Count})";
            _dependToolbar3.text = "Size";

            if (_dependSortMode == EDependSortMode.BundleName)
            {
                if (_dependDescendingSort)
                    _dependToolbar1.text = $"Depend Bundles ({_dependListView.itemsSource.Count}) ↓";
                else
                    _dependToolbar1.text = $"Depend Bundles ({_dependListView.itemsSource.Count}) ↑";
            }
            else if (_dependSortMode == EDependSortMode.BundleSize)
            {
                if (_dependDescendingSort)
                    _dependToolbar3.text = "Size ↓";
                else
                    _dependToolbar3.text = "Size ↑";
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        #region ListView相关

        protected VisualElement MakeIncludeListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 100;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label3";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            return element;
        }

        protected void BindIncludeListViewItem(VisualElement element, int index)
        {
            List<ReportAssetInfo> containsList = _includeListView.itemsSource as List<ReportAssetInfo>;
            ReportAssetInfo assetInfo = containsList[index];

            // Asset Path
            var label1 = element.Q<Label>("Label1");
            label1.text = assetInfo.AssetPath;

            // Asset Source
            var label2 = element.Q<Label>("Label2");
            label2.text = assetInfo.AssetGUID != "--" ? "Main Asset" : "Builtin Asset";

            // GUID
            var label3 = element.Q<Label>("Label3");
            label3.text = assetInfo.AssetGUID;
        }

        protected VisualElement MakeDependListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 2f;
                label.style.width = 280;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label3";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 100;
                element.Add(label);
            }

            return element;
        }

        protected void BindDependListViewItem(VisualElement element, int index)
        {
            List<DependInfo> containsList = _dependListView.itemsSource as List<DependInfo>;
            DependInfo dependInfo = containsList[index];

            // Depend Bundles
            var label1 = element.Q<Label>("Label1");
            label1.text = dependInfo.BundleInfo.BundleName;

            // Assets That Cause Dependence
            var label2 = element.Q<Label>("Label2");
            label2.text = dependInfo.AssetPath;

            // Size
            var label3 = element.Q<Label>("Label3");
            label3.text = EditorUtility.FormatBytes(dependInfo.BundleInfo.FileSize);
        }

        #endregion

        private void OnClickDependToolbar1()
        {
            if (_dependSortMode != EDependSortMode.BundleName)
            {
                _dependSortMode = EDependSortMode.BundleName;
                _dependDescendingSort = false;
                RefreshDependListView();
            }
            else
            {
                _dependDescendingSort = !_dependDescendingSort;
                RefreshDependListView();
            }
        }

        private void OnClickDependToolbar3()
        {
            if (_dependSortMode != EDependSortMode.BundleSize)
            {
                _dependSortMode = EDependSortMode.BundleSize;
                _dependDescendingSort = false;
                RefreshDependListView();
            }
            else
            {
                _dependDescendingSort = !_dependDescendingSort;
                RefreshDependListView();
            }
        }

        #region 树状视图相关

        /// <summary>
        /// 初始化树状视图数据
        /// </summary>
        private void InitTreeData()
        {
            _treeData.Clear();

            foreach (var bundleInfo in _buildReport.BundleInfos)
            {
                if (bundleInfo.BundleContents.Count == 0)
                {
                    continue;
                }

                // 部分bundle可能会有来自多个目录的资产
                HashSet<string> keys = new HashSet<string>();
                foreach (var assetInfo in bundleInfo.BundleContents)
                {
                    string keyTmp = Path.GetDirectoryName(assetInfo.AssetPath);
                    if (string.IsNullOrEmpty(keyTmp))
                    {
                        continue;
                    }

                    string key = keyTmp.Replace('\\', '/');
                    keys.Add(key);
                }

                // 记录当前bundle存在的目录
                foreach (var key in keys)
                {
                    if (!_treeData.ContainsKey(key))
                    {
                        _treeData[key] = new List<ReportBundleInfo> { bundleInfo };
                    }
                    else
                    {
                        if (!_treeData[key].Contains(bundleInfo))
                        {
                            _treeData[key].Add(bundleInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化树状视图
        /// </summary>
        private void InitTreeViewer()
        {
            _treeViewer.ClearAll();

            string assetsPath = Application.dataPath;
            TreeNode root = InitTreeNode(assetsPath);
            if (root != null)
            {
                GetBundleActualData(root);
                _treeViewer.SetRootItem(root);
                _treeViewer.RebuildView();
            }
        }

        /// <summary>
        /// 递归初始化每个节点
        /// </summary>
        private TreeNode InitTreeNode(string path)
        {
            TreeNode root;
            int bundleCount = 0;
            long bundleSize = 0;
            List<string> dirs = Directory.GetDirectories(path).ToList();
            List<TreeNode> children = new List<TreeNode>();

            // 构建子节点
            foreach (var dir in dirs)
            {
                TreeNode child = InitTreeNode(dir);
                if (child == null)
                {
                    continue;
                }

                TreeNodeData userData = child.UserData as TreeNodeData;
                if (userData != null)
                {
                    bundleCount += userData.BundleCount;
                    bundleSize += userData.BundleSize;
                }

                children.Add(child);
            }

            // 构建当前节点
            string key = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)).Replace('\\', '/');
            // 该目录下有bundle
            if (_treeData.ContainsKey(key))
            {
                List<TreeNodeData> childrenData = new List<TreeNodeData>();
                foreach (var bundleInfo in _treeData[key])
                {
                    TreeNodeData userDataBundle = new TreeNodeData();
                    userDataBundle.Name = bundleInfo.BundleName;
                    userDataBundle.IsBundle = true;
                    userDataBundle.BundleCount = 1;
                    userDataBundle.BundleSize = bundleInfo.FileSize;
                    childrenData.Add(userDataBundle);

                    bundleSize += userDataBundle.BundleSize;
                }

                bundleCount += _treeData[key].Count;

                TreeNodeData userDataDir = new TreeNodeData();
                userDataDir.Name = Path.GetFileName(path);
                userDataDir.IsBundle = false;
                userDataDir.BundleCount = bundleCount;
                userDataDir.BundleSize = bundleSize;
                root = new TreeNode(userDataDir);

                foreach (var childData in childrenData)
                {
                    root.AddChild(new TreeNode(childData));
                }
            }
            // 该目录下只有子目录
            else
            {
                TreeNodeData userDataDir = new TreeNodeData();
                userDataDir.Name = Path.GetFileName(path);
                userDataDir.IsBundle = false;
                userDataDir.BundleCount = bundleCount;
                userDataDir.BundleSize = bundleSize;
                root = new TreeNode(userDataDir);
            }

            foreach (var child in children)
            {
                root.AddChild(child);
            }

            // 隐藏没有Bundle的目录
            return bundleCount > 0 ? root : null;
        }

        /// <summary>
        /// 递归获取每个目录下的实际Bundle数量和大小
        /// </summary>
        private HashSet<ReportBundleInfo> GetBundleActualData(TreeNode root)
        {
            long GetBundleActualSize(HashSet<ReportBundleInfo> uniqueBundles)
            {
                long size = 0;
                foreach (var bundle in uniqueBundles)
                {
                    size += bundle.FileSize;
                }

                return size;
            }

            HashSet<ReportBundleInfo> uniqueBundles = new HashSet<ReportBundleInfo>();

            // 先遍历完所有子节点，更新uniqueBundles
            foreach (var child in root.Children)
            {
                uniqueBundles.UnionWith(GetBundleActualData(child));
            }

            TreeNodeData userData = root.UserData as TreeNodeData;
            if (userData != null)
            {
                if (userData.IsBundle)
                {
                    uniqueBundles.Add(_buildReport.GetBundleInfo(userData.Name));
                }
                else
                {
                    userData.BundleActualCount = uniqueBundles.Count;
                    userData.BundleActualSize = GetBundleActualSize(uniqueBundles);
                }
            }

            return uniqueBundles;
        }

        private void MakeTreeViewerItem(VisualElement container)
        {
            var label = new Label
            {
                name = "Label"
            };
            container.Add(label);
        }

        private void BindTreeViewerItem(VisualElement container, object userData)
        {
            var label = container.Q<Label>("Label");

            if (userData is TreeNodeData treeNodeData)
            {
                if (treeNodeData.IsBundle)
                {
                    label.text = treeNodeData.Name;
                }
                else
                {
                    label.text =
                        $"{treeNodeData.Name} ({treeNodeData.BundleActualCount}) ({EditorUtility.FormatBytes(treeNodeData.BundleActualSize)})";
                }
            }

            label.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        private void OnSelectionChange(IEnumerable<object> objs)
        {
            foreach (var item in objs)
            {
                TreeNode treeNode = item as TreeNode;
                if (treeNode != null && treeNode.Children.Count == 0)
                {
                    TreeNodeData treeNodeData = treeNode.UserData as TreeNodeData;
                    if (treeNodeData != null)
                    {
                        ReportBundleInfo bundleInfo = _buildReport.GetBundleInfo(treeNodeData.Name);
                        RebuildView(bundleInfo);
                    }
                }

                break;
            }
        }

        #endregion
    }
}
#endif