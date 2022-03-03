# 资源收集

![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/AssetCollector-img1.jpg)

### 界面介绍

**着色器收集**

勾选收集所有着色器复选框后，打包系统会自动收集所有依赖的材质球使用的着色器，并将这些着色器打进一个AssetBundle文件内。

**Directory**

收集的资源目录，目录下的所有文件将会根据打包规则和过滤规则进行打包。

**PackRule**

打包规则，规则可以自定义扩展。下面是内置的打包规则：

1. PackExplicit 目录下的资源文件会各自打进自己的资源包里。
2. PackDirectory 目录下的资源文件会被打进一个资源包里。
3. PackRawFile 目录下的资源文件会被处理为原生资源包。

自定义扩展范例

````c#
public class PackDirectory : IPackRule
{
    string IPackRule.GetAssetBundleLabel(string assetPath)
    {
        return Path.GetDirectoryName(assetPath); //"Assets/Config/test.txt" --> "Assets/Config"
    }
}
````

**FilterRule**

过滤规则，规则可以自定义扩展。下面是内置的过滤规则：

1. CollectAll 收集目录下的所有资源文件
2. CollectScene 只收集目录下的场景文件
3. CollectPrefab 只收集目录下的预制体文件
4. CollectSprite 只收集目录下的精灵类型的文件

自定义扩展范例

````c#
public class CollectScene : IFilterRule
{
    public bool IsCollectAsset(string assetPath)
    {
        return Path.GetExtension(assetPath) == ".unity";
    }
}
````

**DontWriteAssetPath**

资源目录下的资源对象不写入清单

**AssetTags**

资源标签列表（多个标签使用分号间隔）

### 配置表

点击Import按钮可以导入外部的XML配置表，配置表规范如下图：

````xml
<root>
    <Collector Directory="Assets/GameRes/UIAtlas/" PackRule="PackExplicit" FilterRule="CollectAll" DontWriteAssetPath="0" AssetTags=""/>
    <Collector Directory="Assets/GameRes/UIPanel/" PackRule="PackExplicit" FilterRule="CollectAll" DontWriteAssetPath="0" AssetTags=""/>
</root>
````

