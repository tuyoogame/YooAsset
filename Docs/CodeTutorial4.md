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
    string location = $"Assets/FairyRes/{name}{extension}";
    var assetPackage = YooAssets.GetAssetsPackage("DefaultPackage");
    var handle = assetPackage.LoadAssetSync(location , type);
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

详情参考 [UniTask 配置教程](../Assets/YooAsset/Samples~/UniTask%20Sample/README.md)

### 分布式构建解决方案

**1.3.0+版本升级指南**

在升级之前请导出AssetBundleCollector的配置为XML文件，然后升级YooAssets库。

首次需要打开AssetBundleCollector窗口，然后导入之前保存的XML文件。

在运行游戏之前，请保证资源包可以构建成功！

```c#
IEnumerator Start()
{
    // 初始化YooAssets资源系统（必须代码）
    YooAssets.Initialize();
    
    // 创建资源包实例
    var package = YooAssets.CreateAssetPackage("DefaultPackage");
    
    // 初始化资源包
    ......
    yield return package.InitializeAsync(createParameters);
    
    // 更新资源包版本
    ......
    var operation = package.UpdatePackageManifestAsync(packageCRC);
    yield return operation;
    
    // 下载更新文件
    var downloader = package.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
    downloader.BeginDownload();
    yield return downloader;
    
    // 加载资源对象
    var assetHandle = package.LoadAssetAsync("Assets/GameRes/npc.prefab");
    yield return assetHandle;
    ......
}

```

