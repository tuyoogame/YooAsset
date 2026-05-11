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
    /// 归档文件构建管线（ArchiveFileBuildPipeline）的构建参数查看器
    /// </summary>
    [BuildPipelineAttribute(nameof(EBuildPipeline.ArchiveFileBuildPipeline))]
    internal class ArchiveFileBuildPipelineViewer : BuildPipelineViewerBase
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

        /// <summary>
        /// 资源清单加密器下拉框
        /// </summary>
        protected PopupField<Type> _manifestEncryptorField;

        /// <summary>
        /// 资源清单解密器下拉框
        /// </summary>
        protected PopupField<Type> _manifestDecryptorField;

        /// <summary>
        /// 输出文件名称样式枚举字段
        /// </summary>
        protected EnumField _outputNameStyleField;

        /// <summary>
        /// 首包资源拷贝选项枚举字段
        /// </summary>
        protected EnumField _bundledCopyOptionField;

        /// <summary>
        /// 首包资源拷贝标签参数文本框
        /// </summary>
        protected TextField _bundledCopyParamField;

        /// <summary>
        /// 是否清理构建缓存开关
        /// </summary>
        protected Toggle _clearBuildCacheToggle;

        /// <summary>
        /// 是否使用资源依赖数据库开关
        /// </summary>
        protected Toggle _useAssetDependencyDBToggle;


        public override void CreateView(VisualElement parent)
        {
            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUxml<ArchiveFileBuildPipelineViewer>();
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

            // 清单服务
            var popupContainer = Root.Q("PopupContainer");
            _manifestEncryptorField = CreateManifestEncryptorField(popupContainer);
            _manifestDecryptorField = CreateManifestDecryptorField(popupContainer);

            // 输出文件名称样式
            _outputNameStyleField = Root.Q<EnumField>("FileNameStyle");
            SetOutputNameStyleField(_outputNameStyleField);

            // 首包资源拷贝参数
            _bundledCopyParamField = Root.Q<TextField>("BundledCopyParam");
            SetBundledCopyParamField(_bundledCopyParamField);
            SetBundledCopyParamVisible(_bundledCopyParamField);

            // 首包资源拷贝选项
            _bundledCopyOptionField = Root.Q<EnumField>("BundledCopyOption");
            SetBundledCopyOptionField(_bundledCopyOptionField, _bundledCopyParamField);

            // 清理构建缓存
            _clearBuildCacheToggle = Root.Q<Toggle>("ClearBuildCache");
            SetClearBuildCacheToggle(_clearBuildCacheToggle);

            // 使用资源依赖数据库
            _useAssetDependencyDBToggle = Root.Q<Toggle>("UseAssetDependency");
            SetUseAssetDependencyDBToggle(_useAssetDependencyDBToggle);

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
            var clearBuildCache = BundleBuilderSetting.GetPackageClearBuildCache(PackageName, PipelineName);
            var useAssetDependencyDB = BundleBuilderSetting.GetPackageUseAssetDependencyDB(PackageName, PipelineName);

            ArchiveFileBuildParameters buildParameters = new ArchiveFileBuildParameters();
            buildParameters.BuildOutputRoot = BundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = PipelineName.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.ArchiveBundle;
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = _buildVersionField.value;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BundledCopyOption = bundledCopyOption;
            buildParameters.BundledCopyParams = bundledCopyParams;
            buildParameters.ClearBuildCacheFiles = clearBuildCache;
            buildParameters.UseAssetDependencyDB = useAssetDependencyDB;
            buildParameters.ManifestEncryptor = CreateManifestEncryptorInstance();
            buildParameters.ManifestDecryptor = CreateManifestDecryptorInstance();

            ArchiveFileBuildPipeline pipeline = new ArchiveFileBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }
    }
}
