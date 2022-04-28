# 初始化

资源系统的运行模式支持三种：编辑器模拟模式，单机运行模式，联机运行模式。

````C#
// 资源系统初始化方法，根据不同的模式，我们传递不同的创建参数类
YooAssets.InitializeAsync(CreateParameters parameters);
````

**编辑器模拟模式**

在编辑器下，不需要构建资源包，来模拟运行游戏。

注意：该模式只在编辑器下起效

````c#
private IEnumerator InitializeYooAsset()
{
    var createParameters = new YooAssets.EditorPlayModeParameters();
    createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    yield return YooAssets.InitializeAsync(createParameters);
}
````

**单机运行模式**

对于不需要热更新资源的游戏，可以使用单机运行模式。

注意：该模式需要构建资源包

````c#
private IEnumerator InitializeYooAsset()
{
    var createParameters = new YooAssets.OfflinePlayModeParameters();
    createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    yield return YooAssets.InitializeAsync(createParameters);
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

- ClearCacheWhenDirty : 安装包在覆盖安装的时候，是否清空沙盒缓存文件夹。

- DefaultHostServer : 默认的资源服务器IP地址。

- FallbackHostServer : 备用的资源服务器IP地址。

````c#
private IEnumerator InitializeYooAsset()
{
    var createParameters = new YooAssets.HostPlayModeParameters();
    createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
    createParameters.DecryptionServices = null;
    createParameters.ClearCacheWhenDirty = false;
    createParameters.DefaultHostServer = "http://127.0.0.1/CDN1/Android";
    createParameters.FallbackHostServer = "http://127.0.0.1/CDN2/Android";
    yield return YooAssets.InitializeAsync(createParameters);
}
````

**资源文件解密**  

````c#
public class BundleDecryption : IDecryptionServices
{
    public ulong GetFileOffset(BundleInfo bundleInfo)
    {
        return 32;
    }
}
````

