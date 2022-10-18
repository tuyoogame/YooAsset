# 初始化

初始化资源系统

```c#
// 初始化资源系统
YooAssets.Initialize();

// 创建默认的资源包
var defaultPackage = YooAssets.CreateAssetsPackage("DefaultPackage");

// 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
YooAssets.SetDefaultAssetsPackage(defaultPackage);
```

资源系统的运行模式支持三种：编辑器模拟模式，单机运行模式，联机运行模式。

**编辑器模拟模式**

在编辑器下，不需要构建资源包，来模拟运行游戏。

注意：该模式只在编辑器下起效

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new EditorSimulateModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    initParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
    yield return defaultPackage.InitializeAsync(initParameters);
}
````

**单机运行模式**

对于不需要热更新资源的游戏，可以使用单机运行模式。

注意：该模式需要构建资源包

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new OfflinePlayModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    yield return defaultPackage.InitializeAsync(initParameters);
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

- QueryServices：内置资源查询服务接口。

- DefaultHostServer : 默认的资源服务器IP地址。

- FallbackHostServer : 备用的资源服务器IP地址。

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new HostPlayModeParameters();
    initParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    initParameters.DecryptionServices = new BundleDecryptionServices();
    initParameters.QueryServices = new QueryStreamingAssetsServices();
    initParameters.DefaultHostServer = "http://127.0.0.1/CDN1/Android/v1.0";
    initParameters.FallbackHostServer = "http://127.0.0.1/CDN2/Android/v1.0";
    yield return defaultPackage.InitializeAsync(initParameters);
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
        string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
        return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
    }
}
````

