# OPPO 小游戏文件系统

该示例用于在 YooAsset 的 WebGL 运行模式下接入 OPPO 小游戏。

参考文档：[OPPO 小游戏资源缓存](https://github.com/oppominigame/unity-webgl-to-oppo-minigame/blob/main/doc/AssetCache.md)

## 环境要求

先安装 OPPO Unity WebGL 小游戏 SDK，并将项目切换到 WebGL 构建目标。

在 WebGL Player 的 Scripting Define Symbols 中启用以下宏：

- `OPPOMINIGAME`

该宏是 YooAsset OPPO 小游戏示例约定的编译开关，用于和其它小游戏平台适配代码保持一致。

## 初始化 YooAsset

在 OPPO 小游戏构建中初始化 `WebPlayModeOptions` 时，使用 `OppoFileSystemCreater` 创建文件系统参数。

```csharp
#if UNITY_WEBGL && OPPOMINIGAME && !UNITY_EDITOR
var createParameters = new WebPlayModeOptions();

string defaultHostServer = GetHostServerURL();
string fallbackHostServer = GetHostServerURL();
IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);

createParameters.WebServerFileSystemParameters =
    OppoFileSystemCreater.CreateFileSystemParameters(remoteService);

var initializationOperation = package.InitializePackageAsync(createParameters);
#endif
```

OPPO 真正的 AssetBundle 缓存行为由生成后小游戏工程里的 `manifest.json` 控制。

## OPPO 缓存配置

配置小游戏工程的 `manifest.json`，让 OPPO 能识别 YooAsset 的资源包 URL：

```json
{
  "disableBundleCache": false,
  "gameCDNRoot": "https://your-cdn.example.com/StreamingAssets",
  "bundlePathIdentifier": "StreamingAssets;bundles",
  "excludeFileExtensions": ".json;.hash;.version;.bytes",
  "bundleHashLength": 32,
  "defaultReleaseSize": 30,
  "keepOldVersion": false,
  "enableCacheLog": true
}
```

`gameCDNRoot` 必须和 YooAsset `IRemoteService` 返回的 CDN 根地址一致。如果不匹配，OPPO 会把请求当成普通网络请求处理，不会进入资源缓存逻辑。

`excludeFileExtensions` 建议包含 YooAsset 的清单、版本和哈希文件后缀，让 OPPO 只缓存 AssetBundle 文件。

## 资源包命名

OPPO 通过远端文件名里的 hash 识别可缓存资源。YooAsset 推荐只使用 `HashName` 文件命名风格。

`HashName` 会生成纯 hash 文件名，例如：

```text
8d265a9dfd6cb7669cdb8b726f0afb1e.bundle
```

该命名方式和 OPPO 资源缓存规则最匹配，也能避免暴露原始 Bundle 名称。OPPO 缓存构建不建议使用 `BundleName` 或 `BundleName_HashName`。

## 注意事项

加密 AssetBundle 仍然会走 YooAsset 常规的 Web 下载和解密流程。非加密 AssetBundle 会使用 OPPO 平台适配器；当 `manifest.json` 缓存规则匹配时，可由 OPPO 小游戏运行时缓存。
