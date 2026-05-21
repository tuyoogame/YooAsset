
namespace YooAsset
{
    /// <summary>
    /// 请求版本的操作选项
    /// </summary>
    public readonly struct RequestPackageVersionOptions
    {
        /// <summary>
        /// 是否在URL末尾添加时间戳
        /// </summary>
        public bool AppendTimeTicks { get; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 创建请求版本选项实例
        /// </summary>
        /// <param name="appendTimeTicks">是否在URL末尾添加时间戳</param>
        /// <param name="timeout">超时时间（秒）</param>
        public RequestPackageVersionOptions(bool appendTimeTicks, int timeout)
        {
            AppendTimeTicks = appendTimeTicks;
            Timeout = timeout;
        }

        /// <summary>
        /// 转换为 FileSystem 的请求版本选项
        /// </summary>
        internal FSRequestPackageVersionOptions ConvertTo()
        {
            return new FSRequestPackageVersionOptions(AppendTimeTicks, Timeout);
        }
    }
}