![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/LOGO.png)

# YooAsset

[![License](https://img.shields.io/github/license/tuyoogame/YooAsset)](https://github.com/tuyoogame/YooAsset/blob/master/LICENSE)[![openupm](https://img.shields.io/npm/v/com.tuyoogame.yooasset?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.cn/packages/com.tuyoogame.yooasset/)

**YooAsset**是一套用于Unity3D的资源管理系统，用于帮助研发团队快速部署和交付游戏。

它可以满足商业化游戏的各类需求，并且经历多款百万DAU游戏产品的验证。

## 系统特点
- **安全高效的分包方案**

  基于资源标签的分包方案，自动对依赖资源包进行分类，避免人工维护成本。可以非常方便的实现零资源安装包，或者全量资源安装包。

- **强大灵活的打包系统**

  可以自定义打包策略，自动分析依赖实现资源零冗余，基于资源对象的资源包依赖管理方案，天然的避免了资源包之间循环依赖的问题。

- **基于引用计数方案**

  基于引用计数的管理方案，可以帮助我们实现安全的资源卸载策略，更好的对内存管理，避免资源对象冗余。还有强大的分析器可帮助发现潜在的资源泄漏问题。

- **多种模式自由切换**

  编辑器模拟模式，单机运行模式，联机运行模式。在编辑器模拟模式下，可以不构建资源包来模拟真实环境，在不修改任何代码的情况下，可以自由切换到其它模式。

- **强大安全的加载系统**

  - **异步加载** 支持协程，Task，委托等多种异步加载方式。
  - **同步加载** 支持同步加载和异步加载混合使用。
  - **边玩边下载** 在加载资源对象的时候，如果资源对象依赖的资源包在本地不存在，会自动从服务器下载到本地，然后再加载资源对象。
  - **多线程下载** 支持断点续传，自动验证下载文件，自动修复损坏文件。
  - **多功能下载器** 可以按照资源分类标签创建下载器，也可以按照资源对象创建下载器。可以设置同时下载文件数的限制，设置下载失败重试次数，设置下载超时判定时间。多个下载器同时下载不用担心文件重复下载问题，下载器还提供了下载进度以及下载失败等常用接口。
  
- **原生格式文件管理**

  无缝衔接资源打包系统，可以很方便的实现原生文件的版本管理和下载。
  
- **可寻址资源定位**

  默认支持相对路径的资源定位，也支持可寻址资源定位，不需要繁琐的过程即可高效的配置寻址路径。

- **灵活多变的版本管理**

  支持线上版本快速回退，支持区分审核版本，测试版本，线上版本，支持灰度更新及测试。

## 入门教程
1. [快速开始](https://github.com/tuyoogame/YooAsset/blob/master/Docs/QuickStart.md)
2. [全局配置](https://github.com/tuyoogame/YooAsset/blob/master/Docs/Settings.md)
3. [资源配置](https://github.com/tuyoogame/YooAsset/blob/master/Docs/AssetCollector.md)
4. [资源打包](https://github.com/tuyoogame/YooAsset/blob/master/Docs/AssetBuilder.md)
5. [资源部署](https://github.com/tuyoogame/YooAsset/blob/master/Docs/AssetDeploy.md)
5. [构建报告](https://github.com/tuyoogame/YooAsset/blob/master/Docs/AssetReporter.md)
5. [调试器](https://github.com/tuyoogame/YooAsset/blob/master/Docs/AssetDebugger.md)
5. [常见问题](https://github.com/tuyoogame/YooAsset/blob/master/Docs/FAQ.md)

## 代码教程
1. [初始化](https://github.com/tuyoogame/YooAsset/blob/master/Docs/CodeTutorial1.md)
2. [资源更新](https://github.com/tuyoogame/YooAsset/blob/master/Docs/CodeTutorial2.md)
3. [资源加载](https://github.com/tuyoogame/YooAsset/blob/master/Docs/CodeTutorial3.md)

## 视频教程

[B站视频教程](https://space.bilibili.com/328590743/channel/seriesdetail?sid=2207858)

## 社区

QQ群：**963240451**

[致谢名单](https://github.com/tuyoogame/YooAsset/blob/master/Docs/Contributor.md)👯

[代码贡献](https://github.com/tuyoogame/YooAsset/blob/master/Docs/CodeStyle.md)

## 友情链接
[ET Framework](https://github.com/wqaetly/ET/tree/et7_fgui_yooasset_luban_huatuo): ET 7.0 + FGUI + luban + huatuo + YooAsset + NKGMoba + UniTask，并提供常用的编辑器工具。  
[YooAssetEx](https://gitee.com/liu_zhongxiu/yoo-asset-ex/tree/master): YooAsset Odin扩展工具，用于支持Unity2017和Unity2018版本。

