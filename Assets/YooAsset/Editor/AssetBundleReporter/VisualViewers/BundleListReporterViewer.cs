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
	internal class BundleListReporterViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ToolbarButton _topBar1;
		private ToolbarButton _topBar2;
		private ToolbarButton _topBar3;
		private ToolbarButton _topBar4;
		private ToolbarButton _topBar5;
		private ToolbarButton _bottomBar1;
		private ToolbarButton _bottomBar2;
		private ToolbarButton _bottomBar3;
		private ListView _bundleListView;
		private ListView _includeListView;
		private BuildReport _buildReport;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetPath();
			string uxml = $"{rootPath}/Editor/AssetBundleReporter/VisualViewers/BundleListReporterViewer.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(BundleListReporterViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 顶部按钮栏
			_topBar1 = _root.Q<ToolbarButton>("TopBar1");
			_topBar2 = _root.Q<ToolbarButton>("TopBar2");
			_topBar3 = _root.Q<ToolbarButton>("TopBar3");
			_topBar4 = _root.Q<ToolbarButton>("TopBar4");
			_topBar5 = _root.Q<ToolbarButton>("TopBar5");

			// 底部按钮栏
			_bottomBar1 = _root.Q<ToolbarButton>("BottomBar1");
			_bottomBar2 = _root.Q<ToolbarButton>("BottomBar2");
			_bottomBar3 = _root.Q<ToolbarButton>("BottomBar3");

			// 资源包列表
			_bundleListView = _root.Q<ListView>("TopListView");
			_bundleListView.makeItem = MakeBundleListViewItem;
			_bundleListView.bindItem = BindBundleListViewItem;
#if UNITY_2020_1_OR_NEWER
			_bundleListView.onSelectionChange += BundleListView_onSelectionChange;
#else
			_bundleListView.onSelectionChanged += BundleListView_onSelectionChange;
#endif

			// 包含列表
			_includeListView = _root.Q<ListView>("BottomListView");
			_includeListView.makeItem = MakeContainsListViewItem;
			_includeListView.bindItem = BindContainsListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport, string searchKeyWord)
		{
			_buildReport = buildReport;
			_bundleListView.Clear();
			_bundleListView.itemsSource = FilterViewItems(buildReport, searchKeyWord);
			_topBar1.text = $"Bundle Name ({_bundleListView.itemsSource.Count})";
		}
		private List<ReportBundleInfo> FilterViewItems(BuildReport buildReport, string searchKeyWord)
		{
			List<ReportBundleInfo> result = new List<ReportBundleInfo>(buildReport.BundleInfos.Count);
			foreach (var bundleInfo in buildReport.BundleInfos)
			{
				if (string.IsNullOrEmpty(searchKeyWord) == false)
				{
					if (bundleInfo.BundleName.Contains(searchKeyWord) == false)
						continue;
				}
				result.Add(bundleInfo);
			}
			return result;
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


		// 资源列表相关
		private VisualElement MakeBundleListViewItem()
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
				label.style.width = 250;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label4";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 60;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label5";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 80;
				element.Add(label);
			}

			return element;
		}
		private void BindBundleListViewItem(VisualElement element, int index)
		{
			var sourceData = _bundleListView.itemsSource as List<ReportBundleInfo>;
			var bundleInfo = sourceData[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = (bundleInfo.SizeBytes / 1024f).ToString("f1") + " KB";

			// Hash
			var label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.Hash;

			// Version
			var label4 = element.Q<Label>("Label4");
			label4.text = bundleInfo.Version.ToString();

			// Tags
			var label5 = element.Q<Label>("Label5");
			label5.text = GetTagsString(bundleInfo.Tags);
		}
		private void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				ReportBundleInfo bundleInfo = item as ReportBundleInfo;
				FillContainsListView(bundleInfo);
			}
		}

		// 依赖列表相关
		private void FillContainsListView(ReportBundleInfo bundleInfo)
		{
			List<string> containsList = new List<string>();
			foreach (var assetInfo in _buildReport.AssetInfos)
			{
				if (assetInfo.MainBundle == bundleInfo.BundleName)
					containsList.Add(assetInfo.AssetPath);
			}

			_includeListView.Clear();
#if UNITY_2020_1_OR_NEWER
			_includeListView.ClearSelection();
#endif
			_includeListView.itemsSource = containsList;
			_bottomBar1.text = $"Include Assets ({containsList.Count})";
		}
		private VisualElement MakeContainsListViewItem()
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
				//assetSizeLabel.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 250;
				element.Add(label);
			}

			return element;
		}
		private void BindContainsListViewItem(VisualElement element, int index)
		{
			List<string> containsList = _includeListView.itemsSource as List<string>;
			string assetPath = containsList[index];

			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = assetPath;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = GetAssetFileSize(assetPath);

			// GUID
			var label3 = element.Q<Label>("Label3");
			label3.text = AssetDatabase.AssetPathToGUID(assetPath);
		}
		
		private string GetAssetFileSize(string assetPath)
		{
			string fullPath = EditorTools.GetProjectPath() + "/" + assetPath;
			if (File.Exists(fullPath) == false)
				return "unknown";
			else
				return (EditorTools.GetFileSize(fullPath) / 1024f).ToString("f1") + " KB";
		}
		private string GetTagsString(string[] tags)
		{
			string result = string.Empty;
			if (tags != null)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					result += tags[i];
					result += ";";
				}
			}
			return result;
		}
	}
}
#endif