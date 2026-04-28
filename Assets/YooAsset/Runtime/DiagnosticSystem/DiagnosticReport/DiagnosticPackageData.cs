using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 包裹的诊断数据容器
    /// </summary>
    [Serializable]
    internal class DiagnosticPackageData
    {
        [NonSerialized]
        private readonly Dictionary<string, DiagnosticBundleInfo> _bundleInfoDict = new Dictionary<string, DiagnosticBundleInfo>();
        [NonSerialized]
        private bool _isParsed = false;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 资源加载的诊断信息列表
        /// </summary>
        public List<DiagnosticProviderInfo> ProviderInfos = new List<DiagnosticProviderInfo>(1000);

        /// <summary>
        /// 资源包的诊断信息列表
        /// </summary>
        public List<DiagnosticBundleInfo> BundleInfos = new List<DiagnosticBundleInfo>(1000);

        /// <summary>
        /// 异步操作的诊断信息列表
        /// </summary>
        public List<DiagnosticOperationInfo> OperationInfos = new List<DiagnosticOperationInfo>(1000);

        /// <summary>
        /// 尝试获取资源包的诊断信息
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="bundleInfo">找到时返回对应的诊断信息</param>
        /// <returns>是否成功找到</returns>
        public bool TryGetBundleInfo(string bundleName, out DiagnosticBundleInfo bundleInfo)
        {
            if (_isParsed == false)
            {
                _isParsed = true;
                foreach (var info in BundleInfos)
                {
                    if (_bundleInfoDict.ContainsKey(info.BundleName) == false)
                    {
                        _bundleInfoDict.Add(info.BundleName, info);
                    }
                }
            }

            return _bundleInfoDict.TryGetValue(bundleName, out bundleInfo);
        }
    }
}