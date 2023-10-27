# CHANGELOG

All notable changes to this package will be documented in this file.

## [2.0.3-preview] - 2023-10-27

### Fixed

- (#180) Fixed an issue with invalid encryption task in the build pipeline.
- (#185) Fixed subscene unload error when the scene is not loaded.
- (#190) Fixed webgl platform compile error.

### Improvements

- The asset load method add the priority parameter.
- The async operation class add the priority field.

### Added

- The InitializeParameters calss add new parameter : AutoDestroyAssetProvider

  ```c#
  /// <summary>
  /// 自动销毁不再使用的资源提供者
  /// </summary>
  public bool AutoDestroyAssetProvider = false;
  ```

- Resource package add TryUnloadUnusedAsset function.

  ```c#
  /// <summary>
  /// 尝试卸载指定资源的资源包（包括依赖资源）
  /// </summary>
  public void TryUnloadUnusedAsset(AssetInfo assetInfo)
  ```

### Removed

- The InitializeParameters calss remove the parameter : LoadingMaxTimeSlice 

## [2.0.2-preview] - 2023-10-17

### Fixed

- Fixed the mistaken code in the build window.
- Fixed an issue where auto collect shaders was not effective for dependent resources.

### Improvements

- Add error code for exception output during package building.

## [2.0.1-preview] - 2023-10-11

### Fixed

- (#175) Fixed a bug where the url path of mac platform contains spaces, which would cause the request error.
- (#177) Fixed the inability to load main asset object after loading the sub asset.
- (#178) Fixed the error when initializing resource package that prompted not initialized.
- (#179) Fixed issue with SBP build pipeline packaging reporting errors.

### Added

- Resource downloader add combine function.

  ```c#
  /// <summary>
  /// 合并其它下载器
  /// </summary>
  /// <param name="downloader">合并的下载器</param>
  public void Combine(DownloaderOperation downloader);
  ```

## [2.0.0-preview] - 2023-10-07

This is the preview version.
