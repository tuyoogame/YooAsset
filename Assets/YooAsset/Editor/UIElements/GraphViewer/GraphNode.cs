#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace YooAsset.Editor
{
    public class GraphNode : Node
    {
        /// <summary>
        /// 子节点集合
        /// </summary>
        public new List<GraphNode> Children = new List<GraphNode>(10);

        /// <summary>
        /// 父节点
        /// </summary>
        public GraphNode Parent { get; set; }

        /// <summary>
        /// 输入端口
        /// </summary>
        public List<GraphPort> InputPorts = new List<GraphPort>(5);

        /// <summary>
        /// 输出端口
        /// </summary>
        public List<GraphPort> OutputPorts = new List<GraphPort>(5);

        /// <summary>
        /// 节点与子节点连接的边
        /// </summary>
        private List<Edge> _edges = new List<Edge>();

        /// <summary>
        /// 用户数据
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded { get; set; } = false;

        /// <summary>
        /// 节点坐标
        /// </summary>
        public Vector2 Position { get; set; }

        private Action<GraphNode> _makeGraphNode;
        private Action<GraphNode> _bindGraphNode;

        /// <summary>
        /// 制作节点元素
        /// </summary>
        public Action<GraphNode> MakeGraphNode
        {
            get => _makeGraphNode;
            set
            {
                if (_makeGraphNode == value)
                {
                    return;
                }

                _makeGraphNode = value;
                _makeGraphNode.Invoke(this);
            }
        }

        /// <summary>
        /// 绑定节点数据
        /// </summary>
        public Action<GraphNode> BindGraphNode
        {
            get => _bindGraphNode;
            set
            {
                if (_bindGraphNode == value)
                    return;
                _bindGraphNode = value;
                _bindGraphNode.Invoke(this);
            }
        }

        public Action ShowChildrenCallBack;
        public Action HideChildrenCallBack;

        public GraphNode(object userData)
        {
            UserData = userData;
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
        /// 节点是父节点的第几个子节点
        /// </summary>
        public int GetSiblingIndex()
        {
            if (Parent == null)
            {
                return 0;
            }

            return Parent.Children.IndexOf(this);
        }

        /// <summary>
        /// 添加端口
        /// </summary>
        public void AddGraphPort(GraphPort graphPort)
        {
            if (graphPort.Port.direction == Direction.Input)
            {
                inputContainer.Add(graphPort.Port);
                InputPorts.Add(graphPort);
            }
            else
            {
                outputContainer.Add(graphPort.Port);
                OutputPorts.Add(graphPort);
            }
        }

        /// <summary>
        /// 隐藏所有子孙节点
        /// </summary>
        public void HideChildren()
        {
            IsExpanded = !IsExpanded;

            foreach (var child in Children)
            {
                child.visible = false;
                child.HideChildren();
            }

            foreach (var edge in _edges)
            {
                edge.visible = false;
            }

            HideChildrenCallBack?.Invoke();
        }

        /// <summary>
        /// 显示子节点
        /// </summary>
        public void ShowChildren()
        {
            IsExpanded = !IsExpanded;

            foreach (var child in Children)
            {
                child.visible = true;
            }

            foreach (var edge in _edges)
            {
                edge.visible = true;
            }

            ShowChildrenCallBack?.Invoke();
        }
    }
}

#endif