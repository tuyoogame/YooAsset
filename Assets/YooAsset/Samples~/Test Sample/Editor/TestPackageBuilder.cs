using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

public static class TestPackageBuilder
{
    /// <summary>
    /// 构建资源包
    /// </summary>
    internal static PackageBuildResult BuildPackage(PackageBuildParameters buildParam)
    {
        string packageName = buildParam.PackageName;
        string buildPipelineName = buildParam.BuildPipelineName;

        if (buildPipelineName == EBuildPipeline.EditorSimulateBuildPipeline.ToString())
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            string outputRoot = $"{projectPath}/Bundles/Tester_ESBP";

            var buildParameters = new EditorSimulateBuildParameters();
            buildParameters.BuildOutputRoot = outputRoot;
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.EditorSimulateBuildPipeline.ToString();
            buildParameters.BuildBundleType = buildParam.BuildBundleType;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "TestVersion";
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BundledCopyOption = EBundledCopyOption.None;
            buildParameters.BundledCopyParams = string.Empty;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;

            var pipeline = new EditorSimulateBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                var packageResult = new PackageBuildResult();
                packageResult.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return packageResult;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(EditorSimulateBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.ScriptableBuildPipeline.ToString())
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            string outputRoot = $"{projectPath}/Bundles/Tester_SBP";

            // 内置着色器资源包名称
            var builtinShaderBundleName = GetBuiltinShaderBundleName(packageName);
            var buildParameters = new ScriptableBuildParameters();

            buildParameters.BuildOutputRoot = outputRoot;
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.AssetBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "TestVersion";
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BundledCopyOption = EBundledCopyOption.None;
            buildParameters.BundledCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.BuiltinShadersBundleName = builtinShaderBundleName;
            buildParameters.BundleEncryptor = new TestAssetBundleEncryptor();
            buildParameters.ManifestEncryptor = new TestManifestEncryptor();
            buildParameters.ManifestDecryptor = new TestManifestDecryptor();

            var pipeline = new ScriptableBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                string packageRoot = buildResult.OutputPackageDirectory;
                bool result = BuiltinCatalogHelper.CreateFile(new TestManifestDecryptor(), packageName, packageRoot);
                if (result == false)
                    Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");

                var packageResult = new PackageBuildResult();
                packageResult.PackageRootDirectory = packageRoot;
                return packageResult;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(ScriptableBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.LegacyBuildPipeline.ToString())
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            string outputRoot = $"{projectPath}/Bundles/Tester_LBP";

            var buildParameters = new LegacyBuildParameters();
            buildParameters.BuildOutputRoot = outputRoot;
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.AssetBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "TestVersion";
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BundledCopyOption = EBundledCopyOption.None;
            buildParameters.BundledCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.BundleEncryptor = new TestAssetBundleEncryptor();
            buildParameters.ManifestEncryptor = new TestManifestEncryptor();
            buildParameters.ManifestDecryptor = new TestManifestDecryptor();

            var pipeline = new LegacyBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                string packageRoot = buildResult.OutputPackageDirectory;
                bool result = BuiltinCatalogHelper.CreateFile(new TestManifestDecryptor(), packageName, packageRoot);
                if (result == false)
                    Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");

                var packageResult = new PackageBuildResult();
                packageResult.PackageRootDirectory = packageRoot;
                return packageResult;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(LegacyBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.RawFileBuildPipeline.ToString())
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            string outputRoot = $"{projectPath}/Bundles/Tester_RFBP";

            var buildParameters = new RawFileBuildParameters();
            buildParameters.BuildOutputRoot = outputRoot;
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.RawFileBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.RawBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "TestVersion";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BundledCopyOption = EBundledCopyOption.None;
            buildParameters.BundledCopyParams = string.Empty;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.BundleEncryptor = new TestRawBundleEncryptor();

            var pipeline = new RawFileBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                string packageRoot = buildResult.OutputPackageDirectory;
                bool result = BuiltinCatalogHelper.CreateFile(null, packageName, packageRoot);
                if (result == false)
                    Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");

                var packageResult = new PackageBuildResult();
                packageResult.PackageRootDirectory = packageRoot;
                return packageResult;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(RawFileBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.ArchiveFileBuildPipeline.ToString())
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            string outputRoot = $"{projectPath}/Bundles/Tester_AFBP";

            var buildParameters = new ArchiveFileBuildParameters();
            buildParameters.BuildOutputRoot = outputRoot;
            buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ArchiveFileBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBundleType.ArchiveBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "TestVersion";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BundledCopyOption = EBundledCopyOption.None;
            buildParameters.BundledCopyParams = string.Empty;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.FileAlignment = 4;
            buildParameters.BundleEncryptor = new TestArchiveBundleEncryptor();

            var pipeline = new ArchiveFileBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                string packageRoot = buildResult.OutputPackageDirectory;
                bool result = BuiltinCatalogHelper.CreateFile(null, packageName, packageRoot);
                if (result == false)
                    Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");

                var packageResult = new PackageBuildResult();
                packageResult.PackageRootDirectory = packageRoot;
                return packageResult;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(ArchiveFileBuildPipeline)} build failed !");
            }
        }
        else
        {
            throw new System.NotImplementedException(buildPipelineName);
        }
    }

    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName(string packageName)
    {
        var uniqueBundleName = BundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultBundlePackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(packageName, uniqueBundleName);
    }
}