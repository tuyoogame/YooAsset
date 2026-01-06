# DefaultEditorFileSystem 编辑器模拟文件系统

## 模块概述

DefaultEditorFileSystem 是 YooAsset 的**编辑器模拟文件系统**，专为 Unity 编辑器开发环境设计。该文件系统无需构建实际的 AssetBundle 文件，直接使用 Unity 的 AssetDatabase API 加载资源，实现快速迭代开发。

### 核心特性

- **无需构建资源包**：直接使用 AssetDatabase 加载资源
- **模拟下载流程**：支持模拟网络下载行为（用于 UI 调试）
- **模拟异步延迟**：可配置异步加载的模拟帧数
- **WebGL 模式模拟**：支持模拟 WebGL 平台行为

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **快速迭代** | 无需构建 AssetBundle，修改资源后立即生效 |
| **行为模拟** | 模拟真实环境的下载和加载行为 |
| **调试友好** | 支持 UI 进度条等功能的调试 |
| **零配置** | 开箱即用，最小化配置需求 |

---

## 文件结构

```
DefaultEditorFileSystem/
├── DefaultEditorFileSystem.cs           # 文件系统主类
├── DefaultEditorFileSystemDefine.cs     # 常量定义（预留）
└── Operation/                           # 操作类
    ├── DEFSInitializeOperation.cs       # 初始化操作
    ├── DEFSRequestPackageVersionOperation.cs  # 请求版本操作
    ├── DEFSLoadPackageManifestOperation.cs    # 加载清单操作
    ├── DEFSLoadBundleOperation.cs       # 加载资源包操作
    └── internal/                        # 内部操作类
        ├── DownloadVirutalBundleOperation.cs      # 虚拟下载操作
        ├── LoadEditorPackageVersionOperation.cs   # 加载版本文件操作
        ├── LoadEditorPackageHashOperation.cs      # 加载哈希文件操作
        └── LoadEditorPackageManifestOperation.cs  # 加载清单文件操作
```

---

## 核心类说明

### DefaultEditorFileSystem

编辑器模拟文件系统的主类，实现 `IFileSystem` 接口。

#### 基本属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `FileRoot` | `string` | 文件根目录（清单文件所在目录） |
| `FileCount` | `int` | 文件数量（始终返回 0） |
| `DownloadBackend` | `IDownloadBackend` | 下载后台接口 |

#### 自定义参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `VirtualWebGLMode` | `bool` | `false` | 模拟 WebGL 平台模式 |
| `VirtualDownloadMode` | `bool` | `false` | 模拟虚拟下载模式 |
| `VirtualDownloadSpeed` | `int` | `1024` | 模拟下载速度（字节/秒） |
| `AsyncSimulateMinFrame` | `int` | `1` | 异步加载最小模拟帧数 |
| `AsyncSimulateMaxFrame` | `int` | `1` | 异步加载最大模拟帧数 |

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
bool Belong(PackageBundle bundle);      // 始终返回 true
bool Exists(PackageBundle bundle);      // VirtualDownloadMode 时检查记录
bool NeedDownload(PackageBundle bundle);// VirtualDownloadMode 时返回未记录的文件
bool NeedUnpack(PackageBundle bundle);  // 始终返回 false
bool NeedImport(PackageBundle bundle);  // 始终返回 false

// 文件访问
string GetBundleFilePath(PackageBundle bundle);   // 返回资源路径
byte[] ReadBundleFileData(PackageBundle bundle);  // 读取文件二进制
string ReadBundleFileText(PackageBundle bundle);  // 读取文件文本
```

---

## 操作类说明

### DEFSInitializeOperation

初始化操作，立即完成（无需任何初始化工作）。

```
状态流程：InternalStart() → Status = Succeed
```

### DEFSRequestPackageVersionOperation

请求包裹版本操作，从本地版本文件读取版本号。

```
状态流程：
LoadPackageVersion
    └── LoadEditorPackageVersionOperation
            └── 读取 {PackageName}_Version.txt
                    ├── 成功 → PackageVersion = 文件内容
                    └── 失败 → Error
```

### DEFSLoadPackageManifestOperation

加载资源清单操作，从本地清单文件加载并解析清单。

```
状态流程：
LoadEditorPackageHash
    └── LoadEditorPackageHashOperation
            └── 读取 {PackageName}_{Version}.hash
                    ├── 成功 → PackageHash
                    └── 失败 → Error
                            ↓
LoadEditorPackageManifest
    └── LoadEditorPackageManifestOperation
            ├── 读取 {PackageName}_{Version}.bytes
            ├── 验证哈希值
            └── 反序列化清单
                    ├── 成功 → Manifest
                    └── 失败 → Error
```

### DEFSLoadBundleOperation

加载资源包操作，支持虚拟下载模式和异步模拟延迟。

```
状态流程：
CheckExist
    ├── 文件存在 → LoadAssetBundle
    └── 文件不存在 → DownloadFile
                            ↓
DownloadFile
    └── DownloadVirtualBundleOperation
            ├── 模拟下载进度
            └── 记录下载完成
                    ↓
LoadAssetBundle
    └── 等待模拟帧数
            ↓
CheckResult
    └── 创建 VirtualBundleResult → Status = Succeed
```

#### 状态机枚举

```csharp
private enum ESteps
{
    None,
    CheckExist,       // 检查文件是否存在
    DownloadFile,     // 下载文件（虚拟下载）
    AbortDownload,    // 中断下载
    LoadAssetBundle,  // 加载资源包（模拟延迟）
    CheckResult,      // 检查结果
    Done              // 完成
}
```

### DownloadVirtualBundleOperation

虚拟下载操作，模拟网络下载行为。

**特性：**
- 使用 `VirtualFileDownloader` 模拟下载进度
- 支持失败重试机制
- 下载完成后记录到 `_records` 字典

```
状态流程：
CheckExists
    ├── 文件已记录 → Status = Succeed
    └── 文件未记录 → CreateRequest
                            ↓
CreateRequest
    └── DownloadSimulateRequestArgs
            ├── URL = BundleName
            ├── FileSize = Bundle.FileSize
            └── DownloadSpeed = VirtualDownloadSpeed
                    ↓
CheckRequest
    ├── 下载成功 → RecordDownloadFile() → Status = Succeed
    └── 下载失败 → TryAgain 或 Status = Failed
```

---

## 内部操作类

### LoadEditorPackageVersionOperation

从本地文件加载包裹版本号。

| 属性 | 说明 |
|------|------|
| `PackageVersion` | 读取到的版本号字符串 |

### LoadEditorPackageHashOperation

从本地文件加载包裹哈希值。

| 属性 | 说明 |
|------|------|
| `PackageHash` | 读取到的哈希值字符串 |

### LoadEditorPackageManifestOperation

加载并反序列化资源清单。

| 属性 | 说明 |
|------|------|
| `Manifest` | 反序列化后的清单对象 |

**处理流程：**
1. 读取清单二进制文件
2. 使用哈希值验证文件完整性
3. 反序列化为 `PackageManifest` 对象

---

## 工作原理

### 资源加载机制

```
用户请求加载资源
        │
        ▼
DefaultEditorFileSystem.LoadBundleFile()
        │
        ▼
DEFSLoadBundleOperation
        │
        ▼
创建 VirtualBundleResult
        │
        ▼
VirtualBundleResult.LoadAssetAsync()
        │
        ▼
VirtualBundleLoadAssetOperation
        │
        ▼
AssetDatabase.LoadAssetAtPath()  ← Unity 编辑器 API
        │
        ▼
返回资源对象
```

### 虚拟下载模式

当 `VirtualDownloadMode = true` 时：

1. **首次加载**：资源被视为"未下载"，需要执行虚拟下载
2. **虚拟下载**：使用 `VirtualFileDownloader` 模拟下载进度
3. **记录完成**：下载完成后将 BundleGUID 记录到 `_records` 字典
4. **后续加载**：检查 `_records` 字典，已记录的资源直接加载

```csharp
// 记录下载完成的文件
protected readonly Dictionary<string, string> _records;

// 检查文件是否存在
public virtual bool Exists(PackageBundle bundle)
{
    if (VirtualDownloadMode)
        return _records.ContainsKey(bundle.BundleGUID);
    else
        return true;
}
```

### 异步模拟延迟

通过 `AsyncSimulateMinFrame` 和 `AsyncSimulateMaxFrame` 参数模拟异步加载延迟：

```csharp
// 获取随机模拟帧数
public int GetAsyncSimulateFrame()
{
    return UnityEngine.Random.Range(AsyncSimulateMinFrame, AsyncSimulateMaxFrame + 1);
}

// 在 DEFSLoadBundleOperation 中等待
if (_steps == ESteps.LoadAssetBundle)
{
    if (_asyncSimulateFrame <= 0)
        _steps = ESteps.CheckResult;
    else
        _asyncSimulateFrame--;
}
```

---

## 使用示例

### 基础配置

```csharp
// 创建编辑器文件系统参数
var editorParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
    packageRoot: "Assets/GameRes/Bundles/DefaultPackage"
);

// 初始化包裹
var initParams = new EditorSimulateModeParameters();
initParams.EditorFileSystemParameters = editorParams;
var initOp = package.InitializeAsync(initParams);
```

### 启用虚拟下载模式

```csharp
var editorParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
    packageRoot: "Assets/GameRes/Bundles/DefaultPackage"
);

// 启用虚拟下载模式
editorParams.AddParameter(FileSystemParametersDefine.VIRTUAL_DOWNLOAD_MODE, true);
editorParams.AddParameter(FileSystemParametersDefine.VIRTUAL_DOWNLOAD_SPEED, 1024 * 100); // 100KB/s
```

### 配置异步模拟延迟

```csharp
var editorParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
    packageRoot: "Assets/GameRes/Bundles/DefaultPackage"
);

// 设置异步加载延迟 1-3 帧
editorParams.AddParameter(FileSystemParametersDefine.ASYNC_SIMULATE_MIN_FRAME, 1);
editorParams.AddParameter(FileSystemParametersDefine.ASYNC_SIMULATE_MAX_FRAME, 3);
```

### 模拟 WebGL 模式

```csharp
var editorParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
    packageRoot: "Assets/GameRes/Bundles/DefaultPackage"
);

// 启用 WebGL 模拟模式
editorParams.AddParameter(FileSystemParametersDefine.VIRTUAL_WEBGL_MODE, true);
```

---

## 参数常量

```csharp
// 模拟模式参数
FileSystemParametersDefine.VIRTUAL_WEBGL_MODE        // bool: 模拟 WebGL 模式
FileSystemParametersDefine.VIRTUAL_DOWNLOAD_MODE     // bool: 模拟下载模式
FileSystemParametersDefine.VIRTUAL_DOWNLOAD_SPEED    // int: 模拟下载速度（字节/秒）
FileSystemParametersDefine.ASYNC_SIMULATE_MIN_FRAME  // int: 异步模拟最小帧数
FileSystemParametersDefine.ASYNC_SIMULATE_MAX_FRAME  // int: 异步模拟最大帧数
```

---

## 类继承关系

```
IFileSystem
    └── DefaultEditorFileSystem

FSInitializeFileSystemOperation
    └── DEFSInitializeOperation

FSRequestPackageVersionOperation
    └── DEFSRequestPackageVersionOperation

FSLoadPackageManifestOperation
    └── DEFSLoadPackageManifestOperation

FSLoadBundleOperation
    └── DEFSLoadBundleOperation

FSDownloadFileOperation
    └── DownloadVirtualBundleOperation

AsyncOperationBase
    ├── LoadEditorPackageVersionOperation
    ├── LoadEditorPackageHashOperation
    └── LoadEditorPackageManifestOperation

BundleResult
    └── VirtualBundleResult  ← 编辑器模式专用
```

---

## 注意事项

1. **仅限编辑器**：此文件系统仅在 Unity 编辑器中有效
2. **需要构建清单**：虽然不需要构建 AssetBundle，但需要构建资源清单文件
3. **VirtualBundle 类型**：只支持 `EBuildBundleType.VirtualBundle` 类型的资源包
4. **WebGL 模式限制**：`VirtualWebGLMode` 下不支持同步加载（`WaitForAsyncComplete`）
5. **性能差异**：编辑器模式下的加载性能与真机不同，仅供开发调试使用
