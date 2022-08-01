# 资源加载

**加载方法**

- YooAssets.LoadAssetSync() 同步加载资源对象
- YooAssets.LoadAssetAsync() 异步加载资源对象
- YooAssets.LoadSubAssetsSync() 同步加载子资源对象
- YooAssets.LoadSubAssetsAsync() 异步加载子资源对象
- YooAssets.LoadSceneAsync() 异步加载场景
- YooAssets.GetRawFileAsync() 异步获取原生文件

**统一约定**

**Location**为资源的定位地址，也是加载资源对象的唯一标识符。

- DefaultLocationServices 默认资源定位服务，location代表的是资源对象的相对路径。

```c#
// 以工程内的音频文件为例："Assets/GameRes/Audio/bgMusic.mp3" 
// 设定资源路径的根目录为："Assets/GameRes"，后续加载的资源定位地址填写相对路径："Audio/bgMusic"
var createParameters = new YooAssets.EditorSimulateModeParameters();
createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
yield return YooAssets.InitializeAsync(createParameters);
......
YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic");
```

- AddressLocationServices 可寻址资源定位服务，location代表的是资源对象可寻址地址。

````c#
// 以工程内的音频文件为例："Assets/GameRes/Audio/bgMusic.mp3" 
// 需要在资源配置界面启用可寻址功能（Enable Addressable）。
// 配置界面的可寻址规则为AddressByFileName，那么资源定位地址填写文件名称："bgMusic"
var createParameters = new YooAssets.EditorSimulateModeParameters();
createParameters.LocationServices = new AddressLocationServices();
yield return YooAssets.InitializeAsync(createParameters);
......
YooAssets.LoadAssetAsync<AudioClip>("bgMusic");
````

**注意**：以下范例执行环境是在DefaultLocationServices下。

**加载路径的匹配方式**

````C#
// 不带扩展名的模糊匹配
YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic");

// 带扩展名的精准匹配
YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
````

**异步加载范例**

````C#
// 委托加载方式
void Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
    handle.Completed += Handle_Completed;
}
void Handle_Completed(AssetOperationHandle handle)
{
    AudioClip audioClip = handle.AssetObject as AudioClip;
}
````
````C#
// 协程加载方式
IEnumerator Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
    yield return handle;   
    AudioClip audioClip = handle.AssetObject as AudioClip;
}
````
````C#
// Task加载方式
async void Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
    await handle.Task;
    AudioClip audioClip = handle.AssetObject as AudioClip;	
}
````

**资源卸载范例**

````C#
IEnumerator Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
    yield return handle;
    ...
    handle.Release();
}
````

**资源释放范例**

可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。

注意：只有调用资源释放方法，资源对象才会在内存里被移除。

````c#
private void UnloadAssets()
{
    YooAssets.UnloadUnusedAssets();
}
````

**预制体加载范例**

````C#
IEnumerator Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<GameObject>("Panel/login.prefab");
    yield return handle;
    GameObject go = handle.InstantiateSync();
    Debug.Log($"Prefab name is {go.name}");
}
````

**子对象加载范例**

例如：通过TexturePacker创建的图集，如果需要访问图集的精灵对象，可以通过子对象加载接口。

````c#
IEnumerator Start()
{
    SubAssetsOperationHandle handle = YooAssets.LoadSubAssetsAsync<Sprite>(location);
    yield return handle;
    var sprite = handle.GetSubAssetObject<Sprite>("spriteName");
    Debug.Log($"Sprite name is {sprite.name}");
}
````

**场景异步加载范例**

注意：当加载新的主场景的时候，会自动释放之前加载的主场景以及附加场景。

````c#
IEnumerator Start()
{
    var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
    bool activateOnLoad = true;
    SceneOperationHandle handle = YooAssets.LoadSceneAsync("Scene/Login", sceneMode, activateOnLoad);
    yield return handle;
    Debug.Log($"Scene name is {handle.Scene.name}");
}
````

**原生文件加载范例**

例如：wwise的初始化文件

````c#
IEnumerator Start()
{
    string location = "wwise/init.bnk";
    string copyPath = $"{Application.persistentDataPath}/Audio/init.bnk";
    RawFileOperation operation = YooAssets.GetRawFileAsync(location, copyPath);
    yield return operation;
    byte[] fileData = operation.GetFileData();
    string fileText = operation.GetFileText();
}
````

**获取资源信息列表**

通过资源标签来获取资源信息列表。

````c#
private GetAssetInfosByTag(string tag)
{
    AssetInfo[] assetInfos = YooAssets.GetAssetInfos(tag);
    foreach (var assetInfo in assetInfos)
    {
        Debug.Log(assetInfo.AssetPath);
    }
}
````

