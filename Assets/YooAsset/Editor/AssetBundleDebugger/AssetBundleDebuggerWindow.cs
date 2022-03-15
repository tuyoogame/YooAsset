using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace YooAsset.Editor
{
    public class AssetBundleDebuggerWindow : EditorWindow
    {
        [MenuItem("YooAsset/AssetBundle Debugger", false, 104)]
        public static void ShowExample()
        {
            AssetBundleDebuggerWindow wnd = GetWindow<AssetBundleDebuggerWindow>();
            wnd.titleContent = new GUIContent("资源包调试工具");
            wnd.minSize = new Vector2(800, 600);
        }

        private AssetListDebuggerViewer _assetListViewer;
        private readonly DebugSummy _summy = new DebugSummy();
        private string _searchKeyWord;


        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // 加载布局文件
            string rootPath = EditorTools.GetYooAssetPath();
            string uxml = $"{rootPath}/Editor/AssetBundleDebugger/AssetBundleDebugger.uxml";
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
            if (visualAsset == null)
            {
                Debug.LogError($"Not found AssetBundleDebugger.uxml : {uxml}");
                return;
            }
            visualAsset.CloneTree(root);

            // 采样按钮
			var sampleBtn = root.Q<Button>("SampleButton");
            sampleBtn.clicked += SampleBtn_onClick;

            // 搜索栏
            var searchField = root.Q<ToolbarSearchField>("SearchField");
            searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

            // 加载页面
            _assetListViewer = new AssetListDebuggerViewer();
            _assetListViewer.InitViewer();

            // 初始页面
            _assetListViewer.AttachParent(root);
        }
        private void SampleBtn_onClick()
        {
            YooAssets.GetDebugSummy(_summy);
            _assetListViewer.FillViewData(_summy, _searchKeyWord);
        }
        private void OnSearchKeyWordChange(ChangeEvent<string> e)
        {
            _searchKeyWord = e.newValue;
            _assetListViewer.FillViewData(_summy, _searchKeyWord);
        }
    }
}