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
    [BuildPipelineAttribute(nameof(EBuildPipeline.ScriptableBuildPipeline))]
    internal class ScriptableBuildPipelineViewer : BuildPipelineViewerBase
    {
        protected TemplateContainer Root;
        protected TextField _buildOutputField;
        protected TextField _buildVersionField;
        protected PopupField<Type> _encryptionServicesField;
        protected PopupField<Type> _manifestProcessServicesField;
        protected PopupField<Type> _manifestRestoreServicesField;
        protected EnumField _compressionField;
        protected EnumField _outputNameStyleField;
        protected EnumField _copyBuildinFileOptionField;
        protected TextField _copyBuildinFileTagsField;
        protected Toggle _clearBuildCacheToggle;
        protected Toggle _useAssetDependencyDBToggle;

        public override void CreateView(VisualElement parent)
        {
            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUXML<ScriptableBuildPipelineViewer>();
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

            // 加密方法
            var popupContainer = Root.Q("PopupContainer");
            _encryptionServicesField = CreateEncryptionServicesField(popupContainer);
            _manifestProcessServicesField = CreateManifestProcessServicesField(popupContainer);
            _manifestRestoreServicesField = CreateManifestRestoreServicesField(popupContainer);

            // 压缩方式选项
            _compressionField = Root.Q<EnumField>("Compression");
            SetCompressionField(_compressionField);

            // 输出文件名称样式
            _outputNameStyleField = Root.Q<EnumField>("FileNameStyle");
            SetOutputNameStyleField(_outputNameStyleField);

            // 首包文件拷贝参数
            _copyBuildinFileTagsField = Root.Q<TextField>("CopyBuildinFileParam");
            SetCopyBuildinFileTagsField(_copyBuildinFileTagsField);
            SetCopyBuildinFileTagsVisible(_copyBuildinFileTagsField);

            // 首包文件拷贝选项
            _copyBuildinFileOptionField = Root.Q<EnumField>("CopyBuildinFileOption");
            SetCopyBuildinFileOptionField(_copyBuildinFileOptionField, _copyBuildinFileTagsField);

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
            var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, PipelineName);
            var clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(PackageName, PipelineName);
            var useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(PackageName, PipelineName);

            ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = PipelineName.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = _buildVersionField.value;
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
            buildParameters.BuildinFileCopyParams = buildinFileCopyParams;
            buildParameters.CompressOption = compressOption;
            buildParameters.ClearBuildCacheFiles = clearBuildCache;
            buildParameters.UseAssetDependencyDB = useAssetDependencyDB;
            buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
            buildParameters.ManifestProcessServices = CreateManifestProcessServicesInstance();
            buildParameters.ManifestRestoreServices = CreateManifestRestoreServicesInstance();
            buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName();

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        protected string GetBuiltinShaderBundleName()
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(PackageName, uniqueBundleName);
        }

        /// <summary>
        /// Mono脚本的资源包名称
        /// </summary>
        protected string GetMonoScriptsBundleName()
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateMonosPackRuleResult();
            return packRuleResult.GetBundleName(PackageName, uniqueBundleName);
        }
    }
}
#endif