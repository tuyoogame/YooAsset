# 初始化

初始化资源系统

```c#
// 初始化资源系统
YooAssets.Initialize();

// 创建默认的资源包
var package = YooAssets.CreateAssetsPackage("DefaultPackage");

// 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
YooAssets.SetDefaultAssetsPackage(package);
```

资源系统的运行模式支持三种：编辑器模拟模式，单机运行模式，联机运行模式。

**编辑器模拟模式**

在编辑器下，不需要构建资源包，来模拟运行游戏。

注意：该模式只在编辑器下起效

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new EditorSimulateModeParameters();
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
    yield return defaultPackage.InitializeAsync(initParameters);
}
````

**联机运行模式**

对于需要热更新资源的游戏，可以使用联机运行模式，该模式下初始化参数会很多。

注意：该模式需要构建资源包

- DecryptionServices : 如果资源包在构建的时候有加密，需要提供实现IDecryptionServices接口的实例类。

- QueryServices：内置资源查询服务接口。

- DefaultHostServer : 默认的资源服务器IP地址。

- FallbackHostServer : 备用的资源服务器IP地址。

````c#
private IEnumerator InitializeYooAsset()
{
    var initParameters = new HostPlayModeParameters();
    initParameters.QueryServices = new QueryStreamingAssetsFileServices();
    initParameters.DefaultHostServer = "http://127.0.0.1/CDN1/Android/v1.0";
    initParameters.FallbackHostServer = "http://127.0.0.1/CDN2/Android/v1.0";
    yield return defaultPackage.InitializeAsync(initParameters);
}

// 内置文件查询服务类
private class QueryStreamingAssetsFileServices : IQueryServices
{
    public bool QueryStreamingAssets(string fileName)
    {
        // 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
        string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
        return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
    }
}
````

### 源代码解析

- 编辑器模拟模式

  每次启动调用EditorSimulateModeHelper.SimulateBuild()方法，都会在底层执行一次模拟构建（Simulate Build）。

  如果参与构建的资源对象数量级很大的话则会有卡顿现象，可以通过直接指定已有的清单路径来避免每次都重复执行模拟构建。

- 单机运行模式

  在初始化的时候，会直接读取内置清单文件（StreamingAssets文件夹里的文件），最后根据加载的清单去验证沙盒里缓存的文件。

- 联机运行模式

  在初始化的时候，会优先从沙盒里加载清单，如果沙盒里不存在，则会尝试加载内置清单并将其拷贝到沙盒里。最后根据加载的清单去验证沙盒里缓存的文件。

  **注意**：如果沙盒清单和内置清单都不存在，初始化也会被判定为成功！

  
