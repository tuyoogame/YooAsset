using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源依赖关系缓存
    /// </summary>
    public class AssetDependencyCache
    {
        private readonly AssetDependencyDatabase _database;

        /// <summary>
        /// 初始化资源依赖缓存系统
        /// </summary>
        /// <param name="useAssetDependencyDB">是否使用资源依赖数据库</param>
        public AssetDependencyCache(bool useAssetDependencyDB)
        {
            if (useAssetDependencyDB)
                Debug.Log("Asset dependency database is enabled.");

            string databaseFilePath = "Library/AssetDependencyDB";
            _database = new AssetDependencyDatabase();
            _database.CreateDatabase(useAssetDependencyDB, databaseFilePath);

            if (useAssetDependencyDB)
            {
                _database.SaveDatabase();
            }
        }

        /// <summary>
        ///  获取资源的依赖列表
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="recursive">递归查找所有依赖</param>
        /// <returns>返回依赖的资源路径集合</returns>
        public string[] GetDependencies(string assetPath, bool recursive = true)
        {
            // 通过本地缓存获取依赖关系
            return _database.GetDependencies(assetPath, recursive);

            // 通过Unity引擎获取依赖关系
            //return AssetDatabase.GetDependencies(assetPath, recursive);
        }
    }
}