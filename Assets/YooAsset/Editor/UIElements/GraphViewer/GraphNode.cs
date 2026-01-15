#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class GraphNode : Node
    {
        private readonly ReporterGraphViewer _viewer;
        private readonly List<Port> _inputPorts = new List<Port>(5);
        private readonly List<Port> _outputPorts = new List<Port>(5);
        private readonly List<Edge> _edges = new List<Edge>();
        public new readonly List<GraphNode> Children = new List<GraphNode>(10);

        private bool _isExpanded = false;
        private readonly Button _btnChildren;

        public GraphNode Parent { get; set; }
        public Vector2 Position { get; set; }
        public ReportBundleInfo BundleInfo { get; set; }

        // 当前节点是父节点的第几个子节点
        public int SiblingIndex
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.Children.IndexOf(this);
            }
        }

        public GraphNode(ReportBundleInfo bundleInfo, ReporterGraphViewer viewer)
        {
            _viewer = viewer;
            BundleInfo = bundleInfo;
            title = BundleInfo.BundleName;

            // 添加输入端口
            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                typeof(float));
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);
            _inputPorts.Add(inputPort);

            // 添加输出端口
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                typeof(float));
            outputPort.portName = "Output";
            outputContainer.Add(outputPort);
            _outputPorts.Add(outputPort);

            // 添加内容区域
            var labelSize = new Label($"Size: {EditorUtility.FormatBytes(BundleInfo.FileSize)}");
            labelSize.style.flexGrow = 1;
            mainContainer.Add(labelSize);

            int dependCount = _viewer.GetDependencyCount(this);
            var labelDependCount = new Label($"Depend Count: {dependCount}");
            labelDependCount.style.flexGrow = 1;
            mainContainer.Add(labelDependCount);

            var labelHash = new Label($"Hash: {BundleInfo.FileHash}");
            labelHash.style.flexGrow = 1;
            mainContainer.Add(labelHash);

            // var labelTags = new Label($"Tags: {BundleInfo.GetTagsString()}");
            // labelTags.style.flexGrow = 1;
            // mainContainer.Add(labelTags);

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.height = 30;
            mainContainer.Add(container);

            var buttonShowInfo = new Button();
            buttonShowInfo.text = "Show Info";
            buttonShowInfo.style.width = 100;
            buttonShowInfo.style.flexGrow = 1;
            buttonShowInfo.clicked += OnBtnShowInfoClicked;
            container.Add(buttonShowInfo);

            if (dependCount > 0)
            {
                _btnChildren = new Button();
                _btnChildren.text = "Show Children";
                _btnChildren.style.width = 100;
                _btnChildren.style.flexGrow = 1;
                _btnChildren.clicked += OnBtnChildrenClicked;
                container.Add(_btnChildren);
            }
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(GraphNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// 添加边
        /// </summary>
        public void AddEdge(Edge edge)
        {
            _edges.Add(edge);
        }

        /// <summary>
        /// 清理所有子节点
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.Parent = null;
            }

            Children.Clear();
        }

        /// <summary>
        /// 计算节点的深度
        /// </summary>
        public int GetDepth()
        {
            int depth = 0;
            GraphNode current = this;
            while (current.Parent != null)
            {
                depth++;
                current = current.Parent;
            }

            return depth;
        }

        /// <summary>
        /// 获取端口
        /// </summary>
        public Port GetPort(Direction direction, int index)
        {
            if (direction == Direction.Input)
            {
                return _inputPorts[index];
            }
            else
            {
                return _outputPorts[index];
            }
        }

        private void OnBtnShowInfoClicked()
        {
            _viewer.FillDependListView(BundleInfo);
            _viewer.FillIncludeListView(BundleInfo);
        }

        private void OnBtnChildrenClicked()
        {
            if (_isExpanded == false)
            {
                if (_btnChildren != null)
                {
                    _btnChildren.text = "Hide Children";
                }

                if (Children.Count > 0)
                {
                    ShowChildren();
                }
                else
                {
                    _viewer.AddChildren(this);
                }

                _isExpanded = !_isExpanded;
            }
            else
            {
                HideChildren();
            }
        }

        private void HideChildren()
        {
            if (_btnChildren != null)
            {
                _btnChildren.text = "Show Children";
            }

            _isExpanded = !_isExpanded;

            foreach (var child in Children)
            {
                child.visible = false;
                child.HideChildren();
            }

            foreach (var edge in _edges)
            {
                edge.visible = false;
            }
        }

        private void ShowChildren()
        {
            foreach (var child in Children)
            {
                child.visible = true;
            }

            foreach (var edge in _edges)
            {
                edge.visible = true;
            }
        }
    }
}

#endif