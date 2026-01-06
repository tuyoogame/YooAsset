# DiagnosticSystem 诊断系统

## 模块概述

DiagnosticSystem 是 YooAsset 的**远程调试诊断系统**，提供运行时资源管理状态的实时可视化和性能分析能力。该系统通过编辑器窗口与运行时游戏进行双向通信，实时采集和展示资源加载、Bundle 管理、异步操作等调试信息。

### 核心特性

- **实时远程调试**：在 Unity 编辑器中查看游戏运行时的资源管理状态
- **完整状态快照**：采集所有资源、Bundle、异步操作的实时信息
- **历史数据回溯**：缓存最近 500 帧数据，支持时间回溯分析
- **双模式采样**：支持单次采样和自动连续采样
- **低性能开销**：按需采样，无需连续监控

### 模块统计

| 组件 | 职责 |
|------|------|
| 核心通信 | RemoteDebuggerInRuntime + 双连接层 |
| 数据结构 | 5 个调试信息结构体 |
| 命令系统 | RemoteCommand 命令定义 |
| **总计** | 10 个核心文件，完整的远程诊断框架 |

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **实时可视化** | 在编辑器中实时查看运行时资源状态 |
| **性能监控** | 收集加载耗时、引用计数、下载进度等性能指标 |
| **内存诊断** | 追踪资源出生场景、引用计数，识别潜在内存泄漏 |
| **操作追踪** | 显示异步操作树，包括嵌套和依赖关系 |
| **低开销设计** | 按需采样而非连续监控，DEBUG 模式自动启用 |

---

## 文件结构

```
DiagnosticSystem/
├── RemoteDebuggerDefine.cs        # 全局定义和常量
├── RemoteCommand.cs                # 命令定义和序列化
├── DebugReport.cs                  # 调试报告（顶层容器）
├── DebugPackageData.cs             # 包级调试数据
├── DebugProviderInfo.cs            # 资源加载器调试信息
├── DebugBundleInfo.cs              # 资源包调试信息
├── DebugOperationInfo.cs           # 异步操作调试信息
├── RemoteDebuggerInRuntime.cs      # 运行时调试器主类
├── RemotePlayerConnection.cs       # 编辑器模拟连接层
└── RemoteEditorConnection.cs       # 运行时模拟连接层
```

---

## 核心类说明

### RemoteDebuggerDefine

全局定义类，包含调试器版本和通信协议的 GUID 标识符。

```csharp
internal class RemoteDebuggerDefine
{
    // 调试器版本（用于版本校验）
    public const string DebuggerVersion = "2.3.3";

    // 消息标识符（GUID）
    public static readonly Guid kMsgPlayerSendToEditor =
        new Guid("e34a5702dd353724aa315fb8011f08c3");  // 运行时→编辑器

    public static readonly Guid kMsgEditorSendToPlayer =
        new Guid("4d1926c9df5b052469a1c63448b7609a");  // 编辑器→运行时
}
```

### RemoteCommand

命令定义类，用于编辑器向运行时发送采样指令。

```csharp
internal enum ERemoteCommand
{
    SampleOnce = 0,  // 单次采样
    SampleAuto = 1,  // 自动采样（连续）
}

[Serializable]
internal class RemoteCommand
{
    public int CommandType;      // ERemoteCommand 枚举值
    public string CommandParam;  // 命令参数

    // 序列化/反序列化（JSON 格式）
    public static byte[] Serialize(RemoteCommand command)
    {
        return Encoding.UTF8.GetBytes(JsonUtility.ToJson(command));
    }

    public static RemoteCommand Deserialize(byte[] data)
    {
        return JsonUtility.FromJson<RemoteCommand>(Encoding.UTF8.GetString(data));
    }
}
```

**命令示例：**

```json
// 单次采样
{
  "CommandType": 0,
  "CommandParam": ""
}

// 开启自动采样
{
  "CommandType": 1,
  "CommandParam": "open"
}

// 关闭自动采样
{
  "CommandType": 1,
  "CommandParam": "close"
}
```

### DebugReport

调试报告容器，包含完整的系统状态快照。

```csharp
[Serializable]
internal class DebugReport
{
    // 调试器版本（用于版本校验）
    public string DebuggerVersion = RemoteDebuggerDefine.DebuggerVersion;

    // 游戏帧数
    public int FrameCount;

    // 包级调试数据列表（一个游戏可能有多个资源包）
    public List<DebugPackageData> PackageDatas = new List<DebugPackageData>(10);

    // 序列化/反序列化
    public static byte[] Serialize(DebugReport debugReport);
    public static DebugReport Deserialize(byte[] data);
}
```

### DebugPackageData

包级调试数据，包含单个资源包的所有诊断信息。

```csharp
[Serializable]
internal class DebugPackageData
{
    // 资源包名称
    public string PackageName;

    // 资源加载器列表
    public List<DebugProviderInfo> ProviderInfos = new List<DebugProviderInfo>(1000);

    // 资源包列表
    public List<DebugBundleInfo> BundleInfos = new List<DebugBundleInfo>(1000);

    // 异步操作列表
    public List<DebugOperationInfo> OperationInfos = new List<DebugOperationInfo>(1000);

    // 运行时查询字典（非序列化，按需构建）
    [NonSerialized]
    public Dictionary<string, DebugBundleInfo> BundleInfoDic;

    // 延迟解析字典
    public DebugBundleInfo GetBundleInfo(string bundleName);
}
```

### DebugProviderInfo

资源加载器（Provider）的调试信息。

```csharp
[Serializable]
internal struct DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
{
    public string PackageName;           // 所属包名
    public string AssetPath;             // 资源路径（如 "Assets/Prefabs/Player.prefab"）
    public string SpawnScene;            // 资源加载时的活跃场景名
    public string BeginTime;             // 加载开始时间（格式：HH:mm:ss.fff）
    public long LoadingTime;             // 加载耗时（单位：毫秒）
    public int RefCount;                 // 引用计数
    public string Status;                // 加载状态（None/Processing/Succeed/Failed）
    public List<string> DependBundles;   // 依赖的资源包名列表

    // 按 AssetPath 字母排序
    public int CompareTo(DebugProviderInfo other);
}
```

**关键诊断价值：**
- `SpawnScene`：追踪资源在哪个场景被加载，帮助识别资源泄漏
- `LoadingTime`：性能分析，识别加载慢的资源
- `RefCount`：引用计数监控，RefCount > 0 表示资源仍在使用
- `DependBundles`：依赖分析，理解资源加载的完整依赖链

### DebugBundleInfo

资源包（Bundle）的调试信息。

```csharp
[Serializable]
internal struct DebugBundleInfo : IComparer<DebugBundleInfo>, IComparable<DebugBundleInfo>
{
    public string BundleName;              // 资源包名称
    public int RefCount;                   // 引用计数（当前有多少个 Provider 在使用）
    public string Status;                  // 加载状态
    public List<string> ReferenceBundles;  // 反向依赖（谁引用了我）

    // 按 BundleName 字母排序
    public int CompareTo(DebugBundleInfo other);
}
```

**关键诊断价值：**
- `RefCount`：Bundle 引用计数，为 0 时可以被卸载
- `ReferenceBundles`：反向依赖分析，了解 Bundle 被哪些其他 Bundle 依赖

### DebugOperationInfo

异步操作的调试信息，支持递归树结构。

```csharp
[Serializable]
internal struct DebugOperationInfo : IComparer<DebugOperationInfo>, IComparable<DebugOperationInfo>
{
    public string OperationName;   // 操作类名（如 "LoadAssetOperation"）
    public string OperationDesc;   // 操作说明（自定义描述）
    public uint Priority;          // 优先级（用于操作排序）
    public float Progress;         // 进度（0.0 - 1.0）
    public string BeginTime;       // 操作开始时间
    public long ProcessTime;       // 处理耗时（单位：毫秒）
    public string Status;          // 操作状态（None/Processing/Succeed/Failed）
    public List<DebugOperationInfo> Childs;  // 子操作列表（支持嵌套树结构）

    public int CompareTo(DebugOperationInfo other);
}
```

**递归树结构示例：**
```
InitializationOperation
├─ LoadManifestOperation
│  ├─ LoadBundleFileOperation (manifest.bundle)
│  └─ DeserializeManifestOperation
└─ InitFileSystemOperation
```

**关键诊断价值：**
- `OperationName`：操作类型识别
- `ProcessTime`：性能瓶颈分析
- `Childs`：操作依赖关系可视化

---

## 通信系统

### RemoteDebuggerInRuntime

运行时调试器主类，负责接收命令、采样数据、发送报告。

```csharp
internal class RemoteDebuggerInRuntime : MonoBehaviour
{
    // 采样控制标志
    private static bool _sampleOnce = false;      // 单次采样
    private static bool _autoSample = false;      // 连续采样

    // 运行时初始化
    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad()
    {
        _sampleOnce = false;
        _autoSample = false;
    }

    private void Awake()
    {
        RemotePlayerConnection.Instance.Initialize();
    }

    private void OnEnable()
    {
        // 注册命令接收回调
        RemotePlayerConnection.Instance.Register(
            RemoteDebuggerDefine.kMsgEditorSendToPlayer,
            OnHandleEditorMessage);
    }

    private void LateUpdate()
    {
        // 采样逻辑（在一帧的最后执行）
        if (_autoSample || _sampleOnce)
        {
            _sampleOnce = false;
            var debugReport = YooAssets.GetDebugReport();
            var data = DebugReport.Serialize(debugReport);

            RemotePlayerConnection.Instance.Send(
                RemoteDebuggerDefine.kMsgPlayerSendToEditor,
                data);
        }
    }

    private static void OnHandleEditorMessage(MessageEventArgs args)
    {
        var command = RemoteCommand.Deserialize(args.data);

        if (command.CommandType == (int)ERemoteCommand.SampleOnce)
        {
            _sampleOnce = true;
        }
        else if (command.CommandType == (int)ERemoteCommand.SampleAuto)
        {
            _autoSample = (command.CommandParam == "open");
        }
    }
}
```

**关键设计点：**
1. **LateUpdate 时机**：确保该帧所有资源加载完成后再采样
2. **状态重置**：`[RuntimeInitializeOnLoadMethod]` 确保编辑器重新编译时重置状态
3. **DEBUG 模式自动启用**：通过 `#if DEBUG` 条件编译自动添加组件

### 双连接层架构

YooAsset 支持两种通信模式：

| 模式 | 使用场景 | 实现方式 |
|------|----------|----------|
| **编辑器模拟模式** | 开发调试 | `RemotePlayerConnection` + `RemoteEditorConnection`（虚拟连接） |
| **发布版本** | 运营期监控 | Unity 的 `PlayerConnection` API（真实网络） |

#### RemotePlayerConnection（编辑器模拟模式）

```csharp
internal class RemotePlayerConnection
{
    private static RemotePlayerConnection _instance;
    private readonly Dictionary<Guid, UnityAction<MessageEventArgs>> _messageCallbacks;

    public static RemotePlayerConnection Instance
    {
        get
        {
            if (_instance == null)
                _instance = new RemotePlayerConnection();
            return _instance;
        }
    }

    public void Register(Guid messageID, UnityAction<MessageEventArgs> callback)
    {
        _messageCallbacks.Add(messageID, callback);
    }

    public void Send(Guid messageID, byte[] data)
    {
        // 在编辑器模拟模式下，发送给虚拟编辑器连接
        RemoteEditorConnection.Instance.HandlePlayerMessage(messageID, data);
    }

    internal void HandleEditorMessage(Guid messageID, byte[] data)
    {
        if (_messageCallbacks.TryGetValue(messageID, out var callback))
        {
            callback.Invoke(new MessageEventArgs { playerId = 0, data = data });
        }
    }
}
```

#### RemoteEditorConnection（运行时模拟模式）

```csharp
internal class RemoteEditorConnection
{
    private static RemoteEditorConnection _instance;
    private readonly Dictionary<Guid, UnityAction<MessageEventArgs>> _messageCallbacks;

    public static RemoteEditorConnection Instance
    {
        get
        {
            if (_instance == null)
                _instance = new RemoteEditorConnection();
            return _instance;
        }
    }

    public void Register(Guid messageID, UnityAction<MessageEventArgs> callback)
    {
        _messageCallbacks.Add(messageID, callback);
    }

    public void Send(Guid messageID, byte[] data)
    {
        // 发送给虚拟运行时连接
        RemotePlayerConnection.Instance.HandleEditorMessage(messageID, data);
    }

    internal void HandlePlayerMessage(Guid messageID, byte[] data)
    {
        if (_messageCallbacks.TryGetValue(messageID, out var callback))
        {
            callback.Invoke(new MessageEventArgs { playerId = 0, data = data });
        }
    }
}
```

---

## 通信协议

### 协议规范

**协议版本：** 2.3.3

**编码格式：**
```
C# 对象 → JsonUtility.ToJson() → JSON 字符串 → Encoding.UTF8.GetBytes() → byte[]
```

**消息类型：**

| 方向 | GUID | 数据类型 | 说明 |
|------|------|----------|------|
| 编辑器→运行时 | `4d1926c9df5b052469a1c63448b7609a` | `RemoteCommand` | 采样命令 |
| 运行时→编辑器 | `e34a5702dd353724aa315fb8011f08c3` | `DebugReport` | 调试报告 |

### 双向通信流程

```
[编辑器 UI]
  │
  ├─ 用户点击 "Sample" 按钮
  │   └─ 发送 RemoteCommand (SampleOnce)
  │
  └─ 用户开启 "Record" 开关
      └─ 发送 RemoteCommand (SampleAuto, "open")

      ↓ RemoteEditorConnection.Send()
      ↓
      RemotePlayerConnection.HandleEditorMessage()
      ↓ 触发回调

[运行时]
  RemoteDebuggerInRuntime.OnHandleEditorMessage()
  ↓
  设置采样标志 (_sampleOnce 或 _autoSample)
  ↓
  LateUpdate 中采样
  ↓
  YooAssets.GetDebugReport()
    ├─ 收集所有 ResourcePackage 数据
    ├─ DebugProviderInfo[] (从 ProviderDic)
    ├─ DebugBundleInfo[] (从 LoaderDic)
    └─ DebugOperationInfo[] (从 _operations)
  ↓
  DebugReport.Serialize()
  ↓
  RemotePlayerConnection.Send()
  ↓
  RemoteEditorConnection.HandlePlayerMessage()
  ↓

[编辑器]
  AssetBundleDebuggerWindow.OnHandlePlayerMessage()
  ↓
  版本校验 (DebuggerVersion)
  ↓
  RemotePlayerSession.AddDebugReport()
  ↓
  缓存到历史记录 (最多 500 帧)
  ↓
  UI 刷新显示
```

### 版本校验机制

```csharp
// 编辑器端版本校验
private void OnHandlePlayerMessage(MessageEventArgs args)
{
    var debugReport = DebugReport.Deserialize(args.data);

    // 版本校验
    if (debugReport.DebuggerVersion != RemoteDebuggerDefine.DebuggerVersion)
    {
        Debug.LogWarning(
            $"Debugger versions are inconsistent : " +
            $"{debugReport.DebuggerVersion} != {RemoteDebuggerDefine.DebuggerVersion}");
        return;  // 丢弃不兼容的数据
    }

    // 处理数据...
}
```

**设计意图：** 防止编辑器和运行时的调试器版本不一致导致的数据格式错误。

---

## 数据收集流程

### 完整采样流程

```
RemoteDebuggerInRuntime.LateUpdate()
  ↓
检查 _sampleOnce 或 _autoSample 标志
  ↓ YES
调用 YooAssets.GetDebugReport()
  │
  ├─ 初始化 DebugReport
  ├─ 设置 FrameCount = Time.frameCount
  ├─ 遍历每个 ResourcePackage：
  │   │
  │   └─ package.GetDebugPackageData()
  │       │
  │       ├─ 创建 DebugPackageData
  │       ├─ 设置 PackageName
  │       │
  │       ├─ 收集 ProviderInfos：
  │       │   ResourceManager.GetDebugProviderInfos()
  │       │   遍历 ProviderDic：
  │       │     └─ 每个 ProviderOperation 提供：
  │       │         - MainAssetInfo.AssetPath
  │       │         - SpawnScene（场景名）
  │       │         - BeginTime（开始时间）
  │       │         - ProcessTime（耗时）
  │       │         - RefCount（引用计数）
  │       │         - Status（加载状态）
  │       │         - GetDebugDependBundles()（依赖列表）
  │       │
  │       ├─ 收集 BundleInfos：
  │       │   ResourceManager.GetDebugBundleInfos()
  │       │   遍历 LoaderDic：
  │       │     └─ 每个 LoadBundleFileOperation 提供：
  │       │         - BundleName
  │       │         - RefCount
  │       │         - Status
  │       │         - FilterReferenceBundles()（反向依赖）
  │       │
  │       └─ 收集 OperationInfos：
  │           OperationSystem.GetDebugOperationInfos(PackageName)
  │           遍历 _operations（按 PackageName 过滤）：
  │             └─ 递归构建操作树：
  │                 - GetType().Name（操作类名）
  │                 - GetOperationDesc()（自定义描述）
  │                 - Priority（优先级）
  │                 - Progress（进度）
  │                 - BeginTime（开始时间）
  │                 - ProcessTime（耗时）
  │                 - Status（状态）
  │                 - Childs（子操作列表）
  │
  └─ 返回 DebugReport
```

### 性能指标收集

#### 资源加载耗时

```csharp
// AsyncOperationBase 中的自动计时
private Stopwatch _watch = null;

internal void InternalStart()
{
    if (_watch == null)
    {
        BeginTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
        _watch = Stopwatch.StartNew();
    }
}

internal void InternalUpdate()
{
    ProcessTime = _watch.ElapsedMilliseconds;
    // ... 持续计时
}
```

#### 场景信息追踪

```csharp
// ProviderOperation.cs
[Conditional("DEBUG")]  // 仅在 DEBUG 模式下启用
public void InitProviderDebugInfo()
{
    SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
}
```

#### 引用计数监控

```csharp
// ProviderOperation 和 LoadBundleFileOperation 都维护 RefCount
public int RefCount { get; }  // 当前被引用的次数
```

---

## 与其他模块的交互

```
YooAssets (全局入口)
    │
    ├─ 初始化阶段：
    │   #if DEBUG
    │   _driver.AddComponent<RemoteDebuggerInRuntime>();
    │   #endif
    │
    └─ 数据收集入口：
        GetDebugReport()
            ├─ 遍历所有 ResourcePackage
            └─ 构建 DebugReport

ResourcePackage (资源包)
    │
    └─ GetDebugPackageData()
        ├─ 调用 ResourceManager.GetDebugProviderInfos()
        ├─ 调用 ResourceManager.GetDebugBundleInfos()
        └─ 调用 OperationSystem.GetDebugOperationInfos()

ResourceManager (资源管理器)
    │
    ├─ GetDebugProviderInfos()
    │   └─ 遍历 ProviderDic (Dictionary<string, ProviderOperation>)
    │
    └─ GetDebugBundleInfos()
        └─ 遍历 LoaderDic (Dictionary<string, LoadBundleFileOperation>)

OperationSystem (操作系统)
    │
    └─ GetDebugOperationInfos(packageName)
        └─ 遍历 _operations (List<AsyncOperationBase>)
            └─ 递归收集子操作 (Childs)

AsyncOperationBase (异步操作基类)
    │
    ├─ BeginTime：操作开始时间
    ├─ ProcessTime：累计处理耗时
    ├─ Status：操作状态
    ├─ Progress：进度
    └─ GetOperationDesc()：自定义描述

ProviderOperation (资源提供者)
    │
    ├─ SpawnScene：加载时的活跃场景
    └─ GetDebugDependBundles()：依赖包列表

LoadBundleFileOperation (Bundle 加载器)
    │
    ├─ RefCount：引用计数
    └─ LoadBundleInfo：Bundle 信息
```

---

## 使用场景

### 场景 1：运行时资源泄漏诊断

**问题：** 游戏切换场景后内存持续增长，怀疑有资源未释放。

**诊断步骤：**
1. 打开 AssetBundle Debugger 窗口
2. 开启 Record 模式（自动采样）
3. 切换场景前后观察 ProviderInfos 列表
4. 检查 `RefCount > 0` 且 `SpawnScene` 为旧场景的资源
5. 定位未释放的资源和对应的代码位置

**关键字段：**
- `SpawnScene`：资源在哪个场景被加载
- `RefCount`：引用计数，应该为 0
- `AssetPath`：资源路径，定位具体资源

### 场景 2：资源加载性能分析

**问题：** 首次加载场景卡顿严重。

**诊断步骤：**
1. 单次采样（Sample Once）
2. 切换到 Asset View
3. 按 `LoadingTime` 降序排序
4. 识别加载耗时最长的资源
5. 分析 `DependBundles` 了解依赖链

**关键字段：**
- `LoadingTime`：加载耗时（毫秒）
- `DependBundles`：依赖的 Bundle 列表
- `Status`：加载状态

### 场景 3：Bundle 引用分析

**问题：** 某个 Bundle 无法被卸载。

**诊断步骤：**
1. 切换到 Bundle View
2. 搜索目标 Bundle
3. 检查 `RefCount` 和 `ReferenceBundles`
4. 追踪哪些资源正在使用该 Bundle
5. 定位未释放的资源引用

**关键字段：**
- `RefCount`：Bundle 引用计数
- `ReferenceBundles`：反向依赖列表
- `Status`：Bundle 加载状态

### 场景 4：异步操作监控

**问题：** 复杂的初始化流程卡住，不知道在哪个步骤。

**诊断步骤：**
1. 切换到 Operation View
2. 查看操作树结构
3. 检查 `Status` 为 `Processing` 的操作
4. 分析 `Progress` 了解进度
5. 通过 `Childs` 了解操作依赖关系

**关键字段：**
- `OperationName`：操作类型
- `OperationDesc`：操作描述
- `Progress`：进度（0.0 - 1.0）
- `Childs`：子操作列表

---

## 数据导出

### JSON 导出功能

编辑器窗口支持导出当前帧的完整调试数据为 JSON 文件。

**导出示例：**

```json
{
  "DebuggerVersion": "2.3.3",
  "FrameCount": 1234,
  "PackageDatas": [
    {
      "PackageName": "DefaultPackage",
      "ProviderInfos": [
        {
          "PackageName": "DefaultPackage",
          "AssetPath": "Assets/Prefabs/Player.prefab",
          "SpawnScene": "GameScene",
          "BeginTime": "12:34:56.789",
          "LoadingTime": 45,
          "RefCount": 1,
          "Status": "Succeed",
          "DependBundles": [
            "assets_prefabs.bundle"
          ]
        }
      ],
      "BundleInfos": [
        {
          "BundleName": "assets_prefabs.bundle",
          "RefCount": 1,
          "Status": "Succeed",
          "ReferenceBundles": []
        }
      ],
      "OperationInfos": [
        {
          "OperationName": "LoadAssetOperation",
          "OperationDesc": "Load assets_prefabs.bundle",
          "Priority": 0,
          "Progress": 1.0,
          "BeginTime": "12:34:56.745",
          "ProcessTime": 44,
          "Status": "Succeed",
          "Childs": []
        }
      ]
    }
  ]
}
```

**用途：**
- 离线分析和归档
- 性能数据对比
- 问题复现和追踪

---

## 系统架构图

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Unity Editor Window                           │
│                   AssetBundleDebuggerWindow                          │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ UI Controls:                                                  │   │
│  │  - Sample Button (SampleOnce)                               │   │
│  │  - Record Toggle (SampleAuto)                               │   │
│  │  - View Mode Menu (Asset/Bundle/Operation View)            │   │
│  │  - Frame Slider (历史帧导航)                                │   │
│  │  - Search Field (关键词搜索)                                │   │
│  │  - Export Button (JSON 导出)                                │   │
│  └──────────────────────────────────────────────────────────────┘   │
└────────────┬──────────────────────────────────────────────────────────┘
             │
       ┌─────▼───────────────────────────────────┐
       │  RemoteEditorConnection (虚拟连接)       │
       │  - Register callbacks                   │
       │  - Send/Receive commands & reports      │
       └─────┬──────────────────────────────┬────┘
             │                              │
    ┌────────▼─────────────┐    ┌──────────▼───────────────┐
    │  RemoteCommand       │    │  DebugReport             │
    │  (Serialize)         │    │  (Deserialize)           │
    │  ↓ JSON              │    │  ← JSON                  │
    │  ↓ UTF-8 bytes       │    │  ← UTF-8 bytes           │
    └────────┬─────────────┘    └──────────┬───────────────┘
             │                              │
             │    ═══════════════════════   │
             │    Internet / Emulation      │
             │    ═══════════════════════   │
             │                              │
    ┌────────▼─────────────┐    ┌──────────▼───────────────┐
    │  PlayerConnection    │    │  RemotePlayerConnection  │
    │  (真实连接)          │    │  (虚拟连接)              │
    │  或模拟连接          │    │                          │
    └────────┬─────────────┘    └──────────┬───────────────┘
             │                              │
             └──────────────┬───────────────┘
                            │
                     ┌──────▼──────────────────────────────┐
                     │  RemoteDebuggerInRuntime             │
                     │  (MonoBehaviour)                     │
                     │  ┌──────────────────────────────┐   │
                     │  │ _sampleOnce (bool)           │   │
                     │  │ _autoSample (bool)           │   │
                     │  └──────────────────────────────┘   │
                     │  ┌──────────────────────────────┐   │
                     │  │ Awake() - 初始化连接         │   │
                     │  │ OnEnable() - 注册回调        │   │
                     │  │ LateUpdate() - 采样触发      │   │
                     │  │ OnHandleEditorMessage() - 收命令│  │
                     │  └──────────────────────────────┘   │
                     └──────────┬──────────────────────────┘
                                │
                         ┌──────▼────────────────────┐
                         │  YooAssets.GetDebugReport() │
                         │  (全系统数据收集入口)      │
                         └──────┬────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    │           │           │
         ┌──────────▼──┐  ┌─────▼────┐  ┌──▼─────────────┐
         │ResourcePkg 1│  │ResourcePkg2  │ResourcePackageN│
         └──────┬──────┘  └────┬─────┘  └──┬──────────┘
                │               │          │
                └───────────────┼──────────┘
                                │
                    ┌───────────▼──────────┐
                    │ DebugPackageData     │
                    │ ┌─────────────────┐  │
                    │ │ PackageName     │  │
                    │ │ ProviderInfos[] │  │
                    │ │ BundleInfos[]   │  │
                    │ │ OperationInfos[]│  │
                    │ └─────────────────┘  │
                    └─────────────────────┘
```

---

## 注意事项

1. **DEBUG 模式自动启用**
   - 诊断系统仅在 `DEBUG` 模式下启用（通过 `#if DEBUG` 条件编译）
   - Release 构建中不会包含诊断代码，无性能开销

2. **版本兼容性**
   - 编辑器和运行时的调试器版本必须一致
   - 版本不一致的数据会被自动丢弃

3. **历史数据限制**
   - 最多缓存 500 帧历史数据（可配置）
   - 超过限制后，最早的数据会被移除

4. **JSON 序列化深度限制**
   - Unity JsonUtility 序列化深度限制为 10 层
   - 操作树（Childs）嵌套过深可能导致序列化失败

5. **性能开销**
   - 单次采样：低开销，仅在需要时采集
   - 自动采样：每帧采集，有一定性能开销，建议仅在需要时开启

6. **LateUpdate 时机**
   - 采样在 LateUpdate 中执行，确保该帧所有操作已更新
   - 避免在采样过程中资源状态发生变化

7. **非序列化字典**
   - `DebugPackageData.BundleInfoDic` 使用 `[NonSerialized]` 标记
   - 字典在首次查询时才构建，减少序列化开销

8. **场景追踪条件编译**
   - `SpawnScene` 字段仅在 DEBUG 模式下赋值（`[Conditional("DEBUG")]`）
   - Release 构建中该字段为空字符串

---

## 性能优化建议

1. **按需采样**
   - 优先使用单次采样（Sample Once）
   - 仅在需要连续监控时开启自动采样（Record）

2. **及时关闭 Record**
   - 分析完成后及时关闭自动采样
   - 避免不必要的性能开销

3. **合理设置历史缓存**
   - 根据内存情况调整 `MaxReportCount`
   - 默认 500 帧已足够大多数分析场景

4. **导出数据离线分析**
   - 对于复杂的性能问题，导出 JSON 数据
   - 在编辑器外使用专业工具进行分析

5. **Release 构建移除诊断代码**
   - 确保 Release 构建使用 Release 配置
   - 诊断代码通过 `#if DEBUG` 自动移除
