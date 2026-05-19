# CHANGELOG

All notable changes to this package will be documented in this file.

## [3.0.1-beta] - 2026-05-19

### Added

- 新增归档文件构建管线 `ArchiveFileBuildPipeline`

  支持将同一资源包内的多个原始文件合并为 `ArchiveBundle`，并提供构建参数、构建任务、编辑器构建界面和运行时加载支持。

- 新增 `EnsureBundleFileOperation` 操作

  用于确保资源包文件已就绪，并通过 `EnsureBundleFileOperation.Detail` 获取资源包名称、本地文件路径、资源包类型和加密状态。

  **代码示例**

  ```csharp
  var package = YooAssets.GetPackage("DefaultPackage");
  
  // 确保资源所在的资源包文件已经就绪。
  // 如果资源包未缓存，会自动触发下载或解压流程。
  var operation = package.EnsureBundleFileAsync(new EnsureBundleFileOptions("asset_location"));
  yield return operation;
  
  if (operation.Status == EOperationStatus.Succeeded)
  {
      var detail = operation.Detail;
      string bundleName = detail.BundleName;
      string bundleFilePath = detail.BundleFilePath;
      int bundleType = detail.BundleType;
      bool isEncrypted = detail.IsEncrypted;
  
      UnityEngine.Debug.Log($"BundleName: {bundleName}");
      UnityEngine.Debug.Log($"BundleFilePath: {bundleFilePath}");
      UnityEngine.Debug.Log($"BundleType: {bundleType}");
      UnityEngine.Debug.Log($"IsEncrypted: {isEncrypted}");
  }
  else
  {
      UnityEngine.Debug.LogError(operation.Error);
  }
  ```

- 新增编辑器模拟模式下的 Bundle Cache 标记文件机制

  通过记录缓存状态，提升虚拟下载模式下缓存扫描、恢复和清理的可靠性。

### Changed

- `LoadRawFile` API 重命名为 `LoadBundleFile`

  对应句柄由 `RawFileHandle` 重命名为 `BundleFileHandle`，语义从“原生文件”调整为“资源包文件”。

## [3.0.0-beta] - 2026-05-09

beta released.
