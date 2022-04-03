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
	internal class AssetListReporterViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ToolbarButton _topBar1;
		private ToolbarButton _topBar2;
		private ToolbarButton _topBar3;
		private ToolbarButton _bottomBar1;
		private ToolbarButton _bottomBar2;
		private ToolbarButton _bottomBar3;
		private ListView _assetListView;
		private ListView _dependListView;
		private BuildReport _buildReport;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetPath();
			string uxml = $"{rootPath}/Editor/AssetBundleReporter/VisualViewers/{nameof(AssetListReporterViewer)}.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetListReporterViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 顶部按钮栏
			_topBar1 = _root.Q<ToolbarButton>("TopBar1");
			_topBar2 = _root.Q<ToolbarButton>("TopBar2");
			_topBar3 = _root.Q<ToolbarButton>("TopBar3");
			_topBar1.clicked += TopBar1_clicked;
			_topBar2.clicked += TopBar2_clicked;
			_topBar3.clicked += TopBar3_clicked;

			// 底部按钮栏
			_bottomBar1 = _root.Q<ToolbarButton>("BottomBar1");
			_bottomBar2 = _root.Q<ToolbarButton>("BottomBar2");
			_bottomBar3 = _root.Q<ToolbarButton>("BottomBar3");
			
			// 资源列表
			_assetListView = _root.Q<ListView>("TopListView");
			_assetListView.makeItem = MakeAssetListViewItem;
			_assetListView.bindItem = BindAssetListViewItem;

#if UNITY_2020_1_OR_NEWER
			_assetListView.onSelectionChange += AssetListView_onSelectionChange;
#else
			_assetListView.onSelectionChanged += AssetListView_onSelectionChange;
#endif
			// 依赖列表
			_dependListView = _root.Q<ListView>("BottomListView");
			_dependListView.makeItem = MakeDependListViewItem;
			_dependListView.bindItem = BindDependListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport, string searchKeyWord)
		{
			_buildReport = buildReport;
			_assetListView.Clear();
			_assetListView.itemsSource = FilterViewItems(buildReport, searchKeyWord);
			_topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count})";
		}
		private List<ReportAssetInfo> FilterViewItems(BuildReport buildReport, string searchKeyWord)
		{
			List<ReportAssetInfo> result = new List<ReportAssetInfo>(buildReport.AssetInfos.Count);
			foreach (var assetInfo in buildReport.AssetInfos)
			{
				if(string.IsNullOrEmpty(searchKeyWord) == false)
				{
					if (assetInfo.AssetPath.Contains(searchKeyWord) == false)
						continue;					
				}
				result.Add(assetInfo);
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
		private VisualElement MakeAssetListViewItem()
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
				label.style.flexGrow = 1f;
				label.style.width = 145;
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			var sourceData = _assetListView.itemsSource as List<ReportAssetInfo>;
			var assetInfo = sourceData[index];
			var bundleInfo = _buildReport.GetBundleInfo(assetInfo.MainBundle);

			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = assetInfo.AssetPath;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = GetAssetFileSize(assetInfo.AssetPath);

			// Main Bundle
			var label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.BundleName;
		}
		private void AssetListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				ReportAssetInfo assetInfo = item as ReportAssetInfo;
				FillDependListView(assetInfo);
			}
		}
		private void TopBar1_clicked()
		{		
		}
		private void TopBar2_clicked()
		{
		}
		private void TopBar3_clicked()
		{
		}

		// 依赖列表相关
		private void FillDependListView(ReportAssetInfo assetInfo)
		{
			List<ReportBundleInfo> bundles = new List<ReportBundleInfo>();
			var mainBundle = _buildReport.GetBundleInfo(assetInfo.MainBundle);
			bundles.Add(mainBundle);
			foreach(string dependBundleName in assetInfo.DependBundles)
			{
				var dependBundle = _buildReport.GetBundleInfo(dependBundleName);
				bundles.Add(dependBundle);
			}

			_dependListView.Clear();
			_dependListView.ClearSelection();
			_dependListView.itemsSource = bundles;
			_bottomBar1.text = $"Depend Bundles ({bundles.Count})";
		}
		private VisualElement MakeDependListViewItem()
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

			return element;
		}
		private void BindDependListViewItem(VisualElement element, int index)
		{
			List<ReportBundleInfo> bundles = _dependListView.itemsSource as List<ReportBundleInfo>;
			ReportBundleInfo bundleInfo = bundles[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = (bundleInfo.SizeBytes / 1024f).ToString("f1") + " KB";

			// Hash
			var label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.Hash;
		}

		private string GetAssetFileSize(string assetPath)
		{
			string fullPath = EditorTools.GetProjectPath() + "/" + assetPath;
			if (File.Exists(fullPath) == false)
				return "unknown";
			else
				return (EditorTools.GetFileSize(fullPath) / 1024f).ToString("f1") + " KB";
		}
	}
}
#endif