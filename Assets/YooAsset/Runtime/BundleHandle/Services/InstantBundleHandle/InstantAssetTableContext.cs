#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT

using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// InstantAsset 表上下文
    /// </summary>
    internal sealed class InstantAssetTableContext
    {
        /// <summary>
        /// 初始化结果
        /// </summary>
        public readonly struct InitializeResult
        {
            /// <summary>
            /// 错误信息
            /// </summary>
            public readonly string Error;

            /// <summary>
            /// 初始化是否成功
            /// </summary>
            public bool Succeeded
            {
                get { return Error == null; }
            }

            private InitializeResult(string error)
            {
                Error = error;
            }

            /// <summary>
            /// 创建表示初始化成功的默认结果
            /// </summary>
            public static InitializeResult Default()
            {
                return new InitializeResult(null);
            }

            /// <summary>
            /// 创建表示初始化失败的结果
            /// </summary>
            /// <param name="error">错误信息</param>
            public static InitializeResult Failure(string error)
            {
                return new InitializeResult(error);
            }
        }

        private readonly string _rootPath;
        private readonly string _assetTableName;
        private readonly string _sceneTableName;

        /// <summary>
        /// 资源表
        /// </summary>
        public InstantAssetTable AssetTable { get; private set; }

        /// <summary>
        /// 场景表
        /// </summary>
        /// <remarks>
        /// 如果不包含场景资源，该值为空值。
        /// </remarks>
        public InstantAssetTable SceneTable { get; private set; }

        public InstantAssetTableContext(string rootPath, string assetTableName)
        {
            _rootPath = rootPath;
            _assetTableName = assetTableName;
            _sceneTableName = $"{assetTableName}-scene";
        }

        /// <summary>
        /// 初始化表上下文
        /// </summary>
        /// <returns>初始化结果</returns>
        public InitializeResult Initialize()
        {
            InstantAsset.SetInstantAssetRootPath(_rootPath);

            string assetTablePath = PathUtility.Combine(_rootPath, _assetTableName);
            AssetTable = InstantAsset.ReadAssetTable(assetTablePath) as InstantAssetTable;
            if (AssetTable == null)
            {
                string error = $"Failed to load InstantAssetTable: '{assetTablePath}'.";
                return InitializeResult.Failure(error);
            }

            string sceneTablePath = PathUtility.Combine(_rootPath, _sceneTableName);
            SceneTable = InstantAsset.ReadAssetTable(sceneTablePath) as InstantAssetTable;
            if (SceneTable == null)
            {
                YooLogger.LogWarning($"InstantAsset scene table not found: '{sceneTablePath}'.");
            }

            return InitializeResult.Default();
        }

        /// <summary>
        /// 卸载当前上下文持有的所有表
        /// </summary>
        public void Dispose()
        {
            if (AssetTable != null)
            {
                string assetTablePath = PathUtility.Combine(_rootPath, _assetTableName);
                InstantAsset.UnloadAssetTable(assetTablePath);
                AssetTable = null;
            }

            if (SceneTable != null)
            {
                string sceneTablePath = PathUtility.Combine(_rootPath, _sceneTableName);
                InstantAsset.UnloadAssetTable(sceneTablePath);
                SceneTable = null;
            }
        }
    }
}
#endif
