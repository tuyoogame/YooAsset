
namespace YooAsset
{
    /// <summary>
    /// 加载包裹清单操作的抽象基类
    /// </summary>
    internal abstract class FSLoadPackageManifestOperation : AsyncOperationBase
    {
        /// <summary>
        /// 包裹清单
        /// </summary>
        internal PackageManifest Manifest { get; set; }
    }
}