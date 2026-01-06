# ResourceManager 资源管理器

## 模块概述

ResourceManager 是 YooAsset 资源管理系统的**核心执行层**，负责资源加载的实际执行和生命周期管理。该模块通过 Provider 模式实现资源加载抽象，通过 Handle 模式提供面向用户的类型安全访问接口。

### 核心特性

- **Provider 抽象**：统一的资源加载操作模式，支持复用机制
- **Handle 封装**：类型安全的用户访问接口，支持协程和 async/await
- **引用计数**：精确的资源生命周期追踪，自动卸载未使用资源
- **并发控制**：可配置的 Bundle 加载并发限制，避免系统过载

### 模块统计

| 子模块 | 职责 |
|--------|------|
| 核心 | ResourceManager + DownloadStatus |
| Handle 系统 | 面向用户的资源句柄 |
| Provider 系统 | 资源加载执行器 |
| Operation 系统 | 异步操作集合 |
| **总计** | 完整资源管理系统 |

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **统一加载接口** | 通过 Provider 模式抽象不同类型资源的加载逻辑 |
| **类型安全访问** | Handle 模式封装资源访问，提供编译时类型检查 |
| **资源生命周期** | 引用计数精确追踪，支持自动和手动卸载 |
| **并发流量控制** | 可配置的 Bundle 加载并发数，平衡性能与稳定性 |
| **Provider 复用** | 相同资源的多次加载共享同一个 Provider |

---

## 文件结构

```
ResourceManager/
├── ResourceManager.cs              # 主管理类
├── DownloadStatus.cs               # 下载状态结构体
├── Handle/                         # 句柄系统
│   ├── HandleBase.cs              # 句柄基类
│   ├── HandleFactory.cs           # 句柄工厂
│   ├── AssetHandle.cs             # 资源句柄
│   ├── SceneHandle.cs             # 场景句柄
│   ├── SubAssetsHandle.cs         # 子资源句柄
│   ├── AllAssetsHandle.cs         # 全部资源句柄
│   └── RawFileHandle.cs           # 原生文件句柄
├── Provider/                       # 提供者系统
│   ├── ProviderOperation.cs       # 提供者基类
│   ├── AssetProvider.cs           # 资源提供者
│   ├── SceneProvider.cs           # 场景提供者
│   ├── SubAssetsProvider.cs       # 子资源提供者
│   ├── AllAssetsProvider.cs       # 全部资源提供者
│   ├── RawFileProvider.cs         # 原生文件提供者
│   └── CompletedProvider.cs       # 完成提供者（错误快速返回）
└── Operation/                      # 操作系统
    ├── Internal/
    │   └── LoadBundleFileOperation.cs  # Bundle 加载操作
    ├── InstantiateOperation.cs         # 实例化操作
    ├── UnloadSceneOperation.cs         # 场景卸载操作
    ├── UnloadAllAssetsOperation.cs     # 全部资源卸载操作
    └── UnloadUnusedAssetsOperation.cs  # 未使用资源卸载操作
```

---

## 核心类说明

### ResourceManager

资源管理器主类，作为资源加载操作的中央枢纽。

```csharp
internal class ResourceManager
{
    // 核心数据结构
    internal readonly Dictionary<string, ProviderOperation> ProviderDic;    // 容量 5000
    internal readonly Dictionary<string, LoadBundleFileOperation> LoaderDic; // 容量 5000
    internal readonly List<SceneHandle> SceneHandles;                        // 容量 100

    // 配置属性
    public bool AutoUnloadBundleWhenUnused { get; }      // 自动卸载未使用的 Bundle
    public bool WebGLForceSyncLoadAsset { get; }         // WebGL 强制同步加载
    public bool LockLoadOperation { get; set; }          // 加载操作锁定
    public int BundleLoadingMaxConcurrency { get; }      // Bundle 并发加载数量 (默认32，范围1-256)
    public int BundleLoadingCounter { get; set; }        // 正在加载的 Bundle 计数
}
```

#### 初始化和销毁

```csharp
public void Initialize(InitializeParameters parameters, IBundleQuery bundleServices)
{
    _bundleLoadingMaxConcurrency = parameters.BundleLoadingMaxConcurrency;
    AutoUnloadBundleWhenUnused = parameters.AutoUnloadBundleWhenUnused;
    WebGLForceSyncLoadAsset = parameters.WebGLForceSyncLoadAsset;
    _bundleQuery = bundleServices;
    SceneManager.sceneUnloaded += OnSceneUnloaded;
}

public void Destroy()
{
    SceneManager.sceneUnloaded -= OnSceneUnloaded;
}
```

#### 核心加载 API

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `LoadAssetAsync()` | `AssetHandle` | 加载单个资源 |
| `LoadSceneAsync()` | `SceneHandle` | 加载场景 |
| `LoadSubAssetsAsync()` | `SubAssetsHandle` | 加载子资源 |
| `LoadAllAssetsAsync()` | `AllAssetsHandle` | 加载所有资源 |
| `LoadRawFileAsync()` | `RawFileHandle` | 加载原生文件 |

#### Bundle 加载器管理

```csharp
// 创建主 Bundle 加载器
internal LoadBundleFileOperation CreateMainBundleFileLoader(AssetInfo assetInfo)

// 创建依赖 Bundle 加载器列表
internal List<LoadBundleFileOperation> CreateDependBundleFileLoaders(AssetInfo assetInfo)

// 检查是否繁忙
public bool BundleLoadingIsBusy()
{
    return BundleLoadingCounter >= _bundleLoadingMaxConcurrency;
}
```

### DownloadStatus

下载状态结构体，用于追踪资源下载进度。

```csharp
public struct DownloadStatus
{
    public long DownloadedBytes;  // 已下载字节数
    public long TotalBytes;       // 总字节数
    public float Progress;        // 下载进度 (0-1)

    public static DownloadStatus CreateDefaultStatus()
    {
        return new DownloadStatus
        {
            DownloadedBytes = 0,
            TotalBytes = 0,
            Progress = 1f
        };
    }
}
```

---

## Provider 系统

Provider 系统负责资源加载的实际执行，每种资源类型都有对应的 Provider 实现。

### ProviderOperation（基类）

所有资源提供者的抽象基类，继承自 `AsyncOperationBase`。

#### 状态机

```
None → StartBundleLoader → WaitBundleLoader → ProcessBundleResult → Done
```

```csharp
protected enum ESteps
{
    None = 0,
    StartBundleLoader,      // 启动所有 Bundle 加载器
    WaitBundleLoader,       // 等待 Bundle 加载完成
    ProcessBundleResult,    // 处理 Bundle 结果（子类实现）
    Done,                   // 完成
}
```

#### 核心属性

```csharp
public abstract class ProviderOperation : AsyncOperationBase
{
    public string ProviderGUID { get; }                      // 唯一标识符
    public AssetInfo MainAssetInfo { get; }                  // 资源信息
    public UnityEngine.Object AssetObject { get; }           // 单个资源对象
    public UnityEngine.Object[] AllAssetObjects { get; }     // 全部资源对象数组
    public UnityEngine.Object[] SubAssetObjects { get; }     // 子资源对象数组
    public Scene SceneObject { get; }                        // 场景对象
    public BundleResult BundleResultObject { get; }          // Bundle 结果
    public int RefCount { get; }                             // 引用计数
    public bool IsDestroyed { get; }                         // 销毁标志
    public string SceneName { get; }                         // 场景名称
}
```

#### 引用计数管理

```csharp
// 创建句柄（引用计数 +1）
public T CreateHandle<T>() where T : HandleBase
{
    RefCount++;
    HandleBase handle = HandleFactory.CreateHandle(this, typeof(T));
    _handles.Add(handle);
    return handle as T;
}

// 释放句柄（引用计数 -1）
public void ReleaseHandle(HandleBase handle)
{
    if (RefCount <= 0)
        throw new YooInternalException(...);

    if (_handles.Remove(handle) == false)
        throw new YooInternalException(...);

    RefCount--;
}

// 检查是否可销毁
public bool CanDestroyProvider()
{
    // 注意：正在加载中的任务不可以销毁
    if (IsLoading) return false;
    return RefCount <= 0;
}
```

#### 核心执行流程

```csharp
internal override void InternalUpdate()
{
    if (_steps == ESteps.StartBundleLoader)
    {
        // 启动主 Bundle 加载器和所有依赖 Bundle 加载器
        foreach (var bundleLoader in _bundleLoaders)
        {
            bundleLoader.StartOperation();
            AddChildOperation(bundleLoader);
        }
        _steps = ESteps.WaitBundleLoader;
    }

    if (_steps == ESteps.WaitBundleLoader)
    {
        // 等待所有 Bundle 加载完成并验证成功
        foreach (var bundleLoader in _bundleLoaders)
        {
            if (bundleLoader.IsDone == false) return;
            if (bundleLoader.Status != EOperationStatus.Succeed)
            {
                InvokeCompletion(error, EOperationStatus.Failed);
                return;
            }
        }
        _steps = ESteps.ProcessBundleResult;
    }

    if (_steps == ESteps.ProcessBundleResult)
    {
        // 子类实现的核心加载逻辑
        ProcessBundleResult();
    }
}
```

### Provider 子类

| 类型 | 职责 | 结果属性 | 说明 |
|------|------|----------|------|
| `AssetProvider` | 加载单个资源 | `AssetObject` | 最常用的加载方式 |
| `SceneProvider` | 加载场景 | `SceneObject` | 支持挂起加载 |
| `SubAssetsProvider` | 加载子资源 | `SubAssetObjects` | 用于图集、精灵等 |
| `AllAssetsProvider` | 加载所有资源 | `AllAssetObjects` | 加载 Bundle 内所有资源 |
| `RawFileProvider` | 加载原生文件 | `BundleResultObject` | 配置文件、二进制数据等 |
| `CompletedProvider` | 错误快速返回 | - | 用于无效请求的快速失败 |

#### AssetProvider 示例

```csharp
internal class AssetProvider : ProviderOperation
{
    private FSLoadAssetOperation _loadAssetOp;

    protected override void ProcessBundleResult()
    {
        if (_loadAssetOp == null)
        {
            _loadAssetOp = BundleResultObject.LoadAssetAsync(MainAssetInfo);
            _loadAssetOp.StartOperation();
            AddChildOperation(_loadAssetOp);

#if UNITY_WEBGL
            if (_resManager.WebGLForceSyncLoadAsset)
                _loadAssetOp.WaitForAsyncComplete();
#endif
        }

        _loadAssetOp.UpdateOperation();
        Progress = _loadAssetOp.Progress;

        if (_loadAssetOp.IsDone == false) return;

        if (_loadAssetOp.Status != EOperationStatus.Succeed)
            InvokeCompletion(_loadAssetOp.Error, EOperationStatus.Failed);
        else
        {
            AssetObject = _loadAssetOp.Result;
            InvokeCompletion(string.Empty, EOperationStatus.Succeed);
        }
    }
}
```

#### SceneProvider 示例

```csharp
internal class SceneProvider : ProviderOperation
{
    private LoadSceneParameters _loadParams;
    private bool _suspendLoad;

    public SceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo,
                         LoadSceneParameters loadParams, bool suspendLoad)
        : base(manager, providerGUID, assetInfo)
    {
        _loadParams = loadParams;
        _suspendLoad = suspendLoad;
        SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
    }

    // 取消挂起加载
    public void UnSuspendLoad()
    {
        _suspendLoad = false;
    }

    protected override void ProcessBundleResult()
    {
        if (_loadSceneOp == null)
        {
            _loadSceneOp = BundleResultObject.LoadSceneOperation(
                MainAssetInfo, _loadParams, _suspendLoad);
            _loadSceneOp.StartOperation();
            AddChildOperation(_loadSceneOp);
        }

        // 支持中途取消挂起
        if (_suspendLoad == false)
            _loadSceneOp.UnSuspendLoad();

        // ... 进度更新和完成处理
    }
}
```

---

## Handle 系统

Handle 系统提供面向用户的资源访问接口，通过代理模式封装 Provider。

### HandleBase（基类）

所有句柄的抽象基类，实现 `IEnumerator` 和 `IDisposable` 接口。

```csharp
public abstract class HandleBase : IEnumerator, IDisposable
{
    private readonly AssetInfo _assetInfo;
    internal ProviderOperation Provider { get; private set; }

    // 有效性检查
    public bool IsValid
    {
        get { return Provider != null && Provider.IsDestroyed == false; }
    }

    // 释放句柄
    public void Release()
    {
        if (IsValidWithWarning == false) return;
        Provider.ReleaseHandle(this);

        // 主动卸载零引用的资源包
        if (Provider.RefCount == 0)
            Provider.TryUnloadBundle();

        Provider = null;
    }

    // IDisposable 实现
    public void Dispose()
    {
        this.Release();
    }
}
```

#### 代理 Provider 属性

```csharp
public EOperationStatus Status
{
    get { return IsValidWithWarning ? Provider.Status : EOperationStatus.None; }
}

public string LastError
{
    get { return IsValidWithWarning ? Provider.Error : string.Empty; }
}

public float Progress
{
    get { return IsValidWithWarning ? Provider.Progress : 0; }
}

public bool IsDone
{
    get { return IsValidWithWarning ? Provider.IsDone : true; }
}

public Task Task
{
    get { return IsValidWithWarning ? Provider.Task : null; }
}
```

#### 协程支持

```csharp
bool IEnumerator.MoveNext()
{
    return !IsDone;  // 返回 false 时循环结束
}

void IEnumerator.Reset() { }

object IEnumerator.Current
{
    get { return Provider; }
}
```

### 具体 Handle 类型

#### AssetHandle

资源句柄，用于访问加载的单个资源。

```csharp
public sealed class AssetHandle : HandleBase
{
    private System.Action<AssetHandle> _callback;

    // 完成事件
    public event System.Action<AssetHandle> Completed
    {
        add
        {
            if (IsValidWithWarning == false)
                throw new YooHandleException(...);
            if (Provider.IsDone)
                value.Invoke(this);  // 已完成则立即调用
            else
                _callback += value;  // 未完成则等待
        }
        remove { _callback -= value; }
    }

    // 资源对象访问
    public UnityEngine.Object AssetObject
    {
        get { return IsValidWithWarning ? Provider.AssetObject : null; }
    }

    public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
    {
        return IsValidWithWarning ? Provider.AssetObject as TAsset : null;
    }

    // 实例化支持
    public GameObject InstantiateSync();
    public GameObject InstantiateSync(Transform parent);
    public GameObject InstantiateSync(Transform parent, bool worldPositionStays);
    public GameObject InstantiateSync(Vector3 position, Quaternion rotation);
    public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent);

    public InstantiateOperation InstantiateAsync();
    public InstantiateOperation InstantiateAsync(Transform parent);
    public InstantiateOperation InstantiateAsync(Transform parent, bool worldPositionStays);
    public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation);
    public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent);
}
```

#### SceneHandle

场景句柄，用于管理加载的场景。

```csharp
public class SceneHandle : HandleBase
{
    internal string PackageName { get; set; }

    // 场景名称
    public string SceneName
    {
        get { return IsValidWithWarning ? Provider.SceneName : string.Empty; }
    }

    // 场景对象
    public Scene SceneObject
    {
        get { return IsValidWithWarning ? Provider.SceneObject : new Scene(); }
    }

    // 激活场景
    public bool ActivateScene()
    {
        if (IsValidWithWarning == false) return false;
        if (SceneObject.IsValid() && SceneObject.isLoaded)
            return SceneManager.SetActiveScene(SceneObject);
        return false;
    }

    // 取消挂起加载
    public bool UnSuspend()
    {
        if (IsValidWithWarning == false) return false;
        if (Provider is SceneProvider provider)
            provider.UnSuspendLoad();
        return true;
    }

    // 卸载场景
    public UnloadSceneOperation UnloadAsync()
    {
        if (IsValidWithWarning == false)
        {
            var operation = new UnloadSceneOperation(error);
            OperationSystem.StartOperation(packageName, operation);
            return operation;
        }

        var op = new UnloadSceneOperation(Provider);
        OperationSystem.StartOperation(packageName, op);
        return op;
    }
}
```

#### SubAssetsHandle

子资源句柄，用于访问 Bundle 内的子资源（如图集中的精灵）。

```csharp
public sealed class SubAssetsHandle : HandleBase
{
    // 所有子资源
    public IReadOnlyList<UnityEngine.Object> SubAssetObjects
    {
        get { return IsValidWithWarning ? Provider.SubAssetObjects : null; }
    }

    // 获取指定名称的子资源
    public TObject GetSubAssetObject<TObject>(string assetName)
        where TObject : UnityEngine.Object
    {
        if (IsValidWithWarning == false) return null;

        foreach (var assetObject in Provider.SubAssetObjects)
        {
            if (assetObject.name == assetName && assetObject is TObject)
                return assetObject as TObject;
        }

        YooLogger.Warning($"Not found sub asset object : {assetName}");
        return null;
    }

    // 获取所有指定类型的子资源
    public TObject[] GetSubAssetObjects<TObject>()
        where TObject : UnityEngine.Object
    {
        if (IsValidWithWarning == false) return null;

        List<TObject> result = new List<TObject>(Provider.SubAssetObjects.Length);
        foreach (var assetObject in Provider.SubAssetObjects)
        {
            var retObject = assetObject as TObject;
            if (retObject != null)
                result.Add(retObject);
        }
        return result.ToArray();
    }
}
```

#### AllAssetsHandle

全部资源句柄，用于访问 Bundle 内的所有资源。

```csharp
public sealed class AllAssetsHandle : HandleBase
{
    public IReadOnlyList<UnityEngine.Object> AllAssetObjects
    {
        get { return IsValidWithWarning ? Provider.AllAssetObjects : null; }
    }
}
```

#### RawFileHandle

原生文件句柄，用于访问未经 Unity 处理的原始文件。

```csharp
public class RawFileHandle : HandleBase
{
    // 读取二进制数据
    public byte[] GetRawFileData()
    {
        if (IsValidWithWarning == false) return null;
        return Provider.BundleResultObject.ReadBundleFileData();
    }

    // 读取文本内容
    public string GetRawFileText()
    {
        if (IsValidWithWarning == false) return null;
        return Provider.BundleResultObject.ReadBundleFileText();
    }

    // 获取文件路径
    public string GetRawFilePath()
    {
        if (IsValidWithWarning == false) return string.Empty;
        return Provider.BundleResultObject.GetBundleFilePath();
    }
}
```

### HandleFactory

句柄工厂，使用字典映射类型到构造函数。

```csharp
internal static class HandleFactory
{
    private static readonly Dictionary<Type, Func<ProviderOperation, HandleBase>>
        _handleFactory = new Dictionary<Type, Func<ProviderOperation, HandleBase>>()
    {
        { typeof(AssetHandle), op => new AssetHandle(op) },
        { typeof(SceneHandle), op => new SceneHandle(op) },
        { typeof(SubAssetsHandle), op => new SubAssetsHandle(op) },
        { typeof(AllAssetsHandle), op => new AllAssetsHandle(op) },
        { typeof(RawFileHandle), op => new RawFileHandle(op) }
    };

    public static HandleBase CreateHandle(ProviderOperation operation, Type type)
    {
        if (_handleFactory.TryGetValue(type, out var factory) == false)
            throw new NotImplementedException($"Handle type {type.FullName} is not supported.");
        return factory(operation);
    }
}
```

---

## Loader 系统

### LoadBundleFileOperation

Bundle 文件加载操作，负责 Bundle 的实际加载和并发控制。

#### 状态机

```
None → CheckConcurrency → LoadBundleFile → Done
```

```csharp
private enum ESteps
{
    None,
    CheckConcurrency,   // 检查并发限制
    LoadBundleFile,     // 执行 Bundle 加载
    Done,
}
```

#### 核心属性

```csharp
internal class LoadBundleFileOperation : AsyncOperationBase
{
    public BundleInfo LoadBundleInfo { get; }      // Bundle 信息
    public BundleResult Result { get; set; }       // 加载结果
    public int RefCount { get; }                   // 引用计数
    public long DownloadedBytes { get; set; }      // 已下载字节数
    public float DownloadProgress { get; set; }    // 下载进度
    public bool IsDestroyed { get; }               // 销毁标志
}
```

#### 并发控制流程

```csharp
internal override void InternalUpdate()
{
    if (_steps == ESteps.CheckConcurrency)
    {
        // 检查是否超过最大并发加载数
        if (_resManager.BundleLoadingIsBusy())
            return;  // 等待直到有空闲位置

        _steps = ESteps.LoadBundleFile;
    }

    if (_steps == ESteps.LoadBundleFile)
    {
        if (_loadBundleOp == null)
        {
            // 统计计数增加
            _resManager.BundleLoadingCounter++;
            _loadBundleOp = LoadBundleInfo.LoadBundleFile();
            _loadBundleOp.StartOperation();
            AddChildOperation(_loadBundleOp);
        }

        // ... 等待加载完成 ...

        // 统计计数减少
        _resManager.BundleLoadingCounter--;
    }
}
```

#### 引用计数管理

```csharp
public void Reference()
{
    RefCount++;
}

public void Release()
{
    RefCount--;
}

public bool CanDestroyLoader()
{
    if (CanReleasableLoader() == false)
        return false;

    // 检查引用链上的资源包是否已经全部销毁
    // 注意：互相引用的资源包无法卸载！
    if (LoadBundleInfo.Bundle.ReferenceBundleIDs.Count > 0)
    {
        foreach (var bundleID in LoadBundleInfo.Bundle.ReferenceBundleIDs)
        {
#if YOOASSET_EXPERIMENTAL
            if (_resManager.CheckBundleReleasable(bundleID) == false)
                return false;
#else
            if (_resManager.CheckBundleDestroyed(bundleID) == false)
                return false;
#endif
        }
    }

    return true;
}
```

#### Provider 管理

```csharp
private readonly List<ProviderOperation> _providers = new List<ProviderOperation>(100);

public void AddProvider(ProviderOperation provider)
{
    if (_providers.Contains(provider) == false)
        _providers.Add(provider);
}

public void TryDestroyProviders()
{
    // 获取可销毁的 Provider 列表
    _removeList.Clear();
    foreach (var provider in _providers)
    {
        if (provider.CanDestroyProvider())
            _removeList.Add(provider);
    }

    // 销毁 Provider
    foreach (var provider in _removeList)
    {
        _providers.Remove(provider);
        provider.DestroyProvider();
    }

    // 从 ResourceManager 中移除
    if (_removeList.Count > 0)
    {
        _resManager.RemoveBundleProviders(_removeList);
        _removeList.Clear();
    }
}
```

---

## 操作类系统

### InstantiateOperation

实例化操作，支持同步和异步实例化 GameObject。

```csharp
public class InstantiateOperation : AsyncOperationBase
{
    public GameObject Result { get; private set; }

    // 支持的实例化参数
    private Transform _parent;
    private bool _worldPositionStays;
    private Vector3 _position;
    private Quaternion _rotation;
    private bool _setPositionAndRotation;
}
```

### UnloadSceneOperation

场景卸载操作。

```csharp
public class UnloadSceneOperation : AsyncOperationBase
{
    // 状态流程：UnloadScene → Done
    private enum ESteps
    {
        None,
        UnloadScene,
        Done,
    }
}
```

### UnloadAllAssetsOperation

卸载包裹内所有资源操作。

```csharp
public class UnloadAllAssetsOperation : AsyncOperationBase
{
    // 状态流程：CheckInit → ClearProvider → ClearLoader → Done
    private enum ESteps
    {
        None,
        CheckInit,
        ClearProvider,
        ClearLoader,
        Done,
    }
}
```

### UnloadUnusedAssetsOperation

卸载未使用资源操作，支持迭代清理。

```csharp
public class UnloadUnusedAssetsOperation : AsyncOperationBase
{
    private int _maxIterationCount;  // 最大迭代次数

    // 每次迭代清理一轮未使用的资源
    // 复杂的依赖链可能需要多次迭代
}
```

---

## 关键工作流

### 资源加载流程

```
用户请求 LoadAssetAsync(location)
    ↓
ResourcePackage 转换 → AssetInfo
    ↓
ResourceManager.LoadAssetAsync(assetInfo)
    ├─ 构造 ProviderGUID (LoadAssetAsync + AssetInfo.GUID)
    ├─ 查找已有 Provider (ProviderDic.TryGetValue)
    ├─ 创建新 Provider (如不存在)
    │   ├─ new AssetProvider(...)
    │   ├─ 创建 LoadBundleFileOperation (主 Bundle)
    │   └─ 创建 LoadBundleFileOperation[] (依赖 Bundle)
    ├─ 注册到 ProviderDic
    ├─ 启动 OperationSystem
    └─ 返回 Handle (provider.CreateHandle<AssetHandle>())
    ↓
ProviderOperation.InternalUpdate()
    ├─ StartBundleLoader：启动所有 Bundle 加载器
    ├─ WaitBundleLoader：等待 Bundle 加载完成
    └─ ProcessBundleResult：调用 BundleResult.LoadAssetAsync()
    ↓
LoadBundleFileOperation.InternalUpdate()
    ├─ CheckConcurrency：检查并发限制
    ├─ LoadBundleFile：调用 IFileSystem.LoadBundleFile()
    └─ 返回 BundleResult
    ↓
用户获得 Handle
    ├─ await handle.ToTask()
    ├─ yield return handle
    └─ handle.AssetObject
```

### 资源卸载流程

```
用户释放 handle.Release()
    ↓
HandleBase.Release()
    ├─ Provider.ReleaseHandle(this)
    ├─ RefCount--
    └─ 如果 RefCount == 0：Provider.TryUnloadBundle()
    ↓
ProviderOperation.TryUnloadBundle()
    └─ 如果 AutoUnloadBundleWhenUnused：
        ResourceManager.TryUnloadUnusedAsset(assetInfo, loopCount)
    ↓
ResourceManager.TryUnloadUnusedAsset()
    ├─ 循环处理依赖链 (loopCount 次)
    ├─ 卸载主 Bundle
    │   ├─ TryDestroyProviders()
    │   ├─ CanDestroyLoader() 检查
    │   └─ DestroyLoader()
    └─ 卸载依赖 Bundle
        ├─ CanDestroyLoader() 检查
        └─ DestroyLoader()
    ↓
LoadBundleFileOperation.DestroyLoader()
    └─ BundleResult.UnloadBundleFile()
```

### Provider 复用机制

```
多次加载同一资源：
    LoadAssetAsync("Player.prefab") → Provider A (新建)
    LoadAssetAsync("Player.prefab") → Provider A (复用)
    LoadAssetAsync("Player.prefab") → Provider A (复用)

ProviderGUID 构造规则：
    ProviderGUID = MethodName + AssetInfo.GUID
    例如：LoadAssetAsync + abc123def456 → "LoadAssetAsyncabc123def456"

复用条件：
    if (ProviderDic.TryGetValue(providerGUID, out provider))
        return provider.CreateHandle<T>();  // 复用现有 Provider

每次复用：
    RefCount++ (通过 CreateHandle)
```

---

## 类继承关系图

```
AsyncOperationBase
├── ProviderOperation (资源提供者基类)
│   ├── AssetProvider         (加载单个资源)
│   ├── SceneProvider         (加载场景)
│   ├── SubAssetsProvider     (加载子资源)
│   ├── AllAssetsProvider     (加载所有资源)
│   ├── RawFileProvider       (加载原生文件)
│   └── CompletedProvider     (错误快速返回)
├── LoadBundleFileOperation   (Bundle 加载)
├── InstantiateOperation      (实例化)
├── UnloadSceneOperation      (场景卸载)
├── UnloadAllAssetsOperation  (全部卸载)
└── UnloadUnusedAssetsOperation (未使用卸载)

HandleBase (IEnumerator, IDisposable)
├── AssetHandle      (资源句柄)
├── SceneHandle      (场景句柄)
├── SubAssetsHandle  (子资源句柄)
├── AllAssetsHandle  (全部资源句柄)
└── RawFileHandle    (原生文件句柄)
```

---

## 与其他模块的交互

```
ResourcePackage
    │
    ├─ 持有 → ResourceManager 实例
    │           ├─ LoadAssetAsync()
    │           ├─ LoadSceneAsync()
    │           ├─ LoadSubAssetsAsync()
    │           ├─ LoadAllAssetsAsync()
    │           └─ LoadRawFileAsync()
    │
    └─ 实现 → IBundleQuery 接口
                ├─ GetMainBundleInfo(AssetInfo) → BundleInfo
                ├─ GetDependBundleInfos(AssetInfo) → List<BundleInfo>
                └─ GetMainBundleName(bundleID) → string

ResourceManager
    │
    ├─ 依赖 → IBundleQuery (Bundle 查询)
    │
    ├─ 使用 → BundleInfo
    │           └─ LoadBundleFile() → FSLoadBundleOperation
    │
    └─ 间接使用 → IFileSystem
                    └─ 通过 BundleInfo 调用
                        ├─ LoadBundleFile(Bundle)
                        └─ 返回 FSLoadBundleOperation
```

### 并发流量控制

```
ResourceManager 维护的限制：
├─ BundleLoadingMaxConcurrency (默认 32，范围 1-256)
│   └─ 限制同时加载的 Bundle 文件数量
├─ BundleLoadingCounter (当前计数)
│   └─ 由 LoadBundleFileOperation 增减
└─ BundleLoadingIsBusy() 检查
    └─ 超过限制则阻塞新的 Bundle 加载

FileSystem (Cache) 维护的限制：
├─ FileVerifyMaxConcurrency (默认 32，范围 1-256)
│   └─ 限制同时验证的文件数量
├─ DownloadMaxConcurrency (默认 10，范围 1-64)
│   └─ 限制同时下载的文件数量
└─ DownloadMaxRequestPerFrame (默认 5，范围 1-20)
    └─ 限制每帧发起的下载请求数
```

---

## 使用示例

### 加载资源

```csharp
// 异步加载（async/await）
var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");
await handle.ToTask();
var prefab = handle.GetAssetObject<GameObject>();

// 使用资源
var instance = GameObject.Instantiate(prefab);

// 释放资源（必须！）
handle.Release();
```

```csharp
// 异步加载（协程）
var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");
yield return handle;
var prefab = handle.GetAssetObject<GameObject>();

// 使用资源
var instance = GameObject.Instantiate(prefab);

// 释放资源
handle.Release();
```

```csharp
// 异步加载（回调）
var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");
handle.Completed += (h) =>
{
    var prefab = h.GetAssetObject<GameObject>();
    var instance = GameObject.Instantiate(prefab);

    // 注意：回调中也需要释放
    h.Release();
};
```

### 加载场景

```csharp
// 加载场景（支持挂起）
var sceneHandle = package.LoadSceneAsync("Assets/Scenes/Main.unity",
    LoadSceneMode.Additive, suspendLoad: true);

// 在适当时机取消挂起
sceneHandle.UnSuspend();

await sceneHandle.ToTask();

// 激活场景
sceneHandle.ActivateScene();

// 卸载场景
var unloadOp = sceneHandle.UnloadAsync();
await unloadOp.ToTask();
```

### 加载子资源

```csharp
// 加载图集
var handle = package.LoadSubAssetsAsync<Sprite>("Assets/Atlas/UI.spriteatlas");
await handle.ToTask();

// 获取特定精灵
var homeIcon = handle.GetSubAssetObject<Sprite>("icon_home");
var settingsIcon = handle.GetSubAssetObject<Sprite>("icon_settings");

// 获取所有精灵
var allSprites = handle.GetSubAssetObjects<Sprite>();

// 释放资源
handle.Release();
```

### 加载原生文件

```csharp
// 加载 JSON 配置
var handle = package.LoadRawFileAsync("Assets/Config/settings.json");
await handle.ToTask();

// 读取文本
string json = handle.GetRawFileText();
var settings = JsonUtility.FromJson<Settings>(json);

// 释放资源
handle.Release();
```

```csharp
// 加载二进制文件
var handle = package.LoadRawFileAsync("Assets/Data/binary.bytes");
await handle.ToTask();

// 读取二进制数据
byte[] data = handle.GetRawFileData();

// 或获取文件路径（用于第三方库）
string path = handle.GetRawFilePath();

handle.Release();
```

### 实例化操作

```csharp
var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab");
await handle.ToTask();

// 同步实例化
var go1 = handle.InstantiateSync();
var go2 = handle.InstantiateSync(parentTransform);
var go3 = handle.InstantiateSync(parentTransform, worldPositionStays: false);
var go4 = handle.InstantiateSync(position, rotation);
var go5 = handle.InstantiateSync(position, rotation, parentTransform);

// 异步实例化
var instOp = handle.InstantiateAsync(parentTransform);
await instOp.ToTask();
var go6 = instOp.Result;

// 释放资源
handle.Release();
```

### 使用 using 语句自动释放

```csharp
// 利用 IDisposable 接口
using (var handle = package.LoadAssetAsync<GameObject>("Assets/Prefabs/Player.prefab"))
{
    await handle.ToTask();
    var prefab = handle.GetAssetObject<GameObject>();
    var instance = GameObject.Instantiate(prefab);
}
// using 块结束时自动调用 handle.Dispose() → Release()
```

---

## 注意事项

1. **必须释放 Handle**
   - 使用后必须调用 `handle.Release()` 避免内存泄漏
   - 可以使用 `using` 语句自动释放

2. **Provider 复用**
   - 相同资源的多次加载会复用同一个 Provider
   - 每次加载都会增加引用计数

3. **并发限制**
   - Bundle 加载最大并发数为 256，默认 32
   - 超过限制的加载请求会排队等待

4. **引用计数**
   - RefCount 为 0 且 `AutoUnloadBundleWhenUnused` 为 true 时自动卸载
   - 复杂依赖链可能需要多次迭代才能完全卸载

5. **WebGL 特殊处理**
   - 可配置 `WebGLForceSyncLoadAsset` 强制同步加载
   - WebGL 平台不支持多线程

6. **循环依赖**
   - 通过 `YOOASSET_EXPERIMENTAL` 宏处理循环 Bundle 依赖
   - 实验性功能，需谨慎使用

7. **线程安全**
   - 所有资源管理逻辑在主线程执行
   - 多线程仅用于下载和文件验证

8. **场景加载**
   - 场景不支持 Provider 复用
   - 每次加载都会创建新的 Provider（使用递增的 sceneCreateIndex）

---

## 性能优化建议

1. **合理设置并发数**
   - 根据设备性能调整 `BundleLoadingMaxConcurrency`
   - 移动设备建议 16-32，PC 可设置更高

2. **预加载常用资源**
   - 提前加载常用资源并持有 Handle
   - 避免运行时频繁加载/卸载

3. **批量加载**
   - 使用 `LoadAllAssetsAsync` 加载 Bundle 内所有资源
   - 减少多次加载请求的开销

4. **及时释放**
   - 不再使用的资源及时调用 `Release()`
   - 避免内存占用过高

5. **使用资源标签**
   - 合理规划资源标签
   - 利用标签进行批量下载和加载
