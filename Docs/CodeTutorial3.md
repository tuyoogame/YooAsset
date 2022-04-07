# 资源加载

在加载资源对象的时候只需要提供相对路径，统一约定该相对路径名称为：location

加载接口：

- YooAssets.LoadAssetSync() 同步加载资源对象接口
- YooAssets.LoadSubAssetsSync() 同步加载子资源对象接口
- YooAssets.LoadAssetAsync() 异步加载资源对象接口
- YooAssets.LoadSubAssetsAsync() 异步加载子资源对象接口
- YooAssets.LoadSceneAsync() 异步加载场景接口
- YooAssets.LoadRawFileAsync() 异步读取原生文件接口

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
    string savePath = $"{Application.persistentDataPath}/Audio/init.bnk";
    RawFileOperation operation = YooAssets.LoadRawFileAsync(location, savePath);
    yield return operation;
    byte[] fileData = operation.GetFileData();
    string fileText = operation.GetFileText();
}
````

**FairyGUI加载方案**

注意：在FairyGUI的面板销毁的时候，将资源句柄列表释放，否则会造成资源泄漏。

````c#
// 资源句柄列表
private List<AssetOperationHandle> _handles = new List<AssetOperationHandle>(100);

// 加载方法
private object LoadFunc(string name, string extension, System.Type type, out DestroyMethod method)
{
    method = DestroyMethod.None;
    string location = $"FairyRes/{name}{extension}";
    var handle = YooAssets.LoadAssetSync(location , type);
    _handles.Add(handle);
    return handle.AssetObject;
}

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

