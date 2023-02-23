# 资源部署

在资源补丁包构建成功之后，需要将补丁包传输到CDN服务器上。

如果是本地测试，可以在本地创建一个WEB服务器，然后将补丁包拷贝到WEB服务器下。

**部署目录**

在业务开发过程中，发版本之前都会创建一个SVN或GIT分支，以分支工程为基础去构建APP。

````
CDN
└─android
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
└─iphone
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
````

**APP版本说明**

v1.0 代表的是APP版本，不是资源版本。在没有更换安装包的情况下，不需要新增加APP版本目录。

例如：我们游戏的当前APP版本是v1.0，那么每次生成的补丁文件全部覆盖到v1.0的目录下即可。

下面的示例里一共上传过2次补丁包。第二次上传的补丁包会把第一次的版本记录文件（PatchManifest_DefaultPackage.version）覆盖掉。当我们想回退资源版本的时候，把第一次生成的版本记录文件覆盖到同目录下即可。

````
v1.0（游戏版本）
├─PatchManifest_DefaultPackage.version
├─PatchManifest_DefaultPackage_2023-02-01-654.hash
├─PatchManifest_DefaultPackage_2023-02-01-654.bytes
├─PatchManifest_DefaultPackage_2023-02-12-789.hash
├─PatchManifest_DefaultPackage_2023-02-12-789.bytes
├─2bb5a28d37dabf27df8bc6a4706b8f80.bundle
├─2dbea9c3056c8839bc03d80a2aebd105.bundle
├─6e8c3003a64ead36a0bd2d5cdebfbcf4.bundle
...
````

