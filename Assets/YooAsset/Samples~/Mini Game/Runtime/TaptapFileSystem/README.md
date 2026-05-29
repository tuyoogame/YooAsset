# TapTap 小游戏文件系统

该示例用于在 YooAsset 的 WebGL 运行模式下接入 TapTap 小游戏。

参考文档：[TapTap 小游戏 Unity 适配指南](https://developer.taptap.cn/minigameapidoc/dev/engine/unity-adaptation/guide/)

## 环境要求

先安装 TapTap 小游戏 Unity/团结 WebGL 适配 SDK，并将项目切换到 WebGL 构建目标。

在 WebGL Player 的 Scripting Define Symbols 中启用以下宏：

- `TAPMINIGAME`

该宏是 YooAsset TapTap 小游戏示例约定的编译开关，用于和其它小游戏平台适配代码保持一致。

如果启用宏后编译提示找不到 `TapTapMiniGame`、`TapAssetBundle` 或 `DownloadHandlerTapAssetBundle`，请确认 TapTap SDK 已导入，并将 TapTap SDK 的程序集引用添加到 `YooAsset.MiniGame.asmdef`。

## 初始化 YooAsset

在 TapTap 小游戏构建中初始化 `WebPlayModeOptions` 时，使用 `TaptapFileSystemCreater` 创建文件系统参数。

```csharp
#if UNITY_WEBGL && TAPMINIGAME && !UNITY_EDITOR
var createParameters = new WebPlayModeOptions();

string defaultHostServer = GetHostServerURL();
string fallbackHostServer = GetHostServerURL();
IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);

createParameters.WebServerFileSystemParameters =
    TaptapFileSystemCreater.CreateFileSystemParameters(remoteService);

var initializationOperation = package.InitializePackageAsync(createParameters);
#endif
```

TapTap 小游戏底层会对远程 AssetBundle 请求做平台适配，业务侧仍然按照远程异步加载流程使用 YooAsset。

## 资源包命名

TapTap 小游戏构建推荐让资源包文件名携带 hash。YooAsset 推荐只使用 `HashName` 文件命名风格。

`HashName` 会生成纯 hash 文件名，例如：

```text
8d265a9dfd6cb7669cdb8b726f0afb1e.bundle
```

该命名方式更适合小游戏平台的缓存和更新识别，也能避免暴露原始 Bundle 名称。TapTap 小游戏构建不建议使用 `BundleName` 或 `BundleName_HashName`。

## 注意事项

加密 AssetBundle 仍然会走 YooAsset 常规的 Web 下载和解密流程。非加密 AssetBundle 会使用 TapTap 平台适配器，并通过 `TapAssetBundle.GetAssetBundle` 发起请求。
