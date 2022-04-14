# CHANGELOG

All notable changes to this package will be documented in this file.   

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

