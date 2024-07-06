
namespace YooAsset
{
    internal abstract class FSRequestPackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源版本
        /// </summary>
        internal string PackageVersion { set; get; }
    }
}