# DefaultCacheFileSystem 缓存文件系统

## 模块概述

DefaultCacheFileSystem 是 YooAsset 的**缓存文件系统**，负责管理从远程服务器下载并缓存到本地沙盒的资源文件。该文件系统是联机运行模式（HostPlayMode）的核心组件，提供完整的下载、验证、缓存和加载功能。

### 核心特性

- **智能缓存管理**：基于 GUID 的文件索引，支持增量更新
- **断点续传**：大文件下载支持从断点继续
- **多线程验证**：后台线程验证文件完整性，不阻塞主线程
- **并发下载**：可配置的下载并发数和请求速率
- **覆盖安装检测**：App 版本变更时自动清理过期缓存
- **加密支持**：支持加密资源包的解密加载

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **高性能** | 多线程验证、并发下载、路径缓存优化 |
| **高可靠** | CRC/Hash 验证、损坏文件自动清理、加载失败重试 |
| **可扩展** | 支持自定义解密服务、远程服务、本地拷贝服务 |
| **易配置** | 丰富的参数配置，适应不同网络环境 |

---

## 文件结构

```
DefaultCacheFileSystem/
├── DefaultCacheFileSystem.cs               # 文件系统主类
├── DefaultCacheFileSystemDefine.cs         # 常量定义
├── EOverwriteInstallClearMode.cs           # 覆盖安装清理模式枚举
├── ApplicationFootPrint.cs                 # 应用版本足迹
├── Elements/                               # 元素类
│   ├── RecordFileElement.cs                # 缓存文件记录元素
│   ├── TempFileElement.cs                  # 临时文件元素
│   └── VerifyFileElement.cs                # 验证文件元素
└── Operation/                              # 操作类
    ├── DCFSInitializeOperation.cs          # 初始化操作
    ├── DCFSRequestPackageVersionOperation.cs   # 请求版本操作
    ├── DCFSLoadPackageManifestOperation.cs     # 加载清单操作
    ├── DCFSLoadBundleOperation.cs          # 加载资源包操作
    └── internal/                           # 内部操作类
        ├── SearchCacheFilesOperation.cs        # 搜索缓存文件
        ├── VerifyCacheFilesOperation.cs        # 验证缓存文件
        ├── VerifyTempFileOperation.cs          # 验证临时文件
        ├── DownloadPackageHashOperation.cs     # 下载哈希文件
        ├── DownloadPackageManifestOperation.cs # 下载清单文件
        ├── DownloadPackageBundleOperation.cs   # 下载资源包
        ├── LoadCachePackageHashOperation.cs    # 加载缓存哈希
        ├── LoadCachePackageManifestOperation.cs    # 加载缓存清单
        ├── ClearAllCacheBundleFilesOperation.cs    # 清理所有缓存
        ├── ClearUnusedCacheBundleFilesOperation.cs # 清理未使用缓存
        ├── ClearCacheBundleFilesByTagsOperaiton.cs # 按标签清理
        ├── ClearCacheBundleFilesByLocationsOperaiton.cs # 按位置清理
        ├── ClearAllCacheManifestFilesOperation.cs  # 清理所有清单
        ├── ClearUnusedCacheManifestFilesOperation.cs   # 清理未使用清单
        └── Scheduler/                      # 下载调度器
            ├── DownloadSchedulerOperation.cs       # 下载调度器
            ├── DownloadAndCacheFileOperation.cs    # 下载并缓存基类
            ├── DownloadAndCacheRemoteFileOperation.cs  # 远程文件下载
            └── DownloadAndCacheLocalFileOperation.cs   # 本地文件拷贝
```

---

## 核心类说明

### DefaultCacheFileSystem

缓存文件系统的主类，实现 `IFileSystem` 接口。

#### 基本属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `FileRoot` | `string` | 缓存根目录 |
| `FileCount` | `int` | 已缓存文件数量 |
| `DownloadBackend` | `IDownloadBackend` | 下载后台接口 |
| `DownloadScheduler` | `DownloadSchedulerOperation` | 下载调度器 |

#### 自定义参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RemoteServices` | `IRemoteServices` | - | 远程服务接口（必需） |
| `InstallClearMode` | `EOverwriteInstallClearMode` | `ClearAllManifestFiles` | 覆盖安装缓存清理模式 |
| `FileVerifyLevel` | `EFileVerifyLevel` | `Middle` | 初始化时文件校验级别 |
| `FileVerifyMaxConcurrency` | `int` | `32` | 文件校验最大并发数（1-256） |
| `AppendFileExtension` | `bool` | `false` | 数据文件追加文件扩展名 |
| `DisableOnDemandDownload` | `bool` | `false` | 禁用边玩边下机制 |
| `DownloadMaxConcurrency` | `int` | `10` | 最大并发下载数（1-64） |
| `DownloadMaxRequestPerFrame` | `int` | `5` | 每帧最大请求数（1-20） |
| `DownloadWatchDogTime` | `int` | `0` | 下载看门狗超时时间（秒） |
| `ResumeDownloadMinimumSize` | `long` | `long.MaxValue` | 启用断点续传的最小文件大小 |
| `ResumeDownloadResponseCodes` | `List<long>` | `null` | 断点续传关注的HTTP错误码 |
| `DecryptionServices` | `IDecryptionServices` | `null` | 解密服务接口 |
| `ManifestServices` | `IManifestRestoreServices` | `null` | 清单服务接口 |
| `CopyLocalFileServices` | `ICopyLocalFileServices` | `null` | 本地文件拷贝服务 |

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
bool Belong(PackageBundle bundle);       // 始终返回 true（保底加载）
bool Exists(PackageBundle bundle);       // 检查文件是否已缓存
bool NeedDownload(PackageBundle bundle); // 检查是否需要下载
bool NeedUnpack(PackageBundle bundle);   // 始终返回 false
bool NeedImport(PackageBundle bundle);   // 检查是否需要导入

// 文件访问
string GetBundleFilePath(PackageBundle bundle);
byte[] ReadBundleFileData(PackageBundle bundle);
string ReadBundleFileText(PackageBundle bundle);
```

---

## 缓存目录结构

```
{CacheRoot}/{PackageName}/
├── BundleFiles/                           # 资源包文件目录
│   ├── {Hash[0:2]}/                       # 哈希前两位分组（256个目录）
│   │   ├── {BundleGUID}/                  # 资源包 GUID 目录
│   │   │   ├── __data                     # 数据文件（或 __data.bundle）
│   │   │   └── __info                     # 信息文件（CRC + Size）
│   │   └── ...
│   └── ...
├── ManifestFiles/                         # 清单文件目录
│   ├── {PackageName}_{Version}.bytes      # 清单二进制文件
│   ├── {PackageName}_{Version}.hash       # 清单哈希文件
│   └── __app_footprint.txt                # 应用版本足迹文件
└── TempFiles/                             # 临时文件目录
    ├── {BundleGUID}                       # 下载中的临时文件
    └── ...
```

### 信息文件格式（__info）

```
| 字段 | 类型 | 大小 | 说明 |
|------|------|------|------|
| DataFileCRC | uint32 | 4 bytes | 数据文件 CRC |
| DataFileSize | int64 | 8 bytes | 数据文件大小 |
```

---

## 操作类说明

### DCFSInitializeOperation

初始化操作，执行完整的缓存系统初始化流程。

```
状态流程：
CheckAppFootPrint
    ├── 版本相同 → 继续
    └── 版本不同 → 根据 InstallClearMode 清理缓存
            ↓
SearchCacheFiles
    └── SearchCacheFilesOperation
            └── 遍历 BundleFiles 目录
                    └── 收集需要验证的文件
                            ↓
VerifyCacheFiles
    └── VerifyCacheFilesOperation（多线程）
            ├── 验证成功 → 记录到 _records
            └── 验证失败 → 删除损坏文件
                    ↓
CreateDownloadScheduler
    └── 创建 DownloadSchedulerOperation
            ↓
Done → Status = Succeed
```

#### 状态机枚举

```csharp
private enum ESteps
{
    None,
    CheckAppFootPrint,      // 检查应用版本足迹
    SearchCacheFiles,       // 搜索缓存文件
    VerifyCacheFiles,       // 验证缓存文件
    CreateDownloadScheduler,// 创建下载调度器
    Done                    // 完成
}
```

### DCFSLoadAssetBundleOperation

加载 AssetBundle 操作，支持按需下载和多重容错机制。

```
状态流程：
CheckExist
    ├── 已缓存 → LoadAssetBundle
    └── 未缓存 → 检查 DisableOnDemandDownload
            ├── 禁用 → Failed
            └── 启用 → DownloadFile
                        ↓
DownloadFile
    └── DownloadFileAsync()
            ├── 下载成功 → LoadAssetBundle
            └── 下载失败 → Failed
                        ↓
LoadAssetBundle
    ├── 未加密 → AssetBundle.LoadFromFile[Async]
    └── 已加密 → DecryptionServices.LoadAssetBundle[Async]
                        ↓
CheckResult
    ├── 加载成功 → AssetBundleResult → Succeed
    └── 加载失败 → 验证文件完整性
            ├── 验证通过 → LoadFromMemory 重试
            └── 验证失败 → 删除损坏文件 → Failed
```

#### 移动平台容错机制

```csharp
// 注意：在安卓移动平台，华为和三星真机上有极小概率加载资源包失败。
// 说明：大多数情况在首次安装下载资源到沙盒内，游戏过程中切换到后台再回到游戏内有很大概率触发！
string filePath = _fileSystem.GetCacheBundleFileLoadPath(_bundle);
byte[] fileData = FileUtility.ReadAllBytes(filePath);
if (fileData != null && fileData.Length > 0)
{
    _assetBundle = AssetBundle.LoadFromMemory(fileData);
    // ...
}
```

### DCFSLoadRawBundleOperation

加载原生资源包操作，处理文件格式变更场景。

```csharp
// 注意：缓存的原生文件的格式，可能会在业务端根据需求发生变动！
// 注意：这里需要校验文件格式，如果不一致对本地文件进行修正！
if (File.Exists(filePath) == false)
{
    var recordFileElement = _fileSystem.GetRecordFileElement(_bundle);
    File.Move(recordFileElement.DataFilePath, filePath);
}
```

---

## 下载调度器

### DownloadSchedulerOperation

管理所有活跃的下载任务，控制并发数量。

#### 核心属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Paused` | `bool` | 是否已暂停 |
| `ActiveDownloadCount` | `int` | 当前活跃的下载任务数 |
| `PendingDownloadCount` | `int` | 当前等待中的下载任务数 |

#### 工作原理

```
InternalUpdate()
    │
    ├── 1. 驱动下载后台 _fileSystem.DownloadBackend.Update()
    │
    ├── 2. 遍历下载器集合
    │       ├── 已完成 → 加入移除列表
    │       └── RefCount <= 0 → 中止并移除
    │
    ├── 3. 移除已完成/中止的下载器
    │
    └── 4. 启动新下载任务（如未暂停）
            ├── 计算可启动数量 = min(maxConcurrency - active, maxRequestPerFrame)
            └── 启动等待中的下载器
```

#### 引用计数机制

```csharp
// 查询旧的下载器（复用）
if (_downloaders.TryGetValue(bundle.BundleGUID, out var oldDownloader))
{
    oldDownloader.Reference();  // 引用计数 +1
    return oldDownloader;
}

// 创建新的下载器
DownloadAndCacheFileOperation newDownloader;
// ...
newDownloader.Reference();  // 引用计数 +1
```

### DownloadAndCacheRemoteFileOperation

远程文件下载操作，支持断点续传。

```
状态流程：
CreateRequest
    ├── 文件大小 >= ResumeDownloadMinimumSize
    │       └── CreateResumeRequest（断点续传）
    └── 文件大小 < ResumeDownloadMinimumSize
            └── CreateNormalRequest（普通下载）
                    ↓
CheckRequest
    ├── 下载成功 → VerifyBundleFile
    └── 下载失败 → ClearTempFileWhenError → Failed
                    ↓
VerifyBundleFile
    └── VerifyTempFileOperation（多线程验证）
            ├── 验证成功 → CacheBundleFile
            └── 验证失败 → 删除临时文件 → Failed
                    ↓
CacheBundleFile
    └── WriteCacheBundleFile()
            ├── 成功 → 删除临时文件 → Succeed
            └── 失败 → Failed
```

#### 断点续传实现

```csharp
private IDownloadRequest CreateResumeRequest()
{
    // 获取下载起始位置
    if (File.Exists(_tempFilePath))
    {
        FileInfo fileInfo = new FileInfo(_tempFilePath);
        if (fileInfo.Length >= _bundle.FileSize)
        {
            File.Delete(_tempFilePath);  // 文件已完整，删除重下
        }
        else
        {
            _fileOriginLength = fileInfo.Length;  // 记录已下载大小
        }
    }

    var args = new DownloadFileRequestArgs(
        URL, _tempFilePath, timeout, watchdogTime,
        appendToFile: true,          // 追加写入
        removeFileOnAbort: false,    // 中止时保留文件
        resumeFromBytes: _fileOriginLength  // 断点位置
    );
    return _fileSystem.DownloadBackend.CreateFileRequest(args);
}
```

---

## 缓存验证系统

### VerifyCacheFilesOperation

多线程缓存文件验证，在初始化时执行。

#### 验证流程

```
InitVerify
    ├── 获取系统线程池信息
    └── 计算实际并发数 = min(threads, FileVerifyMaxConcurrency)
            ↓
UpdateVerify（循环）
    ├── 检测已完成的验证任务
    │       ├── 验证成功 → RecordBundleFile
    │       └── 验证失败 → DeleteFiles
    │
    └── 启动新的验证任务
            └── ThreadPool.QueueUserWorkItem(VerifyInThread)
```

#### 验证级别

```csharp
private EFileVerifyResult VerifyingCacheFile(VerifyFileElement element, EFileVerifyLevel verifyLevel)
{
    if (verifyLevel == EFileVerifyLevel.Low)
    {
        // Low：仅检查文件存在
        if (File.Exists(element.InfoFilePath) == false)
            return EFileVerifyResult.InfoFileNotExisted;
        if (File.Exists(element.DataFilePath) == false)
            return EFileVerifyResult.DataFileNotExisted;
        return EFileVerifyResult.Succeed;
    }
    else
    {
        // Middle/High：检查文件存在 + CRC/Size 验证
        _fileSystem.ReadBundleInfoFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
        return FileVerifyHelper.FileVerify(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
    }
}
```

### VerifyTempFileOperation

下载文件验证，在线程池中执行。

```csharp
private void VerifyInThread(object obj)
{
    TempFileElement element = (TempFileElement)obj;
    int result = (int)FileVerifyHelper.FileVerify(
        element.TempFilePath,
        element.TempFileSize,
        element.TempFileCRC,
        EFileVerifyLevel.High  // 始终使用高级验证
    );
    element.Result = result;  // 线程安全的结果设置
}
```

---

## 覆盖安装检测

### ApplicationFootPrint

应用版本足迹，用于检测 App 覆盖安装。

```csharp
public static bool IsDirty(DefaultCacheFileSystem fileSystem)
{
    string filePath = fileSystem.GetSandboxAppFootPrintFilePath();
    if (File.Exists(filePath))
    {
        string footPrint = FileUtility.ReadAllText(filePath);
        return IsValidVersion(footPrint) == false;  // 版本不同
    }
    return true;  // 文件不存在
}
```

### EOverwriteInstallClearMode

覆盖安装时的缓存清理模式。

| 枚举值 | 说明 |
|--------|------|
| `NeverClear` | 不清理任何缓存 |
| `ClearAllManifestFiles` | 清理所有清单文件（默认） |
| `ClearAllBundleAndManifestFiles` | 清理所有资源包和清单文件 |

---

## 缓存清理操作

### 清理模式对照表

| 清理模式 | 操作类 | 说明 |
|----------|--------|------|
| `ClearAllBundleFiles` | `ClearAllCacheBundleFilesOperation` | 清理所有缓存资源包 |
| `ClearUnusedBundleFiles` | `ClearUnusedCacheBundleFilesOperation` | 清理不在清单中的资源包 |
| `ClearBundleFilesByTags` | `ClearCacheBundleFilesByTagsOperaiton` | 按标签清理 |
| `ClearBundleFilesByLocations` | `ClearCacheBundleFilesByLocationsOperaiton` | 按资源路径清理 |
| `ClearAllManifestFiles` | `ClearAllCacheManifestFilesOperation` | 清理所有清单文件 |
| `ClearUnusedManifestFiles` | `ClearUnusedCacheManifestFilesOperation` | 清理未使用的清单文件 |

### 时间切片清理

```csharp
for (int i = _allBundleGUIDs.Count - 1; i >= 0; i--)
{
    string bundleGUID = _allBundleGUIDs[i];
    _fileSystem.DeleteCacheBundleFile(bundleGUID);
    _allBundleGUIDs.RemoveAt(i);

    // 检查操作系统是否繁忙，避免阻塞主线程
    if (OperationSystem.IsBusy)
        break;
}
```

---

## 元素类说明

### RecordFileElement

缓存文件记录元素，存储已验证的缓存文件信息。

```csharp
internal class RecordFileElement
{
    public readonly string InfoFilePath;   // 信息文件路径
    public readonly string DataFilePath;   // 数据文件路径
    public readonly uint DataFileCRC;      // 数据文件 CRC
    public readonly long DataFileSize;     // 数据文件大小

    public bool DeleteFolder();            // 删除整个文件夹
}
```

### TempFileElement

临时文件元素，用于下载文件验证。

```csharp
internal class TempFileElement
{
    public readonly string TempFilePath;   // 临时文件路径
    public readonly uint TempFileCRC;      // 预期 CRC
    public readonly long TempFileSize;     // 预期文件大小

    private int _result = 0;
    public int Result                      // 线程安全的验证结果
    {
        get => Interlocked.CompareExchange(ref _result, 0, 0);
        set => Interlocked.Exchange(ref _result, value);
    }
}
```

### VerifyFileElement

验证文件元素，用于缓存文件批量验证。

```csharp
internal class VerifyFileElement
{
    public readonly string PackageName;    // 包裹名称
    public readonly string BundleGUID;     // 资源包 GUID
    public readonly string FileRootPath;   // 文件根目录
    public readonly string DataFilePath;   // 数据文件路径
    public readonly string InfoFilePath;   // 信息文件路径

    public uint DataFileCRC;               // 数据文件 CRC
    public long DataFileSize;              // 数据文件大小

    private int _result = 0;
    public int Result                      // 线程安全的验证结果
    {
        get => Interlocked.CompareExchange(ref _result, 0, 0);
        set => Interlocked.Exchange(ref _result, value);
    }

    public void DeleteFiles();             // 删除所有相关文件
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

// 创建缓存文件系统参数
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 初始化包裹
var initParams = new HostPlayModeParameters();
initParams.BuildinFileSystemParameters = buildinParams;
initParams.CacheFileSystemParameters = cacheParams;
var initOp = package.InitializeAsync(initParams);
```

### 配置下载参数

```csharp
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 设置下载并发数
cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY, 8);

// 设置每帧最大请求数
cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_REQUEST_PER_FRAME, 3);

// 设置下载看门狗时间（秒）
cacheParams.AddParameter(FileSystemParametersDefine.DOWNLOAD_WATCH_DOG_TIME, 30);
```

### 配置断点续传

```csharp
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 启用断点续传的最小文件大小（1MB）
cacheParams.AddParameter(FileSystemParametersDefine.RESUME_DOWNLOAD_MINMUM_SIZE, 1024 * 1024);

// 断点续传关注的HTTP错误码（这些错误码时删除临时文件重新下载）
var responseCodes = new List<long> { 416 };  // Range Not Satisfiable
cacheParams.AddParameter(FileSystemParametersDefine.RESUME_DOWNLOAD_RESPONSE_CODES, responseCodes);
```

### 配置文件验证

```csharp
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 设置验证级别
cacheParams.AddParameter(FileSystemParametersDefine.FILE_VERIFY_LEVEL, EFileVerifyLevel.High);

// 设置验证并发数
cacheParams.AddParameter(FileSystemParametersDefine.FILE_VERIFY_MAX_CONCURRENCY, 64);
```

### 配置加密支持

```csharp
// 自定义解密服务
class GameDecryptionServices : IDecryptionServices
{
    public DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
    {
        // 实现解密逻辑
        byte[] data = DecryptFile(fileInfo.FileLoadPath);
        AssetBundle bundle = AssetBundle.LoadFromMemory(data);
        return new DecryptResult { Result = bundle };
    }
    // ...
}

var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 设置解密服务
cacheParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, new GameDecryptionServices());
```

### 配置覆盖安装行为

```csharp
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 覆盖安装时清理所有资源包和清单
cacheParams.AddParameter(
    FileSystemParametersDefine.INSTALL_CLEAR_MODE,
    EOverwriteInstallClearMode.ClearAllBundleAndManifestFiles
);
```

### 清理缓存

```csharp
// 清理所有缓存
var clearOp = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
await clearOp.ToTask();

// 清理未使用的缓存
var clearOp = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
await clearOp.ToTask();

// 按标签清理
var clearOp = package.ClearCacheFilesAsync(EFileClearMode.ClearBundleFilesByTags, "dlc1");
await clearOp.ToTask();
```

---

## 参数常量

```csharp
// 远程服务
FileSystemParametersDefine.REMOTE_SERVICES           // IRemoteServices: 远程服务接口

// 覆盖安装
FileSystemParametersDefine.INSTALL_CLEAR_MODE        // EOverwriteInstallClearMode: 清理模式

// 文件验证
FileSystemParametersDefine.FILE_VERIFY_LEVEL         // EFileVerifyLevel: 验证级别
FileSystemParametersDefine.FILE_VERIFY_MAX_CONCURRENCY  // int: 验证并发数

// 文件格式
FileSystemParametersDefine.APPEND_FILE_EXTENSION     // bool: 追加文件扩展名

// 下载控制
FileSystemParametersDefine.DISABLE_ONDEMAND_DOWNLOAD // bool: 禁用边玩边下
FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY  // int: 下载并发数
FileSystemParametersDefine.DOWNLOAD_MAX_REQUEST_PER_FRAME  // int: 每帧请求数
FileSystemParametersDefine.DOWNLOAD_WATCH_DOG_TIME   // int: 看门狗时间

// 断点续传
FileSystemParametersDefine.RESUME_DOWNLOAD_MINMUM_SIZE     // long: 最小文件大小
FileSystemParametersDefine.RESUME_DOWNLOAD_RESPONSE_CODES  // List<long>: 关注错误码

// 服务接口
FileSystemParametersDefine.DECRYPTION_SERVICES       // IDecryptionServices: 解密服务
FileSystemParametersDefine.MANIFEST_SERVICES         // IManifestRestoreServices: 清单服务
FileSystemParametersDefine.COPY_LOCAL_FILE_SERVICES  // ICopyLocalFileServices: 本地拷贝服务
```

---

## 类继承关系

```
IFileSystem
    └── DefaultCacheFileSystem

FSInitializeFileSystemOperation
    └── DCFSInitializeOperation

FSRequestPackageVersionOperation
    └── DCFSRequestPackageVersionOperation

FSLoadPackageManifestOperation
    └── DCFSLoadPackageManifestOperation

FSLoadBundleOperation
    ├── DCFSLoadAssetBundleOperation
    └── DCFSLoadRawBundleOperation

FSDownloadFileOperation
    └── DownloadPackageBundleOperation

FSClearCacheFilesOperation
    ├── ClearAllCacheBundleFilesOperation
    ├── ClearUnusedCacheBundleFilesOperation
    ├── ClearCacheBundleFilesByTagsOperaiton
    ├── ClearCacheBundleFilesByLocationsOperaiton
    ├── ClearAllCacheManifestFilesOperation
    └── ClearUnusedCacheManifestFilesOperation

AsyncOperationBase
    ├── DownloadSchedulerOperation
    ├── DownloadAndCacheFileOperation (abstract)
    │   ├── DownloadAndCacheRemoteFileOperation
    │   └── DownloadAndCacheLocalFileOperation
    ├── SearchCacheFilesOperation
    ├── VerifyCacheFilesOperation
    ├── VerifyTempFileOperation
    ├── DownloadPackageHashOperation
    ├── DownloadPackageManifestOperation
    ├── LoadCachePackageHashOperation
    ├── LoadCachePackageManifestOperation
    └── RequestRemotePackageVersionOperation

BundleResult
    ├── AssetBundleResult  ← AssetBundle 资源
    └── RawBundleResult    ← 原生文件资源
```

---

## 注意事项

1. **远程服务必需**：使用缓存文件系统必须配置 `IRemoteServices` 接口
2. **保底加载**：缓存文件系统的 `Belong()` 始终返回 true，作为资源加载的保底方案
3. **线程安全**：文件验证和下载使用后台线程，但核心逻辑仍在主线程执行
4. **并发限制**：合理设置下载和验证的并发数，避免系统过载
5. **移动平台**：Android 平台存在极小概率的 AssetBundle 加载失败，系统会自动尝试 LoadFromMemory 作为备选方案
6. **断点续传**：启用断点续传时，需要合理设置 `ResumeDownloadMinimumSize` 和 `ResumeDownloadResponseCodes`
7. **覆盖安装**：App 版本更新时会自动检测并根据配置清理缓存，避免旧缓存导致问题
