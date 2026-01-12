
namespace YooAsset
{
    /// <summary>
    /// 加载所有资源操作的抽象基类
    /// </summary>
    internal abstract class BHLoadAllAssetsOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前加载操作产出的全部资源对象集合
        /// </summary>
        public UnityEngine.Object[] Result { get; protected set; }
    }
}