# 支付宝小游戏文件系统

该示例用于在 YooAsset 的 WebGL 运行模式下接入支付宝小游戏。

参考文档：[支付宝小游戏开发文档](https://opendocs.alipay.com/mini-game/)

## 环境要求

先安装支付宝小游戏 Unity/团结 WebGL 适配 SDK，并将项目切换到 WebGL 构建目标。

在 WebGL Player 的 Scripting Define Symbols 中启用以下宏：

- `UNITY_ALIMINIGAME`

该宏是 YooAsset 支付宝小游戏示例约定的编译开关，用于和其它小游戏平台适配代码保持一致。

如果启用宏后编译提示找不到 `AlipaySdk`、`APAssetBundle` 或 `DownloadHandlerAPAssetBundle`，请确认支付宝 SDK 已导入，并将支付宝 SDK 的程序集引用添加到 `YooAsset.MiniGame.asmdef`。

## 初始化 YooAsset

在支付宝小游戏构建中初始化 `WebPlayModeOptions` 时，使用 `AlipayFileSystemCreater` 创建文件系统参数。

```csharp
#if UNITY_WEBGL && UNITY_ALIMINIGAME && !UNITY_EDITOR
var createParameters = new WebPlayModeOptions();

string defaultHostServer = GetHostServerURL();
string fallbackHostServer = GetHostServerURL();
IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);

createParameters.WebServerFileSystemParameters =
    AlipayFileSystemCreater.CreateFileSystemParameters(remoteService);

var initializationOperation = package.InitializePackageAsync(createParameters);
#endif
```

支付宝小游戏底层会对远程 AssetBundle 请求做平台适配，业务侧仍然按照远程异步加载流程使用 YooAsset。

## 资源包命名

支付宝小游戏构建推荐让资源包文件名携带 hash。YooAsset 推荐只使用 `HashName` 文件命名风格。

`HashName` 会生成纯 hash 文件名，例如：

```text
8d265a9dfd6cb7669cdb8b726f0afb1e.bundle
```

该命名方式更适合小游戏平台的缓存和更新识别，也能避免暴露原始 Bundle 名称。支付宝小游戏构建不建议使用 `BundleName` 或 `BundleName_HashName`。

## 注意事项

加密 AssetBundle 仍然会走 YooAsset 常规的 Web 下载和解密流程。非加密 AssetBundle 会使用支付宝平台适配器，并通过 `APAssetBundle.GetAssetBundle` 发起请求。
