# 初始化

初始化资源系统

```c#
YooAssets.Initialize();
```

资源系统的运行模式支持三种：编辑器模拟模式，单机运行模式，联机运行模式。

**编辑器模拟模式**

在编辑器下，不需要构建资源包，来模拟运行游戏。

注意：该模式只在编辑器下起效

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new YooAssets.EditorSimulateModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    initParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage", false);
    yield return YooAssets.InitializeAsync(initParameters);
}
````

**单机运行模式**

对于不需要热更新资源的游戏，可以使用单机运行模式。

注意：该模式需要构建资源包

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new YooAssets.OfflinePlayModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    yield return YooAssets.InitializeAsync(initParameters);
}
````

**联机运行模式**

对于需要热更新资源的游戏，可以使用联机运行模式，该模式下初始化参数会很多。

注意：该模式需要构建资源包

- LocationServices : 资源定位的实例类。
  
  (1) 默认的资源定位服务类（DefaultLocationServices）
  
  (2) 可寻址的资源定位服务类（AddressLocationServices）
  
  (3) 开发者自定义的资源定位服务类，需要提供实现ILocationServices接口的实例类。
  
- DecryptionServices : 如果资源包在构建的时候有加密，需要提供实现IDecryptionServices接口的实例类。

- DefaultHostServer : 默认的资源服务器IP地址。

- FallbackHostServer : 备用的资源服务器IP地址。

- VerifyLevel : 下载文件校验等级

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new YooAssets.HostPlayModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    initParameters.DecryptionServices = new BundleDecryptionServices();
    initParameters.QueryServices = new QueryStreamingAssetsServices();
    initParameters.DefaultHostServer = "http://127.0.0.1/CDN1/Android/v1.0";
    initParameters.FallbackHostServer = "http://127.0.0.1/CDN2/Android/v1.0";
    yield return YooAssets.InitializeAsync(initParameters);
}

// 文件解密服务类
private class BundleDecryptionServices : IDecryptionServices
{
    public ulong GetFileOffset(DecryptionFileInfo fileInfo)
    {
        return 32;
    }
}

// 内置文件查询服务类
private class QueryStreamingAssetsServices : IQueryServices
{
    public bool QueryStreamingAssets(string fileName)
    {
        // 注意：使用了BetterStreamingAssets插件
        return BetterStreamingAssets.FileExists($"YooAssets/{fileName}");
    }
}
````

