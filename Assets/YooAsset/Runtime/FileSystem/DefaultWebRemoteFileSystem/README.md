# DefaultWebRemoteFileSystem Web远程文件系统

## 模块概述

DefaultWebRemoteFileSystem 是 YooAsset 的 **Web 远程文件系统**，专为从远程服务器直接加载资源而设计。该文件系统不缓存文件到本地，每次都从远程 URL 加载资源，适用于 WebGL 平台的跨域资源加载或特殊的网络资源场景。

### 核心特性

- **无本地缓存**：直接从远程 URL 加载，不写入本地文件
- **跨域支持**：通过 `IRemoteServices` 支持跨域资源下载
- **Unity 缓存控制**：可选择禁用 Unity 的 Web 请求缓存
- **加密支持**：支持 `IWebDecryptionServices` 解密 Web 资源
- **失败重试**：内置下载失败自动重试机制

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **轻量级** | 无缓存管理，结构简洁 |
| **即时加载** | 每次从远程获取最新资源 |
| **跨域兼容** | 支持 WebGL 平台的跨域限制处理 |
| **可配置** | 支持 Unity Web 缓存控制和自定义解密 |

---

## 文件结构

```
DefaultWebRemoteFileSystem/
├── DefaultWebRemoteFileSystem.cs           # 文件系统主类
└── Operation/                              # 操作类
    ├── DWRFSInitializeOperation.cs         # 初始化操作
    ├── DWRFSRequestPackageVersionOperation.cs  # 请求版本操作
    ├── DWRFSLoadPackageManifestOperation.cs    # 加载清单操作
    └── DWRFSLoadBundleOperation.cs         # 加载资源包操作
```

### 依赖的共享模块

DefaultWebRemoteFileSystem 依赖 `WebGame` 目录下的共享操作类：

```
FileSystem/WebGame/Operation/
├── LoadWebAssetBundleOperation.cs          # Web 资源包加载基类
├── LoadWebNormalAssetBundleOperation.cs    # 普通资源包加载
├── LoadWebEncryptAssetBundleOperation.cs   # 加密资源包加载
├── RequestWebPackageVersionOperation.cs    # 请求版本文件
├── RequestWebPackageHashOperation.cs       # 请求哈希文件
└── LoadWebPackageManifestOperation.cs      # 加载清单文件
```

---

## 核心类说明

### DefaultWebRemoteFileSystem

Web 远程文件系统的主类，实现 `IFileSystem` 接口。

#### 基本属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `FileRoot` | `string` | 始终返回空字符串（无本地存储） |
| `FileCount` | `int` | 始终返回 0（无本地文件） |
| `DownloadBackend` | `IDownloadBackend` | 下载后台接口 |

#### 自定义参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `DisableUnityWebCache` | `bool` | `false` | 禁用 Unity 的网络缓存 |
| `RemoteServices` | `IRemoteServices` | - | 远程服务接口（必需） |
| `DecryptionServices` | `IWebDecryptionServices` | `null` | Web 解密服务接口 |
| `ManifestServices` | `IManifestRestoreServices` | `null` | 清单服务接口 |

#### 核心方法

```csharp
// 生命周期
void OnCreate(string packageName, string packageRoot);
void OnDestroy();
void SetParameter(string name, object value);

// 异步操作
FSInitializeFileSystemOperation InitializeFileSystemAsync();
FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout);
FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout);
FSLoadBundleOperation LoadBundleFile(PackageBundle bundle);
FSClearCacheFilesOperation ClearCacheFilesAsync(...);  // 直接返回完成

// 不支持的操作
FSDownloadFileOperation DownloadFileAsync(...);  // 抛出 NotImplementedException
string GetBundleFilePath(...);                   // 抛出 NotImplementedException
byte[] ReadBundleFileData(...);                  // 抛出 NotImplementedException
string ReadBundleFileText(...);                  // 抛出 NotImplementedException

// 文件查询（固定返回值）
bool Belong(PackageBundle bundle);      // 始终返回 true
bool Exists(PackageBundle bundle);      // 始终返回 true
bool NeedDownload(PackageBundle bundle);// 始终返回 false
bool NeedUnpack(PackageBundle bundle);  // 始终返回 false
bool NeedImport(PackageBundle bundle);  // 始终返回 false
```

---

## 操作类说明

### DWRFSInitializeOperation

初始化操作，立即完成（无需任何初始化工作）。

```csharp
internal override void InternalStart()
{
    Status = EOperationStatus.Succeed;  // 直接成功
}
```

### DWRFSRequestPackageVersionOperation

请求包裹版本操作，从远程服务器获取版本文件。

```
状态流程：
RequestPackageVersion
    └── RequestWebPackageVersionOperation
            └── 请求 {PackageName}_Version.txt
                    ├── 成功 → PackageVersion = 文件内容 → Succeed
                    └── 失败 → Failed
```

#### 请求地址轮换

```csharp
// 轮流使用主地址和备用地址
if (_requestCount % 2 == 0)
    url = _remoteServices.GetRemoteMainURL(fileName);
else
    url = _remoteServices.GetRemoteFallbackURL(fileName);

// 可选：添加时间戳防止缓存
if (_appendTimeTicks)
    return $"{url}?{System.DateTime.UtcNow.Ticks}";
```

### DWRFSLoadPackageManifestOperation

加载资源清单操作，从远程下载并解析清单。

```
状态流程：
RequestWebPackageHash
    └── RequestWebPackageHashOperation
            └── 请求 {PackageName}_{Version}.hash
                    ├── 成功 → PackageHash
                    └── 失败 → Failed
                            ↓
LoadWebPackageManifest
    └── LoadWebPackageManifestOperation
            └── 请求 {PackageName}_{Version}.bytes
                    ├── 验证哈希
                    └── 反序列化清单
                            ├── 成功 → Manifest → Succeed
                            └── 失败 → Failed
```

### DWRFSLoadAssetBundleOperation

加载资源包操作，从远程 URL 直接加载 AssetBundle。

```
状态流程：
LoadWebAssetBundle
    ├── 未加密 → LoadWebNormalAssetBundleOperation
    │       └── UnityWebRequestAssetBundle.GetAssetBundle()
    │               ├── 成功 → AssetBundle → AssetBundleResult
    │               └── 失败 → TryAgain 或 Failed
    │
    └── 已加密 → LoadWebEncryptAssetBundleOperation
            └── DownloadBytesRequest
                    └── 下载原始字节
                            └── IWebDecryptionServices.LoadAssetBundle()
                                    ├── 成功 → AssetBundle → AssetBundleResult
                                    └── 失败 → TryAgain 或 Failed
```

#### 状态机枚举

```csharp
private enum ESteps
{
    None,
    LoadWebAssetBundle,  // 加载 Web 资源包
    Done                 // 完成
}
```

#### 同步加载限制

```csharp
internal override void InternalWaitForAsyncComplete()
{
    if (_steps != ESteps.Done)
    {
        _steps = ESteps.Done;
        Status = EOperationStatus.Failed;
        Error = "WebGL platform not support sync load method !";
        UnityEngine.Debug.LogError(Error);
    }
}
```

---

## 共享 Web 操作类

### LoadWebNormalAssetBundleOperation

普通（未加密）AssetBundle 的 Web 加载操作。

```
CreateRequest
    └── DownloadAssetBundleRequest
            ├── URL: 主地址或备用地址（轮换）
            ├── DisableUnityWebCache: 是否禁用缓存
            ├── FileHash: 用于 Unity 缓存键
            └── UnityCRC: CRC 验证
                    ↓
CheckRequest
    ├── 成功 → Result = AssetBundle
    └── 失败 → TryAgain（重试）或 Failed
```

#### Unity Web 缓存机制

```csharp
// 使用 Unity 的内置缓存
var args = new DownloadAssetBundleRequestArgs(
    url,
    timeout: 0,
    watchdogTime: 0,
    disableUnityWebCache: _disableUnityWebCache,
    cacheHash: _bundle.FileHash,    // 缓存键
    unityCRC: _bundle.UnityCRC      // CRC 验证
);
```

### LoadWebEncryptAssetBundleOperation

加密 AssetBundle 的 Web 加载操作。

```
CreateRequest
    └── 检查 DecryptionServices
            ├── null → Failed
            └── 有效 → DownloadBytesRequest
                        ↓
CheckRequest
    ├── 下载成功 → LoadEncryptedAssetBundle()
    │       └── IWebDecryptionServices.LoadAssetBundle(fileData)
    │               ├── 解密成功 → Result = AssetBundle
    │               └── 解密失败 → Failed
    └── 下载失败 → TryAgain 或 Failed
```

#### 加密资源加载

```csharp
private AssetBundle LoadEncryptedAssetBundle(byte[] fileData)
{
    var fileInfo = new WebDecryptFileInfo();
    fileInfo.BundleName = _bundle.BundleName;
    fileInfo.FileLoadCRC = _bundle.UnityCRC;
    fileInfo.FileData = fileData;  // 下载的原始字节
    var decryptResult = _decryptionServices.LoadAssetBundle(fileInfo);
    return decryptResult.Result;
}
```

---

## 失败重试机制

Web 加载操作内置失败重试机制：

```csharp
// 检测下载结果
if (_unityAssetBundleRequestOp.Status == EDownloadRequestStatus.Succeed)
{
    _steps = ESteps.Done;
    Status = EOperationStatus.Succeed;
    Result = _unityAssetBundleRequestOp.Result;
}
else
{
    if (_failedTryAgain > 0)
    {
        _steps = ESteps.TryAgain;
        YooLogger.Warning($"Failed download : {url} Try again !");
    }
    else
    {
        _steps = ESteps.Done;
        Status = EOperationStatus.Failed;
        Error = _unityAssetBundleRequestOp.Error;
    }
}

// 重新尝试下载（1秒后）
if (_steps == ESteps.TryAgain)
{
    _tryAgainTimer += Time.unscaledDeltaTime;
    if (_tryAgainTimer > 1f)
    {
        _tryAgainTimer = 0f;
        _failedTryAgain--;
        _steps = ESteps.CreateRequest;  // 重新创建请求
    }
}
```

---

## 使用示例

### 基础配置

```csharp
// 创建远程服务接口
class GameRemoteServices : IRemoteServices
{
    public string GetRemoteMainURL(string fileName)
    {
        return $"https://cdn.example.com/bundles/{fileName}";
    }
    public string GetRemoteFallbackURL(string fileName)
    {
        return $"https://cdn-backup.example.com/bundles/{fileName}";
    }
}

// 创建 Web 远程文件系统参数
var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 初始化包裹（WebGL 模式）
var initParams = new WebPlayModeParameters();
initParams.WebServerFileSystemParameters = webServerParams;
initParams.WebRemoteFileSystemParameters = webRemoteParams;
var initOp = package.InitializeAsync(initParams);
```

### 禁用 Unity Web 缓存

```csharp
var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 禁用 Unity 的 Web 请求缓存（始终获取最新资源）
webRemoteParams.AddParameter(FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE, true);
```

### 配置 Web 解密服务

```csharp
// 自定义 Web 解密服务
class GameWebDecryptionServices : IWebDecryptionServices
{
    public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
    {
        // 解密下载的字节数据
        byte[] decryptedData = Decrypt(fileInfo.FileData);
        AssetBundle bundle = AssetBundle.LoadFromMemory(decryptedData);
        return new WebDecryptResult { Result = bundle };
    }
}

var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 设置 Web 解密服务
webRemoteParams.AddParameter(
    FileSystemParametersDefine.DECRYPTION_SERVICES,
    new GameWebDecryptionServices()
);
```

### 跨域资源加载

```csharp
// 跨域远程服务
class CrossDomainRemoteServices : IRemoteServices
{
    private readonly string _mainDomain;
    private readonly string _fallbackDomain;

    public CrossDomainRemoteServices(string mainDomain, string fallbackDomain)
    {
        _mainDomain = mainDomain;
        _fallbackDomain = fallbackDomain;
    }

    public string GetRemoteMainURL(string fileName)
    {
        // 主 CDN 域名
        return $"https://{_mainDomain}/assets/{fileName}";
    }

    public string GetRemoteFallbackURL(string fileName)
    {
        // 备用 CDN 域名
        return $"https://{_fallbackDomain}/assets/{fileName}";
    }
}

// 使用跨域服务
var remoteServices = new CrossDomainRemoteServices(
    mainDomain: "cdn-us.example.com",
    fallbackDomain: "cdn-eu.example.com"
);

var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: remoteServices
);
```

---

## 参数常量

```csharp
// Unity 缓存控制
FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE  // bool: 禁用 Unity Web 缓存

// 服务接口
FileSystemParametersDefine.REMOTE_SERVICES          // IRemoteServices: 远程服务接口
FileSystemParametersDefine.DECRYPTION_SERVICES      // IWebDecryptionServices: Web 解密服务
FileSystemParametersDefine.MANIFEST_SERVICES        // IManifestRestoreServices: 清单服务
```

---

## 类继承关系

```
IFileSystem
    └── DefaultWebRemoteFileSystem

FSInitializeFileSystemOperation
    └── DWRFSInitializeOperation

FSRequestPackageVersionOperation
    └── DWRFSRequestPackageVersionOperation

FSLoadPackageManifestOperation
    └── DWRFSLoadPackageManifestOperation

FSLoadBundleOperation
    └── DWRFSLoadAssetBundleOperation

AsyncOperationBase
    ├── LoadWebAssetBundleOperation (abstract)
    │   ├── LoadWebNormalAssetBundleOperation
    │   └── LoadWebEncryptAssetBundleOperation
    ├── RequestWebPackageVersionOperation
    ├── RequestWebPackageHashOperation
    └── LoadWebPackageManifestOperation

BundleResult
    └── AssetBundleResult
```

---

## 与其他文件系统对比

| 特性 | DefaultWebRemoteFileSystem | DefaultCacheFileSystem | DefaultWebServerFileSystem |
|------|---------------------------|------------------------|---------------------------|
| 本地缓存 | ❌ 无 | ✅ 有 | ❌ 无 |
| 支持 RawBundle | ❌ | ✅ | ❌ |
| 同步加载 | ❌ | ✅ | ❌ |
| 断点续传 | ❌ | ✅ | ❌ |
| 跨域支持 | ✅ | ✅ | ✅ |
| 适用场景 | WebGL 跨域 | 常规游戏 | WebGL 同域 |

---

## 注意事项

1. **仅支持 AssetBundle**：不支持 RawBundle 类型的资源加载
2. **不支持同步加载**：WebGL 平台限制，`WaitForAsyncComplete()` 会直接返回失败
3. **无本地缓存**：每次加载都从远程获取，注意网络流量
4. **部分方法未实现**：`DownloadFileAsync`、`GetBundleFilePath`、`ReadBundleFileData`、`ReadBundleFileText` 会抛出异常
5. **远程服务必需**：必须配置 `IRemoteServices` 接口
6. **Unity 缓存**：默认使用 Unity 的 Web 请求缓存，可通过参数禁用
7. **加密资源**：加密资源需要配置 `IWebDecryptionServices`（注意是 Web 专用接口，非 `IDecryptionServices`）
