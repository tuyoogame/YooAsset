# DefaultWebServerFileSystem Web服务器文件系统

## 模块概述

DefaultWebServerFileSystem 是 YooAsset 的 **Web 服务器文件系统**，专为 WebGL 平台的**同域资源加载**而设计。该文件系统从与 WebGL 构建相同的服务器（StreamingAssets 目录）加载资源，通过 Catalog 目录文件管理内置资源清单。

### 核心特性

- **同域加载**：从 WebGL 构建所在服务器加载资源
- **Catalog 管理**：通过目录文件追踪可用资源
- **路径映射**：自动将本地路径转换为 WWW 路径
- **Unity 缓存控制**：可选择禁用 Unity 的 Web 请求缓存
- **加密支持**：支持 `IWebDecryptionServices` 解密 Web 资源

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **WebGL 内置资源** | 加载与 WebGL 构建一起部署的资源 |
| **资源追踪** | 通过 Catalog 文件精确知道哪些资源可用 |
| **无跨域问题** | 从同一服务器加载，避免 CORS 问题 |
| **与 Buildin 对应** | WebGL 版本的 DefaultBuildinFileSystem |

---

## 文件结构

```
DefaultWebServerFileSystem/
├── DefaultWebServerFileSystem.cs           # 文件系统主类
└── Operation/                              # 操作类
    ├── DWSFSInitializeOperation.cs         # 初始化操作
    ├── DWSFSRequestPackageVersionOperation.cs  # 请求版本操作
    ├── DWSFSLoadPackageManifestOperation.cs    # 加载清单操作
    ├── DWSFSLoadBundleOperation.cs         # 加载资源包操作
    └── internal/                           # 内部操作类
        ├── LoadWebServerCatalogFileOperation.cs    # 加载目录文件
        ├── RequestWebServerPackageVersionOperation.cs  # 请求版本文件
        ├── RequestWebServerPackageHashOperation.cs     # 请求哈希文件
        └── LoadWebServerPackageManifestOperation.cs    # 加载清单文件
```

### 依赖的共享模块

DefaultWebServerFileSystem 依赖 `WebGame` 目录下的共享操作类：

```
FileSystem/WebGame/Operation/
├── LoadWebAssetBundleOperation.cs          # Web 资源包加载基类
├── LoadWebNormalAssetBundleOperation.cs    # 普通资源包加载
└── LoadWebEncryptAssetBundleOperation.cs   # 加密资源包加载
```

---

## 核心类说明

### DefaultWebServerFileSystem

Web 服务器文件系统的主类，实现 `IFileSystem` 接口。

#### 内部类

```csharp
public class FileWrapper
{
    public string FileName { private set; get; }

    public FileWrapper(string fileName)
    {
        FileName = fileName;
    }
}
```

#### 基本属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `FileRoot` | `string` | Web 包裹根目录 |
| `FileCount` | `int` | 始终返回 0 |
| `DownloadBackend` | `IDownloadBackend` | 下载后台接口 |

#### 自定义参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `DisableUnityWebCache` | `bool` | `false` | 禁用 Unity 的网络缓存 |
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

// 文件查询（基于 Catalog）
bool Belong(PackageBundle bundle);      // 检查是否在 _wrappers 字典中
bool Exists(PackageBundle bundle);      // 检查是否在 _wrappers 字典中
bool NeedDownload(PackageBundle bundle);// 始终返回 false
bool NeedUnpack(PackageBundle bundle);  // 始终返回 false
bool NeedImport(PackageBundle bundle);  // 始终返回 false
```

#### 内部方法

```csharp
// 路径获取
string GetDefaultWebPackageRoot(string packageName);      // 默认包裹根目录
string GetWebFileLoadPath(PackageBundle bundle);          // 资源文件加载路径
string GetWebPackageVersionFilePath();                    // 版本文件路径
string GetWebPackageHashFilePath(string packageVersion);  // 哈希文件路径
string GetWebPackageManifestFilePath(string packageVersion); // 清单文件路径
string GetCatalogBinaryFileLoadPath();                    // Catalog 文件路径

// Catalog 管理
bool RecordCatalogFile(string bundleGUID, FileWrapper wrapper);
```

---

## Catalog 目录系统

DefaultWebServerFileSystem 使用 Catalog 文件追踪可用的内置资源，这与 DefaultBuildinFileSystem 的机制相同。

### Catalog 文件

| 文件 | 路径 | 说明 |
|------|------|------|
| Catalog 二进制文件 | `{PackageRoot}/{PackageName}_buildin.bytes` | 资源目录信息 |

### Catalog 数据结构

```csharp
// _wrappers 字典：BundleGUID → FileWrapper
protected readonly Dictionary<string, FileWrapper> _wrappers;

// FileWrapper 包含文件名信息
public class FileWrapper
{
    public string FileName { private set; get; }
}
```

### Belong 与 Exists 判断

```csharp
public virtual bool Belong(PackageBundle bundle)
{
    // 检查 Catalog 中是否包含该资源
    return _wrappers.ContainsKey(bundle.BundleGUID);
}

public virtual bool Exists(PackageBundle bundle)
{
    // 同样基于 Catalog 判断
    return _wrappers.ContainsKey(bundle.BundleGUID);
}
```

---

## 操作类说明

### DWSFSInitializeOperation

初始化操作，加载 Catalog 目录文件。

```
状态流程：
LoadCatalogFile
    └── LoadWebServerCatalogFileOperation
            └── 请求 {PackageName}_buildin.bytes
                    ├── 下载二进制数据
                    └── 反序列化 Catalog
                            ├── 验证 PackageName
                            └── 遍历 Wrappers
                                    └── RecordCatalogFile()
                                            ├── 成功 → Succeed
                                            └── 失败 → Failed
```

### LoadWebServerCatalogFileOperation

加载 Web 服务器 Catalog 文件的内部操作。

```csharp
// 关键流程
if (_steps == ESteps.LoadCatalog)
{
    var catalog = CatalogTools.DeserializeFromBinary(_webDataRequestOp.Result);

    // 验证包裹名称
    if (catalog.PackageName != _fileSystem.PackageName)
    {
        Error = $"Catalog file package name {catalog.PackageName} cannot match...";
        return;
    }

    // 记录所有内置资源
    foreach (var wrapper in catalog.Wrappers)
    {
        var fileWrapper = new DefaultWebServerFileSystem.FileWrapper(wrapper.FileName);
        _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
    }
}
```

### DWSFSRequestPackageVersionOperation

请求包裹版本操作，从 Web 服务器获取版本文件。

```
状态流程：
RequestPackageVersion
    └── RequestWebServerPackageVersionOperation
            └── 请求 {FileRoot}/{PackageName}_Version.txt
                    ├── 转换为 WWW 路径
                    └── 下载文本内容
                            ├── 成功 → PackageVersion
                            └── 失败 → Failed
```

### DWSFSLoadPackageManifestOperation

加载资源清单操作，从 Web 服务器加载并解析清单。

```
状态流程：
RequestWebPackageHash
    └── RequestWebServerPackageHashOperation
            └── 请求 {PackageName}_{Version}.hash
                    ├── 成功 → PackageHash
                    └── 失败 → Failed
                            ↓
LoadWebPackageManifest
    └── LoadWebServerPackageManifestOperation
            ├── 请求 {PackageName}_{Version}.bytes
            ├── 验证哈希
            └── 反序列化清单
                    ├── 成功 → Manifest
                    └── 失败 → Failed
```

### LoadWebServerPackageManifestOperation

加载清单文件的内部操作，包含哈希验证。

```
状态流程：
RequestFileData
    └── DownloadBytesRequest
            └── 下载清单二进制数据
                    ↓
VerifyFileData
    └── ManifestTools.VerifyManifestData()
            ├── 验证成功 → LoadManifest
            └── 验证失败 → Failed
                    ↓
LoadManifest
    └── DeserializeManifestOperation
            ├── 反序列化成功 → Manifest → Succeed
            └── 反序列化失败 → Failed
```

### DWSFSLoadAssetBundleOperation

加载资源包操作，从 Web 服务器加载 AssetBundle。

```
状态流程：
LoadWebAssetBundle
    ├── 获取文件路径 → 转换为 WWW 路径
    │
    ├── 未加密 → LoadWebNormalAssetBundleOperation
    │       └── UnityWebRequestAssetBundle
    │               ├── 成功 → AssetBundleResult
    │               └── 失败 → Failed
    │
    └── 已加密 → LoadWebEncryptAssetBundleOperation
            └── DownloadBytesRequest
                    └── IWebDecryptionServices.LoadAssetBundle()
                            ├── 成功 → AssetBundleResult
                            └── 失败 → Failed
```

#### 路径转换

```csharp
// 获取本地文件路径
string fileLoadPath = _fileSystem.GetWebFileLoadPath(_bundle);

// 转换为 WWW 请求路径
string mainURL = DownloadSystemHelper.ConvertToWWWPath(fileLoadPath);

// 主 URL 和备用 URL 相同（同域加载）
DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
options.SetURL(mainURL, mainURL);
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

## 路径映射机制

DefaultWebServerFileSystem 使用路径缓存优化性能：

```csharp
// 文件路径缓存
protected readonly Dictionary<string, string> _webFilePathMapping = new Dictionary<string, string>(10000);

public string GetWebFileLoadPath(PackageBundle bundle)
{
    if (_webFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
    {
        // 组合路径：{WebPackageRoot}/{FileName}
        filePath = PathUtility.Combine(_webPackageRoot, bundle.FileName);
        _webFilePathMapping.Add(bundle.BundleGUID, filePath);
    }
    return filePath;
}
```

### 默认路径

```csharp
protected string GetDefaultWebPackageRoot(string packageName)
{
    // 使用默认的内置资源根目录（StreamingAssets）
    string rootDirectory = YooAssetSettingsData.GetYooDefaultBuildinRoot();
    return PathUtility.Combine(rootDirectory, packageName);
}
```

---

## 与 DefaultWebRemoteFileSystem 对比

| 特性 | DefaultWebServerFileSystem | DefaultWebRemoteFileSystem |
|------|---------------------------|---------------------------|
| 用途 | WebGL 同域内置资源 | WebGL 跨域远程资源 |
| Catalog 系统 | ✅ 有 | ❌ 无 |
| Belong/Exists | 基于 Catalog 判断 | 始终返回 true |
| 远程服务 | ❌ 不需要 | ✅ 需要 IRemoteServices |
| URL 生成 | 本地路径转 WWW | 远程服务接口生成 |
| 主/备用地址 | 相同（同域） | 不同（可配置） |

---

## 使用示例

### 基础配置

```csharp
// 创建 Web 服务器文件系统参数
var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();

// 初始化包裹（WebGL 模式）
var initParams = new WebPlayModeParameters();
initParams.WebServerFileSystemParameters = webServerParams;
initParams.WebRemoteFileSystemParameters = webRemoteParams;  // 可选：跨域资源
var initOp = package.InitializeAsync(initParams);
```

### 自定义包裹根目录

```csharp
// 创建参数时指定自定义路径
var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(
    packageRoot: "Assets/StreamingAssets/MyCustomPath/DefaultPackage"
);
```

### 禁用 Unity Web 缓存

```csharp
var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();

// 禁用 Unity 的 Web 请求缓存
webServerParams.AddParameter(FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE, true);
```

### 配置 Web 解密服务

```csharp
// 自定义 Web 解密服务
class GameWebDecryptionServices : IWebDecryptionServices
{
    public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
    {
        byte[] decryptedData = Decrypt(fileInfo.FileData);
        AssetBundle bundle = AssetBundle.LoadFromMemory(decryptedData);
        return new WebDecryptResult { Result = bundle };
    }
}

var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();

// 设置 Web 解密服务
webServerParams.AddParameter(
    FileSystemParametersDefine.DECRYPTION_SERVICES,
    new GameWebDecryptionServices()
);
```

### WebGL 双文件系统配置

```csharp
// WebGL 典型配置：内置 + 远程
var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

var initParams = new WebPlayModeParameters();
initParams.WebServerFileSystemParameters = webServerParams;   // 同域内置资源
initParams.WebRemoteFileSystemParameters = webRemoteParams;   // 跨域热更资源

var initOp = package.InitializeAsync(initParams);
```

---

## 参数常量

```csharp
// Unity 缓存控制
FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE  // bool: 禁用 Unity Web 缓存

// 服务接口
FileSystemParametersDefine.DECRYPTION_SERVICES      // IWebDecryptionServices: Web 解密服务
FileSystemParametersDefine.MANIFEST_SERVICES        // IManifestRestoreServices: 清单服务
```

---

## 类继承关系

```
IFileSystem
    └── DefaultWebServerFileSystem

FSInitializeFileSystemOperation
    └── DWSFSInitializeOperation

FSRequestPackageVersionOperation
    └── DWSFSRequestPackageVersionOperation

FSLoadPackageManifestOperation
    └── DWSFSLoadPackageManifestOperation

FSLoadBundleOperation
    └── DWSFSLoadAssetBundleOperation

AsyncOperationBase
    ├── LoadWebServerCatalogFileOperation
    ├── RequestWebServerPackageVersionOperation
    ├── RequestWebServerPackageHashOperation
    ├── LoadWebServerPackageManifestOperation
    └── LoadWebAssetBundleOperation (共享)
        ├── LoadWebNormalAssetBundleOperation
        └── LoadWebEncryptAssetBundleOperation

BundleResult
    └── AssetBundleResult
```

---

## 与 DefaultBuildinFileSystem 对比

| 特性 | DefaultWebServerFileSystem | DefaultBuildinFileSystem |
|------|---------------------------|-------------------------|
| 平台 | WebGL | 非 WebGL（移动端、PC） |
| 加载方式 | UnityWebRequest | AssetBundle.LoadFromFile |
| Catalog 系统 | ✅ 相同机制 | ✅ 相同机制 |
| 同步加载 | ❌ 不支持 | ✅ 支持 |
| 解包机制 | ❌ 无 | ✅ 有（Android APK） |
| 文件访问 | WWW 路径 | 本地文件路径 |

---

## 注意事项

1. **WebGL 专用**：此文件系统专为 WebGL 平台设计，非 WebGL 平台应使用 DefaultBuildinFileSystem
2. **仅支持 AssetBundle**：不支持 RawBundle 类型的资源加载
3. **不支持同步加载**：WebGL 平台限制，`WaitForAsyncComplete()` 会直接返回失败
4. **Catalog 必需**：初始化时必须成功加载 Catalog 文件，否则无法确定资源归属
5. **同域加载**：资源文件必须与 WebGL 构建在同一服务器上
6. **部分方法未实现**：`DownloadFileAsync`、`GetBundleFilePath`、`ReadBundleFileData`、`ReadBundleFileText` 会抛出异常
7. **加密资源**：加密资源需要配置 `IWebDecryptionServices`（Web 专用接口）
8. **路径缓存**：内部使用字典缓存路径映射，提升重复访问性能
