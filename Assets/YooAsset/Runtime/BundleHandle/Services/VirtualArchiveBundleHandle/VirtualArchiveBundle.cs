using System;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 虚拟归档资源包数据类
    /// </summary>
    internal class VirtualArchiveBundle
    {
        private readonly HashSet<string> _archiveAssetPaths;
        private readonly Dictionary<string, RawFileObject> _cachedObjects = new Dictionary<string, RawFileObject>();
        private bool _isUnloaded;

        /// <summary>
        /// 创建 VirtualArchiveBundle 实例
        /// </summary>
        /// <param name="archiveAssetPaths">归档资源包包含的资源路径集合</param>
        public VirtualArchiveBundle(HashSet<string> archiveAssetPaths)
        {
            if (archiveAssetPaths == null)
                throw new ArgumentNullException(nameof(archiveAssetPaths));

            _archiveAssetPaths = archiveAssetPaths;
            _isUnloaded = false;
        }

        /// <summary>
        /// 根据资源路径从编辑器源文件创建原生文件对象，已创建的对象会被缓存
        /// </summary>
        /// <param name="assetPath">编辑器源文件路径</param>
        /// <returns>创建得到的原生文件对象</returns>
        public RawFileObject CreateRawFileObject(string assetPath)
        {
            if (_isUnloaded)
                throw new InvalidOperationException($"{nameof(VirtualArchiveBundle)} has been unloaded.");
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("Asset path is null or empty.", nameof(assetPath));
            if (_archiveAssetPaths.Contains(assetPath) == false)
                throw new InvalidOperationException($"Asset path '{assetPath}' does not belong to this virtual archive bundle.");

            if (_cachedObjects.TryGetValue(assetPath, out RawFileObject cached))
                return cached;

            byte[] fileData = File.ReadAllBytes(assetPath);
            var rawFileObject = RawFileObject.CreateFromBytes(fileData);
            _cachedObjects[assetPath] = rawFileObject;
            return rawFileObject;
        }

        /// <summary>
        /// 卸载虚拟归档资源包
        /// </summary>
        public void Unload()
        {
            _isUnloaded = true;
            foreach (var obj in _cachedObjects.Values)
            {
                obj.Release();
                UnityEngine.Object.Destroy(obj);
            }
            _cachedObjects.Clear();
            _archiveAssetPaths.Clear();
        }
    }
}
