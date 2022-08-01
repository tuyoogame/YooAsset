# 解决方案

**FairyGUI支持解决方案**

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

**UniTask支持解决方案**

[解决方案](https://github.com/tuyoogame/YooAsset/blob/master/Assets/YooAsset/Samples~/UniTask Sample/README.md)
