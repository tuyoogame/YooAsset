
namespace YooAsset
{
    internal abstract class FSLoadPackageManifestOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源清单
        /// </summary>
        internal PackageManifest Manifest { set; get; }
    }
}