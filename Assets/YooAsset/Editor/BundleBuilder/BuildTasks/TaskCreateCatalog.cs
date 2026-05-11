using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 创建首包资源目录记录（Catalog）文件的任务
    /// </summary>
    public class TaskCreateCatalog
    {
        /// <summary>
        /// 生成首包资源记录文件
        /// </summary>
        internal void CreateCatalogFile(BuildParametersContext buildParametersContext)
        {
            string bundledRootDirectory = buildParametersContext.GetBundledRootDirectory();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            var manifestServices = buildParametersContext.Parameters.ManifestDecryptor;
            BuiltinCatalogHelper.CreateFile(manifestServices, buildPackageName, bundledRootDirectory);

            // 刷新目录
            AssetDatabase.Refresh();
        }
    }
}