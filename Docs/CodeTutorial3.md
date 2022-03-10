# 资源加载

在加载资源对象的时候只需要提供相对路径，统一约定该相对路径名称为：location

资源加载接口：

- YooAssets.LoadAssetSync() 同步加载资源对象接口
- YooAssets.LoadSubAssetsSync() 同步加载子资源对象接口
- YooAssets.LoadAssetAsync() 异步加载资源对象接口
- YooAssets.LoadSubAssetsAsync() 异步加载子资源对象接口
- YooAssets.LoadSceneAsync() 异步加载场景接口

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
void Start()
{
    this.StartCoroutine(AsyncLoad());
}
IEnumerator AsyncLoad()
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
    await AsyncLoad();
}
async Task AsyncLoad()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");
    await handle.Task;
    AudioClip audioClip = handle.AssetObject as AudioClip;	
}
````

**资源卸载范例**

````C#
void Start()
{
    AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Audio/bgMusic.mp3");

    ...

    handle.Release();
}
````

**预制体同步加载范例**

````C#
var handle = YooAssets.LoadAssetSync<GameObject>(location);
GameObject go = handle.InstantiateObject;
````

````c#
var handle = YooAssets.LoadAssetSync<GameObject>(location);
GameObject go = UnityEngine.Object.Instantiate(handle.AssetObject as GameObject);
````

**子对象同步加载范例**

例如：通过TexturePacker创建的图集，如果需要访问图集的精灵对象，可以通过子对象加载接口。

````c#
var handle = YooAssets.LoadSubAssetsSync<Sprite>(location);
foreach (var asset in handle.AllAssets)
{
    Debug.Log($"Sprite name is {asset.name}");
}
````

**场景异步加载范例**

````c#
void Start()
{
    // 场景加载参数
    SceneInstanceParam param = new SceneInstanceParam();
    param.LoadMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
    param.ActivateOnLoad = true;

    AssetOperationHandle handle = YooAssets.LoadSceneAsync("Scene/Login", param);
    handle.Completed += Handle_Completed;
}
void Handle_Completed(AssetOperationHandle handle)
{
    SceneInstance instance = handle.AssetInstance as SceneInstance;
    Debug.Log(instance.Scene.name);
}
````

**原生文件加载范例**

例如：wwise的初始化文件

````c#
void Start()
{
    //获取资源包信息
    string location = "wwise/init.bnk";
    BundleInfo bundleInfo = YooAssets.GetBundleInfo(location);
    
    //文件路径
    string fileSourcePath = bundleInfo.LocalPath;
    string fileDestPath = $"{Application.persistentDataPath}/Audio/init.bnk";
    
    //拷贝文件
    File.Copy(fileSourcePath, fileDestPath, true);
    
    //注意：在安卓平台下，可以通过如下方法判断文件是否在APK内部。
    if(bundleInfo.IsBuildinJarFile())
    {
        ...
    }
}
````

