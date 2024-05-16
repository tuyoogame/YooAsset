# CHANGELOG

All notable changes to this package will be documented in this file.

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

