using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskCreateCatalog
    {
        /// <summary>
        /// 生成内置资源记录文件
        /// </summary>
        internal void CreateCatalogFile(BuildParametersContext buildParametersContext)
        {
            string builtinRootDirectory = buildParametersContext.GetBuiltinRootDirectory();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            var manifestServices = buildParametersContext.Parameters.ManifestRestoreServices;
            CatalogTools.CreateCatalogFile(manifestServices, buildPackageName, builtinRootDirectory);

            // 刷新目录
            AssetDatabase.Refresh();
        }
    }
}