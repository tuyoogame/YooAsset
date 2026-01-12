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
    /// 构建管线查看器基类，提供构建参数 UI 的通用实现
    /// </summary>
    internal abstract class BuildPipelineViewerBase
    {
        /// <summary>
        /// 控件统一宽度
        /// </summary>
        protected const int StyleWidth = 400;

        /// <summary>
        /// 标签最小宽度
        /// </summary>
        protected const int LabelMinWidth = 190;

        /// <summary>
        /// 当前包裹名称
        /// </summary>
        protected string PackageName { private set; get; }

        /// <summary>
        /// 当前构建管线名称
        /// </summary>
        protected string PipelineName { private set; get; }

        /// <summary>
        /// 当前构建目标平台
        /// </summary>
        protected BuildTarget BuildTarget { private set; get; }


        /// <summary>
        /// 初始化查看器
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="pipelineName">构建管线名称</param>
        /// <param name="buildTarget">构建目标平台</param>
        public void InitView(string packageName, string pipelineName, BuildTarget buildTarget)
        {
            PackageName = packageName;
            PipelineName = pipelineName;
            BuildTarget = buildTarget;
        }

        /// <summary>
        /// 创建查看器 UI
        /// </summary>
        /// <param name="parent">父容器元素</param>
        public abstract void CreateView(VisualElement parent);

        /// <summary>
        /// 获取默认的包裹版本号
        /// </summary>
        /// <returns>默认的包裹版本号字符串</returns>
        protected virtual string GetDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        /// <summary>
        /// 创建资源包加密器实例
        /// </summary>
        /// <returns>加密器实例，未配置时返回 null</returns>
        protected IBundleEncryptor CreateBundleEncryptorInstance()
        {
            var className = BundleBuilderSetting.GetPackageBundleEncryptorClassName(PackageName, PipelineName);
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IBundleEncryptor));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IBundleEncryptor)Activator.CreateInstance(classType);

            UnityEngine.Debug.LogWarning($"Bundle encryptor class type not found: '{className}'.");
            return null;
        }

        /// <summary>
        /// 创建资源清单加密器实例
        /// </summary>
        /// <returns>加密器实例，未配置时返回 null</returns>
        protected IManifestEncryptor CreateManifestEncryptorInstance()
        {
            var className = BundleBuilderSetting.GetPackageManifestEncryptorClassName(PackageName, PipelineName);
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IManifestEncryptor));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestEncryptor)Activator.CreateInstance(classType);

            UnityEngine.Debug.LogWarning($"Manifest encryptor class type not found: '{className}'.");
            return null;
        }

        /// <summary>
        /// 创建资源清单解密器实例
        /// </summary>
        /// <returns>解密器实例，未配置时返回 null</returns>
        protected IManifestDecryptor CreateManifestDecryptorInstance()
        {
            var className = BundleBuilderSetting.GetPackageManifestDecryptorClassName(PackageName, PipelineName);
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IManifestDecryptor));
            var classType = classTypes.Find(x => x.FullName.Equals(className));
            if (classType != null)
                return (IManifestDecryptor)Activator.CreateInstance(classType);

            UnityEngine.Debug.LogWarning($"Manifest decryptor class type not found: '{className}'.");
            return null;
        }

        #region UI元素通用处理方法
        /// <summary>
        /// 配置构建输出目录文本框（默认值与只读状态）
        /// </summary>
        protected void SetBuildOutputField(TextField textField)
        {
            string defaultOutputRoot = BundleBuilderHelper.GetDefaultBuildOutputRoot();
            textField.SetValueWithoutNotify(defaultOutputRoot);
            textField.SetEnabled(false);
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }

        /// <summary>
        /// 配置构建版本文本框（默认版本与样式）
        /// </summary>
        protected void SetBuildVersionField(TextField textField)
        {
            textField.style.width = StyleWidth;
            textField.SetValueWithoutNotify(GetDefaultPackageVersion());
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }

        /// <summary>
        /// 配置压缩方式枚举字段并绑定持久化
        /// </summary>
        protected void SetCompressionField(EnumField enumField)
        {
            var compressOption = BundleBuilderSetting.GetPackageCompressOption(PackageName, PipelineName);
            enumField.Init(compressOption);
            enumField.SetValueWithoutNotify(compressOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageCompressOption(PackageName, PipelineName, (ECompressOption)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }

        /// <summary>
        /// 配置输出文件名称样式枚举字段并绑定持久化
        /// </summary>
        protected void SetOutputNameStyleField(EnumField enumField)
        {
            var fileNameStyle = BundleBuilderSetting.GetPackageFileNameStyle(PackageName, PipelineName);
            enumField.Init(fileNameStyle);
            enumField.SetValueWithoutNotify(fileNameStyle);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageFileNameStyle(PackageName, PipelineName, (EFileNameStyle)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }

        /// <summary>
        /// 配置首包资源拷贝选项枚举字段并同步标签参数显隐
        /// </summary>
        protected void SetBundledCopyOptionField(EnumField enumField, TextField tagField)
        {
            var bundledCopyOption = BundleBuilderSetting.GetPackageBundledCopyOption(PackageName, PipelineName);
            enumField.Init(bundledCopyOption);
            enumField.SetValueWithoutNotify(bundledCopyOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageBundledCopyOption(PackageName, PipelineName, (EBundledCopyOption)enumField.value);
                SetBundledCopyParamVisible(tagField);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }

        /// <summary>
        /// 配置首包资源拷贝标签参数文本框并绑定持久化
        /// </summary>
        protected void SetBundledCopyParamField(TextField textField)
        {
            var bundledCopyParams = BundleBuilderSetting.GetPackageBundledCopyParams(PackageName, PipelineName);
            textField.SetValueWithoutNotify(bundledCopyParams);
            textField.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageBundledCopyParams(PackageName, PipelineName, textField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }

        /// <summary>
        /// 根据首包拷贝选项设置标签参数文本框是否可见
        /// </summary>
        protected void SetBundledCopyParamVisible(TextField tagField)
        {
            var option = BundleBuilderSetting.GetPackageBundledCopyOption(PackageName, PipelineName);
            tagField.visible = option == EBundledCopyOption.ClearAndCopyByTags || option == EBundledCopyOption.OnlyCopyByTags;
        }

        /// <summary>
        /// 配置是否清理构建缓存开关并绑定持久化
        /// </summary>
        protected void SetClearBuildCacheToggle(Toggle toggle)
        {
            bool clearBuildCache = BundleBuilderSetting.GetPackageClearBuildCache(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(clearBuildCache);
            toggle.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageClearBuildCache(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }

        /// <summary>
        /// 配置是否使用资源依赖数据库开关并绑定持久化
        /// </summary>
        protected void SetUseAssetDependencyDBToggle(Toggle toggle)
        {
            bool useAssetDependencyDB = BundleBuilderSetting.GetPackageUseAssetDependencyDB(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(useAssetDependencyDB);
            toggle.RegisterValueChangedCallback(evt =>
            {
                BundleBuilderSetting.SetPackageUseAssetDependencyDB(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }

        /// <summary>
        /// 创建资源包加密器下拉框并加入容器
        /// </summary>
        protected PopupField<Type> CreateBundleEncryptorField(VisualElement container)
        {
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IBundleEncryptor));
            if (classTypes.Count > 0)
            {
                var className = BundleBuilderSetting.GetPackageBundleEncryptorClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Bundle Encryptor";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    BundleBuilderSetting.SetPackageBundleEncryptorClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Bundle Encryptor";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }

        /// <summary>
        /// 创建资源清单加密器下拉框并加入容器
        /// </summary>
        protected PopupField<Type> CreateManifestEncryptorField(VisualElement container)
        {
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IManifestEncryptor));
            if (classTypes.Count > 0)
            {
                var className = BundleBuilderSetting.GetPackageManifestEncryptorClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Manifest Encryptor";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    BundleBuilderSetting.SetPackageManifestEncryptorClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Manifest Encryptor";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }

        /// <summary>
        /// 创建资源清单解密器下拉框并加入容器
        /// </summary>
        protected PopupField<Type> CreateManifestDecryptorField(VisualElement container)
        {
            var classTypes = EditorAssemblyUtility.GetAssignableTypes(typeof(IManifestDecryptor));
            if (classTypes.Count > 0)
            {
                var className = BundleBuilderSetting.GetPackageManifestDecryptorClassName(PackageName, PipelineName);
                int defaultIndex = classTypes.FindIndex(x => x.FullName.Equals(className));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var popupField = new PopupField<Type>(classTypes, defaultIndex);
                popupField.label = "Manifest Decryptor";
                popupField.style.width = StyleWidth;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    BundleBuilderSetting.SetPackageManifestDecryptorClassName(PackageName, PipelineName, popupField.value.FullName);
                });
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
            else
            {
                var popupField = new PopupField<Type>();
                popupField.label = "Manifest Decryptor";
                popupField.style.width = StyleWidth;
                container.Add(popupField);
                UIElementsTools.SetElementLabelMinWidth(popupField, LabelMinWidth);
                return popupField;
            }
        }
        #endregion
    }
}