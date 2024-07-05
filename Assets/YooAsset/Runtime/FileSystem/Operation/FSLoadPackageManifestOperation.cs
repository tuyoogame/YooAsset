
namespace YooAsset
{
    internal abstract class FSLoadPackageManifestOperation : AsyncOperationBase
    {
        /// <summary>
        /// 加载结果
        /// </summary>
        internal PackageManifest Result { set; get; }
    }
}