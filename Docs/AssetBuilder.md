# 资源构建

![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/AssetBuilder-img1.jpg)

### 界面介绍

**Build Output**

构建输出的目录，会根据Unity编辑器当前切换的平台自动划分构建结果。

**Build Version**

构建版本号，也是资源版本号，版本号必须大于零。

**Compression**

资源包的压缩方式。

**Append Extension**

构建的资源包文件名是否包含后缀格式。

**Force Rebuild**

是否为强制重新构建，如果不勾选则为增量构建模式。注意：强制重建会删除当前平台下所有的补丁包文件。

**Buildin Tags**

标记为安装包里的资源标签列表。构建成功后，会将相关标记的资源包拷贝到StreamingAssets文件夹下。

**Build**

点击Build按钮会开始构建流程，构建流程分为多个节点顺序执行，如果某个节点发生错误，会导致构建失败。错误信息可以在控制台查看。

### 资源包加密

编写继承IAssetEncrypter接口的加密类。注意：加密类文件需要放置在Editor文件夹里。

````C#
public class AssetEncrypter : IAssetEncrypter
{
	/// <summary>
	/// 检测资源包是否需要加密
	/// </summary>
	bool IAssetEncrypter.Check(string filePath)
	{
		// 对配置表相关的资源包进行加密
		return filePath.Contains("Assets/Config/");
	}

	/// <summary>
	/// 对数据进行加密，并返回加密后的数据
	/// </summary>
	byte[] IAssetEncrypter.Encrypt(byte[] fileData)
	{
		int offset = 32;
		var temper = new byte[fileData.Length + offset];
		Buffer.BlockCopy(fileData, 0, temper, offset, fileData.Length);
		return temper;
	}
}
````

### 补丁包

构建成功后会在输出目录下找到补丁包文件夹，该文件夹名称为本次构建时指定的资源版本号。

补丁包文件夹里包含补丁清单和资源包文件以及说明文件，资源包文件都是以文件的哈希值命名。

![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/AssetBuilder-img4.jpg)

### 补丁清单

补丁清单是一个Json格式的文本文件，里面包含了所有资源包的信息，例如：名称，版本，大小，CRC等。

![image](https://github.com/tuyoogame/YooAsset/raw/main/Docs/Image/AssetBuilder-img2.jpg)

### Jenkins支持

如果需要自动化构建，可以参考如下代码范例：

````c#
private static void BuildInternal(BuildTarget buildTarget)
{
	Debug.Log($"开始构建 : {buildTarget}");

	// 打印命令行参数
	int buildVersion = GetBuildVersion();
	bool isForceBuild = IsForceBuild();
	Debug.Log($"资源版本 : {buildVersion}");
	Debug.Log($"强制重建 : {isForceBuild}");

	// 构建参数
	string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
	AssetBundleBuilder.BuildParameters buildParameters = new AssetBundleBuilder.BuildParameters();
	buildParameters.IsVerifyBuildingResult = true;
	buildParameters.OutputRoot = defaultOutputRoot;
	buildParameters.BuildTarget = buildTarget;
	buildParameters.BuildVersion = buildVersion;
	buildParameters.CompressOption = ECompressOption.LZ4;
    buildParameters.AppendFileExtension = false;
	buildParameters.IsForceRebuild = isForceBuild;
	buildParameters.BuildinTags = "buildin";

	// 执行构建
	AssetBundleBuilder builder = new AssetBundleBuilder();
	builder.Run(buildParameters);

	// 构建完成
	Debug.Log("构建完成");
}

// 从构建命令里获取参数
private static int GetBuildVersion()
{
	foreach (string arg in System.Environment.GetCommandLineArgs())
	{
		if (arg.StartsWith("buildVersion"))
			return int.Parse(arg.Split("="[0])[1]);
	}
	return -1;
}
private static bool IsForceBuild()
{
	foreach (string arg in System.Environment.GetCommandLineArgs())
	{
		if (arg.StartsWith("forceBuild"))
			return arg.Split("="[0])[1] == "true" ? true : false;
	}
	return false;
}
````

