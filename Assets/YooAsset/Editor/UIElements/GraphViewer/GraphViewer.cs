#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class GraphViewer : GraphView
    {
        public new class UxmlFactory : UxmlFactory<GraphViewer, UxmlTraits>
        {
        }

        private readonly List<GraphNode> _roots = new List<GraphNode>();
        private readonly List<GraphNode> _nodes = new List<GraphNode>();
        private readonly List<Edge> _edges = new List<Edge>();

        /// <summary>
        /// 制作节点元素
        /// </summary>
        public Action<GraphNode> MakeGraphNode { get; set; }

        /// <summary>
        /// 绑定节点数据
        /// </summary>
        public Action<GraphNode> BindGraphNode { get; set; }

        public GraphViewer()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void SetRootNode(GraphNode root)
        {
            // 重新设置根节点时清空GraphViewer
            ClearGraphViewer();

            _roots.Add(root);
            _nodes.Add(root);
            AddNode(root);
        }

        public void SetRootNodes(List<GraphNode> roots)
        {
            // 重新设置根节点时清空GraphViewer
            ClearGraphViewer();

            _roots.AddRange(roots);
            _nodes.AddRange(roots);
            foreach (var root in roots)
            {
                AddNode(root);
            }
        }

        private void ClearGraphViewer()
        {
            foreach (var edge in _edges)
            {
                RemoveElement(edge);
            }

            foreach (var node in _nodes)
            {
                RemoveElement(node);
            }

            _edges.Clear();
            _nodes.Clear();
            _roots.Clear();
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        private void AddNode(GraphNode graphNode)
        {
            AddElement(graphNode);
            graphNode.MakeGraphNode = MakeGraphNode;
            graphNode.BindGraphNode = BindGraphNode;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChildren(GraphNode parentNode, List<GraphNode> children)
        {
            foreach (var child in children)
            {
                AddChild(parentNode, child);
            }
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(GraphNode parentNode, GraphNode child)
        {
            AddNode(child);
            _nodes.Add(child);
            parentNode.AddChild(child);
        }

        /// <summary>
        /// 添加边
        /// </summary>
        public void AddEdge(GraphNode parentNode, GraphNode child, int inputIndex, int outputIndex)
        {
            Edge edge = new Edge();

            Port outputPort = parentNode.OutputPorts[inputIndex].Port;
            Port inputPort = child.InputPorts[outputIndex].Port;

            edge.output = outputPort;
            edge.input = inputPort;

            outputPort.Connect(edge);
            inputPort.Connect(edge);

            AddElement(edge);
            _edges.Add(edge);
            parentNode.AddEdge(edge);
        }

        #region 自动布局相关

        public void RebuildView()
        {
            schedule.Execute(LayoutGraphView).StartingIn(10);
        }

        private void LayoutGraphView()
        {
            foreach (var root in _roots)
            {
                LayoutGraphViewNode(root);
            }
        }

        private void LayoutGraphViewNode(GraphNode root)
        {
            if (root == null)
                return;

            Queue<GraphNode> queue = new Queue<GraphNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (node.Parent == null)
                {
                    node.Position = new Vector2(0, 0);
                }
                else
                {
                    float x;
                    if (node.Parent.Parent == null)
                    {
                        x = node.Parent.Position.x + node.Parent.layout.size.x + 50;
                    }
                    else
                    {
                        x = node.Parent.Position.x + GetMaxChildrenWidth(node.Parent.Parent) + 50;
                    }

                    float baseY = node.Parent.Position.y - (node.Parent.Children.Count - 1) * 100;
                    node.Position = new Vector2(x, baseY + node.GetSiblingIndex() * 200);
                }

                node.SetPosition(new Rect(node.Position.x, node.Position.y, 0, 0));

                // 将子节点加入队列
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        private float GetMaxChildrenWidth(GraphNode node)
        {
            if (node == null)
            {
                return 0;
            }

            float maxWidth = 0;

            foreach (var child in node.Children)
            {
                maxWidth = Mathf.Max(child.layout.size.x, maxWidth);
            }

            return maxWidth;
        }

        #endregion
    }
}

#endif