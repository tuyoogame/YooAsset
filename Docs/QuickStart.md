# 快速开始

#### **下载安装**

1. **通过PackageManager安装**

   打开管理界面 **Edit/Project Settings/Package Manager**

   ````
   // 输入以下内容
   Name: package.openupm.cn
   URL: https://package.openupm.cn
   Scope(s): com.tuyoogame.yooasset
   ````

   ![image](./Image/QuickStart-img1.jpg)

   打开管理界面 **Edit/Windows/Package Manager**

   ![image](./Image/QuickStart-img2.jpg)

2. **通过Packages清单安装**

   直接修改Packages文件夹下的清单文件manifest.json

   ````json
   {
     "dependencies": {
       "com.tuyoogame.yooasset": "0.0.1-preview",
       ......
     },
     "scopedRegistries": [
       {
         "name": "package.openupm.cn",
         "url": "https://package.openupm.cn",
         "scopes": [
           "com.tuyoogame.yooasset"
         ]
       }
     ]
   }
   ````

3. **通过Github下载安装**

   在发布的Release版本中，选择最新版本下载Source Code压缩包。

#### **系统需求**

支持版本: Unity2019.4+

支持平台: Windows、OSX、Android、iOS

开发环境: .NET4.x

#### **目录结构**

````
Assets
└─ YooAsset
    ├─ Editor 编辑器源码目录  
    ├─ Runtime 运行时源码目录 
    ├─ LICENSE 版权文档
    └─ README 说明文档 
````

