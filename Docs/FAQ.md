# 常见问题解答

#### 问题：在编辑器下，用离线模式或联机模式运行游戏，为什么游戏里的模型会变成紫色？

如果在打AssetBundle的时候，选定的构建目标是安卓。那么在windows操作系统下，编辑器的默认渲染模式为DX11，我们需要修改编辑器的渲染模式，可以通过UnityHub来修改启动项目的编辑器渲染模式，[参考官方文档](https://docs.unity3d.com/cn/2019.4/Manual/CommandLineArguments.html)。

windows平台添加命令: **-force-gles**

#### 问题：YooAsset的DLL引用丢失导致编译报错了

1. 请在PlayerSetting里修改API Level为.NET 4.x或者.NET Framework
2. 关闭游戏工程后，删除Assets同级目录下所有的csproj文件和sln文件。
3. 删除Library/ScriptAssemblies文件夹。
4. 重新打开游戏工程，然后点击某个脚本重新编译。

#### 问题：无效的资源路径，请检查路径是否带有特殊符号或中文：Assets/xxx/xxx/xxx

如果检查报错的文件路径内无特殊符合和中文字符，也可能是文件路径过长且文件名称带空格，在打包生成的manifest文件里文件路径被截断导致验证失败。

例如：Assets/My Game Res/JMO Assets/Cartoon FX Remaster/CFXR Assets/Shaders/CFXR Particle Glow.shader

解决方案1：缩短文件路径长度。

解决方案2：移除文件名称里的空格。
