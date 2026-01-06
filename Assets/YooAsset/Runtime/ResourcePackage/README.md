# ResourcePackage 资源包管理模块

## 模块概述

ResourcePackage 是 YooAsset 资源管理系统的**核心模块**，作为资源操作的统一入口，负责资源包的完整生命周期管理。该模块提供资源加载、清单管理、下载更新、缓存清理等核心功能。

### 核心特性

- **统一入口**：ResourcePackage 作为资源操作的门面，提供所有公开 API
- **多模式支持**：支持 Editor/Offline/Host/Web/Custom 五种运行模式
- **高效查询**：字典缓存实现 O(1) 资源定位
- **异步优先**：所有操作基于 AsyncOperationBase 异步框架
- **灵活下载**：支持按标签、按路径、按资源的多种下载方式

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **统一门面** | ResourcePackage 封装所有资源操作，简化使用复杂度 |
| **模式抽象** | 通过 IPlayMode 接口统一不同运行模式的行为 |
| **高效索引** | 清单初始化时构建多个字典，加速运行时查询 |
| **生命周期** | 完整管理包裹从创建到销毁的全过程 |

---

## 文件结构

```
ResourcePackage/
├── ResourcePackage.cs              # 包裹管理门面
├── PackageManifest.cs              # 资源清单
├── PackageAsset.cs                 # 资源元数据
├── PackageBundle.cs                # 资源包元数据
├── ManifestTools.cs                # 清单序列化工具
├── ManifestDefine.cs               # 清单常量定义
├── AssetInfo.cs                    # 资源信息（公开API）
├── BundleInfo.cs                   # 资源包信息（内部）
├── PackageDetail.cs                # 包裹详情
├── EBuildBundleType.cs             # 资源包类型枚举
├── EFileNameStyle.cs               # 文件名风格枚举
├── Interface/                      # 接口定义
│   ├── IPlayMode.cs                # 播放模式接口
│   └── IBundleQuery.cs             # 资源包查询接口
├── PlayMode/                       # 播放模式实现
│   ├── PlayModeImpl.cs             # 播放模式核心实现
│   └── EditorSimulateModeHelper.cs # 编辑器模拟辅助
└── Operation/                      # 操作类
    ├── InitializationOperation.cs  # 初始化操作
    ├── DestroyOperation.cs         # 销毁操作
    ├── RequestPackageVersionOperation.cs   # 请求版本
    ├── UpdatePackageManifestOperation.cs   # 更新清单
    ├── PreDownloadContentOperation.cs      # 预下载
    ├── ClearCacheFilesOperation.cs         # 清理缓存
    ├── DownloaderOperation.cs              # 下载操作基类
    └── Internal/
        └── DeserializeManifestOperation.cs # 清单反序列化
```

---

## 核心类说明

### ResourcePackage

包裹管理门面类，提供所有资源操作的公开 API。

#### 核心属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹唯一标识 |
| `InitializeStatus` | `EOperationStatus` | 初始化状态 |
| `PackageValid` | `bool` | 包裹是否有效（清单已加载） |

#### 生命周期方法

```csharp
// 异步初始化包裹
InitializationOperation InitializeAsync(InitializeParameters parameters);

// 异步销毁包裹
DestroyOperation DestroyAsync();
```

#### 清单操作方法

```csharp
// 请求最新版本号
RequestPackageVersionOperation RequestPackageVersionAsync(
    bool appendTimeTicks = true, int timeout = 60);

// 更新资源清单
UpdatePackageManifestOperation UpdatePackageManifestAsync(
    string packageVersion, int timeout = 60);

// 预下载指定版本内容
PreDownloadContentOperation PreDownloadContentAsync(
    string packageVersion, int timeout = 60);

// 获取包裹版本
string GetPackageVersion();

// 获取包裹详细信息
PackageDetails GetPackageDetails();
```

#### 资源加载方法（5种模式）

```csharp
// 1. 加载单个资源
AssetHandle LoadAssetSync<T>(string location);
AssetHandle LoadAssetAsync<T>(string location);
AssetHandle LoadAssetAsync<T>(AssetInfo assetInfo);

// 2. 加载子资源
SubAssetsHandle LoadSubAssetsSync<T>(string location);
SubAssetsHandle LoadSubAssetsAsync<T>(string location);

// 3. 加载资源包内全部资源
AllAssetsHandle LoadAllAssetsSync<T>(string location);
AllAssetsHandle LoadAllAssetsAsync<T>(string location);

// 4. 加载场景
SceneHandle LoadSceneSync(string location, LoadSceneMode sceneMode);
SceneHandle LoadSceneAsync(string location, LoadSceneMode sceneMode);

// 5. 加载原生文件
RawFileHandle LoadRawFileSync(string location);
RawFileHandle LoadRawFileAsync(string location);
```

#### 资源信息查询方法

```csharp
// 按定位地址获取资源信息
AssetInfo GetAssetInfo(string location);

// 按 GUID 获取资源信息
AssetInfo GetAssetInfoByGUID(string assetGUID);

// 按标签获取资源信息列表
AssetInfo[] GetAssetInfos(string tag);
AssetInfo[] GetAssetInfos(string[] tags);

// 获取全部资源信息
AssetInfo[] GetAllAssetInfos();

// 验证定位地址有效性
bool CheckLocationValid(string location);

// 检查是否需要从远端下载
bool IsNeedDownloadFromRemote(string location);
bool IsNeedDownloadFromRemote(AssetInfo assetInfo);
```

#### 下载器创建方法（4种模式）

```csharp
// 下载全部需要下载的资源
ResourceDownloaderOperation CreateResourceDownloader(
    int downloadingMaxNumber, int failedTryAgain);

// 按标签下载
ResourceDownloaderOperation CreateResourceDownloader(
    string tag, int downloadingMaxNumber, int failedTryAgain);
ResourceDownloaderOperation CreateResourceDownloader(
    string[] tags, int downloadingMaxNumber, int failedTryAgain);

// 按资源路径下载
ResourceDownloaderOperation CreateBundleDownloader(
    string location, int downloadingMaxNumber, int failedTryAgain);
ResourceDownloaderOperation CreateBundleDownloader(
    string[] locations, int downloadingMaxNumber, int failedTryAgain);

// 按 AssetInfo 下载
ResourceDownloaderOperation CreateBundleDownloader(
    AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain);
```

#### 解压器创建方法

```csharp
// 解压全部需要解压的资源
ResourceUnpackerOperation CreateResourceUnpacker(
    int unpackingMaxNumber, int failedTryAgain);

// 按标签解压
ResourceUnpackerOperation CreateResourceUnpacker(
    string tag, int unpackingMaxNumber, int failedTryAgain);
ResourceUnpackerOperation CreateResourceUnpacker(
    string[] tags, int unpackingMaxNumber, int failedTryAgain);
```

#### 导入器创建方法

```csharp
// 按文件路径导入
ResourceImporterOperation CreateResourceImporter(
    string[] filePaths, int importerMaxNumber, int failedTryAgain);

// 按导入信息导入
ResourceImporterOperation CreateResourceImporter(
    ImportFileInfo[] fileInfos, int importerMaxNumber, int failedTryAgain);
```

#### 缓存清理方法

```csharp
// 清理缓存文件
ClearCacheFilesOperation ClearCacheFilesAsync(EFileClearMode clearMode);
ClearCacheFilesOperation ClearCacheFilesAsync(string clearMode, object clearParam);
```

#### 资源卸载方法

```csharp
// 强制卸载全部资源
UnloadAllAssetsOperation UnloadAllAssetsAsync();

// 卸载未使用的资源
UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync(int loopCount = 100);

// 尝试卸载指定资源
bool TryUnloadUnusedAsset(string location);
bool TryUnloadUnusedAsset(AssetInfo assetInfo);
```

---

## 清单系统

### PackageManifest

资源清单文件，存储资源和资源包的元数据，是资源查询的数据库。

#### 配置信息

| 属性 | 类型 | 说明 |
|------|------|------|
| `FileVersion` | `string` | 清单文件格式版本 |
| `EnableAddressable` | `bool` | 启用可寻址定位 |
| `SupportExtensionless` | `bool` | 支持无扩展名寻址 |
| `LocationToLower` | `bool` | 定位地址大小写不敏感 |
| `IncludeAssetGUID` | `bool` | 包含资源 GUID |
| `ReplaceAssetPathWithAddress` | `bool` | 用 Address 替换 AssetPath |
| `OutputNameStyle` | `int` | 文件名样式 |
| `BuildBundleType` | `int` | 构建资源包类型 |
| `BuildPipeline` | `string` | 构建管线名称 |

#### 包裹信息

| 属性 | 类型 | 说明 |
|------|------|------|
| `PackageName` | `string` | 包裹名称 |
| `PackageVersion` | `string` | 包裹版本 |
| `PackageNote` | `string` | 包裹备注 |

#### 核心数据

| 属性 | 类型 | 说明 |
|------|------|------|
| `AssetList` | `List<PackageAsset>` | 主资源列表 |
| `BundleList` | `List<PackageBundle>` | 资源包列表 |

#### 运行时字典（初始化时构建）

| 字典 | 键 | 值 | 说明 |
|------|-----|-----|------|
| `AssetDic` | AssetPath | PackageAsset | 资源路径查询 |
| `AssetPathMapping1` | Location | AssetPath | 定位地址→资源路径 |
| `AssetPathMapping2` | AssetGUID | AssetPath | GUID→资源路径 |
| `BundleDic1` | BundleName | PackageBundle | 按名称查询 |
| `BundleDic2` | FileName | PackageBundle | 按文件名查询 |
| `BundleDic3` | BundleGUID | PackageBundle | 按 GUID 查询 |

#### 查询方法

```csharp
// 定位地址转换
bool TryMappingToAssetPath(string location, out string assetPath);

// 资源查询
bool TryGetPackageAsset(string assetPath, out PackageAsset result);

// 资源包查询（3种方式）
bool TryGetPackageBundleByBundleName(string bundleName, out PackageBundle result);
bool TryGetPackageBundleByFileName(string fileName, out PackageBundle result);
bool TryGetPackageBundleByBundleGUID(string bundleGUID, out PackageBundle result);

// 获取资源主包
PackageBundle GetMainPackageBundle(AssetInfo assetInfo);

// 获取资源依赖包
PackageBundle[] GetAssetAllDependencies(PackageAsset asset);

// 获取资源包依赖
PackageBundle[] GetBundleAllDependencies(PackageBundle bundle);

// 定位地址转 AssetInfo
AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType);

// GUID 转 AssetInfo
AssetInfo ConvertAssetGUIDToAssetInfo(string assetGUID, System.Type assetType);

// 按标签获取资源
AssetInfo[] GetAssetInfosByTags(string[] tags);

// 检查资源包是否在清单中
bool IsIncludeBundleFile(string bundleGUID);
```

### PackageAsset

单个资源的元数据（可序列化）。

```csharp
public class PackageAsset
{
    public string Address;              // 可寻址地址
    public string AssetPath;            // 资源路径 (Assets/...)
    public string AssetGUID;            // 资源 GUID
    public string[] AssetTags;          // 分类标签
    public int BundleID;                // 所属资源包 ID（索引）
    public int[] DependBundleIDs;       // 依赖资源包 ID 数组

    // 检查是否包含指定标签
    public bool HasTag(string[] tags);
}
```

### PackageBundle

资源包的元数据（可序列化）。

```csharp
public class PackageBundle
{
    // 基础信息
    public string BundleName;           // 资源包名称
    public string BundleGUID { get; }   // GUID（= FileHash）
    public uint UnityCRC;               // Unity 生成的 CRC
    public string FileHash;             // 文件哈希值
    public uint FileCRC;                // 文件 CRC 校验码
    public long FileSize;               // 文件大小（字节）
    public bool Encrypted;              // 是否加密

    // 分类和依赖
    public string[] Tags;               // 资源包分类标签
    public int[] DependBundleIDs;       // 依赖资源包 ID（引擎层）

    // 运行时属性
    public int BundleType { get; }      // 资源包类型
    public string FileName { get; }     // 远端文件名
    public string FileExtension { get; } // 文件扩展名

    // 运行时列表
    public List<PackageAsset> IncludeMainAssets;   // 包含的主资源
    public List<int> ReferenceBundleIDs;           // 引用此包的 ID 列表

    // 检查是否包含标签
    public bool HasTag(string[] tags);
    public bool HasAnyTags();

    // 对比相等性（基于 FileHash）
    public bool Equals(PackageBundle otherBundle);
}
```

### ManifestTools

清单序列化工具类。

```csharp
// 验证清单数据完整性
static bool VerifyManifestData(byte[] fileData, string hashValue);

// 序列化
static string SerializeToJson(PackageManifest manifest);
static byte[] SerializeToBinary(PackageManifest manifest);

// 反序列化
static PackageManifest DeserializeFromJson(string jsonContent);
static PackageManifest DeserializeFromBinary(byte[] binaryData);

// 文件命名
static string GetRemoteBundleFileExtension(string bundleName);
static string GetRemoteBundleFileName(OutputNameStyle style, ...);
```

### ManifestDefine

清单常量定义。

```csharp
public class ManifestDefine
{
    public const int FileMaxSize = 104857600;           // 100MB
    public const uint FileSign = 0x594F4F;              // "YOO" 标记
    public const string FileVersion = "2025.9.30";      // 当前版本
    public const string VERSION_2025_8_28 = "2025.8.28";
    public const string VERSION_2025_9_30 = "2025.9.30";
    public const bool BackwardCompatible = true;        // 向后兼容
}
```

---

## 运行模式系统

### IPlayMode 接口

定义不同运行模式的统一接口。

```csharp
public interface IPlayMode
{
    // 当前活跃的清单
    PackageManifest ActiveManifest { set; get; }

    // 销毁文件系统
    void DestroyFileSystem();

    // 版本和清单操作
    RequestPackageVersionOperation RequestPackageVersionAsync(...);
    UpdatePackageManifestOperation UpdatePackageManifestAsync(...);
    PreDownloadContentOperation PreDownloadContentAsync(...);
    ClearCacheFilesOperation ClearCacheFilesAsync(...);

    // 下载器创建
    ResourceDownloaderOperation CreateResourceDownloaderByAll(...);
    ResourceDownloaderOperation CreateResourceDownloaderByTags(...);
    ResourceDownloaderOperation CreateResourceDownloaderByPaths(...);

    // 解压器创建
    ResourceUnpackerOperation CreateResourceUnpackerByAll(...);
    ResourceUnpackerOperation CreateResourceUnpackerByTags(...);

    // 导入器创建
    ResourceImporterOperation CreateResourceImporterByFilePaths(...);
    ResourceImporterOperation CreateResourceImporterByFileInfos(...);
}
```

### PlayModeImpl

播放模式核心实现，同时实现 `IPlayMode` 和 `IBundleQuery` 接口。

#### 核心属性

```csharp
public class PlayModeImpl : IPlayMode, IBundleQuery
{
    public readonly string PackageName;
    public readonly EPlayMode PlayMode;
    public readonly List<IFileSystem> FileSystems = new List<IFileSystem>(10);
    public PackageManifest ActiveManifest { set; get; }
}
```

#### 文件系统管理

```csharp
// 获取主文件系统（列表最后一个）
public IFileSystem GetMainFileSystem();

// 获取资源包所属的文件系统
public IFileSystem GetBelongFileSystem(PackageBundle bundle);
```

#### IBundleQuery 实现

```csharp
// 获取主资源包信息
BundleInfo GetMainBundleInfo(AssetInfo assetInfo);

// 获取依赖资源包信息
BundleInfo[] GetDependBundleInfos(AssetInfo assetInfo);

// 获取资源包名称
string GetMainBundleName(int bundleID);
```

#### 下载/解压/导入列表生成

```csharp
// 生成全部下载列表
List<BundleInfo> GetDownloadListByAll(PackageManifest manifest);

// 按标签筛选下载列表
List<BundleInfo> GetDownloadListByTags(PackageManifest manifest, string[] tags);

// 按资源路径筛选下载列表
List<BundleInfo> GetDownloadListByPaths(PackageManifest manifest, string[] locations);

// 解压列表
List<BundleInfo> GetUnpackListByAll(PackageManifest manifest);
List<BundleInfo> GetUnpackListByTags(PackageManifest manifest, string[] tags);

// 导入列表
List<BundleInfo> GetImporterListByFilePaths(...);
List<BundleInfo> GetImporterListByFileInfos(...);
```

### 五种运行模式

| 模式 | 说明 | 文件系统 |
|------|------|----------|
| `EditorSimulateMode` | 编辑器模拟模式 | DefaultEditorFileSystem |
| `OfflinePlayMode` | 单机运行模式 | DefaultBuildinFileSystem |
| `HostPlayMode` | 联机运行模式 | Buildin + Cache |
| `WebPlayMode` | WebGL 运行模式 | WebServer + WebRemote |
| `CustomPlayMode` | 自定义模式 | 用户自定义 |

#### 文件系统优先级

```
FileSystems[0] → FileSystems[1] → ... → FileSystems[N] (主文件系统)
              ↓                              ↓
         编辑器或内置资源          远端或缓存资源
```

---

## 操作类系统

### InitializationOperation

初始化操作，管理包裹的初始化流程。

```
状态流程：
Prepare
    ↓
ClearOldFileSystem
    └── 清理旧的文件系统（如果存在）
            ↓
InitFileSystem
    └── 逐个初始化 FileSystems
            ├── FileSystem[0].InitializeFileSystemAsync()
            ├── FileSystem[1].InitializeFileSystemAsync()
            └── ...
                    ↓
CheckInitResult
    └── 检查所有文件系统初始化结果
            ├── 任一失败 → Status = Failed
            └── 全部成功 → Status = Succeed
                    ↓
Done
```

### DestroyOperation

销毁操作，管理包裹的销毁流程。

```
状态流程：
CheckInitStatus
    ├── 初始化中 → Status = Failed（不允许销毁）
    └── 其他状态 → 继续
            ↓
UnloadAllAssets
    └── 如果初始化成功，卸载所有资源
            ↓
DestroyPackage
    ├── 销毁包裹
    └── OperationSystem.ClearPackageOperation()
            ↓
Done → Status = Succeed
```

### RequestPackageVersionOperation

请求版本操作，获取远端最新版本号。

```
状态流程：
RequestPackageVersion
    └── 委托给主文件系统
            └── FileSystem.RequestPackageVersionAsync()
                    ├── 成功 → PackageVersion = 版本号
                    └── 失败 → Error
                            ↓
Done
```

### UpdatePackageManifestOperation

更新清单操作，加载指定版本的清单。

```
状态流程：
CheckParams
    └── 验证参数有效性
            ↓
CheckActiveManifest
    └── 检查当前清单版本
            ├── 版本匹配 → 直接成功（复用）
            └── 版本不匹配 → 继续加载
                    ↓
LoadPackageManifest
    └── 主文件系统加载清单
            └── FileSystem.LoadPackageManifestAsync()
                    ├── 成功 → Manifest = 新清单
                    └── 失败 → Error
                            ↓
Done
```

### PreDownloadContentOperation

预下载操作，预加载指定版本的清单并支持创建下载器。

```
状态流程：
CheckParams
    └── 验证参数有效性
            ↓
CheckActiveManifest
    └── 检查是否已有相同版本清单
            ├── 有 → 复用清单
            └── 无 → 继续加载
                    ↓
LoadPackageManifest
    └── 加载指定版本清单
            ├── 成功 → 可创建下载器
            └── 失败 → Error
                    ↓
Done
```

#### 预下载后创建下载器

```csharp
// 下载全部
CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain);

// 按标签下载
CreateResourceDownloader(string tag, ...);
CreateResourceDownloader(string[] tags, ...);

// 按资源路径下载
CreateResourceDownloader(string[] locations, ...);
```

### ClearCacheFilesOperation

缓存清理操作，清理指定的缓存文件。

```
状态流程：
Prepare
    └── 准备清理任务
            ↓
ClearCacheFiles
    └── 逐个文件系统清理
            └── FileSystem.ClearCacheFilesAsync(options)
                    ↓
CheckClearResult
    └── 检查清理结果
            ├── 任一失败 → Status = Failed
            └── 全部成功 → Status = Succeed
                    ↓
Done
```

### DownloaderOperation

下载操作基类，管理并发下载任务。

#### 核心属性

```csharp
public abstract class DownloaderOperation : AsyncOperationBase
{
    // 下载统计
    public int TotalDownloadCount { get; }      // 总下载数
    public long TotalDownloadBytes { get; }     // 总大小（字节）
    public int CurrentDownloadCount { get; }    // 已完成数
    public long CurrentDownloadBytes { get; }   // 已完成大小
    public float Progress { get; }              // 进度 0.0~1.0

    // 并发控制
    protected const int MAX_LOADER_COUNT = 64;  // 最大同时下载数
}
```

#### 回调委托

```csharp
// 下载完成回调
public DownloadFinishDelegate DownloadFinishCallback;

// 进度更新回调
public DownloadUpdateDelegate DownloadUpdateCallback;

// 下载错误回调
public DownloadErrorDelegate DownloadErrorCallback;

// 开始下载文件回调
public DownloadFileBeginDelegate DownloadFileBeginCallback;
```

#### 状态流程

```
Check
    └── 检查下载列表是否为空
            ├── 为空 → 直接成功
            └── 不为空 → 开始下载
                    ↓
Loading
    ├── 检测已完成的下载器
    ├── 移除完成的下载器
    ├── 更新进度回调
    ├── 创建新下载器（≤ MAX_LOADER_COUNT）
    └── 处理失败重试
            ↓
Done
```

#### 子类

| 类 | 说明 |
|-----|------|
| `ResourceDownloaderOperation` | 资源下载器 |
| `ResourceUnpackerOperation` | 资源解压器 |
| `ResourceImporterOperation` | 资源导入器 |

### DeserializeManifestOperation

清单反序列化操作，从二进制数据反序列化清单。

```
状态流程：
RestoreFileData
    └── 调用插件还原数据（如果有）
            ↓
DeserializeFileHeader
    └── 读取文件头
            ├── 验证 FileSign (0x594F4F)
            └── 检查 FileVersion 兼容性
                    ↓
PrepareAssetList
    └── 读取资源数量
            ↓
DeserializeAssetList（时间切片）
    └── 逐个反序列化资源
            ↓
PrepareBundleList
    └── 读取资源包数量
            ↓
DeserializeBundleList（时间切片）
    └── 逐个反序列化资源包
            ↓
InitManifest
    └── 初始化清单（构建字典）
            ↓
Done → Manifest
```

---

## 辅助类说明

### AssetInfo

资源信息包装类（公开 API）。

```csharp
public class AssetInfo
{
    // 公开属性
    public string PackageName { get; }      // 所属包裹
    public System.Type AssetType { get; }   // 资源类型
    public string Error { get; }            // 错误信息
    public bool IsInvalid { get; }          // 身份是否无效
    public string Address { get; }          // 可寻址地址
    public string AssetPath { get; }        // 资源路径

    // 内部属性
    internal ELoadMethod LoadMethod;        // 加载方法
    internal string GUID { get; }           // 唯一标识符
    internal PackageAsset Asset { get; }    // 内部对象
}
```

### BundleInfo

资源包信息（内部使用）。

```csharp
public class BundleInfo
{
    private readonly IFileSystem _fileSystem;
    public readonly PackageBundle Bundle;

    // 加载资源包
    public FSLoadBundleOperation LoadBundleFile();

    // 创建下载器
    public FSDownloadFileOperation CreateDownloader(int failedTryAgain);

    // 检查是否需要下载
    public bool IsNeedDownloadFromRemote();

    // 生成唯一标识（用于去重）
    public string GetDownloadCombineGUID();
}
```

### PackageDetails

包裹详细信息（公开 API）。

```csharp
public class PackageDetails
{
    public string FileVersion;
    public bool EnableAddressable;
    public bool SupportExtensionless;
    public bool LocationToLower;
    public bool IncludeAssetGUID;
    public bool ReplaceAssetPathWithAddress;
    public int OutputNameStyle;
    public int BuildBundleType;
    public string BuildPipeline;
    public string PackageName;
    public string PackageVersion;
    public string PackageNote;
    public int AssetTotalCount;     // 主资源总数
    public int BundleTotalCount;    // 资源包总数
}
```

---

## 枚举定义

### EBuildBundleType

资源包类型枚举。

```csharp
public enum EBuildBundleType
{
    Unknown = 0,        // 未知（默认）
    VirtualBundle = 1,  // 虚拟包（编辑器模拟）
    AssetBundle = 2,    // Unity AssetBundle
    RawBundle = 3,      // 原生文件（未压缩）
    InstantBundle = 4,  // Unity China InstantBundle
}
```

### EFileNameStyle

文件名命名风格枚举。

```csharp
public enum EFileNameStyle
{
    HashName = 0,               // "abc123def456.bundle"
    BundleName = 1,             // "assets_ui.bundle"
    BundleName_HashName = 2,    // "assets_ui_abc123def456.bundle"
}
```

### ELoadMethod

资源加载方法枚举（内部）。

```csharp
internal enum ELoadMethod
{
    None = 0,
    LoadAsset,        // 加载单个资源
    LoadSubAssets,    // 加载子资源
    LoadAllAssets,    // 加载资源包内全部
    LoadScene,        // 加载场景
    LoadRawFile,      // 加载原生文件
}
```

---

## 关键工作流

### 初始化流程

```
1. YooAssets.CreatePackage(packageName)
   └── new ResourcePackage(packageName)

2. package.InitializeAsync(parameters)
   ├── CheckInitializeParameters()
   ├── new ResourceManager()
   ├── new PlayModeImpl()
   ├── new InitializationOperation()
   └── OperationSystem.StartOperation()

3. InitializationOperation 逐个初始化 FileSystem
   ├── DefaultEditorFileSystem（编辑器模拟）
   ├── DefaultBuildinFileSystem（StreamingAssets）
   ├── DefaultCacheFileSystem（已缓存资源）
   └── DefaultWebServerFileSystem（WebGL）

4. PlayModeImpl.ActiveManifest = fileSystem.LoadPackageManifest()
   └── 清单反序列化并初始化
```

### 资源加载流程

```
package.LoadAssetAsync<T>(location)
    │
    ├── ConvertLocationToAssetInfo(location)
    │   └── PlayModeImpl.ActiveManifest.ConvertLocationToAssetInfo()
    │       └── AssetPathMapping1[location] → AssetPath
    │           └── AssetDic[assetPath] → PackageAsset
    │
    ├── ResourceManager.LoadAssetAsync(assetInfo)
    │   ├── ProviderDic 查询或创建 Provider
    │   ├── Provider.StartBundleLoader()
    │   │   ├── GetMainBundleInfo() → BundleInfo
    │   │   └── GetDependBundleInfos() → List<BundleInfo>
    │   │
    │   └── 并发加载 AssetBundle
    │       ├── PlayModeImpl.GetBelongFileSystem()
    │       └── IFileSystem.LoadBundleFile()
    │
    └── return AssetHandle
```

### 清单更新流程

```
package.RequestPackageVersionAsync()
    └── PlayModeImpl.GetMainFileSystem().RequestPackageVersionAsync()
            │
            ▼
package.UpdatePackageManifestAsync(version)
    ├── 检查 ActiveManifest 版本是否已匹配
    └── PlayModeImpl.GetMainFileSystem().LoadPackageManifestAsync(version)
        └── ManifestTools.DeserializeFromBinary()
            └── DeserializeManifestOperation（逐个反序列化资源包）
```

### 下载流程

```
downloader = package.CreateResourceDownloader(...)
    └── PlayModeImpl.GetDownloadListByXXX()
        ├── 遍历清单的 BundleList
        ├── GetBelongFileSystem(bundle)
        └── fileSystem.NeedDownload(bundle)
                │
                ▼
downloader.BeginDownload()
    └── DownloaderOperation.InternalUpdate()
        ├── 创建 ≤ MAX_LOADER_COUNT 个 FSDownloadFileOperation
        ├── fileSystem.DownloadFileAsync(bundle, options)
        ├── 更新进度和回调
        └── 失败文件进入重试列表
```

---

## 类继承关系

```
AsyncOperationBase
├── InitializationOperation
├── DestroyOperation
├── RequestPackageVersionOperation
│   └── RequestPackageVersionImplOperation
├── UpdatePackageManifestOperation
├── PreDownloadContentOperation
├── ClearCacheFilesOperation
├── DownloaderOperation (abstract)
│   ├── ResourceDownloaderOperation
│   ├── ResourceUnpackerOperation
│   └── ResourceImporterOperation
└── DeserializeManifestOperation

IPlayMode
└── PlayModeImpl (also implements IBundleQuery)

PackageManifest
├── List<PackageAsset> AssetList
└── List<PackageBundle> BundleList
```

---

## 使用示例

### 初始化包裹（联机模式）

```csharp
// 创建包裹
var package = YooAssets.CreatePackage("DefaultPackage");

// 创建内置文件系统参数
var buildinParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

// 创建缓存文件系统参数
var cacheParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(
    remoteServices: new GameRemoteServices()
);

// 创建联机模式初始化参数
var initParams = new HostPlayModeParameters();
initParams.BuildinFileSystemParameters = buildinParams;
initParams.CacheFileSystemParameters = cacheParams;

// 异步初始化
var initOp = package.InitializeAsync(initParams);
await initOp.ToTask();

if (initOp.Status == EOperationStatus.Succeed)
{
    Debug.Log("包裹初始化成功！");
}
```

### 更新清单

```csharp
// 请求最新版本
var versionOp = package.RequestPackageVersionAsync();
await versionOp.ToTask();

if (versionOp.Status != EOperationStatus.Succeed)
{
    Debug.LogError($"请求版本失败：{versionOp.Error}");
    return;
}

string packageVersion = versionOp.PackageVersion;
Debug.Log($"最新版本：{packageVersion}");

// 更新清单
var updateOp = package.UpdatePackageManifestAsync(packageVersion);
await updateOp.ToTask();

if (updateOp.Status == EOperationStatus.Succeed)
{
    Debug.Log("清单更新成功！");
}
```

### 下载资源

```csharp
// 创建下载器（最多10个并发，失败重试3次）
var downloader = package.CreateResourceDownloader(10, 3);

if (downloader.TotalDownloadCount == 0)
{
    Debug.Log("没有需要下载的资源");
    return;
}

Debug.Log($"需要下载 {downloader.TotalDownloadCount} 个文件，" +
          $"总大小 {downloader.TotalDownloadBytes / 1024 / 1024}MB");

// 注册回调
downloader.DownloadUpdateCallback = (info) =>
{
    Debug.Log($"下载进度：{info.Progress * 100:F1}%");
};

downloader.DownloadErrorCallback = (info) =>
{
    Debug.LogError($"下载错误：{info.FileName} - {info.Error}");
};

// 开始下载
downloader.BeginDownload();
await downloader.ToTask();

if (downloader.Status == EOperationStatus.Succeed)
{
    Debug.Log("下载完成！");
}
```

### 按标签下载

```csharp
// 按单个标签下载
var downloader = package.CreateResourceDownloader("level1", 10, 3);

// 按多个标签下载
var downloader = package.CreateResourceDownloader(
    new string[] { "level1", "level2" }, 10, 3);

downloader.BeginDownload();
await downloader.ToTask();
```

### 加载资源

```csharp
// 异步加载预制体
var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");
await handle.ToTask();

if (handle.Status == EOperationStatus.Succeed)
{
    var prefab = handle.AssetObject as GameObject;
    var player = GameObject.Instantiate(prefab);

    // 使用完毕后释放
    // handle.Release();
}
else
{
    Debug.LogError($"加载失败：{handle.LastError}");
}
```

### 加载场景

```csharp
// 异步加载场景（叠加模式）
var handle = package.LoadSceneAsync("Assets/Scenes/GameScene.unity", LoadSceneMode.Additive);
await handle.ToTask();

if (handle.Status == EOperationStatus.Succeed)
{
    Debug.Log("场景加载成功！");
}
```

### 预下载指定版本

```csharp
// 预下载特定版本
var predownloadOp = package.PreDownloadContentAsync("1.0.5");
await predownloadOp.ToTask();

if (predownloadOp.Status == EOperationStatus.Succeed)
{
    // 创建下载器
    var downloader = predownloadOp.CreateResourceDownloader(10, 3);
    downloader.BeginDownload();
    await downloader.ToTask();
}
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
var clearOp = package.ClearCacheFilesAsync(
    EFileClearMode.ClearBundleFilesByTags.ToString(), "dlc_old");
await clearOp.ToTask();
```

### 卸载资源

```csharp
// 卸载未使用的资源
var unloadOp = package.UnloadUnusedAssetsAsync();
await unloadOp.ToTask();

// 强制卸载全部资源
var unloadOp = package.UnloadAllAssetsAsync();
await unloadOp.ToTask();
```

---

## 注意事项

1. **初始化顺序**：必须先初始化包裹才能进行其他操作
2. **清单更新**：建议在更新清单前调用 `UnloadAllAssetsAsync()` 释放已加载资源
3. **资源释放**：使用后必须调用 `Handle.Release()` 避免内存泄漏
4. **并发限制**：下载器最大并发数为 64，建议根据网络环境合理设置
5. **版本兼容**：清单文件版本需 >= 2025.8.28
6. **异常处理**：操作失败时检查 `Status` 和 `Error` 属性
7. **线程安全**：所有核心逻辑在 Unity 主线程执行，无需额外同步
