#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	internal class SummaryReporterViewer
	{
		private class ItemWrapper
		{
			public string Title { private set; get; }
			public string Value { private set; get; }

			public ItemWrapper(string title, string value)
			{
				Title = title;
				Value = value;
			}
		}

		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ListView _listView;
		private BuildReport _buildReport;
		private readonly List<ItemWrapper> _items = new List<ItemWrapper>();


		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetPath();
			string uxml = $"{rootPath}/Editor/AssetBundleReporter/VisualViewers/SummaryReporterViewer.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(SummaryReporterViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 概述列表
			_listView = _root.Q<ListView>("ListView");
			_listView.makeItem = MakeListViewItem;
			_listView.bindItem = BindListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport)
		{
			_buildReport = buildReport;
			_listView.Clear();
			_items.Clear();

			_items.Add(new ItemWrapper("引擎版本", buildReport.Summary.UnityVersion));
			_items.Add(new ItemWrapper("构建时间", buildReport.Summary.BuildTime));
			_items.Add(new ItemWrapper("构建耗时", $"{buildReport.Summary.BuildSeconds}秒"));
			_items.Add(new ItemWrapper("构建平台", $"{buildReport.Summary.BuildTarget}"));
			_items.Add(new ItemWrapper("构建版本", $"{buildReport.Summary.BuildVersion}"));

			_items.Add(new ItemWrapper("开启自动分包", $"{buildReport.Summary.ApplyRedundancy}"));
			_items.Add(new ItemWrapper("开启资源包后缀名", $"{buildReport.Summary.AppendFileExtension}"));

			_items.Add(new ItemWrapper("自动收集着色器", $"{buildReport.Summary.IsCollectAllShaders}"));
			_items.Add(new ItemWrapper("着色器资源包名称", $"{buildReport.Summary.ShadersBundleName}"));

			_items.Add(new ItemWrapper("高级构建选项", $"---------------------------------"));
			_items.Add(new ItemWrapper("IsForceRebuild", $"{buildReport.Summary.IsForceRebuild}"));
			_items.Add(new ItemWrapper("BuildinTags", $"{buildReport.Summary.BuildinTags}"));
			_items.Add(new ItemWrapper("CompressOption", $"{buildReport.Summary.CompressOption}"));
			_items.Add(new ItemWrapper("IsAppendHash", $"{buildReport.Summary.IsAppendHash}"));
			_items.Add(new ItemWrapper("IsDisableWriteTypeTree", $"{buildReport.Summary.IsDisableWriteTypeTree}"));
			_items.Add(new ItemWrapper("IsIgnoreTypeTreeChanges", $"{buildReport.Summary.IsIgnoreTypeTreeChanges}"));
			_items.Add(new ItemWrapper("IsDisableLoadAssetByFileName", $"{buildReport.Summary.IsDisableLoadAssetByFileName}"));
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

		// 列表相关
		private VisualElement MakeListViewItem()
		{
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label2";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 150;
				element.Add(label);
			}

			return element;
		}
		private void BindListViewItem(VisualElement element, int index)
		{
			var itemWrapper = _items[index];

			// Title
			var label1 = element.Q<Label>("Label1");
			label1.text = itemWrapper.Title;

			// Value
			var label2 = element.Q<Label>("Label2");
			label2.text = itemWrapper.Value;
		}
	}
}
#endif