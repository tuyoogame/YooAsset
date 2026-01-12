using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线（EditorSimulateBuildPipeline）的构建参数查看器
    /// </summary>
    [BuildPipelineAttribute(nameof(EBuildPipeline.EditorSimulateBuildPipeline))]
    internal class EditorSimulateBuildPipelineViewer : BuildPipelineViewerBase
    {
        /// <summary>
        /// 根布局容器（UXML 克隆实例）
        /// </summary>
        protected TemplateContainer Root;

        /// <summary>
        /// 构建输出目录文本框
        /// </summary>
        protected TextField _buildOutputField;

        /// <summary>
        /// 构建版本文本框
        /// </summary>
        protected TextField _buildVersionField;


        public override void CreateView(VisualElement parent)
        {
            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUxml<EditorSimulateBuildPipelineViewer>();
            if (visualAsset == null)
                return;

            Root = visualAsset.CloneTree();
            Root.style.flexGrow = 1f;
            parent.Add(Root);

            // 输出目录
            _buildOutputField = Root.Q<TextField>("BuildOutput");
            SetBuildOutputField(_buildOutputField);

            // 构建版本
            _buildVersionField = Root.Q<TextField>("BuildVersion");
            SetBuildVersionField(_buildVersionField);

            // 构建按钮
            var buildButton = Root.Q<Button>("Build");
            buildButton.clicked += BuildButton_clicked;
        }
        private void BuildButton_clicked()
        {
            if (EditorUtility.DisplayDialog("Info", $"Start building resource package '{PackageName}'.", "Yes", "No"))
            {
                EditorWindowUtility.ClearUnityConsole();
                EditorApplication.delayCall += ExecuteBuild;
            }
            else
            {
                Debug.LogWarning("Packaging has been canceled.");
            }
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        protected virtual void ExecuteBuild()
        {
            var fileNameStyle = BundleBuilderSetting.GetPackageFileNameStyle(PackageName, PipelineName);
            var bundledCopyOption = BundleBuilderSetting.GetPackageBundledCopyOption(PackageName, PipelineName);
            var bundledCopyParams = BundleBuilderSetting.GetPackageBundledCopyParams(PackageName, PipelineName);

            EditorSimulateBuildParameters buildParameters = new EditorSimulateBuildParameters();
            buildParameters.BuildOutputRoot = BundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = PipelineName.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.VirtualBundle;
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = _buildVersionField.value;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BundledCopyOption = bundledCopyOption;
            buildParameters.BundledCopyParams = bundledCopyParams;

            EditorSimulateBuildPipeline pipeline = new EditorSimulateBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }
    }
}