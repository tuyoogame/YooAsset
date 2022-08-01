# 解决方案

### FairyGUI支持解决方案

注意：在FairyGUI的面板销毁的时候，将资源句柄列表释放，否则会造成资源泄漏。

````c#
// 资源句柄列表
private List<AssetOperationHandle> _handles = new List<AssetOperationHandle>(100);

// 加载方法
private object LoadFunc(string name, string extension, System.Type type, out DestroyMethod method)
{
    method = DestroyMethod.None; //注意：这里一定要设置为None
    string location = $"FairyRes/{name}{extension}";
    var handle = YooAssets.LoadAssetSync(location , type);
    _handles.Add(handle);
    return handle.AssetObject;
}

// 执行FairyGUI的添加包函数
UIPackage.AddPackage(name, LoadFunc);

// 释放资源句柄列表
private void ReleaseHandles()
{
    foreach(var handle in _handles)
    {
        handle.Release();
    }
    _handles.Clear();
}
````

### UniTask支持解决方案

[仓库链接](https://github.com/Cysharp/UniTask) 

- 请去下载对应的源码，并删除此目录最后的波浪线
- 在项目的 `asmdef` 文件中添加对 `UniTask.YooAsset` 的引用
- 在 UniTask `_InternalVisibleTo.cs` 文件中增加 `[assembly: InternalsVisibleTo("UniTask.YooAsset")]` 后即可使用

代码示例

```csharp
var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/Res/Prefabs/TestImg.prefab");

await handle.ToUniTask();

var obj = handle.AssetObject as GameObject;
var go  = Instantiate(obj, transform);

go.transform.localPosition = Vector3.zero;
go.transform.localScale    = Vector3.one;
```
