using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建报告
    /// </summary>
    [Serializable]
    public class BuildReport
    {
        [NonSerialized]
        private Dictionary<string, ReportBundleInfo> _bundleInfoLookup;
        [NonSerialized]
        private Dictionary<string, ReportAssetInfo> _assetInfoLookup;

        /// <summary>
        /// 汇总信息
        /// </summary>
        public ReportSummary Summary = new ReportSummary();

        /// <summary>
        /// 资源对象列表
        /// </summary>
        public List<ReportAssetInfo> AssetInfos = new List<ReportAssetInfo>();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ReportBundleInfo> BundleInfos = new List<ReportBundleInfo>();

        /// <summary>
        /// 未被依赖的资源列表
        /// </summary>
        public List<ReportIndependentAsset> IndependentAssets = new List<ReportIndependentAsset>();


        /// <summary>
        /// 获取指定名称的资源包信息
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <returns>匹配的资源包信息</returns>
        public ReportBundleInfo GetBundleInfo(string bundleName)
        {
            if (bundleName == null)
                throw new ArgumentNullException(nameof(bundleName));

            EnsureBundleLookup();
            if (_bundleInfoLookup.TryGetValue(bundleName, out var bundleInfo))
                return bundleInfo;

            throw new ArgumentException($"Bundle not found: '{bundleName}'.", nameof(bundleName));
        }

        /// <summary>
        /// 获取指定路径的资源信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>匹配的资源信息</returns>
        public ReportAssetInfo GetAssetInfo(string assetPath)
        {
            if (assetPath == null)
                throw new ArgumentNullException(nameof(assetPath));

            EnsureAssetLookup();
            if (_assetInfoLookup.TryGetValue(assetPath, out var assetInfo))
                return assetInfo;

            throw new ArgumentException($"Asset not found: '{assetPath}'.", nameof(assetPath));
        }


        /// <summary>
        /// 将构建报告序列化为 JSON 并写入文件
        /// </summary>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="buildReport">要序列化的构建报告</param>
        public static void Serialize(string savePath, BuildReport buildReport)
        {
            if (savePath == null)
                throw new ArgumentNullException(nameof(savePath));
            if (buildReport == null)
                throw new ArgumentNullException(nameof(buildReport));

            if (File.Exists(savePath))
                File.Delete(savePath);

            string json = JsonUtility.ToJson(buildReport, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化构建报告
        /// </summary>
        /// <param name="jsonData">JSON 格式的报告数据</param>
        /// <returns>反序列化后的构建报告实例</returns>
        public static BuildReport Deserialize(string jsonData)
        {
            if (jsonData == null)
                throw new ArgumentNullException(nameof(jsonData));

            BuildReport report = JsonUtility.FromJson<BuildReport>(jsonData);
            return report;
        }

        private void EnsureBundleLookup()
        {
            if (_bundleInfoLookup == null)
            {
                _bundleInfoLookup = new Dictionary<string, ReportBundleInfo>(BundleInfos.Count);
                foreach (var bundleInfo in BundleInfos)
                {
                    _bundleInfoLookup[bundleInfo.BundleName] = bundleInfo;
                }
            }
        }
        private void EnsureAssetLookup()
        {
            if (_assetInfoLookup == null)
            {
                _assetInfoLookup = new Dictionary<string, ReportAssetInfo>(AssetInfos.Count);
                foreach (var assetInfo in AssetInfos)
                {
                    _assetInfoLookup[assetInfo.AssetPath] = assetInfo;
                }
            }
        }
    }
}
