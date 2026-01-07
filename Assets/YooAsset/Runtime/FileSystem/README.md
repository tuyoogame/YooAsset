# FileSystem 文件系统模块

## 模块概述

FileSystem 是 YooAsset 资源管理系统的**文件访问抽象层**，负责统一管理不同来源的资源文件访问。该模块采用策略模式设计，通过 `IFileSystem` 接口抽象不同的文件存储策略，支持编辑器模拟、内置资源、缓存资源、WebGL 等多种场景。

### 核心职责

- 统一的文件系统接口抽象
- 资源包文件的加载和管理
- 资源清单的请求和加载
- 缓存文件的验证和清理
- 资源下载的调度和管理

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **统一抽象** | 通过 IFileSystem 接口统一不同来源的文件访问 |
| **策略模式** | 可插拔的文件系统实现，支持自定义扩展 |
| **多场景支持** | 编辑器、单机、联机、WebGL 等多种运行模式 |
| **灵活配置** | 通过参数系统支持丰富的自定义配置 |

---

## 架构概念

### 系统架构

```
┌─────────────────────────────────────────────────────────┐
│                   ResourcePackage                        │
│                    (资源包管理)                           │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                    PlayModeImpl                          │
│                  (运行模式实现)                           │
│         管理多个 IFileSystem，按优先级查询                 │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                    IFileSystem                           │
│                   (文件系统接口)                          │
├─────────────────────────────────────────────────────────┤
│  DefaultEditorFileSystem    │  编辑器模拟文件系统         │
│  DefaultBuildinFileSystem   │  内置资源文件系统           │
│  DefaultCacheFileSystem     │  缓存资源文件系统           │
│  DefaultUnpackFileSystem    │  解压资源文件系统           │
│  DefaultWebServerFileSystem │  WebGL 服务器文件系统       │
│  DefaultWebRemoteFileSystem │  WebGL 远程文件系统         │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                    BundleResult                          │
│                  (资源包加载结果)                         │
├─────────────────────────────────────────────────────────┤
│  AssetBundleResult  │  Unity AssetBundle 加载结果        │
│  RawBundleResult    │  原生文件加载结果                   │
│  VirtualBundleResult│  虚拟资源包结果（编辑器模拟）        │
└─────────────────────────────────────────────────────────┘
```

### 核心组件

- **IFileSystem**: 文件系统核心接口，定义所有文件操作的契约
- **FileSystemParameters**: 文件系统参数配置，支持自定义参数注入
- **BundleResult**: 资源包加载结果抽象，封装不同类型的加载结果
- **FSOperation**: 文件系统操作基类，定义异步操作的抽象

---

## 文件结构

```
FileSystem/
├── Interface/                      # 接口定义
│   └── IFileSystem.cs              # 文件系统核心接口
│
├── Operation/                      # 操作基类定义
│   ├── FSInitializeFileSystemOperation.cs    # 初始化操作
│   ├── FSRequestPackageVersionOperation.cs   # 请求版本操作
│   ├── FSLoadPackageManifestOperation.cs     # 加载清单操作
│   ├── FSLoadBundleOperation.cs              # 加载资源包操作
│   ├── FSDownloadFileOperation.cs            # 下载文件操作
│   ├── FSClearCacheFilesOperation.cs         # 清理缓存操作
│   ├── FSLoadAssetOperation.cs               # 加载资源操作
│   ├── FSLoadAllAssetsOperation.cs           # 加载所有资源操作
│   ├── FSLoadSubAssetsOperation.cs           # 加载子资源操作
│   └── FSLoadSceneOperation.cs               # 加载场景操作
│
├── BundleResult/                   # 资源包加载结果
│   ├── BundleResult.cs             # 结果基类
│   ├── AssetBundleResult/          # AssetBundle 结果实现
│   ├── RawBundleResult/            # 原生文件结果实现
│   └── VirtualBundleResult/        # 虚拟资源包结果实现
│
├── WebGame/                        # WebGL 相关操作
│   └── Operation/                  # WebGL 专用操作类
│
├── FileSystemParameters.cs         # 文件系统参数
├── FileSystemParametersDefine.cs   # 参数名称常量定义
├── FileVerifyHelper.cs             # 文件校验辅助类
├── EFileVerifyLevel.cs             # 文件校验等级枚举
├── EFileVerifyResult.cs            # 文件校验结果枚举
└── EFileClearMode.cs               # 文件清理模式枚举
```

---

## 核心接口

### IFileSystem（文件系统接口）

定义文件系统的核心契约，所有文件系统实现都必须实现此接口。

```csharp
internal interface IFileSystem
{
    // 基本属性
    string PackageName { get; }     // 包裹名称
    string FileRoot { get; }        // 文件根目录
    int FileCount { get; }          // 文件数量

    // 生命周期
    void OnCreate(string packageName, string packageRoot);
    void OnDestroy();
    void SetParameter(string name, object value);

    // 异步操作
    FSInitializeFileSystemOperation InitializeFileSystemAsync();
    FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout);
    FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout);
    FSLoadBundleOperation LoadBundleFile(PackageBundle bundle);
    FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadFileOptions options);
    FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, ClearCacheFilesOptions options);

    // 文件查询
    bool Belong(PackageBundle bundle);      // 查询文件归属
    bool Exists(PackageBundle bundle);      // 查询文件是否存在
    bool NeedDownload(PackageBundle bundle);// 是否需要下载
    bool NeedUnpack(PackageBundle bundle);  // 是否需要解压
    bool NeedImport(PackageBundle bundle);  // 是否需要导入

    // 文件访问
    string GetBundleFilePath(PackageBundle bundle);
    byte[] ReadBundleFileData(PackageBundle bundle);
    string ReadBundleFileText(PackageBundle bundle);
}
```

---

## 操作类定义

### 文件系统操作基类

所有文件系统操作都继承自 `AsyncOperationBase`，提供异步执行能力。

| 操作类 | 说明 | 输出属性 |
|--------|------|----------|
| `FSInitializeFileSystemOperation` | 初始化文件系统 | - |
| `FSRequestPackageVersionOperation` | 请求包裹版本 | `PackageVersion` |
| `FSLoadPackageManifestOperation` | 加载资源清单 | `Manifest` |
| `FSLoadBundleOperation` | 加载资源包文件 | `Result`, `DownloadProgress`, `DownloadedBytes` |
| `FSDownloadFileOperation` | 下载文件 | `DownloadedBytes`, `DownloadProgress` |
| `FSClearCacheFilesOperation` | 清理缓存文件 | - |

### 资源加载操作基类

| 操作类 | 说明 | 输出属性 |
|--------|------|----------|
| `FSLoadAssetOperation` | 加载单个资源 | `Result` (Object) |
| `FSLoadAllAssetsOperation` | 加载所有资源 | `Result` (Object[]) |
| `FSLoadSubAssetsOperation` | 加载子资源 | `Result` (Object[]) |
| `FSLoadSceneOperation` | 加载场景 | `Result` (Scene) |

---

## BundleResult 资源包结果

### 基类定义

```csharp
internal abstract class BundleResult
{
    // 资源包管理
    public abstract void UnloadBundleFile();
    public abstract string GetBundleFilePath();
    public abstract byte[] ReadBundleFileData();
    public abstract string ReadBundleFileText();

    // 资源加载
    public abstract FSLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo);
    public abstract FSLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo);
    public abstract FSLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo);
    public abstract FSLoadSceneOperation LoadSceneOperation(AssetInfo assetInfo, LoadSceneParameters loadParams, bool suspendLoad);
}
```

### 实现类型

| 类型 | 说明 | 使用场景 |
|------|------|----------|
| `AssetBundleResult` | Unity AssetBundle 结果 | 正常构建的 AssetBundle 文件 |
| `RawBundleResult` | 原生文件结果 | 原生文件（非 AssetBundle） |
| `VirtualBundleResult` | 虚拟资源包结果 | 编辑器模拟模式 |

---

## 枚举定义

### EFileVerifyLevel（文件校验等级）

```csharp
public enum EFileVerifyLevel
{
    Low = 1,      // 仅验证文件存在
    Middle = 2,   // 验证文件大小
    High = 3      // 验证文件大小和 CRC
}
```

### EFileVerifyResult（文件校验结果）

```csharp
internal enum EFileVerifyResult
{
    Exception = -7,        // 验证异常
    CacheNotFound = -6,    // 未找到缓存信息
    InfoFileNotExisted = -5,  // 信息文件不存在
    DataFileNotExisted = -4,  // 数据文件不存在
    FileNotComplete = -3,     // 文件内容不足
    FileOverflow = -2,        // 文件内容溢出
    FileCrcError = -1,        // 文件 CRC 不匹配
    None = 0,                 // 默认状态
    Succeed = 1               // 验证成功
}
```

### EFileClearMode（文件清理模式）

```csharp
public enum EFileClearMode
{
    ClearAllBundleFiles,        // 清理所有资源文件
    ClearUnusedBundleFiles,     // 清理未使用的资源文件
    ClearBundleFilesByLocations,// 按地址清理资源文件
    ClearBundleFilesByTags,     // 按标签清理资源文件
    ClearAllManifestFiles,      // 清理所有清单文件
    ClearUnusedManifestFiles    // 清理未使用的清单文件
}
```

---

## 参数系统

### FileSystemParameters（文件系统参数）

用于创建和配置文件系统实例。

```csharp
public class FileSystemParameters
{
    public string FileSystemClass { get; }   // 文件系统类名
    public string PackageRoot { get; }       // 包裹根目录

    public void AddParameter(string name, object value);
}
```

### 参数名称常量

```csharp
public class FileSystemParametersDefine
{
    // 文件校验
    public const string FILE_VERIFY_LEVEL = "FILE_VERIFY_LEVEL";
    public const string FILE_VERIFY_MAX_CONCURRENCY = "FILE_VERIFY_MAX_CONCURRENCY";

    // 下载配置
    public const string DOWNLOAD_MAX_CONCURRENCY = "DOWNLOAD_MAX_CONCURRENCY";
    public const string DOWNLOAD_MAX_REQUEST_PER_FRAME = "DOWNLOAD_MAX_REQUEST_PER_FRAME";
    public const string DOWNLOAD_WATCH_DOG_TIME = "DOWNLOAD_WATCH_DOG_TIME";
    public const string RESUME_DOWNLOAD_MINMUM_SIZE = "RESUME_DOWNLOAD_MINMUM_SIZE";
    public const string RESUME_DOWNLOAD_RESPONSE_CODES = "RESUME_DOWNLOAD_RESPONSE_CODES";

    // 服务接口
    public const string REMOTE_SERVICES = "REMOTE_SERVICES";
    public const string DECRYPTION_SERVICES = "DECRYPTION_SERVICES";
    public const string MANIFEST_SERVICES = "MANIFEST_SERVICES";
    public const string COPY_LOCAL_FILE_SERVICES = "COPY_LOCAL_FILE_SERVICES";

    // 功能开关
    public const string DISABLE_CATALOG_FILE = "DISABLE_CATALOG_FILE";
    public const string DISABLE_UNITY_WEB_CACHE = "DISABLE_UNITY_WEB_CACHE";
    public const string DISABLE_ONDEMAND_DOWNLOAD = "DISABLE_ONDEMAND_DOWNLOAD";
    public const string APPEND_FILE_EXTENSION = "APPEND_FILE_EXTENSION";

    // 模拟模式
    public const string VIRTUAL_WEBGL_MODE = "VIRTUAL_WEBGL_MODE";
    public const string VIRTUAL_DOWNLOAD_MODE = "VIRTUAL_DOWNLOAD_MODE";
    public const string VIRTUAL_DOWNLOAD_SPEED = "VIRTUAL_DOWNLOAD_SPEED";
    public const string ASYNC_SIMULATE_MIN_FRAME = "ASYNC_SIMULATE_MIN_FRAME";
    public const string ASYNC_SIMULATE_MAX_FRAME = "ASYNC_SIMULATE_MAX_FRAME";

    // 其他
    public const string INSTALL_CLEAR_MODE = "INSTALL_CLEAR_MODE";
    public const string COPY_BUILDIN_PACKAGE_MANIFEST = "COPY_BUILDIN_PACKAGE_MANIFEST";
    public const string UNPACK_FILE_SYSTEM_ROOT = "UNPACK_FILE_SYSTEM_ROOT";
}
```

---

## 使用示例

### 创建文件系统参数

```csharp
// 编辑器文件系统
var editorParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
    packageRoot: "Assets/GameRes/Bundles"
);

// 内置文件系统
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(
    decryptionServices: new MyDecryptionServices()
);

// 缓存文件系统
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new MyRemoteServices(),
    decryptionServices: new MyDecryptionServices()
);

// WebGL 服务器文件系统
var webServerParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(
    disableUnityWebCache: true
);

// WebGL 远程文件系统
var webRemoteParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(
    remoteServices: new MyRemoteServices(),
    disableUnityWebCache: true
);
```

### 添加自定义参数

```csharp
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);

// 设置 UnityWebRequest 创建委托（用于证书/代理/自定义 Header 等）
cacheParams.AddParameter(FileSystemParametersDefine.UNITY_WEB_REQUEST_CREATOR, (UnityWebRequestCreator)((url, method) =>
{
    var request = new UnityEngine.Networking.UnityWebRequest(url, method);
    // 自定义配置...
    return request;
}));

// 设置自定义下载后端（若设置则不再创建默认 UnityWebRequestBackend）
// cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_BACKEND, myDownloadBackend);

// 设置文件校验级别
cacheParams.AddParameter(FileSystemParametersDefine.FILE_VERIFY_LEVEL, EFileVerifyLevel.High);

// 设置下载并发数
cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY, 16);

// 设置看门狗时间
cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_WATCH_DOG_TIME, 30);
```

---

## 文件系统实现

| 文件系统 | 说明 | 适用场景 |
|----------|------|----------|
| `DefaultEditorFileSystem` | 编辑器模拟文件系统 | 编辑器开发，快速迭代 |
| `DefaultBuildinFileSystem` | 内置资源文件系统 | StreamingAssets 内置资源 |
| `DefaultCacheFileSystem` | 缓存资源文件系统 | 下载并缓存的远程资源 |
| `DefaultUnpackFileSystem` | 解压资源文件系统 | 从内置解压到沙盒的资源 |
| `DefaultWebServerFileSystem` | WebGL 服务器文件系统 | WebGL 本地服务器资源 |
| `DefaultWebRemoteFileSystem` | WebGL 远程文件系统 | WebGL 远程 CDN 资源 |

---

## 设计模式

### 策略模式

通过 `IFileSystem` 接口实现可插拔的文件系统：

```
IFileSystem (接口)
    │
    ├── DefaultEditorFileSystem
    ├── DefaultBuildinFileSystem
    ├── DefaultCacheFileSystem
    │       └── DefaultUnpackFileSystem (继承)
    ├── DefaultWebServerFileSystem
    └── DefaultWebRemoteFileSystem
```

### 工厂模式

`FileSystemParameters` 通过反射创建文件系统实例：

```csharp
internal IFileSystem CreateFileSystem(string packageName)
{
    Type classType = Type.GetType(FileSystemClass);
    var instance = (IFileSystem)Activator.CreateInstance(classType, true);

    foreach (var param in CreateParameters)
        instance.SetParameter(param.Key, param.Value);

    instance.OnCreate(packageName, PackageRoot);
    return instance;
}
```

### 模板方法模式

`BundleResult` 定义资源加载的算法骨架，子类实现具体加载逻辑：

```
BundleResult (抽象基类)
    │
    ├── AssetBundleResult  → AssetBundle.LoadAssetAsync
    ├── RawBundleResult    → 直接读取文件
    └── VirtualBundleResult → AssetDatabase.LoadAssetAtPath
```

---

## 类继承关系

```
AsyncOperationBase
    │
    ├── FSInitializeFileSystemOperation
    ├── FSRequestPackageVersionOperation
    ├── FSLoadPackageManifestOperation
    ├── FSLoadBundleOperation
    │       └── FSLoadBundleCompleteOperation
    ├── FSDownloadFileOperation
    ├── FSClearCacheFilesOperation
    │       └── FSClearCacheFilesCompleteOperation
    ├── FSLoadAssetOperation
    ├── FSLoadAllAssetsOperation
    ├── FSLoadSubAssetsOperation
    └── FSLoadSceneOperation

BundleResult
    │
    ├── AssetBundleResult
    ├── RawBundleResult
    └── VirtualBundleResult
```

---

## 注意事项

1. **文件系统选择**：根据运行模式选择合适的文件系统组合
2. **参数配置**：合理配置校验等级和并发数，平衡性能和安全性
3. **自定义扩展**：可通过实现 `IFileSystem` 接口创建自定义文件系统
4. **WebGL 限制**：WebGL 平台不支持持久化缓存，需使用专用文件系统
5. **解密服务**：加密资源需要提供对应的解密服务实现
