
namespace YooAsset
{
    /// <summary>
    /// 请求包裹版本操作的抽象基类
    /// </summary>
    internal abstract class FSRequestPackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 包裹版本
        /// </summary>
        internal string PackageVersion { get; set; }
    }
}