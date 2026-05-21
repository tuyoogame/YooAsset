
namespace YooAsset
{
    /// <summary>
    /// 请求包裹版本的操作选项
    /// </summary>
    internal readonly struct FSRequestPackageVersionOptions
    {
        /// <summary>
        /// 在URL末尾添加时间戳
        /// </summary>
        public bool AppendTimeTicks { get; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; }

        public FSRequestPackageVersionOptions(bool appendTimeTicks, int timeout)
        {
            AppendTimeTicks = appendTimeTicks;
            Timeout = timeout;
        }
    }
}
