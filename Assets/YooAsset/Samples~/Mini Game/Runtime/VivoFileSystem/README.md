# vivo 小游戏文件系统

该示例用于在 YooAsset 的 WebGL 运行模式下接入 vivo 小游戏。

参考文档：[vivo 小游戏使用 AssetBundle 进行资源按需加载](https://h5.vivo.com.cn/vmix/vivo-unity-doc/lesson/UsingAssetBundle.html)

## 环境要求

先安装 vivo Unity 小游戏适配插件，并将项目切换到 WebGL 构建目标。

在 WebGL Player 的 Scripting Define Symbols 中启用以下宏：

- `VIVOMINIGAME`

该宏是 YooAsset vivo 小游戏示例约定的编译开关，用于和其它小游戏平台适配代码保持一致。

## 初始化 YooAsset

在 vivo 小游戏构建中初始化 `WebPlayModeOptions` 时，使用 `VivoFileSystemCreater` 创建文件系统参数。

```csharp
#if UNITY_WEBGL && VIVOMINIGAME && !UNITY_EDITOR
var createParameters = new WebPlayModeOptions();

string defaultHostServer = GetHostServerURL();
string fallbackHostServer = GetHostServerURL();
IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);

createParameters.WebServerFileSystemParameters =
    VivoFileSystemCreater.CreateFileSystemParameters(remoteService);

var initializationOperation = package.InitializePackageAsync(createParameters);
#endif
```

vivo 小游戏底层会对远程 AssetBundle 请求做缓存，业务侧仍然按照远程异步加载流程使用 YooAsset。

## 资源包命名

vivo 小游戏底层缓存依赖资源包文件名中的 hash。YooAsset 推荐只使用 `HashName` 文件命名风格。

`HashName` 会生成纯 hash 文件名，例如：

```text
8d265a9dfd6cb7669cdb8b726f0afb1e.bundle
```

该命名方式和 vivo 资源缓存规则最匹配，也能避免暴露原始 Bundle 名称。vivo 小游戏构建不建议使用 `BundleName` 或 `BundleName_HashName`。

## 注意事项

加密 AssetBundle 仍然会走 YooAsset 常规的 Web 下载和解密流程。非加密 AssetBundle 会使用 vivo 平台适配器，并通过 `UnityWebRequestAssetBundle.GetAssetBundle` 发起请求，匹配 vivo 小游戏底层缓存流程。
