# CHANGELOG

All notable changes to this package will be documented in this file.

## [2.3.17] - 2025-10-30

**非常重要**：修复了#627优化导致的资源清单CRC值为空的问题。

该问题会导致下载的损坏文件验证通过。

影响范围：v2.3.15版本，v2.3.16版本。

**非常重要**：(#661) 修复了Package销毁过程中，遇到正在加载的AssetBundle会导致无法卸载的问题。

该问题是偶现，引擎会提示AssetBundle已经加载，无法加载新的文件，导致资源对象加载失败！

影响范围：所有版本！

### Improvements

- 重构并统一了资源清单的反序列化逻辑。

### Fixed

- (#645) 修复了着色器变种收集工具，在极端情况下变种收集不完整的问题。
- (#646) 修复了EditorSimulateMode模式下开启模拟下载tag不生效的问题。
- (#667) 修复了所有编辑器窗口针对中文IME的输入问题。
- (#670) 修复了Catalog文件生成过程中白名单未考虑自定义清单前缀名。

### Improvements

- (#650) 解决互相依赖的资源包无法卸载的问题。需要开启宏定义：YOOASSET_EXPERIMENTAL
- (#655) 优化了初始化的时候，缓存文件搜索效率。安卓平台性能提升1倍，IOS平台性能提升3倍。

### Added

- (#643) 新增构建参数，可以节省资源清单运行时内存

  ```csharp
  class ScriptableBuildParameters
  {
      /// <summary>
      /// 使用可寻址地址代替资源路径
      /// 说明：开启此项可以节省运行时清单占用的内存！
      /// </summary>
      public bool ReplaceAssetPathWithAddress = false;
  }
  ```

- (#648) 新增初始化参数，可以自动释放引用计数为零的资源包

  ```csharp
  class InitializeParameters
  {
      /// <summary>
      /// 当资源引用计数为零的时候自动释放资源包
      /// </summary>
      public bool AutoUnloadBundleWhenUnused = false;
  }
  ```

### Changed

- 程序集宏定义代码转移到扩展工程。参考MacroSupport文件夹。

## [2.3.16] - 2025-09-17

### Improvements

- (#638) 优化了Provider加载机制，引用计数为零时自动挂起！

### Fixed

- (#644) [**严重**] 修复了2.3.15版本，资产量巨大的情况下，编辑器下模拟模式初始化耗时很久的问题。

### Added

- (#639) 新增了文件系统参数：VIRTUAL_DOWNLOAD_MODE 和 VIRTUAL_DOWNLOAD_SPEED 

  编辑器下不需要构建AB，也可以模拟远端资源下载，等同真机运行环境。

  ```csharp
  class DefaultEditorFIleSystem
  {
      /// <summary>
      /// 模拟虚拟下载模式
      /// </summary>
      public bool VirtualDownloadMode { private set; get; } = false;
  
      /// <summary>
      /// 模拟虚拟下载的网速（单位：字节）
      /// </summary>
      public int VirtualDownloadSpeed { private set; get; } = 1024;
  }
  ```

- (#640) 新增了文件系统参数：VIRTUAL_WEBGL_MODE 

  编辑器下不需要构建AB，也可以模拟小游戏开发环境，等同真机运行环境。

  ```csharp
  class DefaultEditorFIleSystem
  {
      /// <summary>
      /// 模拟WebGL平台模式
      /// </summary>
      public bool VirtualWebGLMode { private set; get; } = false;
  }
  ```

- (#642) 新增了文件系统参数：DOWNLOAD_WATCH_DOG_TIME

  监控时间范围内，如果没有接收到任何下载数据，那么直接终止任务！

  ```csharp
  class DefaultCacheFIleSystem
  {
      /// <summary>
      /// 自定义参数：下载任务的看门狗机制监控时间
      /// </summary>
      public int DownloadWatchDogTime { private set; get; } = int.MaxValue;
  }
  ```

### Changed

- 下载器参数timeout移除。

  可以使用文件系统的看门狗机制代替。

- (#632) IFilterRule接口变动。

  收集器可以指定搜寻的资源类型，在收集目录资产量巨大的情况下，可以极大加快打包速度！

  ```csharp
  public interface IFilterRule
  {
      /// <summary>
      /// 搜寻的资源类型
      /// 说明：使用引擎方法搜索获取所有资源列表
      /// </summary>
      string FindAssetType { get; } 
  }
  ```

  

## [2.3.15] - 2025-09-09

**重要**：升级了资源清单版本，不兼容老版本。建议重新提审安装包。

### Improvements

- 重构了UniTask扩展库的目录结构和说明文档。
- 重构了内置文件系统类的加载和拷贝逻辑，解决在一些特殊机型上遇到的偶发性拷贝失败问题。
- 增加了生成内置清单文件的窗口工具，详情见扩展工程里CreateBuildinCatalog目录。
- 优化了异步操作系统的繁忙检测机制。
- (#621) 资源配置页面可以展示DependCollector和StaticCollector包含的文件列表内容。
- (#627) 优化了资源清单部分字段类型，CRC字段从字符串类型调整为整形，可以降低清单尺寸。

### Fixed

- 修复了构建页面扩展类缺少指定属性报错的问题。
- (#611)  修复了资源扫描器配置页面，修改备注信息后会丢失焦点的问题。
- (#622)  修复了纯鸿蒙系统读取内置加密文件失败的问题。
- (#620)  修复了LINUX系统URL地址转换失败的问题。
- (#631)  修复了NET 4.x程序集库Math.Clamp导致的编译错误。

### Added

- 新增了支持支付宝小游戏的文件系统扩展类。

- 新增了支持Taptap小游戏的文件系统扩展类。

- 新增了资源系统初始化参数：UseWeakReferenceHandle 

  目前处于预览版，可以在引擎设置页面开启宏：YOOASSET_EXPERIMENTAL

  ```csharp
  /// <summary>
  /// 启用弱引用资源句柄
  /// </summary>
  public bool UseWeakReferenceHandle = false;
  ```

- 内置文件系统和缓存文件系统新增初始化参数：FILE_VERIFY_MAX_CONCURRENCY 

  ```csharp
  /// <summary>
  /// 自定义参数：初始化的时候缓存文件校验最大并发数
  /// </summary>
  public int FileVerifyMaxConcurrency { private set; get; }
  ```

- (#623)  内置构建管线新增构建参数：StripUnityVersion 

  ```csharp
  /// <summary>
  /// 从文件头里剥离Unity版本信息
  /// </summary>
  public bool StripUnityVersion = false;
  ```

- 可编程构建管线新增构建参数：TrackSpriteAtlasDependencies

  ```csharp
  /// <summary>
  /// 自动建立资源对象对图集的依赖关系
  /// </summary>
  public bool TrackSpriteAtlasDependencies = false;
  ```

- (#617) 新增资源收集配置参数：SupportExtensionless

  在不需要模糊加载模式的前提下，关闭此选项，可以降低运行时内存大小。

  该选项默认开启！

  ```csharp
  public class CollectCommand
  {
      /// <summary>
      /// 支持无后缀名的资源定位地址
      /// </summary>
      public bool SupportExtensionless { set; get; }  
  }
  ```

- (#625) 异步操作系统类新增监听方法。

  ```csharp
  class OperationSystem
  {
      /// <summary>
      /// 监听任务开始
      /// </summary>
      public static void RegisterStartCallback(Action<string, AsyncOperationBase> callback);
          
      /// <summary>
      /// 监听任务结束
      /// </summary>
      public static void RegisterFinishCallback(Action<string, AsyncOperationBase> callback);
  }
  ```

  

## [2.3.14] - 2025-07-23

**重要**：**所有下载相关的超时参数（timeout）已更新判定逻辑**

超时不再以‘指定时间内未接收到任何数据’为判定条件，而是以‘指定时间内未完成整个下载任务’为判定条件。

### Improvements

- 重构了核心代码的下载逻辑，解决了同步加载触发的下载任务没有完成的问题。
- 扩展工程里新增了PreprocessBuildCatalog类，用于处理在构建应用程序前自动生成内置资源目录文件。
- (#592) 优化了资源清单逻辑里不必要产生的GC逻辑。

### Fixed

- (#590) 修复了TryUnloadUnusedAsset方法，在依赖嵌套层数过深导致没有卸载的问题。

### Added

- 新增了支持Google Play的文件系统扩展示例。

- 新增了支持DefaultCacheFileSystem的单元测试用例。

- 新增了文件系统配置参数：DISABLE_ONDEMAND_DOWNLOAD

  ```csharp
  public class FileSystemParametersDefine
  {
      // 禁用边玩边下机制
      public const string DISABLE_ONDEMAND_DOWNLOAD = "DISABLE_ONDEMAND_DOWNLO";
  }
  ```

### Changed

- IManifestServices接口拆分为了IManifestProcessServices和IManifestRestoreServices

  ```csharp
  public interface IManifestProcessServices
  {
      /// <summary>
      /// 处理资源清单（压缩或加密）
      /// </summary>
      byte[] ProcessManifest(byte[] fileData);
  }
  
  public interface IManifestRestoreServices
  {
      /// <summary>
      /// 还原资源清单（解压或解密）
      /// </summary>
      byte[] RestoreManifest(byte[] fileData);
  }
  ```

## [2.3.12] - 2025-07-01

### Improvements

- 优化了同步接口导致的资源拷贝和资源验证性能开销高的现象。
- 微信小游戏和抖音小游戏支持资源清单加密。

### Fixed

- (#579) 修复了2.3.10版本资源包构建页面里CopyBuildinFileParam无法编辑问题。
- (#572) 修复了资源收集页面指定收集的预制体名称变动的问题。
- (#582) 修复了非递归收集依赖时，依赖列表中才包含主资源的问题。

### Added

- 新增初始化参数：WebGLForceSyncLoadAsset

  ```csharp
  public abstract class InitializeParameters
  {
      /// <summary>
      /// WebGL平台强制同步加载资源对象
      /// </summary>Add commentMore actions
      public bool WebGLForceSyncLoadAsset = false;
  }
  ```

- (#576) 新增了资源清单服务类：IManifestServices

  ```csharp
  /// <summary>
  /// 资源清单文件处理服务接口
  /// </summary>
  public interface IManifestServices
  {
      /// <summary>
      /// 处理资源清单（压缩和加密）
      /// </summary>
      byte[] ProcessManifest(byte[] fileData);
          
      /// <summary>
      /// 还原资源清单（解压和解密）
      /// </summary>
      byte[] RestoreManifest(byte[] fileData);
  } 
  ```

- (#585) 新增了本地文件拷贝服务类：ICopyLocalFileServices

  ```csharp
  /// <summary>
  /// 本地文件拷贝服务类
  /// </summary>
  public interface ICopyLocalFileServices
  {
      void CopyFile(LocalFileInfo sourceFileInfo, string destFilePath);
  }
  ```

## [2.3.10] - 2025-06-17

### Improvements

- 小游戏扩展库已经独立，可以单独导入到项目工程。
- 编辑器里的TableView视图新增了AssetObjectCell类。
- (#552) 微信小游戏文件系统类，增加了URL合法性的初始化检测机制。
- (#566) 重构了资源构建页面，方便扩展自定义界面。
- (#573) 完善了AssetDependencyDB的输出日志，可以正确输出丢失的引用资产信息。

### Fixed

- 修复太空战机DEMO在退出运行模式时的报错。
- (#551) 修复了Unity2019, Unity2020的代码兼容性报错。
- (#569) 修复了TVOS平台的兼容问题。  
- (#564) 修复了TiktokFileSystem文件系统里appendTimeTicks无效的问题。

### Added

- (#562) 新增了解密方法。

  ```csharp
  public interface IDecryptionServices
  {
      /// <summary>
      /// 后备方式获取解密的资源包对象
      /// 注意：当正常解密方法失败后，会触发后备加载！
      /// 说明：建议通过LoadFromMemory()方法加载资源对象作为保底机制。
      /// issues : https://github.com/tuyoogame/YooAsset/issues/562
      /// </summary>
      DecryptResult LoadAssetBundleFallback(DecryptFileInfo fileInfo);    
  }
  ```



## [2.3.9] - 2025-05-13

### Improvements

- 增加了YOO_ASSET_EXPERIMENT宏，用于控制实验性代码的开关。
- 构建管线目前会输出构建日志到输出目录下，方便查看引擎在构建时主动清空的控制台日志。
- 优化了收集器tag传染扩散逻辑，避免Group里配置了Tag导致的无意义的警告信息。
- 扩展工程内PanelMonitor代码默认关闭状态。

### Fixed

- (#528) 修复了AssetDependencyDatabase在查询引擎资源对象是否存在的时效问题。

### Added

- (#542) 新增了资源管理系统销毁方法。

  该方法会销毁所有的资源包裹和异步操作任务，以及卸载所有AssetBundle对象！

  ```csharp
  public class YooAssets
  {
      /// <summary>
      /// 销毁资源系统
      /// </summary>
      public static void Destroy();
  }
  ```

- 新增了SBP构建管线的构建参数

  ```csharp
  /// <summary>
  /// 从AssetBundle文件头里剥离Unity版本信息
  /// </summary>
  public bool StripUnityVersion = false;
  ```

- 新增了构建错误码：BuiltinShadersBundleNameIsNull 

## [2.3.8] - 2025-04-17

### Improvements

- 扩展工程里增加了“图集丢失变白块的解决方案”的相关代码。

### Fixed

- (#528) 修复了微信小游戏平台WXFSClearUnusedBundleFiles无法清理的问题。
- (#531) 修复了微信小游戏平台WXFSClearUnusedBundleFiles没有适配BundleName_HashName命名方式。
- (#533) 修复了Editor程序集下无法访问YooAsset.Editor程序集里的internal字段的问题。
- (#534) 修复了资源报告窗口AssetView视图里，依赖资源包列表显示不准确的问题。

## [2.3.7] - 2025-04-01

### Improvements

- (#526) 运行时资源清单的哈希值验证兼容了MD5和CRC32两种方式。
- (#515) 优化了资源路径大小写不敏感的逻辑代码，减少字符串操作产生的GC。
- (#523) UnloadUnusedAssetsOperation方法支持了分帧处理。

### Fixed

- (#520) 修复了UWP平台获取WWW加载路径未适配的问题。

### Added

- 新增了文件系统初始化参数：INSTALL_CLEAR_MODE

  ```csharp
  /// <summary>
  /// 覆盖安装清理模式
  /// </summary>
  public enum EOverwriteInstallClearMode
  {
      /// <summary>
      /// 不做任何处理
      /// </summary>
      None = 0,
   
      /// <summary>
      /// 清理所有缓存文件（包含资源文件和清单文件）
      /// </summary>
      ClearAllCacheFiles = 1,
   
      /// <summary>
      /// 清理所有缓存的资源文件
      /// </summary>
      ClearAllBundleFiles = 2,
   
      /// <summary>
      /// 清理所有缓存的清单文件
      /// </summary>
      ClearAllManifestFiles = 3,
  }
  ```

- 新增了初始化参数：BundleLoadingMaxConcurrency

  ```csharp
  public abstract class InitializeParameters
  {
      /// <summary>
      /// 同时加载Bundle文件的最大并发数
      /// </summary>
      public int BundleLoadingMaxConcurrency = int.MaxValue;
  }
  ```

## [2.3.6] - 2025-03-25

### Improvements

- 构建管线新增了TaskCreateCatalog任务节点。
- 内置文件系统的catalog文件现在存储在streammingAssets目录下。

### Fixed

- (#486) 修复了微信小游戏文件系统调用ClearUnusedBundleFiles时候的异常。

## [2.3.5-preview] - 2025-03-14

### Fixed

- (#502) 修复了原生缓存文件由于文件格式变动导致的加载本地缓存文件失败的问题。
- (#504) 修复了MacOS平台Offline Play Mode模式请求本地资源清单失败的问题。
- (#506) 修复了v2.3x版本LoadAllAssets方法计算依赖Bundle不完整的问题。
- (#506) 修复了微信小游戏文件系统，在启用加密算法后卸载bundle报错的问题。

## [2.3.4-preview] - 2025-03-08

### Improvements

- YooAsset支持了版本宏定义。

  ```csharp
  YOO_ASSET_2
  YOO_ASSET_2_3
  YOO_ASSET_2_3_OR_NEWER
  ```

### Fixed

- (#389) 修复了禁用域重载(Reload Domain)的情况下，再次启动游戏报错的问题。
- (#496) 修复了文件系统参数RESUME_DOWNLOAD_MINMUM_SIZE传入int值会导致异常的错误。
- (#498) 修复了v2.3版本尝试加载安卓包内的原生资源包失败的问题。

### Added

- 新增了YooAssets.GetAllPackages()方法

  ```csharp
  /// <summary>
  /// 获取所有资源包裹
  /// </summary>
  public static List<ResourcePackage> GetAllPackages()
  ```

## [2.3.3-preview] - 2025-03-06

### Improvements

- 新增了异步操作任务调试器，AssetBundleDebugger窗口-->OperationView视图模式
- 编辑器下模拟构建默认启用依赖关系数据库，可以大幅降低编辑器下启动游戏的时间。
- 单元测试用例增加加密解密测试用例。

### Fixed

- (#492) 修复了发布的MAC平台应用，在启动的时候提示权限无法获取的问题。

## [2.3.2-preview] - 2025-02-27

### Fixed

- (2.3.1) 修复小游戏平台下载器不生效的问题。
- (#480) 修复了Unity工程打包导出时的报错。

### Added

- 下载器新增参数：recursiveDownload

  ```csharp
  /// <summary>
  /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
  /// </summary>
  /// <param name="recursiveDownload">下载资源对象所属资源包内所有资源对象依赖的资源包
  public ResourceDownloaderOperation CreateBundleDownloader()
  ```

- 新增CustomPlayMode模式

  ```csharp
  /// <summary>
  /// 自定义运行模式的初始化参数
  /// </summary>
  public class CustomPlayModeParameters : InitializeParameters
  {
      /// <summary>
      /// 文件系统初始化参数列表
      /// 注意：列表最后一个元素作为主文件系统！
      /// </summary>
      public List<FileSystemParameters> FileSystemParameterList;
  }
  ```

## [2.3.1-preview] - 2025-02-25

**资源加载依赖计算方式还原为了1.5x版本的模式，只加载资源对象实际依赖的资源包，不再以资源对象所在资源包的依赖关系为加载标准**。

### Improvements

- 优化OperationSystem的更新机制，异步加载的耗时降低了50%。
- 优化了Debugger窗口的显示页面，BundleView页面增加资源包的引用列表。
- 优化了Reporter窗口的显示页面。

### Fixed

- 修复了怀旧依赖模式下，TAG传染不正确的问题。

## [2.3.0-preview] - 2025-02-19

### Improvements

资源收集窗口列表元素支持手动上下拖拽排序！

资源扫描窗口列表元素支持手动上下拖拽排序！

### Added

- 新增了UIElements扩展类ReorderableListView

- 新增初始化方法

  ```csharp
  public class YooAssets
  {
      /// <summary>
      /// 设置异步系统参数，快速启动模式的开关
      /// 注意：该模式默认开启
      /// </summary>
      public static void SetOperationSystemQuickStartMode(bool state)
  }
  ```

- 新增打包构建参数

  ```csharp
  public class BuildParameters
  {
      /// <summary>
      /// 旧版依赖模式
      /// 说明：兼容YooAssets1.5.x版本
      /// </summary>
      public bool LegacyDependency = false;    
  }
  ```

### Fixed

- (#472) 修复了Unity6平台，TableView视图无法显示问题。
- 修复了微信小游戏和抖音小游戏未正确使用插件的卸载方法。

## [2.2.12] - 2025-02-14

### Improvements

- WebGL网页平台支持文件加密。
- 微信小游戏平台支持文件加密。
- 抖音小游戏平台支持文件加密。

### Fixed

- (#466) 修复了微信小游戏文件系统查询机制不生效！
- (#341) 修复了微信小游戏的下载进度异常问题。
- (#471) 修复了Unity2019,Unity2020平台上，TableView视图无法显示的问题。

### Added

- 新增了ResourcePackage.UnloadAllAssetsAsync(UnloadAllAssetsOptions options)方法

  ```csharp
  public sealed class UnloadAllAssetsOptions
  {
      /// <summary>
      /// 释放所有资源句柄，防止卸载过程中触发完成回调！
      /// </summary>
      public bool ReleaseAllHandles = true;
       
      /// <summary>
      /// 卸载过程中锁定加载操作，防止新的任务请求！
      /// </summary>
      public bool LockLoadOperation = true;
  }
  ```

## [2.2.11] - 2025-02-10

### Improvements

- AssetArtScanner配置和生成报告的容错性检测。

### Fixed

- (#465) 修复了特殊情况下，没有配置资源包文件后缀名构建失败的问题。
- (#468) 修复了安卓平台二次启动加载原生文件或加密文件失败的问题。

## [2.2.10] - 2025-02-08

### Improvements

- 新增了可扩展的AssetArtScanner资源扫描工具，详细请见官方说明文档。
- 优化了AssetBundleReporter页面。
- 优化了AssetBundleDebugger页面。
- 优化了微信小游戏文件系统的缓存查询机制。
- 优化了抖音小游戏文件系统的缓存查询机制。

### Fixed

- (#447) 修复了Unity2019平台代码编译错误问题。
- (#456) 修复了在Package未激活有效清单之前，无法销毁的问题。
- (#452) 修复了内置文件系统类NeedPack方法总是返回TRUE的问题。
- (#424) 适配了Unity6000版本替换了过时方法。

### Added

- 新增了SBP构建管线构建参数：BuiltinShadersBundleName

- 新增了SBP构建管线构建参数：MonoScriptsBundleName

- 新增了全局构建管线构建参数：SingleReferencedPackAlone

  ```csharp
  /// <summary>
  /// 对单独引用的共享资源进行独立打包
  /// 说明：关闭该选项单独引用的共享资源将会构建到引用它的资源包内！
  /// </summary>
  public bool SingleReferencedPackAlone = true;
  ```

- 新增了内置文件系统初始化参数：COPY_BUILDIN_PACKAGE_MANIFEST

  ```csharp
  // 内置文件系统初始化的时候，自动拷贝内置清单到沙盒目录。
  var systemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
  systemParameters.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);
  ```

## [2.2.9] - 2025-01-14

### Fixed

- (#438) 修复了纯血鸿蒙加载本地文件失败的问题。
- (#445) 修复了小游戏扩展文件系统脚本编译错误。

### Changed

- EditorSimulateModeHelper.SimulateBuild()方法变更

  ```csharp
  public static PackageInvokeBuildResult SimulateBuild(string packageName);
  ```

## [2.2.8-preview] - 2025-01-03

新增了单元测试用例。

### Improvements

- EditorSimulateModeHelper.SimulateBuild()方法提供指定自定义构建类

  ```csharp
  public class EditorSimulateBuildParam
  {
      /// <summary>
      /// 模拟构建类所属程序集名称
      /// </summary>
      public string InvokeAssmeblyName = "YooAsset.Editor";
  
      /// <summary>
      /// 模拟构建执行的类名全称       
      /// 注意：类名必须包含命名空间！  
      /// </summary>    
      public string InvokeClassFullName = "YooAsset.Editor.AssetBundleSimulateBuilder";
  
      /// <summary>     
      /// 模拟构建执行的方法名称    
      /// 注意：执行方法必须满足 BindingFlags.Public | BindingFlags.Static      
      /// </summary>       
      public string InvokeMethodName = "SimulateBuild";
  }
  ```

- 文件清理方式新增清理缓存清单。

  ```csharp
  /// <summary>
  /// 文件清理方式
  /// </summary>
  public enum EFileClearMode
  {
      /// <summary>
      /// 清理所有清单
      /// </summary>
      ClearAllManifestFiles,
  
      /// <summary>
      /// 清理未在使用的清单 
      /// </summary> 
      ClearUnusedManifestFiles,    
  }
  ```

### Fixed

- (#426) 修复了鸿蒙next平台加载内置文件路径报错的问题。
- (#428) 修复了鸿蒙next平台加载内置文件路径报错的问题。
- (#434) 修复了2.2版本 catalog文件对Json格式原生文件不记录的问题。
- (#435) 修复了WebGL平台调用MD5算法触发异常的问题。

### Added

- 新增了视频打包规则。

  ```csharp
  /// <summary>
  /// 打包视频文件
  /// </summary>
  [DisplayName("打包视频文件")]
  public class PackVideoFile : IPackRule
  ```

### Changed

- 重命名FileSystemParameters.RootDirectory字段为PackageRoot
- 重命名ResourcePackage.ClearCacheBundleFilesAsync()方法为ClearCacheFilesAsync()

## [2.2.7-preview] - 2024-12-30

### Improvements

- 重构了下载器的委托方法。

- YooAssetSettings配置文件新增Package Manifest Prefix参数。

  ```csharp
  /// <summary>
  /// 资源清单前缀名称（默认为空)
  /// </summary>
  public string PackageManifestPrefix = string.Empty;
  ```

### Fixed

- (#422) 修复了同步加载场景的NotImplementedException异常报错。
- (#418) 修复了web远程文件系统初始化不正确的问题
- (#392) 修复了引擎版本代码兼容相关的警告。
- (#332) 修复了当用户的设备中有特殊字符时，URL路径无法被正确识别的问题。

### Added

- 新增代码字段：AsyncOperationBase.PackageName

### Changed

- 重命名DownloaderOperation.OnDownloadOver()方法为DownloaderFinish()
- 重命名DownloaderOperation.OnDownloadProgress()方法为DownloadUpdate()
- 重命名DownloaderOperation.OnDownloadError()方法为DownloadError()
- 重命名DownloaderOperation.OnStartDownloadFile()方法为DownloadFileBegin()

## [2.2.6-preview] - 2024-12-27

### Improvements

- 增强了对Steam平台DLC拓展包的支持。

  ```csharp
  // 新增参数关闭Catalog目录查询内置文件的功能
  var fileSystemParams = CreateDefaultBuildinFileSystemParameters();
  fileSystemParams .AddParameter(FileSystemParametersDefine.DISABLE_CATALOG_FILE, true);
  ```

- 资源句柄基类提供了统一的Release方法。

  ```csharp
  public abstract class HandleBase : IEnumerator, IDisposable
  {
      /// <summary>
      /// 释放资源句柄
      /// </summary>
      public void Release();
  
      /// <summary>
      /// 释放资源句柄
      /// </summary>
      public void Dispose();
  }
  ```

- 优化了场景卸载逻辑。

  ```csharp
  //框架内不在区分主场景和附加场景。
  //场景卸载后自动释放资源句柄。
  ```

### Fixed

- 修复了Unity2020版本提示的脚本编译错误。
- (#417) 修复了DefaultWebServerFileSystem文件系统内Catalog未起效的问题。

### Added

- 新增示例文件 GetCacheBundleSizeOperation.cs

  可以获取指定Package的缓存资源总大小。

### Removed

- 移除了SceneHandle.IsMainScene()方法。

## [2.2.5-preview] - 2024-12-25

依赖的ScriptableBuildPipeline (SBP) 插件库版本切换为1.21.25版本！

重构了ResourceManager相关的核心代码，方便借助文件系统扩展和支持更复杂的需求！

### Editor

- 新增了编辑器模拟构建管线 EditorSimulateBuildPipeline
- 移除了EBuildMode枚举类型，构建界面有变动。
- IActiveRule分组激活接口新增GroupData类。

### Improvements

- 增加抖音小游戏文件系统，见扩展示例代码。

- 微信小游戏文件系统支持删除无用缓存文件和全部缓存文件。

- 资源构建管线现在默认剔除了Gizmos和编辑器资源。

- 优化了资源构建管线里资源收集速度。

  资源收集速度提升100倍！

  ```csharp
  class BuildParameters
  {
      /// <summary>
      /// 使用资源依赖缓存数据库
      /// 说明：开启此项可以极大提高资源收集速度
      /// </summary>
      public bool UseAssetDependencyDB = false;
  }
  ```

- WebPlayMode支持跨域加载。

  ```csharp
  // 创建默认的WebServer文件系统参数
  public static FileSystemParameters CreateDefaultWebServerFileSystemParameters(bool disableUnityWebCache = false)
  
  // 创建默认的WebRemote文件系统参数（支持跨域加载）
  public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteServices remoteServices, bool disableUnityWebCache = false)
  ```

- 编辑器模拟文件系统新增初始化参数：支持异步模拟加载帧数。

  ```csharp
  /// <summary>
  /// 异步模拟加载最小帧数
  /// </summary>
  FileSystemParametersDefine.ASYNC_SIMULATE_MIN_FRAME
  
  /// <summary>
  /// 异步模拟加载最大帧数
  /// </summary>
  FileSystemParametersDefine.ASYNC_SIMULATE_MAX_FRAME
  ```

- 缓存文件系统新增初始化参数：支持设置下载器最大并发连接数和单帧最大请求数

  ```csharp
  var fileSystremParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters();
  fileSystremParams .AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY, 99);
  fileSystremParams .AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_REQUEST_PER_FRAME, 10);
  ```

### Fixed

- (#349) 修复了在加载清单的时候，即使本地存在缓存文件还会去远端下载。
- (#361) 修复了协程里等待的asset handle被release，会无限等待并输出警告信息。
- (#359) 修复了SubAssetsHandle.GetSubAssetObject会获取到同名的主资源。
- (#387) 修复了加密后文件哈希冲突的时候没有抛出异常错误。
- (#404) 修复了Unity2022.3.8版本提示编译错误：Cannot resolve symbol 'AsyncInstantiateOperation' 

### Added

- 新增示例文件 CopyBuildinManifestOperation.cs

- 新增示例文件 LoadGameObjectOperation.cs

- 新增了获取配置清单详情的方法

  ```csharp
  class ResourcePackage
  {
     public PackageDetails GetPackageDetails() 
  }
  ```

- 新增了获取所有资源信息的方法

  ```csharp
  class ResourcePackage
  {
      public AssetInfo[] GetAllAssetInfos() 
  }
  ```

- 新增了清理缓存文件的通用方法

  ```csharp
  /// <summary>
  /// 文件清理方式
  /// </summary>
  public enum EFileClearMode
  {
      /// <summary>
      /// 清理所有文件
      /// </summary>
      ClearAllBundleFiles = 1,
      /// <summary>
      /// 清理未在使用的文件
      /// </summary>
      ClearUnusedBundleFiles = 2,
      /// <summary>   
      /// 清理指定标签的文件   
      /// 说明：需要指定参数，可选：string, string[], List<string>   
      /// </summary>   
      ClearBundleFilesByTags = 3,
  }
  class ResourcePackage
  {
      /// <summary>
      /// 清理缓存文件
      /// </summary>
      /// <param name="clearMode">清理方式</param>
      /// <param name="clearParam">执行参数</param>
      public ClearCacheBundleFilesOperation ClearCacheBundleFilesAsync(EFileClearMode clearMode, object clearParam = null)
  }
  ```

### Changed

- 修改了EditorSimulateModeHelper.SimulateBuild()方法

- 重命名ResourcePackage.GetAssetsInfoByTags()方法为GetAssetInfosByTags()

- 实例化对象方法增加激活参数。

  ```csharp
  public InstantiateOperation InstantiateAsync(bool actived = true)
  ```

- 清单文件的版本提升到2.2.5版本

  ```csharp
  /// <summary>
  /// 资源包裹的备注信息
  /// </summary>
  public string PackageNote;
  ```
  

### Removed

- 移除了HostPlayModeParameters.DeliveryFileSystemParameters字段
- 移除了ResourcePackage.ClearAllBundleFilesAsync()方法
- 移除了ResourcePackage.ClearUnusedBundleFilesAsync()方法
- 移除了FileSystemParameters.CreateDefaultBuildinRawFileSystemParameters()方法
- 移除了FileSystemParameters.CreateDefaultCacheRawFileSystemParameters()方法
- 移除了枚举类型：EDefaultBuildPipeline
- 移除了配置参数：YooAssetSettings.ManifestFileName

## [2.2.4-preview] - 2024-08-15

### Fixed

- 修复了HostPlayMode初始化卡死的问题。

## [2.2.3-preview] - 2024-08-13

### Fixed

- (#311) 修复了断点续传下载器极小概率报错 : “416 Range Not Satisfiable”

### Improvements

- 原生文件构建管线支持原生文件加密。

- HostPlayMode模式下内置文件系统初始化参数可以为空。

- 场景加载增加了LocalPhysicsMode参数来控制物理运行模式。

- 默认的内置文件系统和缓存文件系统增加解密方法。

  ```csharp
  /// <summary>
  /// 创建默认的内置文件系统参数
  /// </summary>
  /// <param name="decryptionServices">加密文件解密服务类</param>
  /// <param name="verifyLevel">缓存文件的校验等级</param>
  /// <param name="rootDirectory">内置文件的根路径</param>
  public static FileSystemParameters CreateDefaultBuildinFileSystemParameters(IDecryptionServices decryptionServices, EFileVerifyLevel verifyLevel, string rootDirectory);
  
  /// <summary>
  /// 创建默认的缓存文件系统参数
  /// </summary>
  /// <param name="remoteServices">远端资源地址查询服务类</param>
  /// <param name="decryptionServices">加密文件解密服务类</param>
  /// <param name="verifyLevel">缓存文件的校验等级</param>
  /// <param name="rootDirectory">文件系统的根目录</param>
  public static FileSystemParameters CreateDefaultCacheFileSystemParameters(IRemoteServices remoteServices, IDecryptionServices decryptionServices, EFileVerifyLevel verifyLevel, string rootDirectory);
  ```

## [2.2.2-preview] - 2024-07-31

### Fixed

- (#321) 修复了在Unity2022里编辑器下离线模式运行失败的问题。
- (#325) 修复了在Unity2019里编译报错问题。

## [2.2.1-preview] - 2024-07-10

统一了所有PlayMode的初始化逻辑，EditorSimulateMode和OfflinePlayMode初始化不再主动加载资源清单！

### Added

- 新增了IFileSystem.ReadFileData方法，支持原生文件自定义获取文本和二进制数据。

### Improvements

- 优化了DefaultWebFileSystem和DefaultBuildFileSystem文件系统的内部初始化逻辑。

## [2.2.0-preview] - 2024-07-07

重构了运行时代码，新增了文件系统接口（IFileSystem）方便开发者扩展特殊需求。

新增微信小游戏文件系统示例代码，详细见Extension Sample/Runtime/WechatFileSystem

### Added

- 新增了ResourcePackage.DestroyAsync方法

- 新增了FileSystemParameters类帮助初始化文件系统

  内置了编辑器文件系统参数，内置文件系统参数，缓存文件系统参数，Web文件系统参数。

  ```csharp
  public class FileSystemParameters
  {
      /// <summary>
      /// 文件系统类
      /// </summary>
      public string FileSystemClass { private set; get; }
      
      /// <summary>
      /// 文件系统的根目录
      /// </summary>
      public string RootDirectory { private set; get; }   
      
      /// <summary>
      /// 添加自定义参数
      /// </summary>
      public void AddParameter(string name, object value)    
  }
  ```

### Changed

- 重构了InitializeParameters初始化参数
- 重命名YooAssets.DestroyPackage方法为RemovePackage
- 重命名ResourcePackage.UpdatePackageVersionAsync方法为RequestPackageVersionAsync
- 重命名ResourcePackage.UnloadUnusedAssets方法为UnloadUnusedAssetsAsync
- 重命名ResourcePackage.ForceUnloadAllAssets方法为UnloadAllAssetsAsync
- 重命名ResourcePackage.ClearUnusedCacheFilesAsync方法为ClearUnusedBundleFilesAsync
- 重命名ResourcePackage.ClearAllCacheFilesAsync方法为ClearAllBundleFilesAsync

### Removed

- 移除了YooAssets.Destroy方法
- 移除了YooAssets.SetDownloadSystemClearFileResponseCode方法
- 移除了YooAssets.SetCacheSystemDisableCacheOnWebGL方法
- 移除了ResourcePackage.GetPackageBuildinRootDirectory方法
- 移除了ResourcePackage.GetPackageSandboxRootDirectory方法
- 移除了ResourcePackage.ClearPackageSandbox方法
- 移除了IBuildinQueryServices接口
- 移除了IDeliveryLoadServices接口
- 移除了IDeliveryQueryServices接口


## [2.1.2] - 2024-05-16

SBP库依赖版本升级至2.1.3

### Fixed

- (#236) 修复了资源配置界面AutoCollectShader复选框没有刷新的问题。
- (#244) 修复了导入器在安卓平台导入本地下载的资源失败的问题。
- (#268) 修复了挂起场景未解除状态前无法卸载的问题。
- (#269) 优化场景挂起流程，支持中途取消挂起操作。
- (#276) 修复了HostPlayMode模式下，如果内置清单是最新版本，每次运行都会触发拷贝行为。
- (#289) 修复了Unity2019版本脚本IWebRequester编译报错。
- (#295) 解决了在安卓移动平台，华为和三星真机上有极小概率加载资源包失败 : Unable to open archive file

### Added

- 新增GetAllCacheFileInfosOperation()获取缓存文件信息的方法。

- 新增LoadSceneSync()同步加载场景的方法。

- 新增IIgnoreRule接口，资源收集流程可以自定义。

- 新增IWechatQueryServices接口，用于微信平台本地文件查询。

  后续将会通过虚拟文件系统来支持！

### Changed

- 调整了UnloadSceneOperation代码里场景的卸载顺序。

### Improvements

- 优化了资源清单的解析过程。
- 移除资源包名里的空格字符。
- 支持华为鸿蒙系统。

## [2.1.1] - 2024-01-17

### Fixed

- (#224)  修复了编辑器模式打包时 SimulateBuild 报错的问题。
- (#223)  修复了资源构建界面读取配置导致的报错问题。

### Added

- 支持共享资源打包规则，可以定制化独立的构建规则。

  ```c#
  public class BuildParameters
  {
     /// <summary>
      /// 是否启用共享资源打包
      /// </summary>
      public bool EnableSharePackRule = false; 
  }
  ```

- 微信小游戏平台，资源下载器支持底层缓存查询。

## [2.1.0] - 2023-12-27

升级了 Scriptable build pipeline (SBP) 的版本，来解决图集引用的精灵图片冗余问题。

### Fixed

- (#195) 修复了在EditorPlayMode模式下，AssetHandle.GetDownloadStatus()发生异常的问题。
- (#201) 修复了断点续传失效的问题。
- (#202) 修复了打包参数FileNameStyle设置为BundleName后，IQueryServices会一直返回true的问题。
- (#205) 修复了HybridCLR插件里创建资源下载器触发的异常。
- (#210) 修复了DownloaderOperation在未开始下载前，内部的PackageName为空的问题。
- (#220) 修复了资源收集界面关闭后，撤回操作还会生效的问题。
- 修复了下载器合并后重新计算下载字节数不正确的问题。

### Improvements

- (#198) 资源收集界面禁用的分组不再检测合法性。
- (#203) 资源构建类容许自定义打包的输出目录。
- 资源构建报告增加未依赖的资源信息列表。

### Changed

- IBuildinQueryServices和IDeliveryQueryServices查询方法变更。

  ```c#
      public interface IBuildinQueryServices
      {
          /// <summary>
          /// 查询是否为应用程序内置的资源文件
          /// </summary>
          /// <param name="packageName">包裹名称</param>
          /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
          /// <param name="fileCRC">文件哈希值</param>
          /// <returns>返回查询结果</returns>
          bool Query(string packageName, string fileName, string fileCRC);
      }
  
     	public interface IDeliveryQueryServices
      {
          /// <summary>
          /// 查询是否为开发者分发的资源文件
          /// </summary>
          /// <param name="packageName">包裹名称</param>
          /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
          /// <param name="fileCRC">文件哈希值</param>
          /// <returns>返回查询结果</returns>
          bool Query(string packageName, string fileName, string fileCRC);
      }
  ```

  

### Removed

- (#212)  移除了构建报告里的资源冗余信息列表。

