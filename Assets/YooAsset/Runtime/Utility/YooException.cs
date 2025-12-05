using System;

namespace YooAsset
{
    /// <summary>
    /// YooAsset 异常基类
    /// </summary>
    [Serializable]
    public class YooException : Exception
    {
        public YooException() : base() { }
        public YooException(string message) : base(message) { }
        public YooException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 内部错误异常
    /// 当发生不应该发生的内部错误时抛出（通常表示代码逻辑错误）
    /// 这类异常通常表示需要修复代码，而不是用户使用错误
    /// </summary>
    [Serializable]
    public class YooInternalException : YooException
    {
        public YooInternalException() : base($"Internal error (should never happen)") { }
        public YooInternalException(string message) : base($"Internal error (should never happen) {message}") { }
        public YooInternalException(Exception inner) : base($"Internal error (should never happen)", inner) { }
        public YooInternalException(string message, Exception inner) : base($"Internal error (should never happen) {message}", inner) { }
    }

    /// <summary>
    /// 初始化相关异常
    /// 当 YooAsset 系统初始化失败或未初始化时访问功能时抛出
    /// </summary>
    [Serializable]
    public class YooInitializeException : YooException
    {
        public YooInitializeException() : base() { }
        public YooInitializeException(string message) : base(message) { }
        public YooInitializeException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 平台不支持异常
    /// 当功能在当前平台不支持时抛出
    /// </summary>
    [Serializable]
    public class YooPlatformNotSupportedException : YooException
    {
        public YooPlatformNotSupportedException() : base()
        {
        }
        public YooPlatformNotSupportedException(string message) : base(message)
        {
        }
        public YooPlatformNotSupportedException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 包裹管理异常
    /// 当包裹创建、销毁、初始化等操作失败时抛出
    /// </summary>
    [Serializable]
    public class YooPackageException : YooException
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        public YooPackageException(string packageName) : base()
        {
            PackageName = packageName;
        }
        public YooPackageException(string packageName, string message) : base(message)
        {
            PackageName = packageName;
        }
        public YooPackageException(string packageName, string message, Exception inner) : base(message, inner)
        {
            PackageName = packageName;
        }
    }

    /// <summary>
    /// 资源清单文件异常
    /// 当资源清单加载、数据无效或损坏时抛出
    /// </summary>
    [Serializable]
    public class YooManifestException : YooException
    {
        public YooManifestException() : base()
        {
        }
        public YooManifestException(string message) : base(message)
        {
        }
        public YooManifestException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// 资源加载异常
    /// 当资源加载类型不匹配时抛出
    /// </summary>
    [Serializable]
    public class YooLoadException : YooException
    {
        public YooLoadException() : base()
        {
        }
        public YooLoadException(string message) : base(message)
        {
        }
        public YooLoadException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// 资源句柄异常
    /// 当句柄无效或操作句柄失败时抛出
    /// </summary>
    [Serializable]
    public class YooHandleException : YooException
    {
        public YooHandleException() : base()
        {
        }
        public YooHandleException(string message) : base(message)
        {
        }
        public YooHandleException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// 文件系统异常
    /// 当文件读写、验证、缓存操作失败时抛出
    /// </summary>
    [Serializable]
    public class YooFileSystemException : YooException
    {
        public YooFileSystemException() : base()
        {
        }
        public YooFileSystemException(string message) : base(message)
        {
        }
        public YooFileSystemException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}