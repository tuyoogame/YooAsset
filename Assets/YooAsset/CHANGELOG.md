# CHANGELOG

All notable changes to this package will be documented in this file.

## [3.0.5] - 2026-07-20

本版本将多开客户端的缓存目录隔离方案由全局实例标识改为显式传入独立的 `packageRoot`。

### Removed

- 移除全局文件系统实例标识

  多开客户端的缓存目录隔离不再依赖全局实例标识，改为在创建文件系统时为各文件系统显式传入独立的 `packageRoot`。

  移除 `YooAssetConfiguration.SetFileSystemInstanceId()` / `GetFileSystemInstanceId()` 。

## [3.0.4] - 2026-07-14

本版本新增多开客户端缓存目录隔离和稳定加密构建任务，并修复资源清单加载进度及实例化激活状态问题。

### Added

- 新增多开客户端缓存目录隔离机制

  支持通过 `YooAssetConfiguration.SetFileSystemInstanceId()` 设置全局实例标识，使 `SandboxFileSystem` 和 `BuiltinFileSystem` 的默认可写缓存目录按客户端实例隔离。

- 新增稳定加密构建任务 `TaskStableEncryption`

  构建时生成的密文与已有文件一致时复用原文件，避免更新文件修改时间，减少增量构建中的无效文件变更。

### Fixed

- 修复同步和异步实例化的激活状态不一致

  `InstantiateOperation` 现在会统一应用实例化选项中的 `IsActive` 状态。

- 修复资源清单加载进度不准确

  完善资源清单下载、加载和预取操作之间的进度传递，避免进度缺失或回退。

## [3.0.3-beta] - 2026-06-18

本版本重点优化资源清单二进制结构，显著减小清单体积、同时降低运行时内存占用并提升资源定位查询效率。。

### Added

- 新增 `AssetInfo.Tags` 资源标签访问接口

  支持通过 `AssetInfo` 直接访问资源标签集合，便于在资源查询结果中判断和遍历标签。

- 新增 Bundle Collector 可编辑 Inspector 扩展

  支持在 Inspector 面板中扩展和编辑 Bundle Collector 相关配置，方便自定义收集器配置界面。

### Changed

- 优化资源清单二进制结构

  减小资源清单文件体积，降低运行时内存占用，并提升资源定位查询效率。

  小型清单体积约减少 20%，重度项目预计可减少 35%~50%。

- 重构 `AssetReference`

  调整资源引用相关实现，提升接口一致性和维护性，新增对 `Texture2D` 的支持。

- 调整编辑器缓存目录位置

  编辑器模式下的缓存根目录调整到项目 `Library` 目录内，避免缓存文件直接生成在项目根目录，减少对工程目录的污染。

### Fixed

- (#752) 修复小游戏扩展类解密器枚举命名错误。

- (#660) 修复了UniTask扩展类里 IL2CPP 逆变委托崩溃。


## [3.0.2-beta] - 2026-05-28

### Added

- 新增 `WebNetworkFileSystem` 文件系统

  用于统一 WebGL 远程加载和 Mini Game 平台资源加载流程，并通过 `IWebPlatformStrategy` 适配不同平台的 AssetBundle 请求、提取和卸载行为。

- 新增 Web 文件系统对 `RawBundle` 和 `ArchiveBundle` 的加载支持

  `WebServerFileSystem` 和 `WebNetworkFileSystem` 支持在 WebGL 平台加载原始文件包和归档资源包。

- 新增加密 `ArchiveBundle` 构建和加载支持

  `ArchiveFileBuildPipeline` 支持加密归档资源包，运行时可通过 `EFileSystemParameter.ArchiveBundleDecryptor` 配置对应解密器。

- 新增 `IBundleUnpackPolicy` 内置资源包解包策略

  支持根据资源包名称、文件名、类型、加密状态和标签，自定义哪些内置资源包需要解包。

- 新增 `IBuiltinFileAccessor` 内置文件访问器

  支持为 `StreamingAssets` 文件提供自定义存在检测和字节读取能力，方便 Android 等平台接入第三方同步读取方案。

- 新增 Bundle Collector 资源搜索功能

  支持在收集器窗口输入或拖入资源路径，定位资源所在的分组和收集器，并高亮命中的资源配置。

- 新增 Mini Game 平台示例

  补充 OPPO、vivo、快手小游戏文件系统示例。

### Changed

- WebGL 运行模式文件系统结构调整

  移除旧版 `WebGameFileSystem` 和 `WebRemoteFileSystem`，相关能力已合并到新的文件系统 `WebNetworkFileSystem`。

### Fixed

- 修复微信小游戏示例宏兼容问题

  微信小游戏文件系统同时支持 `WEIXINMINIGAME` 和 `UNITY_WECHATMINIGAME` 宏。

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
