
namespace YooAsset
{
    /// <summary>
    /// 加载资源包操作基类
    /// </summary>
    internal abstract class BCLoadBundleOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源包加载的内部结果
        /// </summary>
        protected readonly struct LoadResult
        {
            /// <summary>
            /// 错误信息
            /// </summary>
            public readonly string Error;

            /// <summary>
            /// 加载是否成功
            /// </summary>
            public bool Succeeded
            {
                get { return Error == null; }
            }

            private LoadResult(string error)
            {
                Error = error;
            }

            /// <summary>
            /// 创建表示加载成功的默认结果
            /// </summary>
            /// <returns>不携带错误信息的成功结果</returns>
            public static LoadResult Default()
            {
                return new LoadResult(null);
            }

            /// <summary>
            /// 创建表示加载失败的结果
            /// </summary>
            /// <param name="error">错误信息</param>
            /// <returns>携带错误信息的失败结果</returns>
            public static LoadResult Failure(string error)
            {
                return new LoadResult(error);
            }
        }

        /// <summary>
        /// 资源包句柄
        /// </summary>
        public IBundleHandle BundleHandle { get; protected set; }
    }

    /// <summary>
    /// 加载资源包失败操作
    /// </summary>
    internal sealed class BCLoadBundleErrorOperation : BCLoadBundleOperation
    {
        private readonly string _error;

        /// <summary>
        /// 创建加载资源包错误操作实例
        /// </summary>
        /// <param name="error">错误信息</param>
        internal BCLoadBundleErrorOperation(string error)
        {
            _error = error;
        }
        protected override void InternalStart()
        {
            SetError(_error);
        }
        protected override void InternalUpdate()
        {
        }
    }
}