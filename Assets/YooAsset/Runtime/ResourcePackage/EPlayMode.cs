
namespace YooAsset
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum EPlayMode
    {
        /// <summary>
        /// 未指定运行模式
        /// </summary>
        None = 0,

        /// <summary>
        /// 编辑器下的模拟模式
        /// </summary>
        EditorSimulateMode,

        /// <summary>
        /// 离线运行模式
        /// </summary>
        OfflinePlayMode,

        /// <summary>
        /// 联机运行模式
        /// </summary>
        HostPlayMode,

        /// <summary>
        /// WebGL运行模式
        /// </summary>
        WebPlayMode,

        /// <summary>
        /// 自定义运行模式
        /// </summary>
        CustomPlayMode,

        /// <summary>
        /// 免构建运行模式
        /// </summary>
        //DatalessPlayMode,
    }
}