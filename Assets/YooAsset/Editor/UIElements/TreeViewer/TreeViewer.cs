#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class TreeViewer : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TreeViewer, UxmlTraits>
        {
        }

        private readonly ListView _listView;
        private readonly List<TreeNode> _flattenList = new List<TreeNode>(1000);
        private readonly List<TreeNode> _rootList = new List<TreeNode>(100);

        /// <summary>
        /// 制作列表元素
        /// </summary>
        public Action<VisualElement> makeItem { get; set; }

        /// <summary>
        /// 绑定列表数据
        /// </summary>
        public Action<VisualElement, object> bindItem { get; set; }

        /// <summary>
        /// 绑定点击事件
        /// </summary>
        public Action<IEnumerable<object>> onSelectionChange { get; set; }

        public TreeViewer()
        {
            this.style.flexShrink = 1f;
            this.style.flexGrow = 1f;

            // 创建ListView
            _listView = new ListView();
            _listView.style.flexShrink = 1f;
            _listView.style.flexGrow = 1f;
            _listView.itemsSource = _flattenList;
            _listView.makeItem = MakeItemInternal;
            _listView.bindItem = BindItemInternal;
#if UNITY_2020_1_OR_NEWER
            _listView.onSelectionChange += OnSelectionChangeInternal;
#else
            _listView.onSelectionChanged += OnSelectionChangeInternal;
#endif
            this.Add(_listView);
        }

        /// <summary>
        /// 设置根节点数据
        /// </summary>
        public void SetRootItem(TreeNode rootNode)
        {
            _rootList.Add(rootNode);
        }

        /// <summary>
        /// 设置根节点数据
        /// </summary>
        public void SetRootItems(List<TreeNode> rootNodes)
        {
            _rootList.AddRange(rootNodes);
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void ClearAll()
        {
            _rootList.Clear();
        }

        /// <summary>
        /// 重新绘制视图
        /// </summary>
        public void RebuildView()
        {
            _flattenList.Clear();
            foreach (var treeRoot in _rootList)
            {
                FlattenTree(treeRoot, 0);
            }

            _listView.Rebuild();
        }

        /// <summary>
        /// 将树形结构扁平化为列表
        /// </summary>
        private void FlattenTree(TreeNode node, int depth)
        {
            _flattenList.Add(node);
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    FlattenTree(child, depth + 1);
                }
            }
        }

        private VisualElement MakeItemInternal()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            // 折叠按钮
            var toggle = new ToggleFoldout();
            toggle.text = string.Empty;
            toggle.name = "foldout";
            toggle.style.alignSelf = Align.Center;
            toggle.style.width = 15;
            toggle.style.height = 15;
            toggle.RegisterValueChangedCallback((ChangeEvent<bool> callback) =>
            {
                var treeNode = toggle.userData as TreeNode;
                treeNode.IsExpanded = toggle.value;
                RebuildView();
            });
            container.Add(toggle);

            // 用户自定义元素
            if (makeItem != null)
            {
                makeItem.Invoke(container);
            }

            return container;
        }

        private void BindItemInternal(VisualElement item, int index)
        {
            var treeNode = _flattenList[index];

            // 设置折叠状态
            var toggle = item.Q<ToggleFoldout>("foldout");
            toggle.SetValueWithoutNotify(treeNode.IsExpanded);
            toggle.userData = treeNode;
            toggle.style.marginLeft = treeNode.GetDepth() * 15;

            // 隐藏折叠按钮
            if (treeNode.Children.Count == 0)
            {
                toggle.style.visibility = Visibility.Hidden;
            }
            else
            {
                toggle.style.visibility = Visibility.Visible;
            }

            // 用户自定义元素
            if (bindItem != null)
            {
                bindItem.Invoke(item, treeNode.UserData);
            }
        }

        private void OnSelectionChangeInternal(IEnumerable<object> objs)
        {
            onSelectionChange(objs);
        }
    }
}
#endif