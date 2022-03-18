#if UNITY_2019_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	public class AssetBundleReporterWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Reporter", false, 103)]
		public static void ShowExample()
		{
			AssetBundleReporterWindow window = GetWindow<AssetBundleReporterWindow>();
			window.titleContent = new GUIContent("资源包报告工具");
			window.minSize = new Vector2(800, 600);
		}

		/// <summary>
		/// 显示模式
		/// </summary>
		private enum EShowMode
		{
			/// <summary>
			/// 概览
			/// </summary>
			Summary,

			/// <summary>
			/// 资源对象列表显示模式
			/// </summary>
			AssetList,

			/// <summary>
			/// 资源包列表显示模式
			/// </summary>
			BundleList,
		}

		private ToolbarMenu _showModeMenu;
		private SummaryReporterViewer _summaryViewer;
		private AssetListReporterViewer _assetListViewer;
		private BundleListReporterViewer _bundleListViewer;

		private EShowMode _showMode;
		private string _searchKeyWord;
		private BuildReport _buildReport;


		public void CreateGUI()
		{
			VisualElement root = this.rootVisualElement;

			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetPath();
			string uxml = $"{rootPath}/Editor/AssetBundleReporter/AssetBundleReporter.uxml";
			var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (visualAsset == null)
			{
				Debug.LogError($"Not found AssetBundleReporter.uxml : {uxml}");
				return;
			}
			visualAsset.CloneTree(root);

			// 导入按钮
			var importBtn = root.Q<Button>("ImportButton");
			importBtn.clicked += ImportBtn_onClick;

			// 显示模式菜单
			_showModeMenu = root.Q<ToolbarMenu>("ShowModeMenu");
			_showModeMenu.menu.AppendAction(EShowMode.Summary.ToString(), ShowModeMenuAction0, ShowModeMenuFun0);
			_showModeMenu.menu.AppendAction(EShowMode.AssetList.ToString(), ShowModeMenuAction1, ShowModeMenuFun1);
			_showModeMenu.menu.AppendAction(EShowMode.BundleList.ToString(), ShowModeMenuAction2, ShowModeMenuFun2);

			// 搜索栏
			var searchField = root.Q<ToolbarSearchField>("SearchField");
			searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

			// 加载页面
			_summaryViewer = new SummaryReporterViewer();
			_summaryViewer.InitViewer();

			// 加载页面
			_assetListViewer = new AssetListReporterViewer();
			_assetListViewer.InitViewer();

			// 加载页面
			_bundleListViewer = new BundleListReporterViewer();
			_bundleListViewer.InitViewer();

			// 初始页面
			_showMode = EShowMode.Summary;
			_showModeMenu.text = EShowMode.Summary.ToString();
			_summaryViewer.AttachParent(root);
		}

		private void ImportBtn_onClick()
		{
			string selectFilePath = EditorUtility.OpenFilePanel("导入报告", EditorTools.GetProjectPath(), "json");
			if (string.IsNullOrEmpty(selectFilePath))
				return;

			string jsonData = FileUtility.ReadFile(selectFilePath);
			_buildReport = BuildReport.Deserialize(jsonData);
			_assetListViewer.FillViewData(_buildReport, _searchKeyWord);
			_bundleListViewer.FillViewData(_buildReport, _searchKeyWord);
			_summaryViewer.FillViewData(_buildReport);
		}
		private void OnSearchKeyWordChange(ChangeEvent<string> e)
		{
			_searchKeyWord = e.newValue;
			if(_buildReport != null)
			{
				_assetListViewer.FillViewData(_buildReport, _searchKeyWord);
				_bundleListViewer.FillViewData(_buildReport, _searchKeyWord);
			}
		}
		private void ShowModeMenuAction0(DropdownMenuAction action)
		{
			if (_showMode != EShowMode.Summary)
			{
				_showMode = EShowMode.Summary;
				VisualElement root = this.rootVisualElement;
				_showModeMenu.text = EShowMode.Summary.ToString();
				_summaryViewer.AttachParent(root);
				_assetListViewer.DetachParent();
				_bundleListViewer.DetachParent();
			}
		}
		private void ShowModeMenuAction1(DropdownMenuAction action)
		{
			if (_showMode != EShowMode.AssetList)
			{
				_showMode = EShowMode.AssetList;
				VisualElement root = this.rootVisualElement;
				_showModeMenu.text = EShowMode.AssetList.ToString();
				_summaryViewer.DetachParent();
				_assetListViewer.AttachParent(root);
				_bundleListViewer.DetachParent();
			}
		}
		private void ShowModeMenuAction2(DropdownMenuAction action)
		{
			if (_showMode != EShowMode.BundleList)
			{
				_showMode = EShowMode.BundleList;
				VisualElement root = this.rootVisualElement;
				_showModeMenu.text = EShowMode.BundleList.ToString();
				_summaryViewer.DetachParent();
				_assetListViewer.DetachParent();
				_bundleListViewer.AttachParent(root);
			}
		}
		private DropdownMenuAction.Status ShowModeMenuFun0(DropdownMenuAction action)
		{
			if (_showMode == EShowMode.Summary)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
		private DropdownMenuAction.Status ShowModeMenuFun1(DropdownMenuAction action)
		{
			if (_showMode == EShowMode.AssetList)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
		private DropdownMenuAction.Status ShowModeMenuFun2(DropdownMenuAction action)
		{
			if (_showMode == EShowMode.BundleList)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
	}
}
#endif