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
    public class AssetBundleBuilderWindow : EditorWindow
    {
        [MenuItem("YooAsset/AssetBundle Builder", false, 102)]
        public static void OpenWindow()
        {
            AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(800, 600);
        }

        private string _buildPackage;
        private string _buildPipeline;

        private Dictionary<string, Type> _viewClassDic = new Dictionary<string, Type>(10);

        private Toolbar _toolbar;
        private ToolbarMenu _packageMenu;
        private ToolbarMenu _pipelineMenu;
        private VisualElement _container;


        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<AssetBundleBuilderWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);
                _toolbar = root.Q<Toolbar>("Toolbar");
                _container = root.Q("Container");

                // 检测构建包裹
                var packageNames = GetBuildPackageNames();
                if (packageNames.Count == 0)
                {
                    var label = new Label();
                    label.text = "Not found any package";
                    label.style.width = 100;
                    _toolbar.Add(label);
                    return;
                }

                // 构建包裹
                {
                    _buildPackage = packageNames[0];
                    _packageMenu = new ToolbarMenu();
                    _packageMenu.style.width = 200;
                    foreach (var packageName in packageNames)
                    {
                        _packageMenu.menu.AppendAction(packageName, PackageMenuAction, PackageMenuFun, packageName);
                    }
                    _toolbar.Add(_packageMenu);
                }

                // 构建管线
                {
                    _pipelineMenu = new ToolbarMenu();
                    _pipelineMenu.style.width = 200;
                    _toolbar.Add(_pipelineMenu);

                    var viewerClassTypes = EditorTools.GetAssignableTypes(typeof(BuildPipelineViewerBase));
                    foreach (var classType in viewerClassTypes)
                    {
                        var buildPipelineAttribute = EditorTools.GetAttribute<BuildPipelineAttribute>(classType);
                        if (buildPipelineAttribute == null)
                        {
                            Debug.LogWarning($"The class {classType.FullName} need attribute {nameof(BuildPipelineAttribute)}");
                            continue;
                        }

                        string pipelineName = buildPipelineAttribute.PipelineName;
                        if (_viewClassDic.ContainsKey(pipelineName))
                        {
                            Debug.LogWarning($"The pipeline has already exist : {pipelineName}");
                        }
                        else
                        {
                            _viewClassDic.Add(pipelineName, classType);
                            _pipelineMenu.menu.AppendAction(pipelineName, PipelineMenuAction, PipelineMenuFun);
                        }
                    }
                }

                RefreshBuildPipelineView();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void RefreshBuildPipelineView()
        {
            // 清空扩展区域
            _container.Clear();

            _buildPipeline = AssetBundleBuilderSetting.GetPackageBuildPipeline(_buildPackage);
            _packageMenu.text = _buildPackage;
            _pipelineMenu.text = _buildPipeline;

            if (_viewClassDic.TryGetValue(_buildPipeline, out Type value))
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var viewer = Activator.CreateInstance(value) as BuildPipelineViewerBase;
                viewer.InitView(_buildPackage, _buildPipeline, buildTarget);
                viewer.CreateView(_container);
            }
            else
            {
                Debug.LogError($"Not found build pipeline : {_buildPipeline}");
            }
        }
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }

        private void PackageMenuAction(DropdownMenuAction action)
        {
            var packageName = (string)action.userData;
            if (_buildPackage != packageName)
            {
                _buildPackage = packageName;
                RefreshBuildPipelineView();
            }
        }
        private DropdownMenuAction.Status PackageMenuFun(DropdownMenuAction action)
        {
            var packageName = (string)action.userData;
            if (_buildPackage == packageName)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }

        private void PipelineMenuAction(DropdownMenuAction action)
        {
            if (_buildPipeline != action.name)
            {
                _buildPipeline = action.name;
                AssetBundleBuilderSetting.SetPackageBuildPipeline(_buildPackage, _buildPipeline);
                RefreshBuildPipelineView();
            }
        }
        private DropdownMenuAction.Status PipelineMenuFun(DropdownMenuAction action)
        {
            if (_buildPipeline == action.name)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }
    }
}
#endif