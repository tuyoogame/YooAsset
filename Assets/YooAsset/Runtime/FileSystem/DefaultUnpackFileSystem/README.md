# DefaultUnpackFileSystem 解压文件系统

## 模块概述

DefaultUnpackFileSystem 是 YooAsset 的**解压文件系统**，专为处理 Android 和 OpenHarmony 平台的内置资源解压需求而设计。该文件系统继承自 `DefaultCacheFileSystem`，复用其完整的下载、验证、缓存功能，仅重定义存储目录结构。

### 核心特性

- **继承复用**：完全继承 DefaultCacheFileSystem 的所有功能
- **独立存储**：使用独立的目录存储解压后的资源
- **本地下载**：通过 WWW 路径从 StreamingAssets "下载"资源
- **平台适配**：解决 Android APK 内文件无法直接访问的问题

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **平台兼容** | 解决 Android/OpenHarmony 平台 APK 内文件访问限制 |
| **代码复用** | 继承 DefaultCacheFileSystem，避免重复实现 |
| **资源隔离** | 解压资源与下载缓存分开存储，便于管理 |
| **透明集成** | 作为 DefaultBuildinFileSystem 的内部组件工作 |

---

## 文件结构

```
DefaultUnpackFileSystem/
├── DefaultUnpackFileSystem.cs           # 解压文件系统主类
├── DefaultUnpackFileSystemDefine.cs     # 常量定义
└── DefaultUnpackRemoteServices.cs       # 本地资源服务接口
```

---

## 核心类说明

### DefaultUnpackFileSystem

解压文件系统主类，继承自 `DefaultCacheFileSystem`。

```csharp
internal class DefaultUnpackFileSystem : DefaultCacheFileSystem
{
    public override void OnCreate(string packageName, string rootDirectory)
    {
        base.OnCreate(packageName, rootDirectory);

        // 重写保存根目录和临时目录
        _cacheBundleFilesRoot = PathUtility.Combine(_packageRoot, "UnpackBundleFiles");
        _cacheManifestFilesRoot = PathUtility.Combine(_packageRoot, "UnpackManifestFiles");
        _tempFilesRoot = PathUtility.Combine(_packageRoot, "UnpackTempFiles");
    }
}
```

#### 继承的功能

由于继承自 `DefaultCacheFileSystem`，DefaultUnpackFileSystem 拥有以下完整功能：

| 功能 | 说明 |
|------|------|
| 文件下载 | 通过 DownloadScheduler 下载资源 |
| 断点续传 | 支持大文件断点续传 |
| 文件验证 | 多线程 CRC/Hash 验证 |
| 缓存管理 | 记录已解压文件，避免重复解压 |
| 加密支持 | 支持 IDecryptionServices 解密 |
| 覆盖安装检测 | App 版本变更时清理解压缓存 |

### DefaultUnpackFileSystemDefine

常量定义类，定义解压目录名称。

```csharp
internal class DefaultUnpackFileSystemDefine
{
    /// <summary>
    /// 保存的资源文件的文件夹名称
    /// </summary>
    public const string SaveBundleFilesFolderName = "UnpackBundleFiles";

    /// <summary>
    /// 保存的清单文件的文件夹名称
    /// </summary>
    public const string SaveManifestFilesFolderName = "UnpackManifestFiles";

    /// <summary>
    /// 下载的临时文件的文件夹名称
    /// </summary>
    public const string TempFilesFolderName = "UnpackTempFiles";
}
```

### DefaultUnpackRemoteServices

本地资源服务接口，将 StreamingAssets 路径转换为 WWW 请求路径。

```csharp
internal class DefaultUnpackRemoteServices : IRemoteServices
{
    private readonly string _buildinPackageRoot;
    protected readonly Dictionary<string, string> _mapping = new Dictionary<string, string>(10000);

    public DefaultUnpackRemoteServices(string buildinPackRoot)
    {
        _buildinPackageRoot = buildinPackRoot;
    }

    // 主地址和备用地址相同（本地文件）
    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return GetFileLoadURL(fileName);
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return GetFileLoadURL(fileName);
    }

    private string GetFileLoadURL(string fileName)
    {
        if (_mapping.TryGetValue(fileName, out string url) == false)
        {
            string filePath = PathUtility.Combine(_buildinPackageRoot, fileName);
            url = DownloadSystemHelper.ConvertToWWWPath(filePath);
            _mapping.Add(fileName, url);
        }
        return url;
    }
}
```

#### 路径转换示例

```
输入文件名: bundle_abc123.bundle

StreamingAssets 路径:
  {Application.streamingAssetsPath}/DefaultPackage/bundle_abc123.bundle

WWW 请求路径 (Android):
  jar:file:///data/app/com.example.game.apk!/assets/DefaultPackage/bundle_abc123.bundle
```

---

## 目录结构

### 与 DefaultCacheFileSystem 对比

| 目录类型 | DefaultCacheFileSystem | DefaultUnpackFileSystem |
|----------|----------------------|------------------------|
| 资源文件 | `BundleFiles/` | `UnpackBundleFiles/` |
| 清单文件 | `ManifestFiles/` | `UnpackManifestFiles/` |
| 临时文件 | `TempFiles/` | `UnpackTempFiles/` |

### 实际目录结构

```
{SandboxRoot}/{PackageName}/
├── BundleFiles/              # DefaultCacheFileSystem 下载缓存
│   └── ...
├── ManifestFiles/            # DefaultCacheFileSystem 清单缓存
│   └── ...
├── UnpackBundleFiles/        # DefaultUnpackFileSystem 解压缓存
│   ├── {Hash[0:2]}/
│   │   └── {BundleGUID}/
│   │       ├── __data
│   │       └── __info
│   └── ...
├── UnpackManifestFiles/      # DefaultUnpackFileSystem 清单
│   └── ...
└── UnpackTempFiles/          # DefaultUnpackFileSystem 临时文件
    └── ...
```

---

## 工作原理

### 解压触发条件

在 `DefaultBuildinFileSystem` 中，以下情况会触发解压：

```csharp
protected virtual bool IsUnpackBundleFile(PackageBundle bundle)
{
    if (Belong(bundle) == false)
        return false;

#if UNITY_ANDROID || UNITY_OPENHARMONY
    // Android/OpenHarmony 平台
    if (bundle.Encrypted)
        return true;           // 加密资源需要解压

    if (bundle.BundleType == (int)EBuildBundleType.RawBundle)
        return true;           // 原生文件需要解压

    return false;              // 普通 AssetBundle 不需要解压
#else
    return false;              // 其他平台不需要解压
#endif
}
```

### 解压流程

```
DefaultBuildinFileSystem
    │
    ├── NeedUnpack(bundle) 检查
    │       └── IsUnpackBundleFile(bundle)
    │               ├── Android/OpenHarmony 平台
    │               │       ├── 加密资源 → true
    │               │       └── RawBundle → true
    │               └── 其他平台 → false
    │
    ├── 需要解压时
    │       └── _unpackFileSystem.DownloadFileAsync(bundle, options)
    │               └── DefaultUnpackRemoteServices.GetRemoteMainURL()
    │                       └── 返回 StreamingAssets 的 WWW 路径
    │                               ↓
    │               └── DownloadAndCacheRemoteFileOperation
    │                       ├── 从 APK 内 "下载" 资源
    │                       ├── 验证文件完整性
    │                       └── 保存到 UnpackBundleFiles 目录
    │
    └── 加载资源时
            └── _unpackFileSystem.LoadBundleFile(bundle)
                    └── 从 UnpackBundleFiles 加载
```

### 资源加载委托

```csharp
// DefaultBuildinFileSystem.LoadBundleFile()
public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
{
    // 需要解压的资源，委托给解压文件系统加载
    if (IsUnpackBundleFile(bundle))
    {
        return _unpackFileSystem.LoadBundleFile(bundle);
    }

    // 普通资源直接从 StreamingAssets 加载
    // ...
}
```

---

## 与 DefaultBuildinFileSystem 集成

### 初始化流程

```csharp
// DefaultBuildinFileSystem.OnCreate()
public virtual void OnCreate(string packageName, string packageRoot)
{
    // ... 基础初始化 ...

    // 创建解压文件系统
    var remoteServices = new DefaultUnpackRemoteServices(_packageRoot);
    _unpackFileSystem = new DefaultUnpackFileSystem();

    // 传递配置参数
    _unpackFileSystem.SetParameter(REMOTE_SERVICES, remoteServices);
    _unpackFileSystem.SetParameter(INSTALL_CLEAR_MODE, InstallClearMode);
    _unpackFileSystem.SetParameter(FILE_VERIFY_LEVEL, FileVerifyLevel);
    _unpackFileSystem.SetParameter(FILE_VERIFY_MAX_CONCURRENCY, FileVerifyMaxConcurrency);
    _unpackFileSystem.SetParameter(APPEND_FILE_EXTENSION, AppendFileExtension);
    _unpackFileSystem.SetParameter(DECRYPTION_SERVICES, DecryptionServices);
    _unpackFileSystem.SetParameter(COPY_LOCAL_FILE_SERVICES, CopyLocalFileServices);

    // 使用指定的解压根目录
    _unpackFileSystem.OnCreate(packageName, UnpackFileSystemRoot);
}
```

### 方法委托关系

| DefaultBuildinFileSystem 方法 | 解压资源时的委托目标 |
|------------------------------|-------------------|
| `LoadBundleFile()` | `_unpackFileSystem.LoadBundleFile()` |
| `GetBundleFilePath()` | `_unpackFileSystem.GetBundleFilePath()` |
| `ReadBundleFileData()` | `_unpackFileSystem.ReadBundleFileData()` |
| `ReadBundleFileText()` | `_unpackFileSystem.ReadBundleFileText()` |
| `DownloadFileAsync()` | `_unpackFileSystem.DownloadFileAsync()` |
| `ClearCacheFilesAsync()` | `_unpackFileSystem.ClearCacheFilesAsync()` |

---

## 使用场景

### 场景 1：Android 加密资源

```
问题：Android 平台无法直接从 APK 内读取文件进行解密
解决：先解压到沙盒，再从沙盒读取并解密

流程：
1. 检测到加密资源 → IsUnpackBundleFile() = true
2. 首次加载 → NeedUnpack() = true
3. 执行解压 → DownloadFileAsync() 从 APK 复制到沙盒
4. 后续加载 → NeedUnpack() = false，直接从沙盒加载
```

### 场景 2：Android 原生文件

```
问题：RawBundle 需要通过文件路径访问，APK 内路径不可直接访问
解决：解压到沙盒，返回沙盒内的文件路径

流程：
1. 检测到 RawBundle → IsUnpackBundleFile() = true
2. 首次访问 → 解压到 UnpackBundleFiles
3. GetBundleFilePath() 返回沙盒路径
4. 业务代码使用标准文件 API 访问
```

### 场景 3：普通 AssetBundle

```
情况：Android 平台的普通（未加密）AssetBundle
处理：不需要解压，Unity 可以直接从 APK 内加载

流程：
1. IsUnpackBundleFile() = false
2. 直接使用 AssetBundle.LoadFromFile() 加载
3. Unity 内部处理 APK 访问
```

---

## 类继承关系

```
IFileSystem
    └── DefaultCacheFileSystem
            └── DefaultUnpackFileSystem  ← 仅重写目录名称

IRemoteServices
    └── DefaultUnpackRemoteServices      ← 本地 WWW 路径服务
```

---

## 配置参数

DefaultUnpackFileSystem 继承 DefaultCacheFileSystem 的所有参数：

| 参数 | 类型 | 说明 |
|------|------|------|
| `REMOTE_SERVICES` | `IRemoteServices` | 由 DefaultUnpackRemoteServices 提供 |
| `INSTALL_CLEAR_MODE` | `EOverwriteInstallClearMode` | 覆盖安装清理模式 |
| `FILE_VERIFY_LEVEL` | `EFileVerifyLevel` | 文件验证级别 |
| `FILE_VERIFY_MAX_CONCURRENCY` | `int` | 验证并发数 |
| `APPEND_FILE_EXTENSION` | `bool` | 追加文件扩展名 |
| `DECRYPTION_SERVICES` | `IDecryptionServices` | 解密服务 |
| `COPY_LOCAL_FILE_SERVICES` | `ICopyLocalFileServices` | 本地拷贝服务 |

---

## 注意事项

1. **内部组件**：DefaultUnpackFileSystem 是 DefaultBuildinFileSystem 的内部组件，不建议单独使用
2. **平台限定**：解压功能仅在 Android 和 OpenHarmony 平台生效
3. **存储占用**：解压会额外占用设备存储空间（相当于资源的两份拷贝）
4. **首次加载**：需要解压的资源首次加载会有额外耗时
5. **自动管理**：解压缓存由系统自动管理，包括覆盖安装时的清理
6. **继承完整性**：继承了 DefaultCacheFileSystem 的所有功能，包括断点续传、多线程验证等

---

## 与其他文件系统对比

| 特性 | DefaultUnpackFileSystem | DefaultCacheFileSystem |
|------|------------------------|----------------------|
| 数据来源 | StreamingAssets (本地) | 远程服务器 |
| 主/备用地址 | 相同（本地路径） | 不同（CDN 地址） |
| 存储目录 | UnpackXxxFiles | XxxFiles |
| 使用方式 | 作为内部组件 | 独立使用 |
| 继承关系 | 子类 | 父类 |
