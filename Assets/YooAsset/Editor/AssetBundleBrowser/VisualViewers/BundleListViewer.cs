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
	internal class BundleListViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ListView _bundleListView;
		private ListView _includeListView;
		private PatchManifest _manifest;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string uxml = "Assets/YooAsset/Editor/AssetBundleBrowser/VisualViewers/BundleListViewer.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(BundleListViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

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
		public void FillViewData(PatchManifest manifest, string searchKeyWord)
		{
			_manifest = manifest;
			_bundleListView.Clear();
			_bundleListView.itemsSource = FilterViewItems(manifest, searchKeyWord);
		}
		private List<PatchBundle> FilterViewItems(PatchManifest manifest, string searchKeyWord)
		{
			List<PatchBundle> result = new List<PatchBundle>(manifest.BundleList.Count);
			foreach (var patchBundle in manifest.BundleList)
			{
				if (string.IsNullOrEmpty(searchKeyWord) == false)
				{
					if (patchBundle.BundleName.Contains(searchKeyWord) == false)
						continue;
				}
				result.Add(patchBundle);
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
			var sourceData = _bundleListView.itemsSource as List<PatchBundle>;
			var patchBundle = sourceData[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = patchBundle.BundleName;

			// Size
			var label2 = element.Q<Label>("Label2");
			label2.text = (patchBundle.SizeBytes / 1024f).ToString("f1") + " KB";

			// Hash
			var label3 = element.Q<Label>("Label3");
			label3.text = patchBundle.Hash;

			// Version
			var label4 = element.Q<Label>("Label4");
			label4.text = patchBundle.Version.ToString();

			// Tags
			var label5 = element.Q<Label>("Label5");
			label5.text = GetTagsString(patchBundle.Tags);
		}
		private void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				PatchBundle patchBundle = item as PatchBundle;
				FillContainsListView(patchBundle);
			}
		}

		// 依赖列表相关
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
		private void FillContainsListView(PatchBundle patchBundle)
		{
			List<string> containsList = new List<string>();

			int bundleID = -1;
			for (int i = 0; i < _manifest.BundleList.Count; i++)
			{
				if (_manifest.BundleList[i] == patchBundle)
				{
					bundleID = i;
					break;
				}
			}
			if (bundleID == -1)
			{
				Debug.LogError($"Not found bundle in PatchManifest : {patchBundle.BundleName}");
				return;
			}

			foreach (var patchAsset in _manifest.AssetList)
			{
				if (patchAsset.BundleID == bundleID)
					containsList.Add(patchAsset.AssetPath);
			}

			_includeListView.Clear();
#if UNITY_2020_1_OR_NEWER
			_includeListView.ClearSelection();
#endif
			_includeListView.itemsSource = containsList;
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