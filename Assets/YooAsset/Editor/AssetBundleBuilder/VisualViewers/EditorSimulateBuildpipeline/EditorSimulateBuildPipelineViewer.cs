#if UNITY_2019_4_OR_NEWER
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
    [BuildPipelineAttribute(nameof(EBuildPipeline.EditorSimulateBuildPipeline))]
    internal class EditorSimulateBuildPipelineViewer : BuildPipelineViewerBase
    {
        protected TemplateContainer Root;
        protected TextField _buildOutputField;
        protected TextField _buildVersionField;

        public override void CreateView(VisualElement parent)
        {
            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUXML<EditorSimulateBuildPipelineViewer>();
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
            if (EditorUtility.DisplayDialog("提示", $"开始构建资源包[{PackageName}]！", "Yes", "No"))
            {
                EditorTools.ClearUnityConsole();
                EditorApplication.delayCall += ExecuteBuild;
            }
            else
            {
                Debug.LogWarning("[Build] 打包已经取消");
            }
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        protected virtual void ExecuteBuild()
        {
            var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, PipelineName);
            var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, PipelineName);
            var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, PipelineName);

            EditorSimulateBuildParameters buildParameters = new EditorSimulateBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = PipelineName.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.VirtualBundle;
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = _buildVersionField.value;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
            buildParameters.BuildinFileCopyParams = buildinFileCopyParams;

            EditorSimulateBuildPipeline pipeline = new EditorSimulateBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }
    }
}
#endif