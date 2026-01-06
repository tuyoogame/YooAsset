# OperationSystem 异步操作系统

## 模块概述

OperationSystem 是 YooAsset 资源管理系统的**异步操作调度核心**，负责管理所有异步操作的生命周期、调度执行和状态追踪。该模块提供了统一的异步操作抽象，支持协程、async/await、回调等多种异步编程模式。

### 核心职责

- 异步操作的统一抽象和生命周期管理
- 基于优先级的操作调度
- 时间切片执行（防止主线程阻塞）
- 多种异步编程模式支持
- 操作状态追踪和调试信息收集

---

## 设计目标

| 目标 | 说明 |
|------|------|
| **统一抽象** | 所有异步操作继承同一基类，接口一致 |
| **灵活调度** | 支持优先级排序、时间切片、帧预算控制 |
| **多模式支持** | 协程（IEnumerator）、Task（async/await）、回调 |
| **可调试性** | 完整的状态追踪、耗时统计、层级关系 |
| **线程安全** | 所有调度逻辑在主线程执行 |

---

## 架构概念

### 系统架构

```
┌─────────────────────────────────────────────────────────┐
│                    上层调用者                            │
│         (ResourceManager / FileSystem / 业务层)          │
└─────────────────────────┬───────────────────────────────┘
                          │ StartOperation()
┌─────────────────────────▼───────────────────────────────┐
│                   OperationSystem                        │
│                     (调度器)                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │  优先级队列  │  │  时间切片   │  │  回调通知   │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
└─────────────────────────┬───────────────────────────────┘
                          │ UpdateOperation()
┌─────────────────────────▼───────────────────────────────┐
│                  AsyncOperationBase                      │
│                    (操作基类)                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │  状态机     │  │  子任务管理  │  │  异步模式   │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
└─────────────────────────────────────────────────────────┘
```

### 核心组件

- **OperationSystem**: 静态调度器，管理所有操作的执行
- **AsyncOperationBase**: 异步操作基类，定义生命周期和状态
- **GameAsyncOperation**: 游戏层操作基类，提供更友好的 API
- **EOperationStatus**: 操作状态枚举

---

## 文件结构

```
OperationSystem/
├── EOperationStatus.cs        # 操作状态枚举
├── AsyncOperationBase.cs      # 异步操作基类
├── OperationSystem.cs         # 异步操作调度器
└── GameAsyncOperation.cs      # 游戏层操作基类
```

---

## 枚举定义

### EOperationStatus（操作状态）

```csharp
public enum EOperationStatus
{
    None,        // 未开始
    Processing,  // 处理中
    Succeed,     // 已成功
    Failed       // 已失败
}
```

**状态转换：**

```
      StartOperation()              InternalUpdate()
None ─────────────────► Processing ─────────────────┬──► Succeed
                                                    │
                                                    └──► Failed
```

---

## 核心类说明

### AsyncOperationBase（异步操作基类）

所有异步操作的抽象基类，实现了 `IEnumerator` 和 `IComparable<AsyncOperationBase>` 接口。

#### 公共属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Priority` | `uint` | 任务优先级（值越大越优先） |
| `Status` | `EOperationStatus` | 当前状态 |
| `Error` | `string` | 错误信息（失败时） |
| `Progress` | `float` | 处理进度（0-1） |
| `PackageName` | `string` | 所属包裹名称 |
| `IsDone` | `bool` | 是否已完成（Succeed 或 Failed） |
| `Task` | `Task` | 用于 async/await |
| `BeginTime` | `string` | 开始时间（调试用） |
| `ProcessTime` | `long` | 处理耗时毫秒（调试用） |

#### 公共事件

```csharp
/// <summary>
/// 完成事件（支持后注册立即触发）
/// </summary>
public event Action<AsyncOperationBase> Completed;
```

#### 公共方法

```csharp
/// <summary>
/// 同步等待异步操作完成
/// </summary>
public void WaitForAsyncComplete();
```

#### 内部抽象方法（子类实现）

| 方法 | 说明 |
|------|------|
| `InternalStart()` | 操作开始时调用 |
| `InternalUpdate()` | 每帧更新时调用 |
| `InternalAbort()` | 操作中止时调用（可选） |
| `InternalWaitForAsyncComplete()` | 同步等待时调用（可选） |
| `InternalGetDesc()` | 获取操作描述（可选） |

#### 子任务管理

```csharp
// 子任务列表
internal readonly List<AsyncOperationBase> Childs;

// 添加/移除子任务
internal void AddChildOperation(AsyncOperationBase child);
internal void RemoveChildOperation(AsyncOperationBase child);
```

---

### OperationSystem（调度器）

静态类，负责异步操作的调度和管理。

#### 配置属性

```csharp
/// <summary>
/// 每帧最大执行时间（毫秒）
/// 默认值：long.MaxValue（无限制）
/// </summary>
public static long MaxTimeSlice { set; get; }

/// <summary>
/// 当前帧是否已超时
/// </summary>
public static bool IsBusy { get; }
```

#### 核心方法

```csharp
/// <summary>
/// 初始化异步操作系统
/// </summary>
public static void Initialize();

/// <summary>
/// 每帧更新（由 YooAssets 驱动）
/// </summary>
public static void Update();

/// <summary>
/// 销毁所有操作
/// </summary>
public static void DestroyAll();

/// <summary>
/// 清理指定包裹的所有操作
/// </summary>
public static void ClearPackageOperation(string packageName);

/// <summary>
/// 启动异步操作
/// </summary>
public static void StartOperation(string packageName, AsyncOperationBase operation);
```

#### 回调监听

```csharp
/// <summary>
/// 注册任务开始回调
/// </summary>
public static void RegisterStartCallback(Action<string, AsyncOperationBase> callback);

/// <summary>
/// 注册任务结束回调
/// </summary>
public static void RegisterFinishCallback(Action<string, AsyncOperationBase> callback);
```

---

### GameAsyncOperation（游戏层基类）

继承 `AsyncOperationBase`，为业务层提供更友好的 API。

```csharp
public abstract class GameAsyncOperation : AsyncOperationBase
{
    /// <summary>
    /// 异步操作开始
    /// </summary>
    protected abstract void OnStart();

    /// <summary>
    /// 异步操作更新
    /// </summary>
    protected abstract void OnUpdate();

    /// <summary>
    /// 异步操作终止
    /// </summary>
    protected abstract void OnAbort();

    /// <summary>
    /// 异步等待完成（可选重写）
    /// </summary>
    protected virtual void OnWaitForAsyncComplete();

    /// <summary>
    /// 异步操作系统是否繁忙
    /// </summary>
    protected bool IsBusy();

    /// <summary>
    /// 终止异步操作
    /// </summary>
    protected void Abort();
}
```

---

## 异步编程模式

### 1. 协程模式（IEnumerator）

```csharp
IEnumerator LoadAsset()
{
    var operation = package.LoadAssetAsync<GameObject>("Assets/Prefab.prefab");
    yield return operation;

    if (operation.Status == EOperationStatus.Succeed)
    {
        GameObject prefab = operation.AssetObject as GameObject;
    }
}
```

### 2. Task 模式（async/await）

```csharp
async Task LoadAssetAsync()
{
    var operation = package.LoadAssetAsync<GameObject>("Assets/Prefab.prefab");
    await operation.Task;

    if (operation.Status == EOperationStatus.Succeed)
    {
        GameObject prefab = operation.AssetObject as GameObject;
    }
}
```

### 3. 回调模式（Completed 事件）

```csharp
void LoadAsset()
{
    var operation = package.LoadAssetAsync<GameObject>("Assets/Prefab.prefab");
    operation.Completed += OnLoadCompleted;
}

void OnLoadCompleted(AsyncOperationBase op)
{
    var operation = op as AssetHandle;
    if (operation.Status == EOperationStatus.Succeed)
    {
        GameObject prefab = operation.AssetObject as GameObject;
    }
}
```

### 4. 同步等待模式

```csharp
void LoadAssetSync()
{
    var operation = package.LoadAssetAsync<GameObject>("Assets/Prefab.prefab");
    operation.WaitForAsyncComplete();  // 阻塞等待完成

    if (operation.Status == EOperationStatus.Succeed)
    {
        GameObject prefab = operation.AssetObject as GameObject;
    }
}
```

---

## 调度机制

### 优先级调度

操作按 `Priority` 属性降序排列，优先级高的操作先执行。

```csharp
var operation = package.LoadAssetAsync<GameObject>(location);
operation.Priority = 100;  // 设置高优先级
```

**排序规则：**
- 新操作添加时检查是否需要排序
- 仅当存在非零优先级时触发排序
- 使用 `List.Sort()` 进行原地排序

### 时间切片

通过 `MaxTimeSlice` 控制每帧最大执行时间，防止主线程阻塞。

```csharp
// 设置每帧最多执行 8 毫秒
OperationSystem.MaxTimeSlice = 8;
```

**执行流程：**

```
每帧 Update()
    │
    ├── 记录帧开始时间 _frameTime
    │
    └── 遍历操作队列
            │
            ├── 检查 IsBusy（是否超时）
            │       │
            │       └── 超时则中断本帧
            │
            └── 执行 operation.UpdateOperation()
```

### 操作生命周期

```
┌─────────────────────────────────────────────────────────────────┐
│                        操作生命周期                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   1. 创建操作                                                    │
│      └── Status = None                                          │
│                                                                 │
│   2. StartOperation()                                           │
│      ├── Status = Processing                                    │
│      ├── DebugBeginRecording()                                  │
│      ├── InternalStart()                                        │
│      └── 添加到 _newList                                         │
│                                                                 │
│   3. Update() - 每帧调度                                         │
│      ├── 移除已完成操作                                          │
│      ├── 合并 _newList 到 _operations                            │
│      ├── 按优先级排序（如需要）                                   │
│      └── 遍历执行 UpdateOperation()                              │
│                                                                 │
│   4. UpdateOperation()                                          │
│      ├── DebugUpdateRecording()                                 │
│      ├── InternalUpdate()                                       │
│      └── 检查 IsDone                                             │
│             │                                                   │
│             └── 完成时：                                         │
│                  ├── IsFinish = true                            │
│                  ├── Progress = 1f                              │
│                  ├── DebugEndRecording()                        │
│                  ├── 触发 Completed 回调                         │
│                  └── 设置 TaskCompletionSource                   │
│                                                                 │
│   5. 下一帧移除完成的操作                                         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 调试支持

### 调试信息结构

```csharp
internal struct DebugOperationInfo
{
    public string OperationName;   // 操作类型名
    public string OperationDesc;   // 操作描述
    public uint Priority;          // 优先级
    public float Progress;         // 进度
    public string BeginTime;       // 开始时间
    public long ProcessTime;       // 处理耗时（毫秒）
    public string Status;          // 状态
    public List<DebugOperationInfo> Childs;  // 子操作
}
```

### 获取调试信息

```csharp
// 获取指定包裹的所有操作信息
var infos = OperationSystem.GetDebugOperationInfos("DefaultPackage");

foreach (var info in infos)
{
    Debug.Log($"{info.OperationName}: {info.Status}, {info.ProcessTime}ms");
}
```

### 耗时统计

在 DEBUG 模式下自动统计操作耗时：

```csharp
// 操作完成后可获取耗时
Debug.Log($"开始时间: {operation.BeginTime}");
Debug.Log($"处理耗时: {operation.ProcessTime}ms");
```

---

## 使用示例

### 自定义异步操作

```csharp
public class MyCustomOperation : GameAsyncOperation
{
    private int _step = 0;

    protected override void OnStart()
    {
        // 初始化操作
        _step = 0;
    }

    protected override void OnUpdate()
    {
        // 检查系统是否繁忙（时间切片）
        if (IsBusy())
            return;

        // 执行步骤
        switch (_step)
        {
            case 0:
                // 第一步
                Progress = 0.3f;
                _step = 1;
                break;
            case 1:
                // 第二步
                Progress = 0.6f;
                _step = 2;
                break;
            case 2:
                // 完成
                Status = EOperationStatus.Succeed;
                break;
        }
    }

    protected override void OnAbort()
    {
        // 清理资源
    }
}
```

### 启动自定义操作

```csharp
var operation = new MyCustomOperation();
OperationSystem.StartOperation("DefaultPackage", operation);

// 使用回调
operation.Completed += (op) =>
{
    if (op.Status == EOperationStatus.Succeed)
        Debug.Log("操作成功");
};

// 或使用 await
await operation.Task;
```

### 带子任务的操作

```csharp
public class ParentOperation : GameAsyncOperation
{
    private ChildOperation _child;

    protected override void OnStart()
    {
        _child = new ChildOperation();
        AddChildOperation(_child);  // 添加子任务
        OperationSystem.StartOperation(PackageName, _child);
    }

    protected override void OnUpdate()
    {
        if (_child.IsDone)
        {
            if (_child.Status == EOperationStatus.Succeed)
                Status = EOperationStatus.Succeed;
            else
                Status = EOperationStatus.Failed;
        }
    }

    protected override void OnAbort()
    {
        // 子任务会自动中止
    }
}
```

---

## 设计模式

### 模板方法模式

`AsyncOperationBase` 定义算法骨架，子类实现具体步骤：

```
AsyncOperationBase
    │
    ├── StartOperation()     ──► InternalStart()    [子类实现]
    ├── UpdateOperation()    ──► InternalUpdate()   [子类实现]
    ├── AbortOperation()     ──► InternalAbort()    [子类实现]
    └── WaitForAsyncComplete() ──► InternalWaitForAsyncComplete() [子类实现]
```

### 状态机模式

操作状态由 `EOperationStatus` 管理：

```
┌──────┐  StartOperation()  ┌────────────┐  UpdateOperation()  ┌─────────┐
│ None │ ─────────────────► │ Processing │ ──────────────────► │ Succeed │
└──────┘                    └────────────┘                     └─────────┘
                                   │
                                   │ UpdateOperation() / AbortOperation()
                                   ▼
                            ┌──────────┐
                            │  Failed  │
                            └──────────┘
```

### 组合模式

通过 `Childs` 列表支持父子操作关系：

```
ParentOperation
    ├── ChildOperation1
    ├── ChildOperation2
    └── ChildOperation3
            └── GrandChildOperation
```

---

## 类继承关系

```
IEnumerator + IComparable<AsyncOperationBase>
              │
              ▼
    AsyncOperationBase (抽象基类)
              │
              ├── GameAsyncOperation (游戏层基类)
              │         │
              │         └── [业务层自定义操作]
              │
              └── [YooAsset 内部操作]
                      │
                      ├── InitializationOperation
                      ├── LoadAssetOperation
                      ├── LoadSceneOperation
                      ├── DownloadOperation
                      └── ...
```

---

## 注意事项

1. **主线程执行**：所有操作的调度和更新都在 Unity 主线程执行
2. **时间切片**：设置合理的 `MaxTimeSlice` 避免卡顿（建议 8-16ms）
3. **同步等待**：`WaitForAsyncComplete()` 会阻塞主线程，谨慎使用
4. **子任务中止**：父操作中止时会自动中止所有子操作
5. **回调异常**：`Completed` 回调中的异常会被捕获并记录，不会中断系统
6. **编辑器重置**：编辑器中使用 `RuntimeInitializeOnLoadMethod` 自动重置状态
7. **循环保护**：`WaitForAsyncComplete()` 有 1000 帧上限，防止无限循环
