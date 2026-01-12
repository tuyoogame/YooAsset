using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集结果
    /// </summary>
    public class CollectResult
    {
        /// <summary>
        /// 收集命令
        /// </summary>
        public CollectCommand Command { private set; get; }

        /// <summary>
        /// 收集的资源信息列表
        /// </summary>
        public List<CollectAssetInfo> CollectAssets { private set; get; }

        /// <summary>
        /// 创建 CollectResult 实例
        /// </summary>
        /// <param name="command">收集命令</param>
        /// <param name="collectAssets">收集的资源信息列表</param>
        public CollectResult(CollectCommand command, List<CollectAssetInfo> collectAssets)
        {
            Command = command;
            CollectAssets = collectAssets;
        }
    }
}