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
    internal abstract class BuildPipelineViewerBase
    {
        protected const int StyleWidth = 400;
        protected const int LabelMinWidth = 190;

        protected string PackageName { private set; get; }
        protected string PipelineName { private set; get; }
        protected BuildTarget BuildTarget { private set; get; }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView(string packageName, string pipelineName, BuildTarget buildTarget)
        {
            PackageName = packageName;
            PipelineName = pipelineName;
            BuildTarget = buildTarget;
        }

        /// <summary>
        /// 创建视图
        /// </summary>
        public abstract void CreateView(VisualElement parent);

        /// <summary>
        /// 获取默认版本
        /// </summary>
        protected virtual string GetDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        /// <summary>
        /// 创建资源包加密服务类实例
        /// </summary>
        protected IEncryptionServices CreateEncryptionServicesInstance()
        {
            var className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(PackageName, PipelineName);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IEncryptionServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        /// <summary>
        /// 创建资源清单加密服务类实例
        /// </summary>
        protected IManifestProcessServices CreateManifestProcessServicesInstance()
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(PackageName, PipelineName);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestProcessServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        /// <summary>
        /// 创建资源清单解密服务类实例
        /// </summary>
        protected IManifestRestoreServices CreateManifestRestoreServicesInstance()
        {
            var className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(PackageName, PipelineName);
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestRestoreServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        #region UI元素通用处理方法
        protected void SetBuildOutputField(TextField textField)
        {
            // 输出目录
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            textField.SetValueWithoutNotify(defaultOutputRoot);
            textField.SetEnabled(false);
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetBuildVersionField(TextField textField)
        {
            // 构建版本
            textField.style.width = StyleWidth;
            textField.SetValueWithoutNotify(GetDefaultPackageVersion());
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetCompressionField(EnumField enumField)
        {
            // 压缩方式选项
            var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, PipelineName);
            enumField.Init(compressOption);
            enumField.SetValueWithoutNotify(compressOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageCompressOption(PackageName, PipelineName, (ECompressOption)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetOutputNameStyleField(EnumField enumField)
        {
            // 输出文件名称样式
            var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, PipelineName);
            enumField.Init(fileNameStyle);
            enumField.SetValueWithoutNotify(fileNameStyle);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageFileNameStyle(PackageName, PipelineName, (EFileNameStyle)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetCopyBuildinFileOptionField(EnumField enumField, TextField tagField)
        {
            // 首包文件拷贝选项
            var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, PipelineName);
            enumField.Init(buildinFileCopyOption);
            enumField.SetValueWithoutNotify(buildinFileCopyOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyOption(PackageName, PipelineName, (EBuildinFileCopyOption)enumField.value);

                // 设置内置资源标签显隐
                SetCopyBuildinFileTagsVisible(tagField);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetCopyBuildinFileTagsVisible(TextField tagField)
        {
            var option = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, PipelineName);
            tagField.visible = option == EBuildinFileCopyOption.ClearAndCopyByTags || option == EBuildinFileCopyOption.OnlyCopyByTags;
        }
        protected void SetCopyBuildinFileTagsField(TextField textField)
        {
            // 首包文件拷贝参数
            var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, PipelineName);
            textField.SetValueWithoutNotify(buildinFileCopyParams);
            textField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageBuildinFileCopyParams(PackageName, PipelineName, textField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetClearBuildCacheToggle(Toggle toggle)
        {
            // 清理构建缓存
            bool clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(clearBuildCache);
            toggle.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageClearBuildCache(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }
        protected void SetUseAssetDependencyDBToggle(Toggle toggle)
        {
            // 使用资源依赖数据库
            bool useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(useAssetDependencyDB);
            toggle.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSetting.SetPackageUseAssetDependencyDB(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }
        protected PopupField<Type> CreateEncryptionServicesField(VisualElement container)
        {
            // 资源包加密服务类
            var classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            if (classTypes.Count > 0)
            {
                var className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Encryption Services";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleBuilderSetting.SetPackageEncyptionServicesClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Encryption Services";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }
        protected PopupField<Type> CreateManifestProcessServicesField(VisualElement container)
        {
            // 资源清单加密服务类
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
            if (classTypes.Count > 0)
            {
                var className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Manifest Process Services";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleBuilderSetting.SetPackageManifestProcessServicesClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Manifest Process Services";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }
        protected PopupField<Type> CreateManifestRestoreServicesField(VisualElement container)
        {
            // 资源清单加密服务类
            var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
            if (classTypes.Count > 0)
            {
                var className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Manifest Restore Services";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleBuilderSetting.SetPackageManifestRestoreServicesClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Manifest Restore Services";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }
        #endregion
    }
}
#endif