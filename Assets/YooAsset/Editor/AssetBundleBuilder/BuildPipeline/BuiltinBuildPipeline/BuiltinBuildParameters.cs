using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class BuiltinBuildParameters : BuildParameters
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
        /// 忽略类型树变化
        /// </summary>
        public bool IgnoreTypeTreeChanges = true;

        /// <summary>
        /// 使用可寻址地址代替资源路径
        /// 说明：开启此项可以节省运行时清单占用的内存！
        /// </summary>
        public bool ReplaceAssetPathWithAddress = false;


        /// <summary>
        /// 获取内置构建管线的构建选项
        /// </summary>
        public BuildAssetBundleOptions GetBundleBuildOptions()
        {
            // For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
            // 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

            if (CompressOption == ECompressOption.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
            else if (CompressOption == ECompressOption.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;

            if (ClearBuildCacheFiles)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
            if (StripUnityVersion)
                opt |= BuildAssetBundleOptions.AssetBundleStripUnityVersion; //Removes the Unity Version number in the Archive File & Serialized File headers
            if (DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            if (IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName; //Disables Asset Bundle LoadAsset by file name.
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

            return opt;
        }
    }
}