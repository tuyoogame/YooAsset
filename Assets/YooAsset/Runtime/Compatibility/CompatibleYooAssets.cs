#if YOOASSET_LEGACY_API
// YooAsset v2.3 兼容层 - YooAssets 静态类兼容
// 通过 partial class 为 YooAssets 补充 v2.3 的旧静态属性和方法。

using System;
using System.Collections.Generic;
using System.Linq;

namespace YooAsset
{
    public static partial class YooAssets
    {
        /// <summary>
        /// v2.3: YooAssets.Initialized
        /// </summary>
        [Obsolete("Use IsInitialized instead.")]
        public static bool Initialized => IsInitialized;

        /// <summary>
        /// v2.3: YooAssets.GetAllPackages()
        /// </summary>
        [Obsolete("Use GetPackages() instead.")]
        public static List<ResourcePackage> GetAllPackages()
        {
            return GetPackages().ToList();
        }

        /// <summary>
        /// v2.3: YooAssets.TryGetPackage(string)
        /// </summary>
        [Obsolete("Use TryGetPackage(string, out ResourcePackage) instead.")]
        public static ResourcePackage TryGetPackage(string packageName)
        {
            TryGetPackage(packageName, out var package);
            return package;
        }

        /// <summary>
        /// v2.3: YooAssets.RemovePackage(ResourcePackage)
        /// </summary>
        [Obsolete("Use RemovePackage(string) instead.")]
        public static bool RemovePackage(ResourcePackage package)
        {
            try
            {
                RemovePackage(package.PackageName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// v2.3: YooAssets.SetOperationSystemMaxTimeSlice(long)
        /// </summary>
        [Obsolete("Use SetAsyncOperationMaxTimeSlice() instead.")]
        public static void SetOperationSystemMaxTimeSlice(long milliseconds)
        {
            SetAsyncOperationMaxTimeSlice(milliseconds);
        }

        /// <summary>
        /// v2.3: YooAssets.SetDownloadSystemUnityWebRequest(delegate)
        /// </summary>
        [Obsolete("This API has been removed in v3.")]
        public static void SetDownloadSystemUnityWebRequest(object createDelegate)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// v2.3: YooAssets.StartOperation(GameAsyncOperation)
        /// </summary>
        [Obsolete("GameAsyncOperation has been removed in v3.")]
        public static void StartOperation(GameAsyncOperation operation)
        {
            AsyncOperationSystem.StartOperation(AsyncOperationSystem.GlobalSchedulerName, operation);
        }
    }
}
#endif
