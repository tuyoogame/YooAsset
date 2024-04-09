using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 自动导入SBP依赖类
    /// </summary>
    [InitializeOnLoad]
    public static class SBPPackageImporter
    {
        static SBPPackageImporter()
        {
            InstallPackage("com.unity.scriptablebuildpipeline");
        }
        
        private static bool IsPackageInstalled(string packageName)
        {
            return UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{packageName}") != null;
        }

        public static bool InstallPackage(string packageName)
        {
            if (IsPackageInstalled(packageName))
            {
                return false;
            }
            Debug.Log($"Install...{packageName}");
            var request = Client.Add(packageName);
            while (!request.IsCompleted) { };
            if (request.Error != null) Debug.LogError(request.Error.message);
            return request.Error == null;
        }
    }
}