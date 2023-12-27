# CHANGELOG

All notable changes to this package will be documented in this file.

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

