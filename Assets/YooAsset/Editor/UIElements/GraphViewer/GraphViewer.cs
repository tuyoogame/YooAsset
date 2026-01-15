#if UNITY_2019_4_OR_NEWER
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

        public GraphViewer()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // var grid = new ABDVGraphViewGirdBg();
            // Insert(0, grid);
            // grid.StretchToParentSize();
        }

        public void SetRootNode(GraphNode root)
        {
            // 重新设置根节点时清空GraphView
            ClearGraphView();

            _roots.Add(root);
            _nodes.Add(root);
            AddElement(root);
        }

        public void SetRootNodes(List<GraphNode> roots)
        {
            // 重新设置根节点时清空GraphView
            ClearGraphView();

            _roots.AddRange(roots);
            _nodes.AddRange(roots);
            foreach (var root in roots)
            {
                AddElement(root);
            }
        }

        private void ClearGraphView()
        {
            foreach (var edge in _edges)
            {
                RemoveElement(edge);
            }

            _edges.Clear();
            foreach (var node in _nodes)
            {
                RemoveElement(node);
            }

            _nodes.Clear();
            _roots.Clear();
        }

        /// <summary>
        /// 添加子节点同时添加边
        /// </summary>
        public void AddChildAndEdge(GraphNode parent, GraphNode child)
        {
            _nodes.Add(child);
            AddElement(child);
            parent.AddChild(child);

            AddEdge(parent, child, 0, 0);
        }

        /// <summary>
        /// 添加边
        /// </summary>
        private void AddEdge(GraphNode inputNode, GraphNode outputNode, int inputIndex, int outputIndex)
        {
            var outputPort = inputNode.GetPort(Direction.Output, inputIndex);
            var inputPort = outputNode.GetPort(Direction.Input, outputIndex);
            Edge edge = new Edge();
            edge.output = outputPort;
            edge.input = inputPort;

            outputPort.Connect(edge);
            inputPort.Connect(edge);

            AddElement(edge);
            _edges.Add(edge);
            inputNode.AddEdge(edge);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        #region 自动布局相关

        public void LayoutGraphView()
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
                    node.Position = new Vector2(x, baseY + node.SiblingIndex * 200);
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