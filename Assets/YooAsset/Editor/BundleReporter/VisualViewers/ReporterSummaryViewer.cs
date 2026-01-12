using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ReporterSummaryViewer
    {
        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;
        private ScrollView _scrollView;

        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUxml<ReporterSummaryViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 概述列表
            _scrollView = _root.Q<ScrollView>("ScrollView");
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        /// <param name="buildReport">构建报告数据</param>
        public void FillViewData(BuildReport buildReport)
        {
            _scrollView.Clear();

            BindListViewHeader("Build Infos");
            BindListViewItem("YooAsset Version", buildReport.Summary.YooVersion);
            BindListViewItem("UnityEngine Version", buildReport.Summary.UnityVersion);
            BindListViewItem("Build Date", buildReport.Summary.BuildDate);
            BindListViewItem("Build Seconds", ConvertTime(buildReport.Summary.BuildSeconds));
            BindListViewItem("Build Target", $"{buildReport.Summary.BuildTarget}");
            BindListViewItem("Build Pipeline", $"{buildReport.Summary.BuildPipeline}");
            BindListViewItem("Build Bundle Type", buildReport.Summary.BuildBundleType.ToString());
            BindListViewItem("Package Name", buildReport.Summary.BuildPackageName);
            BindListViewItem("Package Version", buildReport.Summary.BuildPackageVersion);
            BindListViewItem("Package Note", buildReport.Summary.BuildPackageNote);
            BindListViewItem(string.Empty, string.Empty);

            BindListViewHeader("Collect Settings");
            BindListViewItem("Unique Bundle Name", $"{buildReport.Summary.UniqueBundleName}");
            BindListViewItem("Enable Addressable", $"{buildReport.Summary.EnableAddressable}");
            BindListViewItem("Support Extensionless", $"{buildReport.Summary.SupportExtensionless}");
            BindListViewItem("Location To Lower", $"{buildReport.Summary.LocationToLower}");
            BindListViewItem("Include Asset GUID", $"{buildReport.Summary.IncludeAssetGuid}");
            BindListViewItem("Auto Collect Shaders", $"{buildReport.Summary.AutoCollectShaders}");
            BindListViewItem("Ignore Rule Name", $"{buildReport.Summary.IgnoreRuleName}");
            BindListViewItem(string.Empty, string.Empty);

            BindListViewHeader("Build Params");
            BindListViewItem("Clear Build Cache Files", $"{buildReport.Summary.ClearBuildCacheFiles}");
            BindListViewItem("Use Asset Dependency DB", $"{buildReport.Summary.UseAssetDependencyDB}");
            BindListViewItem("Enable Share Pack Rule", $"{buildReport.Summary.EnableSharePackRule}");
            BindListViewItem("Single Referenced Pack Alone", $"{buildReport.Summary.SingleReferencedPackAlone}");
            BindListViewItem("Encryption Services", buildReport.Summary.EncryptionServicesClassName);
            BindListViewItem("Manifest Process Services", buildReport.Summary.ManifestProcessServicesClassName);
            BindListViewItem("Manifest Restore Services", buildReport.Summary.ManifestRestoreServicesClassName);
            BindListViewItem("FileNameStyle", $"{buildReport.Summary.FileNameStyle}");
            BindListViewItem("CompressOption", $"{buildReport.Summary.CompressOption}");
            BindListViewItem("DisableWriteTypeTree", $"{buildReport.Summary.DisableWriteTypeTree}");
            BindListViewItem("IgnoreTypeTreeChanges", $"{buildReport.Summary.IgnoreTypeTreeChanges}");
            BindListViewItem("ReplaceAssetPathWithAddress", $"{buildReport.Summary.ReplaceAssetPathWithAddress}");
            BindListViewItem(string.Empty, string.Empty);

            BindListViewHeader("Build Results");
            BindListViewItem("Asset File Total Count", $"{buildReport.Summary.AssetFileTotalCount}");
            BindListViewItem("Main Asset Total Count", $"{buildReport.Summary.MainAssetTotalCount}");
            BindListViewItem("All Bundle Total Count", $"{buildReport.Summary.AllBundleTotalCount}");
            BindListViewItem("All Bundle Total Size", ConvertSize(buildReport.Summary.AllBundleTotalSize));
            BindListViewItem("Encrypted Bundle Total Count", $"{buildReport.Summary.EncryptedBundleTotalCount}");
            BindListViewItem("Encrypted Bundle Total Size", ConvertSize(buildReport.Summary.EncryptedBundleTotalSize));
        }

        /// <summary>
        /// 挂接到父级页面上
        /// </summary>
        /// <param name="parent">父级视觉元素</param>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父级页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        // 列表相关
        private void BindListViewHeader(string title)
        {
            Toolbar toolbar = new Toolbar();
            _scrollView.Add(toolbar);

            ToolbarButton titleButton = new ToolbarButton();
            titleButton.text = title;
            titleButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleButton.style.width = 200;
            toolbar.Add(titleButton);

            ToolbarButton valueButton = new ToolbarButton();
            valueButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueButton.style.width = 150;
            valueButton.style.flexShrink = 1;
            valueButton.style.flexGrow = 1;
            valueButton.SetEnabled(false);
            toolbar.Add(valueButton);
        }
        private void BindListViewItem(string name, string value)
        {
            VisualElement element = MakeListViewItem();
            _scrollView.Add(element);

            var titleLabel = element.Q<Label>("TitleLabel");
            titleLabel.text = name;

            var valueLabel = element.Q<Label>("ValueLabel");
            valueLabel.text = value;
        }
        private VisualElement MakeListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            var titleLabel = new Label();
            titleLabel.name = "TitleLabel";
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            titleLabel.style.marginLeft = 3f;
            titleLabel.style.width = 200;
            element.Add(titleLabel);

            var valueLabel = new Label();
            valueLabel.name = "ValueLabel";
            valueLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            valueLabel.style.marginLeft = 3f;
            valueLabel.style.flexGrow = 1f;
            valueLabel.style.width = 150;
            element.Add(valueLabel);

            return element;
        }

        private string ConvertTime(int totalSeconds)
        {
            if (totalSeconds < 0)
                totalSeconds = 0;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (hours > 0)
                return $"{hours}小时{minutes}分{seconds}秒";
            else if (minutes > 0)
                return $"{minutes}分{seconds}秒";
            else
                return $"{seconds}秒";
        }
        private string ConvertSize(long size)
        {
            if (size == 0)
                return "0";
            return EditorUtility.FormatBytes(size);
        }
    }
}