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
	internal class AssetListViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ListView _assetListView;
		private ListView _dependListView;
		private PatchManifest _manifest;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string uxml = "Assets/YooAsset/Editor/AssetBundleBrowser/VisualViewers/AssetListViewer.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetListViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

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
		public void FillViewData(PatchManifest manifest, string searchKeyWord)
		{
			_manifest = manifest;
			_assetListView.Clear();
			_assetListView.itemsSource = FilterViewItems(manifest, searchKeyWord);
		}
		private List<PatchAsset> FilterViewItems(PatchManifest manifest, string searchKeyWord)
		{
			List<PatchAsset> result = new List<PatchAsset>(manifest.AssetList.Count);
			foreach (var patchAsset in manifest.AssetList)
			{
				if(string.IsNullOrEmpty(searchKeyWord) == false)
				{
					if (patchAsset.AssetPath.Contains(searchKeyWord) == false)
						continue;					
				}
				result.Add(patchAsset);
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
			var sourceData = _assetListView.itemsSource as List<PatchAsset>;
			var patchAsset = sourceData[index];
			var patchBundle = _manifest.BundleList[patchAsset.BundleID];

			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = patchAsset.AssetPath;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = GetAssetFileSize(patchAsset.AssetPath);

			// Main Bundle
			var label3 = element.Q<Label>("Label3");
			label3.text = patchBundle.BundleName;
		}
		private void AssetListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				PatchAsset patchAsset = item as PatchAsset;
				FillDependListView(patchAsset);
			}
		}

		// 依赖列表相关
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
			List<PatchBundle> bundles = _dependListView.itemsSource as List<PatchBundle>;
			PatchBundle patchBundle = bundles[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = patchBundle.BundleName;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = (patchBundle.SizeBytes / 1024f).ToString("f1") + " KB";

			// Hash
			var label3 = element.Q<Label>("Label3");
			label3.text = patchBundle.Hash;
		}
		private void FillDependListView(PatchAsset patchAsset)
		{
			List<PatchBundle> bundles = new List<PatchBundle>();
			var mainBundle = _manifest.BundleList[patchAsset.BundleID];
			bundles.Add(mainBundle);
			for (int i = 0; i < patchAsset.DependIDs.Length; i++)
			{
				int bundleID = patchAsset.DependIDs[i];
				var dependBundle = _manifest.BundleList[bundleID];
				bundles.Add(dependBundle);
			}

			_dependListView.Clear();
#if UNITY_2020_1_OR_NEWER
			_dependListView.ClearSelection();
#endif
			_dependListView.itemsSource = bundles;
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