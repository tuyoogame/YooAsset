
namespace YooAsset
{
    internal abstract class FSRequestPackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 查询的最新版本信息
        /// </summary>
        internal string PackageVersion { set; get; }
    }
}