# 资源收集

![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/AssetGrouper-img1.png)

左侧为分组列表，右侧为该分组的配置界面。

导出按钮可以将配置数据导出为XML文件，导入按钮可以导入保存的XML文件。

#### 着色器收集

- Auto Collect Shaders

  自动收集所有依赖的材质球使用的着色器，并将这些着色器打进一个资源包里。

- Shader Bundle Name

  收集的着色器资源包名称。

#### 资源分组

- Grouper Name

  分组名称

- Grouper Desc

  分组备注信息

- Asset Tags

  资源分类标签列表，该分组下收集的资源会全部被打上该标签。

  注意：多个标签用分号隔开，例如 level1;level2;level3

#### 资源搜集器

- **Collect Path**

  收集路径，可以指定文件夹或单个资源文件。

- **PackRule**

  打包规则，规则可以自定义扩展。下面是内置的打包规则：

  - PackSeparately 以文件路径作为资源包名，每个资源文件单独打包。
  - PackDirectory 以父类文件夹路径作为资源包名，打进一个资源包。
  - PackCollector 以收集器路径作为资源包名，打进一个资源包。
  - PackGrouper 以分组名称作为资源包名，打进一个资源包。
  - PackRawFile 目录下的资源文件会被处理为原生资源包。

````c#
//自定义扩展范例
public class PackDirectory : IPackRule
{
    string IPackRule.GetBundleName(PackRuleData data)
    {
        return Path.GetDirectoryName(data.AssetPath); //"Assets/Config/test.txt" --> "Assets/Config"
    }
}
````

- **FilterRule**

  过滤规则，规则可以自定义扩展。下面是内置的过滤规则：

  - CollectAll 收集目录下的所有资源文件

  - CollectScene 只收集目录下的场景文件

  - CollectPrefab 只收集目录下的预制体文件

  - CollectSprite 只收集目录下的精灵类型的文件

````c#
//自定义扩展范例
public class CollectScene : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        return Path.GetExtension(data.AssetPath) == ".unity";
    }
}
````

- **NotWriteToAssetList**

  资源对象不写入资源列表

- **AssetTags**

  资源分类标签列表，该收集器下收集的资源会全部被打上该标签。


