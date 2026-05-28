#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结引擎构建管线的构建参数
    /// </summary>
    public class InstantAssetBuildParameters : BuildParameters
    {
        /// <summary>
        /// 压缩选项
        /// </summary>
        public ECompressOption CompressOption = ECompressOption.Uncompressed;

        /// <summary>
        /// 获取 InstantAsset 构建选项
        /// </summary>
        public InstantAssetOptions GetInstantAssetBuildOptions()
        {
            InstantAssetOptions opt = InstantAssetOptions.None;

            if (CompressOption == ECompressOption.Uncompressed)
                opt |= InstantAssetOptions.ScatteredFiles;
            else if (CompressOption == ECompressOption.LZ4)
                opt |= InstantAssetOptions.CompressionLz4HC;
            else if (CompressOption == ECompressOption.LZMA)
                opt |= InstantAssetOptions.CompressionLzma;

            // TODO：在开启 ForceRebuild 后，引擎会删除预生成的 AssetPackerRecordFile.json，导致引擎按照自身规则重新分包。
            /*
            if (ClearBuildCacheFiles)
                opt |= InstantAssetOptions.ForceRebuild;
            */

            return opt;
        }
    }
}
#endif
