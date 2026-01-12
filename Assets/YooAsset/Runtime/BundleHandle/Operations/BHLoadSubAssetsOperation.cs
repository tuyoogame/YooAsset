
namespace YooAsset
{
    /// <summary>
    /// 加载子资源操作的抽象基类
    /// </summary>
    internal abstract class BHLoadSubAssetsOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前加载操作产出的子资源对象集合
        /// </summary>
        public UnityEngine.Object[] Result { get; protected set; }
    }
}