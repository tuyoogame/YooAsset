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
            var initializeFileSystemOp = fileSystem.InitializeFileSystemAsync();
            OperationSystem.StartOperation(packageName, initializeFileSystemOp);
            yield return initializeFileSystemOp;
            if (initializeFileSystemOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"初始化小游戏文件系统失败！{initializeFileSystemOp.Error}");
                yield break;
            }

            // 请求资源版本
            Debug.Log("请求资源版本信息！");
            var requestPackageVersionOp = fileSystem.RequestPackageVersionAsync(true, 60);
            OperationSystem.StartOperation(packageName, requestPackageVersionOp);
            yield return requestPackageVersionOp;
            if (requestPackageVersionOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"请求资源版本信息失败！{requestPackageVersionOp.Error}");
                yield break;
            }

            // 请求资源清单
            string packageVersion = requestPackageVersionOp.PackageVersion;
            Debug.Log($"加载资源清单文件！{packageVersion}");
            var loadPackageManifestOp = fileSystem.LoadPackageManifestAsync(packageVersion, 60);
            OperationSystem.StartOperation(packageName, loadPackageManifestOp);
            yield return loadPackageManifestOp;
            if (loadPackageManifestOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"加载资源清单文件失败！{loadPackageManifestOp.Error}");
                yield break;
            }

            // 预下载资源包
            Debug.Log("预下载资源包！");
            {
                var manifest = loadPackageManifestOp.Manifest;
                var packageBundle = GetPackageBundle(manifest, testLocation);
                var options = new DownloadFileOptions(1);
                var downloadFileOp = fileSystem.DownloadFileAsync(packageBundle, options);
                OperationSystem.StartOperation(packageName, downloadFileOp);
                yield return downloadFileOp;
                if (downloadFileOp.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"预下载资源包失败！{downloadFileOp.Error}");
                    yield break;
                }
                else
                {
                    Debug.Log("预下载资源包成功！");
                }
            }

            // 加载资源包
            Debug.Log("加载资源包！");
            {
                var manifest = loadPackageManifestOp.Manifest;
                var packageBundle = GetPackageBundle(manifest, testLocation);
                var loadBundleFileOp = fileSystem.LoadBundleFile(packageBundle);
                OperationSystem.StartOperation(packageName, loadBundleFileOp);
                yield return loadBundleFileOp;
                if (loadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"加载资源包失败！{loadBundleFileOp.Error}");
                    yield break;
                }
                else
                {
                    Debug.Log("加载资源包成功！");
                }

                // 卸载资源包
                loadBundleFileOp.Result.UnloadBundleFile();
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