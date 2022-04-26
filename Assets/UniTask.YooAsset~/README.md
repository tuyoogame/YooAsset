# UniTask 扩展

[仓库链接](https://github.com/Cysharp/UniTask) 
- 请去下载对应的源码，并删除此目录最后的波浪线
- 在 UniTask `_InternalVisibleTo.cs` 文件中增加 `[assembly: InternalsVisibleTo("UniTask.YooAsset")]` 后即可使用

## 代码示例

```csharp
var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/Res/Prefabs/TestImg.prefab");

await handle.ToUniTask();

var obj = handle.AssetObject as GameObject;
var go  = Instantiate(obj, transform);

go.transform.localPosition = Vector3.zero;
go.transform.localScale    = Vector3.one;
```