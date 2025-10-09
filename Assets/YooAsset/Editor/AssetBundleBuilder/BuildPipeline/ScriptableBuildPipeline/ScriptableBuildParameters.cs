using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    public class ScriptableBuildParameters : BuildParameters
    {
        /// <summary>
        /// 压缩选项
        /// </summary>
        public ECompressOption CompressOption = ECompressOption.Uncompressed;

        /// <summary>
        /// 从文件头里剥离Unity版本信息
        /// </summary>
        public bool StripUnityVersion = false;

        /// <summary>
        /// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
        /// </summary>
        public bool DisableWriteTypeTree = false;

        /// <summary>
        /// 忽略类型树变化（无效参数）
        /// </summary>
        public bool IgnoreTypeTreeChanges = true;

        /// <summary>
        /// 使用可寻址地址代替资源路径
        /// 说明：开启此项可以节省运行时清单占用的内存！
        /// </summary>
        public bool ReplaceAssetPathWithAddress = false;

        /// <summary>
        /// 自动建立资源对象对图集的依赖关系
        /// </summary>
        public bool TrackSpriteAtlasDependencies = false;


        /// <summary>
        /// 生成代码防裁剪配置
        /// </summary>
        public bool WriteLinkXML = true;

        /// <summary>
        /// 缓存服务器地址
        /// </summary>
        public string CacheServerHost;

        /// <summary>
        /// 缓存服务器端口
        /// </summary>
        public int CacheServerPort;


        /// <summary>
        /// 内置着色器资源包名称
        /// </summary>
        public string BuiltinShadersBundleName;

        /// <summary>
        /// Mono脚本资源包名称
        /// </summary>
        public string MonoScriptsBundleName;


        /// <summary>
        /// 获取可编程构建管线的构建参数
        /// </summary>
        public BundleBuildParameters GetBundleBuildParameters()
        {
            var targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(BuildTarget);
            var pipelineOutputDirectory = GetPipelineOutputDirectory();
            var buildParams = new BundleBuildParameters(BuildTarget, targetGroup, pipelineOutputDirectory);

            if (CompressOption == ECompressOption.Uncompressed)
                buildParams.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
            else if (CompressOption == ECompressOption.LZMA)
                buildParams.BundleCompression = UnityEngine.BuildCompression.LZMA;
            else if (CompressOption == ECompressOption.LZ4)
                buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
            else
                throw new System.NotImplementedException(CompressOption.ToString());

            if (StripUnityVersion)
                buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.StripUnityVersion; // Build Flag to indicate the Unity Version should not be written to the serialized file.
            if (DisableWriteTypeTree)
                buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree; //Do not include type information within the built content.

            buildParams.UseCache = true;
            buildParams.CacheServerHost = CacheServerHost;
            buildParams.CacheServerPort = CacheServerPort;
            buildParams.WriteLinkXML = WriteLinkXML;

            return buildParams;
        }
    }
}