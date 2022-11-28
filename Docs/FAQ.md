# 常见问题解答

#### 问题：在编辑器下，用离线模式或联机模式运行游戏，为什么游戏里的模型会变成紫色？

如果在打AssetBundle的时候，选定的构建目标是安卓。那么在windows操作系统下，编辑器的默认渲染模式为DX11，我们需要修改编辑器的渲染模式，可以通过UnityHub来修改启动项目的编辑器渲染模式，[参考官方文档](https://docs.unity3d.com/cn/2019.4/Manual/CommandLineArguments.html)。

windows平台添加命令: **-force-gles**

#### 问题：Unity2021编辑器运行游戏提示YooAssets is initialized !

尝试关闭：Project Setting ---> Editor ---> Enter Play Mode Options

#### 问题：YooAsset的DLL引用丢失导致编译报错了

1. 请在PlayerSetting里修改API Level为.NET 4.x或者.NET Framework
2. 关闭游戏工程后，删除Assets同级目录下所有的csproj文件和sln文件。
3. 删除Library/ScriptAssemblies文件夹。
4. 重新打开游戏工程，然后点击某个脚本重新编译。

#### 问题：UnityEditor.Build.Pipeline引用丢失问题

YooAsset依赖于ScriptBuildPipeline（SBP），在PackageManager里找到SBP插件安装就可以了。

#### 问题：使用FileZilla等FTP上传工具后，文件下载总是验证失败

把传输类型修改为二进制就可以了。

#### 问题：打包的时候报错：Cannot mark assets and scenes in one AssetBundle. AssetBundle name is "assets_xxxx_scenes.bundle

Unity引擎不允许把场景文件和其它资源文件一起打包。

#### 问题：ClearCacheWhenDirty参数没了吗？

不是很必须的一个功能，已经移除了。可以使用以下方法代替：

````c#
// 参考DEMO的代码
internal class FsmClearCache : IFsmNode
{
    void IFsmNode.OnEnter()
    {
        Debug.Log("清理未使用的缓存文件！");
        var package = YooAssets.GetPackage("DefaultPackage");
        var operation = package.ClearUnusedCacheFiles();
        operation.Completed += Operation_Completed;
    }

    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
        Debug.Log("开始游戏！");
        ......
    }
}
````

#### 问题：YooAsset支持Unity2018吗

YooAsset分俩部分，编辑器代码和运行时代码。因为工具界面是使用UIElements编写的，所以在Unity2019以前的版本是使用不了界面化工具。但是这并没有影响我们使用YooAsset，以下提供一种解决方案。

请先通过Package Manager安装Scriptable Build Pipeline插件。

1. 资源包收集工具替代方案

````C#
// 推荐直接手动编辑资源收集配置文件，在Sample工程里可以找到AssetBundleCollectorConfig.xml的文件，我们直接拿过来做模板。
// 然后通过以下代码来导入配置文件，成功之后AssetBundleCollectorSetting.asset文件会被刷新，就可以运行游戏了。
// 注意：每次修改完XML文件，都需要导入配置文件。
AssetBundleCollectorConfig.ImportXmlConfig("C://Demo//Assets//AssetBundleCollectorConfig.xml");
````

2. 资源包构建工具替代方案

````c#
// 资源包构建可以直接参考教程文档，在文档的最下面有Jenkins支持介绍。
````

资源构建教程：https://github.com/tuyoogame/YooAsset/blob/main/Docs/AssetBundleBuilder.md

3. 资源包报告工具替代方案

````c#
// 我们可以使用Unity2019或更高版本来创建一个包含YooAsset的工程查看构建报告。
````

4. 资源包调试工具替代方案

````c#
// YooAsset支持真机远程调试，我们可以使用Unity2019或更高版本来创建一个包含YooAsset的工程调试。
// 如果想在编辑器下调试，可以仿照编写一个调试界面。
````

