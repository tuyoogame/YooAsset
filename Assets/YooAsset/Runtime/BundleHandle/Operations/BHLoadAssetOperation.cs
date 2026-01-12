
namespace YooAsset
{
    /// <summary>
    /// 加载单个资源操作的抽象基类
    /// </summary>
    internal abstract class BHLoadAssetOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前加载操作产出的资源对象
        /// </summary>
        public UnityEngine.Object Result { get; protected set; }
    }
}