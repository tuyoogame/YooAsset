using NUnit.Framework;
using System;
using System.Collections.Generic;
using YooAsset;

/// <summary>
/// 运行时资源相关的工具类
/// </summary>
internal static class RuntimeResourceUtility 
{
    public static PackageBundle CreateBundle(string name)
    {
        return new PackageBundle
        {
            BundleName = name,
            FileHash = name,
            DependentBundleIDs = new int[0],
            ReferrerBundleIDs = new List<int>()
        };
    }
}