using System;

namespace YooAsset
{
    /// <summary>
    /// Base exception for all YooAsset custom exceptions.
    /// </summary>
    /// <remarks>
    /// Use catch (YooException) to handle all YooAsset-related exceptions.
    /// </remarks>
    [Serializable]
    public class YooException : Exception
    {
        public YooException() : base() { }

        /// <param name="message">The error message that describes the reason for the exception.</param>
        public YooException(string message) : base(message) { }

        /// <param name="message">The error message that describes the reason for the exception.</param>
        /// <param name="inner">The exception that caused the current exception.</param>
        public YooException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an internal logic error occurs in YooAsset.
    /// </summary>
    /// <remarks>
    /// <para>Thrown when an unexpected internal error occurs, typically indicating a code defect that needs to be fixed.</para>
    /// <para>The message is automatically prefixed with "An internal error occurred: " for identification.</para>
    /// </remarks>
    [Serializable]
    public class YooInternalException : YooException
    {
        public YooInternalException() : base("Internal error occurred. This indicates a bug in the code.") { }
        public YooInternalException(string message) : base($"Internal error occurred: {message}") { }
        public YooInternalException(Exception inner) : base("Internal error occurred. This indicates a bug in the code.", inner) { }
        public YooInternalException(string message, Exception inner) : base($"Internal error occurred: {message}", inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a resource package is in an invalid state.
    /// </summary>
    /// <remarks>
    /// Thrown when the package is not initialized, initialization is incomplete,
    /// initialization has failed, or the active manifest is unavailable.
    /// </remarks>
    [Serializable]
    public class YooPackageInvalidException : YooException
    {
        /// <summary>
        /// Gets the name of the package that caused the exception.
        /// </summary>
        public string PackageName { get; }

        public YooPackageInvalidException(string packageName) : base()
        {
            PackageName = packageName;
        }
        public YooPackageInvalidException(string packageName, string message) : base(message)
        {
            PackageName = packageName;
        }
        public YooPackageInvalidException(string packageName, string message, Exception inner) : base(message, inner)
        {
            PackageName = packageName;
        }
    }

    /// <summary>
    /// The exception that is thrown when the resource manifest data is invalid.
    /// </summary>
    /// <remarks>
    /// Thrown when the manifest contains configuration conflicts, duplicate paths, duplicate GUIDs, or other logical errors.
    /// </remarks>
    [Serializable]
    public class YooManifestInvalidException : YooException
    {
        public YooManifestInvalidException() : base() { }
        public YooManifestInvalidException(string message) : base(message) { }
        public YooManifestInvalidException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a resource handle is invalid.
    /// </summary>
    /// <remarks>
    /// Thrown when attempting to operate on a handle that has been released or whose associated provider has been destroyed.
    /// </remarks>
    [Serializable]
    public class YooHandleInvalidException : YooException
    {
        public YooHandleInvalidException() : base() { }
        public YooHandleInvalidException(string message) : base(message) { }
        public YooHandleInvalidException(string message, Exception inner) : base(message, inner) { }
    }

}
