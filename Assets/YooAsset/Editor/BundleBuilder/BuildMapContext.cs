using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建资源映射上下文，管理构建过程中所有资源包和资源的映射关系
    /// </summary>
    [ContextObject]
    public class BuildMapContext
    {
        private readonly Dictionary<string, BuildBundleInfo> _bundleInfoDictionary = new Dictionary<string, BuildBundleInfo>(10000);
        private readonly List<BuildAssetInfo> _spriteAtlasAssetList = new List<BuildAssetInfo>(10000);
        private readonly List<ReportIndependentAsset> _independentAssets = new List<ReportIndependentAsset>(1000);



        /// <summary>
        /// 参与构建的资源总数
        /// </summary>
        /// <remarks>包括主动收集的资源以及其依赖的所有资源。</remarks>
        public int AssetFileCount { get; set; }

        /// <summary>
        /// 资源收集命令
        /// </summary>
        public CollectCommand Command { set; get; }

        /// <summary>
        /// 资源包信息列表
        /// </summary>
        public Dictionary<string, BuildBundleInfo>.ValueCollection Collection
        {
            get
            {
                return _bundleInfoDictionary.Values;
            }
        }

        /// <summary>
        /// 图集资源集合
        /// </summary>
        public IReadOnlyList<BuildAssetInfo> SpriteAtlasAssetList
        {
            get { return _spriteAtlasAssetList; }
        }

        /// <summary>
        /// 未被依赖的资源列表
        /// </summary>
        public IReadOnlyList<ReportIndependentAsset> IndependentAssets
        {
            get { return _independentAssets; }
        }


        /// <summary>
        /// 添加一个未被依赖的资源
        /// </summary>
        internal void AddIndependentAsset(ReportIndependentAsset asset)
        {
            _independentAssets.Add(asset);
        }

        /// <summary>
        /// 添加一个打包资源
        /// </summary>
        /// <param name="assetInfo">待打包的资源信息</param>
        public void PackAsset(BuildAssetInfo assetInfo)
        {
            string bundleName = assetInfo.BundleName;
            if (string.IsNullOrEmpty(bundleName))
                throw new YooInternalException("Should never get here.");

            if (_bundleInfoDictionary.TryGetValue(bundleName, out BuildBundleInfo bundleInfo))
            {
                bundleInfo.PackAsset(assetInfo);
            }
            else
            {
                BuildBundleInfo newBundleInfo = new BuildBundleInfo(bundleName);
                newBundleInfo.PackAsset(assetInfo);
                _bundleInfoDictionary.Add(bundleName, newBundleInfo);
            }

            // 统计所有的精灵图集
            if (assetInfo.AssetInfo.IsSpriteAtlas())
            {
                _spriteAtlasAssetList.Add(assetInfo);
            }
        }

        /// <summary>
        /// 检查是否包含指定名称的资源包
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <returns>包含返回 true，否则返回 false</returns>
        public bool IsContainsBundle(string bundleName)
        {
            return _bundleInfoDictionary.ContainsKey(bundleName);
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <returns>对应的资源包构建信息</returns>
        public BuildBundleInfo GetBundleInfo(string bundleName)
        {
            if (_bundleInfoDictionary.TryGetValue(bundleName, out BuildBundleInfo result))
            {
                return result;
            }
            throw new YooInternalException($"Should never get here. Bundle not found: '{bundleName}'.");
        }

        /// <summary>
        /// 获取构建管线里需要的数据
        /// </summary>
        /// <param name="replaceAssetPathWithAddress">是否使用可寻址地址替代资源路径</param>
        /// <returns>所有资源包的 AssetBundleBuild 数据数组</returns>
        public UnityEditor.AssetBundleBuild[] GetPipelineBuilds(bool replaceAssetPathWithAddress)
        {
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(_bundleInfoDictionary.Count);
            foreach (var bundleInfo in _bundleInfoDictionary.Values)
            {
                builds.Add(bundleInfo.CreatePipelineBuild(replaceAssetPathWithAddress));
            }
            return builds.ToArray();
        }

        /// <summary>
        /// 创建空的资源包
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        public void CreateEmptyBundleInfo(string bundleName)
        {
            if (IsContainsBundle(bundleName) == false)
            {
                var bundleInfo = new BuildBundleInfo(bundleName);
                _bundleInfoDictionary.Add(bundleName, bundleInfo);
            }
        }
    }
}