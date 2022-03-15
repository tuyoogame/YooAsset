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
	public class AssetBundleBrowserWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Browser", false, 103)]
		public static void ShowExample()
		{
			AssetBundleBrowserWindow wnd = GetWindow<AssetBundleBrowserWindow>();
			wnd.titleContent = new GUIContent("资源包浏览工具");
			wnd.minSize = new Vector2(800, 600);
		}

		/// <summary>
		/// 显示模式
		/// </summary>
		private enum EShowMode
		{
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
		private AssetListBrowserViewer _assetListViewer;
		private BundleListBrowserViewer _bundleListViewer;

		private EShowMode _showMode;
		private string _searchKeyWord;
		private PatchManifest _manifest;


		public void CreateGUI()
		{
			VisualElement root = this.rootVisualElement;

			// 加载布局文件
			string uxml = "Assets/YooAsset/Editor/AssetBundleBrowser/AssetBundleBrowser.uxml";
			var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (visualAsset == null)
			{
				Debug.LogError($"Not found AssetBundleBrowser.uxml : {uxml}");
				return;
			}
			visualAsset.CloneTree(root);

			// 导入按钮
			var importBtn = root.Q<Button>("ImportButton");
			importBtn.clicked += ImportBtn_onClick;

			// 显示模式菜单
			_showModeMenu = root.Q<ToolbarMenu>("ShowModeMenu");
			_showModeMenu.menu.AppendAction(EShowMode.AssetList.ToString(), ShowModeMenuAction1);
			_showModeMenu.menu.AppendAction(EShowMode.BundleList.ToString(), ShowModeMenuAction2);

			// 搜索栏
			var searchField = root.Q<ToolbarPopupSearchField>("SearchField");
			searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

			// 加载页面
			_assetListViewer = new AssetListBrowserViewer();
			_assetListViewer.InitViewer();

			// 加载页面
			_bundleListViewer = new BundleListBrowserViewer();
			_bundleListViewer.InitViewer();

			// 初始页面
			_showMode = EShowMode.AssetList;
			_showModeMenu.text = EShowMode.AssetList.ToString();
			_assetListViewer.AttachParent(root);
		}

		private void ImportBtn_onClick()
		{
			string selectFilePath = EditorUtility.OpenFilePanel("导入补丁清单", EditorTools.GetProjectPath(), "bytes");
			if (string.IsNullOrEmpty(selectFilePath))
				return;

			string jsonData = FileUtility.ReadFile(selectFilePath);
			_manifest = PatchManifest.Deserialize(jsonData);
			_assetListViewer.FillViewData(_manifest, _searchKeyWord);
			_bundleListViewer.FillViewData(_manifest, _searchKeyWord);
		}
		private void OnSearchKeyWordChange(ChangeEvent<string> e)
		{
			_searchKeyWord = e.newValue;
			_assetListViewer.FillViewData(_manifest, _searchKeyWord);
			_bundleListViewer.FillViewData(_manifest, _searchKeyWord);
		}
		private void ShowModeMenuAction1(DropdownMenuAction action)
		{
			if (_showMode != EShowMode.AssetList)
			{
				_showMode = EShowMode.AssetList;
				VisualElement root = this.rootVisualElement;
				_showModeMenu.text = EShowMode.AssetList.ToString();
				_bundleListViewer.DetachParent();
				_assetListViewer.AttachParent(root);
			}
		}
		private void ShowModeMenuAction2(DropdownMenuAction action)
		{
			if (_showMode != EShowMode.BundleList)
			{
				_showMode = EShowMode.BundleList;
				VisualElement root = this.rootVisualElement;
				_showModeMenu.text = EShowMode.BundleList.ToString();
				_assetListViewer.DetachParent();
				_bundleListViewer.AttachParent(root);
			}
		}
	}
}
#endif