using System.Collections;
using UnityEngine;

namespace YooAsset
{
    internal class FileSystemTester
    {
        public IEnumerator RunTester(IFileSystem fileSystem, string testLocation)
        {
            string packageName = fileSystem.PackageName;

            // 初始化小游戏文件系统
            Debug.Log("初始化小游戏文件系统！");
            var initializeFileSystemOp = fileSystem.InitializeAsync();
            AsyncOperationSystem.StartOperation(packageName, initializeFileSystemOp);
            yield return initializeFileSystemOp;
            if (initializeFileSystemOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"初始化小游戏文件系统失败！{initializeFileSystemOp.Error}");
                yield break;
            }

            // 请求资源版本
            Debug.Log("请求资源版本信息！");
            var requestPackageVersionOptions = new FSRequestPackageVersionOptions(true, 60);
            var requestPackageVersionOp = fileSystem.RequestPackageVersionAsync(requestPackageVersionOptions);
            AsyncOperationSystem.StartOperation(packageName, requestPackageVersionOp);
            yield return requestPackageVersionOp;
            if (requestPackageVersionOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"请求资源版本信息失败！{requestPackageVersionOp.Error}");
                yield break;
            }

            // 请求资源清单
            string packageVersion = requestPackageVersionOp.PackageVersion;
            Debug.Log($"加载资源清单文件！{packageVersion}");
            var loadPackageManifestOptions = new FSLoadPackageManifestOptions(packageVersion, 60);
            var loadPackageManifestOp = fileSystem.LoadPackageManifestAsync(loadPackageManifestOptions);
            AsyncOperationSystem.StartOperation(packageName, loadPackageManifestOp);
            yield return loadPackageManifestOp;
            if (loadPackageManifestOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"加载资源清单文件失败！{loadPackageManifestOp.Error}");
                yield break;
            }

            // 加载资源包
            Debug.Log("加载资源包！");
            {
                var manifest = loadPackageManifestOp.Manifest;
                var packageBundle = GetPackageBundle(manifest, testLocation);
                var loadBundleFileOptions = new FSLoadPackageBundleOptions(packageBundle);
                var loadBundleFileOp = fileSystem.LoadPackageBundleAsync(loadBundleFileOptions);
                AsyncOperationSystem.StartOperation(packageName, loadBundleFileOp);
                yield return loadBundleFileOp;
                if (loadBundleFileOp.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"加载资源包失败！{loadBundleFileOp.Error}");
                    yield break;
                }
                else
                {
                    Debug.Log("加载资源包成功！");
                }

                // 卸载资源包
                loadBundleFileOp.BundleHandle.UnloadBundle();
            }

            Debug.Log("完整测试成功！");
        }

        private PackageBundle GetPackageBundle(PackageManifest manifest, string location)
        {
            var assetInfo = manifest.ConvertLocationToAssetInfo(location, typeof(GameObject));
            var packageBundle = manifest.GetMainPackageBundle(assetInfo.Asset);
            return packageBundle;
        }
    }
}