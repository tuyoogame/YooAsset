using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 资源句柄工厂，根据类型创建对应的句柄实例。
    /// </summary>
    internal static class HandleFactory
    {
        private static readonly Dictionary<Type, Func<ProviderBase, HandleBase>> s_handleFactory = new Dictionary<Type, Func<ProviderBase, HandleBase>>()
        {
            { typeof(AssetHandle), op => new AssetHandle(op) },
            { typeof(SceneHandle), op => new SceneHandle(op) },
            { typeof(SubAssetsHandle), op => new SubAssetsHandle(op) },
            { typeof(AllAssetsHandle), op => new AllAssetsHandle(op) },
            { typeof(BundleFileHandle), op => new BundleFileHandle(op) }
        };

        /// <summary>
        /// 根据类型创建资源句柄
        /// </summary>
        /// <param name="provider">资源提供者</param>
        /// <param name="type">要创建的句柄类型，必须是 HandleBase 的派生类型。</param>
        public static HandleBase CreateHandle(ProviderBase provider, Type type)
        {
            if (s_handleFactory.TryGetValue(type, out var factory) == false)
            {
                throw new NotImplementedException($"Handle type {type.FullName} is not supported.");
            }
            return factory(provider);
        }
    }
}