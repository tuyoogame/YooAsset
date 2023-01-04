# UniTask 扩展

这里为了照顾新手使用，做了一些妥协，有定制需求的需要手动调整一下

## 代码示例

```csharp
public async UniTask Example(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
{
    var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/Res/Prefabs/  TestImg.prefab");

    await handle.ToUniTask(progress, timing);

    var obj = handle.AssetObject as GameObject;
    var go  = Instantiate(obj, transform);

    go.transform.localPosition = Vector3.zero;
    go.transform.localScale    = Vector3.one;
}
```

## 初学者教程

**如果你弄不明白 asmdef 文件到底是啥，就按照下发内容操作**

- 将 `Samples/UniTask Sample/UniTask` 文件夹拷入游戏中
- 如果项目有 `asmdef`，则引用 `UniTask` 和 `YooAsset`，如果没有，就不用关心这一步


## 项目定制教程

- 请去下载 [UniTask](https://github.com/Cysharp/UniTask) 源码
    - 注意不要用 `Sample` 里面的  `UniTask` 这个是专门给新手定制的
- 将 `Samples/UniTask Sample/UniTask/Runtime/External/YooAsset` 文件夹拷贝到 `UniTask/Runtime/External/YooAsset` 中
- 创建 `UniTask.YooAsset.asmdef` 文件
- 添加 `UniTask` 和 `YooAsset` 的引用 
- 在 UniTask `_InternalVisibleTo.cs` 文件中增加 `[assembly: InternalsVisibleTo("UniTask.YooAsset")]` 后即可使用

