# DefaultBuildinFileSystem 内置文件系统

## 模块概述

DefaultBuildinFileSystem 是 YooAsset 的**内置资源文件系统**，用于管理打包到应用程序中的资源文件（StreamingAssets）。该文件系统支持 AssetBundle 和原生文件的加载，并内置解压文件系统以处理 Android/OpenHarmony 平台的特殊需求。

### 核心特性

- **内置资源管理**：管理 StreamingAssets 目录下的资源文件
- **Catalog 目录系统**：使用目录文件快速查询内置资源
- **自动解压机制**：Android/OpenHarmony 平台自动解压加密和原生文件
- **清单拷贝功能**：支持将内置清单拷贝到沙盒目录
- **加密资源支持**：通过解密服务接口支持加密资源加载

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **跨平台支持** | 统一处理各平台 StreamingAssets 的访问差异 |
| **高效查询** | 通过 Catalog 文件快速判断资源是否内置 |
| **自动解压** | 自动处理 Android 平台无法直接访问的资源 |
| **灵活配置** | 支持多种参数配置适应不同需求 |

---

## 文件结构

```
DefaultBuildinFileSystem/
├── DefaultBuildinFileSystem.cs           # 文件系统主类
├── DefaultBuildinFileSystemDefine.cs     # 常量定义
├── DefaultBuildinFileCatalog.cs          # 内置资源目录结构
├── CatalogDefine.cs                      # Catalog 文件格式定义
├── CatalogTools.cs                       # Catalog 序列化工具
└── Operation/                            # 操作类
    ├── DBFSInitializeOperation.cs        # 初始化操作
    ├── DBFSRequestPackageVersionOperation.cs   # 请求版本操作
    ├── DBFSLoadPackageManifestOperation.cs     # 加载清单操作
    ├── DBFSLoadBundleOperation.cs        # 加载资源包操作
    └── internal/                         # 内部操作类
        ├── CopyBuildinFileOperation.cs           # 拷贝内置文件操作
        ├── LoadBuildinCatalogFileOperation.cs    # 加载 Catalog 文件操作
        ├── LoadBuildinPackageManifestOperation.cs# 加载清单文件操作
        ├── RequestBuildinPackageHashOperation.cs # 请求哈希文件操作
        └── RequestBuildinPackageVersionOperation.cs # 请求版本文件操作
```

---

## 核心类说明

### DefaultBuildinFileSystem

内置文件系统的主类，实现 `IFileSystem` 接口。

#### 基本属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `FileRoot` | `string` | 文件根目录（StreamingAssets 下的包裹目录） |
| `FileCount` | `int` | 已记录的内置文件数量 |
| `DownloadBackend` | `IDownloadBackend` | 下载后台接口 |

#### 自定义参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `InstallClearMode` | `EOverwriteInstallClearMode` | `ClearAllManifestFiles` | 覆盖安装时的缓存清理模式 |
| `FileVerifyLevel` | `EFileVerifyLevel` | `Middle` | 文件校验级别 |
| `FileVerifyMaxConcurrency` | `int` | `32` | 文件校验最大并发数 |
| `AppendFileExtension` | `bool` | `false` | 是否追加文件扩展名 |
| `DisableCatalogFile` | `bool` | `false` | 禁用 Catalog 目录文件 |
| `CopyBuildinPackageManifest` | `bool` | `false` | 是否拷贝内置清单到沙盒 |
| `CopyBuildinPackageManifestDestRoot` | `string` | `null` | 清单拷贝目标目录 |
| `UnpackFileSystemRoot` | `string` | `null` | 解压文件系统根目录 |
| `DecryptionServices` | `IDecryptionServices` | `null` | 解密服务接口 |
| `ManifestServices` | `IManifestRestoreServices` | `null` | 清单恢复服务接口 |
| `CopyLocalFileServices` | `ICopyLocalFileServices` | `null` | 本地文件拷贝服务接口 |

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
FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadFileOptions options);
FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, ClearCacheFilesOptions options);

// 文件查询
bool Belong(PackageBundle bundle);      // 检查是否属于内置文件
bool Exists(PackageBundle bundle);      // 检查文件是否存在
bool NeedDownload(PackageBundle bundle);// 始终返回 false
bool NeedUnpack(PackageBundle bundle);  // 检查是否需要解压
bool NeedImport(PackageBundle bundle);  // 始终返回 false

// 文件访问
string GetBundleFilePath(PackageBundle bundle);
byte[] ReadBundleFileData(PackageBundle bundle);
string ReadBundleFileText(PackageBundle bundle);
```

---

## Catalog 目录系统

### DefaultBuildinFileCatalog

内置资源目录结构，记录所有内置资源文件的信息。

```csharp
[Serializable]
internal class DefaultBuildinFileCatalog
{
    [Serializable]
    public class FileWrapper
    {
        public string BundleGUID;   // 资源包 GUID
        public string FileName;     // 文件名
    }

    public string FileVersion;      // 文件版本
    public string PackageName;      // 包裹名称
    public string PackageVersion;   // 包裹版本
    public List<FileWrapper> Wrappers;  // 文件列表
}
```

### CatalogDefine

Catalog 文件格式常量定义。

```csharp
internal class CatalogDefine
{
    public const int FileMaxSize = 104857600;   // 文件极限大小（100MB）
    public const uint FileSign = 0x133C5EE;     // 文件头标记
    public const string FileVersion = "1.0.0";  // 文件格式版本
}
```

### CatalogTools

Catalog 文件的序列化和反序列化工具。

| 方法 | 说明 |
|------|------|
| `CreateCatalogFile()` | 生成包裹的内置资源目录文件（编辑器） |
| `CreateEmptyCatalogFile()` | 生成空的内置资源目录文件（编辑器） |
| `SerializeToJson()` | 序列化为 JSON 文件 |
| `DeserializeFromJson()` | 从 JSON 文件反序列化 |
| `SerializeToBinary()` | 序列化为二进制文件 |
| `DeserializeFromBinary()` | 从二进制文件反序列化 |

---

## 操作类说明

### DBFSInitializeOperation

初始化操作，执行以下步骤：

```
状态流程：
┌─────────────────────────────────────────────────────────────┐
│  CopyBuildinPackageManifest = true ?                        │
│      │                                                      │
│      ├── Yes ──► LoadBuildinPackageVersion                  │
│      │               └── RequestBuildinPackageVersionOp     │
│      │                       ↓                              │
│      │           CopyBuildinPackageHash                     │
│      │               └── CopyBuildinFileOperation           │
│      │                       ↓                              │
│      │           CopyBuildinPackageManifest                 │
│      │               └── CopyBuildinFileOperation           │
│      │                       ↓                              │
│      └── No ─────────────────┘                              │
│                              ↓                              │
│                  InitUnpackFileSystem                       │
│                      └── DefaultUnpackFileSystem.Init       │
│                              ↓                              │
│                  DisableCatalogFile = true ?                │
│                      ├── Yes ──► Done (Succeed)             │
│                      └── No ──► LoadCatalogFile             │
│                                     └── LoadBuildinCatalog  │
│                                             ↓               │
│                                     RecordCatalogFile       │
│                                             ↓               │
│                                     Done (Succeed)          │
└─────────────────────────────────────────────────────────────┘
```

### DBFSLoadBundleOperation

加载资源包操作，支持多种资源类型。

#### DBFSLoadAssetBundleOperation

加载 AssetBundle 文件。

```
状态流程：
LoadAssetBundle
    ├── 加密资源 ──► DecryptionServices.LoadAssetBundle[Async]
    └── 普通资源 ──► AssetBundle.LoadFromFile[Async]
            ↓
CheckResult
    ├── 成功 ──► AssetBundleResult
    └── 失败 ──► Error
```

#### DBFSLoadRawBundleOperation

加载原生文件。

```
状态流程：
LoadBuildinRawBundle
    ├── Android 平台 ──► Error（不支持直接读取）
    └── 其他平台 ──► RawBundleResult
```

#### DBFSLoadInstantBundleOperation

加载团结引擎（Tuanjie）专用资源包（需要 `TUANJIE_1_7_OR_NEWER` 宏）。

---

## 内部操作类

### LoadBuildinCatalogFileOperation

加载 Catalog 目录文件。

```
状态流程：
TryLoadFileData
    ├── 文件存在 ──► File.ReadAllBytes
    └── 文件不存在 ──► RequestFileData (UnityWebRequest)
            ↓
LoadCatalog
    └── CatalogTools.DeserializeFromBinary
```

### CopyBuildinFileOperation

拷贝内置文件到目标路径。

```
状态流程：
CheckFileExist
    ├── 目标已存在 ──► Done (Succeed)
    └── 目标不存在 ──► TryCopyFile
            ↓
TryCopyFile
    ├── 源文件存在 ──► File.Copy
    └── 源文件不存在 ──► UnpackFile (UnityWebRequest)
```

---

## 解压机制

### 自动解压条件

在 Android/OpenHarmony 平台上，以下情况需要解压到沙盒：

```csharp
protected virtual bool IsUnpackBundleFile(PackageBundle bundle)
{
#if UNITY_ANDROID || UNITY_OPENHARMONY
    if (bundle.Encrypted)       // 加密资源
        return true;
    if (bundle.BundleType == RawBundle)  // 原生文件
        return true;
    return false;
#else
    return false;
#endif
}
```

### 解压文件系统

内置文件系统在创建时会自动创建一个 `DefaultUnpackFileSystem` 实例：

```csharp
public virtual void OnCreate(string packageName, string packageRoot)
{
    // 创建解压文件系统
    var remoteServices = new DefaultUnpackRemoteServices(_packageRoot);
    _unpackFileSystem = new DefaultUnpackFileSystem();
    _unpackFileSystem.SetParameter(REMOTE_SERVICES, remoteServices);
    _unpackFileSystem.SetParameter(FILE_VERIFY_LEVEL, FileVerifyLevel);
    // ... 其他参数
    _unpackFileSystem.OnCreate(packageName, UnpackFileSystemRoot);
}
```

---

## 平台差异处理

### Android 平台限制

```
┌─────────────────────────────────────────────────────────────┐
│                     Android 平台特殊处理                     │
├─────────────────────────────────────────────────────────────┤
│  StreamingAssets 文件位于 APK 压缩包内，无法直接访问：        │
│                                                             │
│  ✓ AssetBundle.LoadFromFile    支持（Unity 内部处理）        │
│  ✗ File.ReadAllBytes           不支持                       │
│  ✗ File.Exists                 不支持                       │
│  ✓ UnityWebRequest             支持（jar:file:// 协议）      │
│                                                             │
│  解决方案：                                                  │
│  1. 加密资源 → 自动解压到沙盒                                │
│  2. 原生文件 → 自动解压到沙盒                                │
│  3. Catalog  → 使用 UnityWebRequest 读取                    │
└─────────────────────────────────────────────────────────────┘
```

### WebGL 平台

```csharp
#if UNITY_WEBGL
    _steps = ESteps.Done;
    Status = EOperationStatus.Failed;
    Error = $"{nameof(DefaultBuildinFileSystem)} is not support WEBGL platform !";
#endif
```

WebGL 平台不支持 DefaultBuildinFileSystem，应使用 `DefaultWebServerFileSystem`。

---

## 使用示例

### 基础配置

```csharp
// 创建内置文件系统参数
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 初始化包裹
var initParams = new OfflinePlayModeParameters();
initParams.BuildinFileSystemParameters = buildinParams;
var initOp = package.InitializeAsync(initParams);
```

### 配置解密服务

```csharp
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 设置解密服务
buildinParams.AddParameter(
    FileSystemParametersDefine.DECRYPTION_SERVICES,
    new MyDecryptionServices()
);
```

### 配置清单拷贝

```csharp
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 启用清单拷贝（用于离线模式切换到联机模式）
buildinParams.AddParameter(
    FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST,
    true
);

// 可选：指定拷贝目标目录
buildinParams.AddParameter(
    FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT,
    "/custom/path"
);
```

### 禁用 Catalog 文件

```csharp
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 禁用 Catalog（所有资源视为内置）
buildinParams.AddParameter(
    FileSystemParametersDefine.DISABLE_CATALOG_FILE,
    true
);
```

### 配置解压文件系统根目录

```csharp
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 设置解压文件系统的根目录
buildinParams.AddParameter(
    FileSystemParametersDefine.UNPACK_FILE_SYSTEM_ROOT,
    "/custom/unpack/path"
);
```

---

## 参数常量

```csharp
// 安装清理
FileSystemParametersDefine.INSTALL_CLEAR_MODE              // EOverwriteInstallClearMode

// 文件校验
FileSystemParametersDefine.FILE_VERIFY_LEVEL               // EFileVerifyLevel
FileSystemParametersDefine.FILE_VERIFY_MAX_CONCURRENCY     // int

// 文件配置
FileSystemParametersDefine.APPEND_FILE_EXTENSION           // bool
FileSystemParametersDefine.DISABLE_CATALOG_FILE            // bool

// 清单拷贝
FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST   // bool
FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT  // string

// 解压配置
FileSystemParametersDefine.UNPACK_FILE_SYSTEM_ROOT         // string

// 服务接口
FileSystemParametersDefine.DECRYPTION_SERVICES             // IDecryptionServices
FileSystemParametersDefine.MANIFEST_SERVICES               // IManifestRestoreServices
FileSystemParametersDefine.COPY_LOCAL_FILE_SERVICES        // ICopyLocalFileServices
```

---

## 类继承关系

```
IFileSystem
    └── DefaultBuildinFileSystem
            └── (内部持有) DefaultUnpackFileSystem

FSInitializeFileSystemOperation
    └── DBFSInitializeOperation

FSRequestPackageVersionOperation
    └── DBFSRequestPackageVersionOperation

FSLoadPackageManifestOperation
    └── DBFSLoadPackageManifestOperation

FSLoadBundleOperation
    ├── DBFSLoadAssetBundleOperation
    ├── DBFSLoadRawBundleOperation
    └── DBFSLoadInstantBundleOperation (Tuanjie)

AsyncOperationBase
    ├── LoadBuildinCatalogFileOperation
    ├── CopyBuildinFileOperation
    ├── LoadBuildinPackageManifestOperation
    ├── RequestBuildinPackageHashOperation
    └── RequestBuildinPackageVersionOperation

BundleResult
    ├── AssetBundleResult  ← AssetBundle 资源
    └── RawBundleResult    ← 原生文件
```

---

## 注意事项

1. **WebGL 不支持**：DefaultBuildinFileSystem 不支持 WebGL 平台
2. **Android 限制**：Android 平台无法直接读取 StreamingAssets 中的原生文件
3. **Catalog 文件**：构建时需要生成 Catalog 文件，否则需要禁用 Catalog 功能
4. **解压目录**：解压的文件存储在 `UnpackFileSystemRoot` 指定的目录
5. **加密资源**：加密资源在 Android/OpenHarmony 平台会自动解压到沙盒
6. **清单拷贝**：启用清单拷贝可以支持从离线模式平滑切换到联机模式
