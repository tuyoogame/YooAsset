# CHANGELOG

All notable changes to this package will be documented in this file.

## [1.2.4] - 2022-09-22

### Fixed

- 修复了加密文件下载验证失败的问题。
- 修复了可编程构建管线下模拟构建模式报错的问题。

### Changed

- 可编程构建管线强制使用增量构建模式。
- 移除了对Gizmos资源的打包限制。
- AssetBundleCollector窗口增加配置表修复功能。

## [1.2.3] - 2022-09-09

### Fixed

- 修复了资源收集器无法识别.bank音频文件格式。

### Changed

- **HostPlayMode正式支持WebGL平台。**
- AssetBundleCollector里的着色器收集选项已经移除，现在必定收集。
- AssetBundleCollector修改了默认的打包规则类。
- AssetBundleBuilder现在构建结果增加补丁包目录。
- 更新了UniTask的Sample。
- 优化了缓存系统的代码结构。
- 使用了新的断点续传下载器。

### Added

- 增加清理缓存资源的异步操作类。

````c#
/// <summary>
/// 清空未被使用的缓存文件
/// </summary>
public static ClearUnusedCacheFilesOperation ClearUnusedCacheFiles();
````

## [1.2.2] - 2022-07-31

### Fixed

- 修复了加载多个相同的子场景而无法全部卸载的问题。

### Changed

- ShaderVariantCollecor支持在CI上调用运行。

- 资源补丁清单增加文件版本校验功能。

- AssetBundleBuilder现在构建结果可以查询构建失败信息。

- AssetBundleBuilder现在资源包文件名称样式提供选择功能。

  ````c#
  class BuildParameters
  {
      /// <summary>
      /// 补丁文件名称的样式
      /// </summary>
      public EOutputNameStyle OutputNameStyle;
  }
  ````

### Added

- 增加获取资源信息新方法。

  ````c#
  /// <summary>
  /// 获取资源信息
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  public static AssetInfo GetAssetInfo(string location);
  ````

## [1.2.1] - 2022-07-23

### Fixed

- (#25)修复了资源文件不存在返回的handle无法完成的问题。
- (#26)修复多个场景打进一个AB包时，卸载子场景时抛出异常。

### Changed

- 构建报告里增加主资源总数的统计。
- 资源构建系统里修改了内置构建管线的构建结果验证逻辑，移除了对中文路径的检测。
- 资源构建系统里移除了对增量更新初次无法构建的限制。
- 优化了缓存验证逻辑，不期望删除断点续传的资源文件。
- 资源构建系统里SBP构建参数增加了缓存服务器的地址和端口。

## [1.2.0] - 2022-07-18

### Fixed

- 修复了ShaderVariantCollection刷新不及时问题。

### Changed

- 资源收集忽略了Gizmos资源文件。
- 解密服务接口增加解密文件信息参数。
- 资源收集窗体增加配置保存按钮。
- 资源构建窗体增加配置保存按钮。

### Added

- 资源构建模块增加了可编程构建管线(SBP)的支持，开发者可以在内置构建管线和可编程构建管线之间自由选择，零修改成本。

## [1.1.1] - 2022-07-07

### Fixed

- 修复了AssetBundleDebugger窗口，View下拉页签切换无效的问题。
- 修复了在Unity2020.3版本下UniTask在真机上的一个IL2CPP相关的错误。

### Changed

- 优化了AssetBundleDebugger窗口，增加了帧数显示以及回放功能。
- 优化了AssetBundleBuilder的代码结构。
- 增强了YooAssets.GetRawFileAsync()方法的容错。

### Added

- 新增了OperationHandleBase.GetAssetInfo()方法。

  ````c#
  /// <summary>
  /// 获取资源信息
  /// </summary>
  public AssetInfo GetAssetInfo();
  ````

- 新增了AssetOperationHandle.GetAssetObjet<TAsset>()方法。

  ````c#
  /// <summary>
  /// 获取资源对象
  /// </summary>
  /// <typeparam name="TAsset">资源类型</typeparam>
  public TAsset GetAssetObjet<TAsset>()；
  ````

- 新增了弱联网情况下加载补丁清单方法。

  ````c#
  /// <summary>
  /// 弱联网情况下加载补丁清单
  /// 注意：当指定版本内容验证失败后会返回失败。
  /// </summary>
  /// <param name="resourceVersion">指定的资源版本</param>
  public static UpdateManifestOperation WeaklyUpdateManifestAsync(int resourceVersion)；
  ````

### Removed

- 离线运行模式（OfflinePlayMode）下移除了资内置资源解压相关逻辑。
- 移除了初始化参数：AutoReleaseGameObjectHandle及相关代码逻辑。

## [1.1.0] - 2022-06-23

### Fixed

- 修复了AssetBundleCollector窗口，在切换EnableAddressable时未及时刷新界面的问题。
- 修复了AssetBundleCollector窗口，资源过滤器CollectSprite无效的问题。
- 修复了AssetBundleCollector窗口，无法正常预览StaticAssetCollector的资源列表的问题。
- 修复了在离线模式下原生文件每次都从包内加载的问题。

### Changed

- 变更了共享资源打包机制。
- AssetBundleCollector窗口增加了分组禁用功能。
- AssetBundleDebugger窗口增加了真机远程调试功能。
- AssetBundleBuilder窗口在构建成功后自动显示构建文件夹。
- DownloaderOperation.OnDownloadFileFailedCallback委托变更为OnDownloadErrorCallback委托。

### Added

- 新增UpdateManifestOperation.FoundNewManifest字段。
- 新增DownloaderOperation.OnStartDownloadFileCallback委托。
- 新增AssetInfo.Address字段。
- 新增YooAssets.IsInitialized字段。
- 新增YooAssets初始化参数。

  ````c#
  /// <summary>
  /// 下载文件校验等级
  /// </summary>
  public EVerifyLevel VerifyLevel = EVerifyLevel.High;
  ````

- 新增YooAssets获取资源完成路径的方法。

  ````c#
  /// <summary>
  /// 获取资源路径
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  /// <returns>如果location地址无效，则返回空字符串</returns>
  public static string GetAssetPath(string location);
  ````

- 新增YooAssets初始化参数。

  ```c#
  /// <summary>
  /// 自动释放游戏对象所属资源句柄
  /// 说明：通过资源句柄实例化的游戏对象在销毁之后，会自动释放所属资源句柄。
  /// </summary>
  public bool AutoReleaseGameObjectHandle = false;
  ```

## [1.0.10] - 2022-05-22

### Fixed

- 修复了资源收集配置存在多个的时候，导致后续无法打开窗口的问题。
- 修复了在编辑器模拟模式下加载精灵图片失败的问题。
- 修复了在Unity2019版本无法识别配置文件的问题。

### Changed

- 资源构建增加内置资源文件（首包资源文件）拷贝的选项。
- 补丁下载器增加暂停方法和恢复方法。
- 在资源收集界面，对Collector的增加和删除支持撤销和恢复操作。

## [1.0.9] - 2022-05-14

### Fixed

- 修复了YooAssets.GetAssetInfos(string Tag)方法返回了无关的资源信息的问题。

### Changed

- 编辑器下的模拟运行模式，不再依赖配置里的构建版本。
- 更新资源清单结构，资源对象类增加分类标签。
- 优化了资源工具相关配置文件的加载方式和途径，这些配置文件可以放置在任何目录下。
- 优化了Location无效后的错误报告方式。
- 优化了资源包的构建参数，现在始终开启DisableLoadAssetByFileName，帮助减小运行时的内存。
- YooAssets.ProcessOperation()重命名为YooAssets.StartOperation()

### Added

- 新增YooAssets.IsNeedDownloadFromRemote()方法。

  ````c#
  public static bool IsNeedDownloadFromRemote(string location);
  ````

- 新增获取所有子资源对象的方法。

  ````c#
  class SubAssetsOperationHandle
  {
      public TObject[] GetSubAssetObjects<TObject>();
  }
  ````

### Removed

- YooAssets.GetBundleInfo()方法已经移除。

## [1.0.8] - 2022-05-08

### Fixed

- 修复了资源收集器导出配置文件时没有导出公共设置。
- 修复了不兼容Unity2018版本的错误。

### Changed

- AssetBundleGrouper窗口变更为AssetBundleCollector窗口。
- **优化了编辑器下模拟运行的初始化速度**。
- **优化了资源收集窗口打开时卡顿的问题**。
- 资源收集XML配表支持版本兼容。
- 资源报告查看窗口支持预览AssetBundle文件内容的功能。
- 完善了对UniTask的支持。
- YooAssets所有接口支持初始化容错检测。

### Added

- 异步操作类增加进度查询字段。

  ```c#
  class AsyncOperationBase
  {
      /// <summary>
      /// 处理进度
      /// </summary>
      public float Progress { get; protected set; } 
  }
  ```

- 增加开启异步操作的方法。

  ```c#
  /// <summary>
  /// 开启一个异步操作
  /// </summary>
  /// <param name="operation">异步操作对象</param>
  public static void ProcessOperaiton(GameAsyncOperation operation)
  ```

- 新增编辑器下模拟模式的初始化参数。

  ````c#
  /// <summary>
  /// 用于模拟运行的资源清单路径
  /// 注意：如果路径为空，会自动重新构建补丁清单。
  /// </summary>
  public string SimulatePatchManifestPath;
  ````

- 新增通用的初始化参数。

  ```c#
  /// <summary>
  /// 资源定位地址大小写不敏感
  /// </summary>
  public bool LocationToLower = false;
  ```

## [1.0.7] - 2022-05-04

### Fixed

- 修复了异步操作系统的Task再次等待无效的问题。

### Changed

- YooAssets.LoadRawFileAsync()方法重新命名为YooAssets.GetRawFileAsync()
- YooAssetSetting文件夹支持了全路径搜索定位。
- 优化了打包的核心逻辑，对依赖资源进行自动划分，以及支持设置依赖资源收集器。
- 初始化的时候，删除验证失败的资源文件。
- 构建报告浏览窗口支持排序功能。
- 着色器变种收集工具支持了配置缓存。

### Added

- 支持可寻址资源定位系统，包括编辑器和运行时环境。
- 增加快速构建模式，用于EditorPlayMode完美模拟线上环境。
- 增加了Window Dock功能，已打开的界面会自动停靠在一个窗体下。
- 增加一个新的打包规则：PackTopDirectory。
- 增加获取资源信息的方法。
  ```c#
  public static AssetInfo[] GetAssetInfos(string tag)
  ```
- 增加补丁下载器下载全部资源的方法。
  ```c#
  public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
  ```
- 增加指定资源版本的资源更新下载方法。
  ```c#
  public static UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout = 60)
  ```

### Removed

- 移除了自动释放资源的初始化参数。

## [1.0.6] - 2022-04-26

### Fixed

- 修复工具界面显示异常在Unity2021版本下。

### Changed

- 操作句柄支持错误信息查询。
- 支持UniTask异步操作库。
- 优化类型搜索方式，改为全域搜索类型。
- AssetBundleGrouper窗口添加和移除Grouper支持操作回退。

## [1.0.5] - 2022-04-22

### Fixed

- 修复了非主动收集的着色器没有打进统一的着色器资源包的问题。
- 修复了单个收集的资源对象没有设置依赖资源列表的问题。
- 修复Task异步加载一直等待的问题。

### Changed

- 资源打包的过滤文件列表增加cginc格式。
- 增加编辑器扩展的支持，第三方实现YooAsset插件。
- 优化原生文件加载逻辑，支持离线运行模式和编辑器运行模式。
- 优化场景卸载逻辑，在加载新的主场景的时候自动卸载已经加载的所有场景。
- 支持演练构建模式，在不生成资源包的情况下快速构建查看结果。
- 新增调试信息，出生场景和出生时间。

## [1.0.4] - 2022-04-18

### Fixed

- 修复资源清单附加版本之后引发的一个流程错误。
- 修复原生文件拷贝目录不存导致的加载失败。

### Changed

- 在编辑器下检测资源路径是否合法并警告。
- 完善原生文件异步加载接口。

## [1.0.3] - 2022-04-14

### Fixed

- 修复了AssetBundleDebugger窗口的BundleView视口下，Using列表显示不完整的问题。
- 修复了AssetBundleDebugger窗口的BundleView视口下，Bundle列表内元素重复的问题。
- 修复了特殊情况下依赖的资源包列表里包含主资源包的问题。

### Changed

- 实例化GameObject的时候，如果没有传递坐标和角度则使用默认值。
- 优化了资源分组配置保存策略，修改为窗口关闭时保存。
- 简化了资源版本概念，降低学习成本，统一了CDN上的目录结构。
- 资源定位接口扩展，方便开发可寻址资产定位功能。

### Added

- 离线运行模式支持WEBGL平台。
- 保留构建窗口界面的配置数据。

## [1.0.2] - 2022-04-07

### Fixed

- 修复在资源加载完成回调内释放自身资源句柄时的异常报错。
- 修复了资源分组在特殊情况下打包报错的问题。

### Changed

- StreamingAssets目录下增加了用于存放打包资源的总文件夹。

## [1.0.1] - 2022-04-07

### Fixed

- 修复Assets目录下存在多个YooAsset同名文件夹时，工具窗口无法显示的问题。
- 修复通过Packages导入YooAsset，工具窗口无法显示的问题。

## [1.0.0] - 2022-04-05
*Compatible with Unity 2019.4*

