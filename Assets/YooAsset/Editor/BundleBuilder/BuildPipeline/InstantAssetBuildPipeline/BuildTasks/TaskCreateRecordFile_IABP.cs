#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的分包配置文件创建任务
    /// </summary>
    public class TaskCreateRecordFile_IABP : IBuildTask
    {
        /// <summary>
        /// 分包配置文件上下文
        /// </summary>
        [ContextObject]
        public class RecordFileContext
        {
            /// <summary>
            /// BundleName 到 InstantAsset 产出文件名的映射
            /// </summary>
            public Dictionary<string, string> BundleNameToGuidMap;

            /// <summary>
            /// InstantAsset 产出文件名到 BundleName 的映射
            /// </summary>
            public Dictionary<string, string> GuidToBundleNameMap;

            /// <summary>
            /// 预期的 InstantAsset 分包记录
            /// </summary>
            public Dictionary<string, List<string>> RecordMap;

            /// <summary>
            /// 获取用于日志展示的资源包名称
            /// </summary>
            public string GetDisplayBundleName(string bundleGuid)
            {
                if (GuidToBundleNameMap.TryGetValue(bundleGuid, out string bundleName))
                    return $"{bundleName} (Instant: '{bundleGuid}')";
                return bundleGuid;
            }
        }

        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();

            // 记录映射关系
            int count = buildMapContext.Collection.Count;
            var bundleNameToGuidMap = new Dictionary<string, string>(count);
            var guidToBundleNameMap = new Dictionary<string, string>(count);
            var recordMap = new Dictionary<string, List<string>>(count);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string bundleGuid = HashUtility.ComputeMD5(bundleInfo.BundleName);
                bundleNameToGuidMap[bundleInfo.BundleName] = bundleGuid;
                guidToBundleNameMap[bundleGuid] = bundleInfo.BundleName;

                var assetGuids = new List<string>(bundleInfo.AllPackAssets.Count);
                foreach (var asset in bundleInfo.AllPackAssets)
                {
                    assetGuids.Add(asset.AssetInfo.AssetGUID);
                }
                recordMap[bundleGuid] = assetGuids;
            }

            // 创建分包配置文件
            InstantAssetRecordFile.Save(pipelineOutputDirectory, recordMap);

            var recordFileContext = new RecordFileContext();
            recordFileContext.BundleNameToGuidMap = bundleNameToGuidMap;
            recordFileContext.GuidToBundleNameMap = guidToBundleNameMap;
            recordFileContext.RecordMap = recordMap;
            context.SetContextObject(recordFileContext);
        }
    }
}
#endif
