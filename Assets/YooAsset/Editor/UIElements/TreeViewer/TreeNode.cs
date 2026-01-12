using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 树形结构的节点，持有用户数据并维护父子关系
    /// </summary>
    public class TreeNode
    {
        private const int MaxDepth = 100;

        /// <summary>
        /// 子节点集合
        /// </summary>
        public List<TreeNode> Children { get; } = new List<TreeNode>(10);

        /// <summary>
        /// 父节点
        /// </summary>
        public TreeNode Parent { get; set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded { get; set; } = false;


        /// <summary>
        /// 创建树节点实例
        /// </summary>
        /// <param name="userData">节点携带的用户自定义数据</param>
        public TreeNode(object userData)
        {
            UserData = userData;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="child">要添加的子节点</param>
        public void AddChild(TreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// 清理所有子节点
        /// </summary>
        public void ClearChildren()
        {
            foreach(var child in Children)
            {
                child.Parent = null;
            }
            Children.Clear();
        }

        /// <summary>
        /// 计算节点在树中的深度
        /// </summary>
        /// <returns>从根节点到当前节点的层级数，根节点深度为 0。</returns>
        public int GetDepth()
        {
            int depth = 0;
            TreeNode current = this;
            while (current.Parent != null)
            {
                depth++;
                current = current.Parent;

                // 注意：检测无限循环
                if (depth >= MaxDepth)
                {
                    Debug.LogError($"TreeNode.GetDepth: max depth {MaxDepth} exceeded.");
                    return depth;
                }
            }
            return depth;
        }
    }
}