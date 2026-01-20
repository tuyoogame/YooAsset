#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ReporterGraphViewer
    {
        private class BundleTableData : DefaultTableData
        {
            public ReportBundleInfo BundleInfo;
        }

        private class TreeNodeData
        {
            public string Name;
            public bool IsBundle;
            public int BundleCount;
            public int BundleActualCount; // 当前目录下去掉冗余后的bundle数量
            public long BundleSize;
            public long BundleActualSize; // 当前目录下去掉冗余后的bundle总大小
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableViewer _dependTableView;
        private TableViewer _referenceTableView;
        private TreeViewer _treeViewer;
        private Dictionary<string, List<ReportBundleInfo>> _treeData = new Dictionary<string, List<ReportBundleInfo>>();
        private GraphViewer _graphViewer;

        private BuildReport _buildReport;
        private string _reportFilePath;

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

            // 依赖Bundle列表
            _dependTableView = _root.Q<TableViewer>("LeftTableView");
            _dependTableView.ClickTableDataEvent = OnClickTableView;
            CreateTableViewColumns(_dependTableView);

            // 被依赖Bundle列表
            _referenceTableView = _root.Q<TableViewer>("RightTableView");
            _referenceTableView.ClickTableDataEvent = OnClickTableView;
            CreateTableViewColumns(_referenceTableView);

            // 树状视图
            _treeViewer = _root.Q<TreeViewer>("FileExplorer");
            _treeViewer.makeItem = MakeTreeViewerItem;
            _treeViewer.bindItem = BindTreeViewerItem;
            _treeViewer.onSelectionChange = OnSelectionChange;

            // 网状视图
            _graphViewer = _root.Q<GraphViewer>("GraphView");
            _graphViewer.MakeGraphNode = MakeGraphNode;

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

            GraphNode root = new GraphNode(bundleInfo);
            _graphViewer.SetRootNode(root);

            FillInDependTableView(bundleInfo);
            FillInReferenceTableView(bundleInfo);
        }

        private void RebuildView(ReportBundleInfo bundleInfo)
        {
            if (bundleInfo == null)
            {
                return;
            }

            GraphNode root = new GraphNode(bundleInfo);
            _graphViewer.SetRootNode(root);

            FillInDependTableView(bundleInfo);
            FillInReferenceTableView(bundleInfo);
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport, string reprotFilePath)
        {
            _buildReport = buildReport;
            _reportFilePath = reprotFilePath;

            // 清空旧数据
            _dependTableView.ClearAll(false, true);
            _referenceTableView.ClearAll(false, true);

            InitTreeData();
            InitTreeViewer();
        }

        #region 表格视图相关

        private void CreateTableViewColumns(TableViewer tableViewer)
        {
            //BundleName
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                columnStyle.Counter = true;
                var headerTitle = tableViewer.name == "LeftTableView" ? "Depend Bundle Name" : "Reference Bundle Name";
                var column = new TableColumn("BundleName", headerTitle, columnStyle);
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
                tableViewer.AddColumn(column);
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
                tableViewer.AddColumn(column);
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
                tableViewer.AddColumn(column);
            }
        }

        private void FillInDependTableView(ReportBundleInfo bundleInfo)
        {
            var sourceDatas = new List<ITableData>(bundleInfo.DependBundles.Count);

            foreach (var bundleName in bundleInfo.DependBundles)
            {
                var dependBundleInfo = _buildReport.GetBundleInfo(bundleName);
                var rowData = new BundleTableData();
                rowData.BundleInfo = dependBundleInfo;
                rowData.AddStringValueCell("BundleName", dependBundleInfo.BundleName);
                rowData.AddLongValueCell("FileSize", dependBundleInfo.FileSize);
                rowData.AddStringValueCell("Tags", string.Join(";", dependBundleInfo.Tags));
                sourceDatas.Add(rowData);
            }

            _dependTableView.itemsSource = sourceDatas;
            _dependTableView.RebuildView();
        }

        private void FillInReferenceTableView(ReportBundleInfo bundleInfo)
        {
            var sourceDatas = new List<ITableData>(bundleInfo.ReferenceBundles.Count);

            foreach (var bundleName in bundleInfo.ReferenceBundles)
            {
                var referenceBundleInfo = _buildReport.GetBundleInfo(bundleName);
                var rowData = new BundleTableData();
                rowData.BundleInfo = referenceBundleInfo;
                rowData.AddStringValueCell("BundleName", referenceBundleInfo.BundleName);
                rowData.AddLongValueCell("FileSize", referenceBundleInfo.FileSize);
                rowData.AddStringValueCell("Tags", string.Join(";", referenceBundleInfo.Tags));
                sourceDatas.Add(rowData);
            }

            _referenceTableView.itemsSource = sourceDatas;
            _referenceTableView.RebuildView();
        }

        private void OnClickTableView(PointerDownEvent evt, ITableData data)
        {
            // 鼠标双击后检视
            if (evt.clickCount != 2)
                return;

            var bundleTableData = data as BundleTableData;
            if (bundleTableData.BundleInfo.Encrypted)
                return;

            if (_buildReport.Summary.BuildBundleType == (int)EBuildBundleType.AssetBundle)
            {
                string rootDirectory = Path.GetDirectoryName(_reportFilePath);
                string filePath = $"{rootDirectory}/{bundleTableData.BundleInfo.FileName}";
                if (File.Exists(filePath))
                    Selection.activeObject = AssetBundleRecorder.GetAssetBundle(filePath);
                else
                    Selection.activeObject = null;
            }
        }

        #endregion

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
                name = "Label",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft
                }
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

        #region 网状视图相关

        private void MakeGraphNode(GraphNode graphNode)
        {
            // 添加输入输出端口
            GraphPort inputPort = new GraphPort("Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                typeof(float));
            GraphPort outputPort = new GraphPort("Output", Orientation.Horizontal, Direction.Output,
                Port.Capacity.Single,
                typeof(float));
            graphNode.AddGraphPort(inputPort);
            graphNode.AddGraphPort(outputPort);

            // 添加自定义元素
            var graphNodeData = graphNode.UserData as ReportBundleInfo;
            if (graphNodeData == null)
            {
                return;
            }

            // 标题文本
            graphNode.title = graphNodeData.BundleName;

            // Bundle大小
            var labelFileSize = new Label($"File Size: {EditorUtility.FormatBytes(graphNodeData.FileSize)}")
            {
                name = "FileSize",
                style =
                {
                    flexGrow = 1
                }
            };
            graphNode.mainContainer.Add(labelFileSize);

            // 依赖Bundle数量
            var labelDependCount = new Label($"Depend Count: {graphNodeData.DependBundles.Count}")
            {
                name = "DependCount",
                style =
                {
                    flexGrow = 1
                }
            };
            graphNode.mainContainer.Add(labelDependCount);

            // 被依赖Bundle数量
            var labelReferenceCount = new Label($"Reference Count: {graphNodeData.ReferenceBundles.Count}")
            {
                name = "ReferenceCount",
                style =
                {
                    flexGrow = 1
                }
            };
            graphNode.mainContainer.Add(labelReferenceCount);

            var btnContainer = new VisualElement
            {
                name = "BtnContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 30
                }
            };
            graphNode.mainContainer.Add(btnContainer);

            var btnShowInfo = new Button
            {
                name = "BtnShowInfo",
                text = "Show Info",
                style =
                {
                    width = 100,
                    flexGrow = 1
                }
            };
            btnShowInfo.clicked += () =>
            {
                FillInDependTableView(graphNodeData);
                FillInReferenceTableView(graphNodeData);
            };
            btnContainer.Add(btnShowInfo);

            if (graphNodeData.DependBundles.Count > 0)
            {
                var btnChildren = new Button
                {
                    name = "BtnChildren",
                    text = "Show Children",
                    style =
                    {
                        width = 100,
                        flexGrow = 1
                    }
                };
                btnChildren.clicked += () =>
                {
                    if (graphNode.IsExpanded == false)
                    {
                        if (graphNode.Children.Count > 0)
                        {
                            graphNode.ShowChildren();
                        }
                        else
                        {
                            graphNode.IsExpanded = !graphNode.IsExpanded;

                            List<GraphNode> children = new List<GraphNode>();
                            foreach (var bundleName in graphNodeData.DependBundles)
                            {
                                children.Add(new GraphNode(_buildReport.GetBundleInfo(bundleName)));
                            }

                            _graphViewer.AddChildren(graphNode, children);
                            foreach (var child in children)
                            {
                                _graphViewer.AddEdge(graphNode, child, 0, 0);
                            }

                            _graphViewer.RebuildView();

                            btnChildren.text = "Hide Children";
                        }
                    }
                    else
                    {
                        graphNode.HideChildren();
                    }
                };
                btnContainer.Add(btnChildren);

                graphNode.ShowChildrenCallBack = () => btnChildren.text = "Hide Children";
                graphNode.HideChildrenCallBack = () => btnChildren.text = "Show Children";
            }
        }

        #endregion
    }
}
#endif