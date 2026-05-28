using System;
using System.Text;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试前置初始化：创建收集器配置并初始化 YooAssets
/// </summary>
/// <remarks>
/// IPrebuildSetup.Setup 阶段在 Editor 下创建 AssetBundle 和 RawBundle 两个包裹的收集器配置。
/// [Test] InitializeYooAssets 负责调用 YooAssets.Initialize()。
/// </remarks>
public class T0_InitYooAssets : IPrebuildSetup, IPostBuildCleanup
{
    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 清空旧数据
        YooAsset.Editor.BundleCollectorSettingData.ClearAll();

        // 创建包裹配置
        CreateAssetBundlePackageCollector();
        CreateRawBundlePackageCollector();
        CreateArchiveBundlePackageCollector();

        // 修正配置路径为空导致的错误
        YooAsset.Editor.BundleCollectorSettingData.FixFile();
#endif
    }
    void IPostBuildCleanup.Cleanup()
    {
    }

#if UNITY_EDITOR
    private static void CreateAssetBundlePackageCollector()
    {
        // 创建AssetBundlePackage
        var testPackage = YooAsset.Editor.BundleCollectorSettingData.CreatePackage(TestConsts.AssetBundlePackageName);
        testPackage.EnableAddressable = true;
        testPackage.AutoCollectShaders = true;
        testPackage.IgnoreRuleName = "NormalIgnoreRule";

        // 音频
        var audioGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "AudioGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "bbce3e09a17b55c46b5615e995b5fc70"; //TestRes/Audios目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(audioGroup, collector1);
        }

        // 图片
        var imageGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "ImageGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "d4768b7c3d3101d4ea693f95b337861d"; //TestRes/Image目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(imageGroup, collector1);
        }

        // 图集
        var spriteGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "SpriteGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "634f8145b892c554ba440c212b36a933"; //TestRes/SpriteAtlas目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(spriteGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "e41a9b5f04378154f9bd69ac5d52ec44"; //TestRes/Sprites目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.StaticAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(spriteGroup, collector2);
        }

        // 面板
        var panelGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "PanelGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "4e9a00d6e825d644b9be75155d88daa6"; //TestRes/Panel目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(panelGroup, collector1);
        }

        // 预制体
        var prefabGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "PrefabGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "8da7a00d90270b44898e9b165f86f005"; //TestRes/Prefab目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(prefabGroup, collector1);
        }

        // 场景
        var sceneGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "SceneGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "3e169b07999abb0489113f5f4c015c89"; //TestRes/Scene目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(sceneGroup, collector1);
        }

        // 序列化文件
        var scriptableObjectGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "ScriptableObjectGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "af885cf1a7abb8c44bd9d139409d2961"; //TestRes/ScriptableObject目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(scriptableObjectGroup, collector1);
        }

        // 引用测试文件
        var referenceGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "ReferenceGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "26b9f7e0454f2bc4a84b44a018075d8f"; //TestRes2/PanelA目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(referenceGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "b5cace4be4d008e408c0738f157708a0"; //TestRes2/PanelB目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(referenceGroup, collector2);

            var collector3 = new YooAsset.Editor.BundleCollector();
            collector3.CollectPath = "";
            collector3.CollectorGUID = "aa7f70ef09d60844ba62f85ff2414a9c"; //TestRes2/PanelAImage目录
            collector3.CollectorType = YooAsset.Editor.ECollectorType.DependAssetCollector;
            collector3.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(referenceGroup, collector3);

            var collector4 = new YooAsset.Editor.BundleCollector();
            collector4.CollectPath = "";
            collector4.CollectorGUID = "96d800f068cc69c4dbd20ffdcec40920"; //TestRes2/PanelBImage目录
            collector4.CollectorType = YooAsset.Editor.ECollectorType.DependAssetCollector;
            collector4.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(referenceGroup, collector4);

            var collector5 = new YooAsset.Editor.BundleCollector();
            collector5.CollectPath = "";
            collector5.CollectorGUID = "4264f3aa222d7f548a028d6c3411b1b0"; //TestRes2/PanelMat目录
            collector5.CollectorType = YooAsset.Editor.ECollectorType.DependAssetCollector;
            collector5.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(referenceGroup, collector5);
        }

        // 加密测试文件
        var encryptGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "EncryptGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "e082d492b9da65e499cee3495be3645d"; //TestRes/EncryptFiles目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(encryptGroup, collector1);
        }

        // 拷贝测试文件
        var copyGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "CopyGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "35f454fb80a715047bcf0ce30c7c4f18"; //TestRes3/ImportFiles目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.AssetTags = "import";
            collector1.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(copyGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "401af1ca0abf3ae4594631e5f71bfe27"; //TestRes3/UnpackFiles目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector2.AssetTags = "unpack";
            collector2.PackRuleName = nameof(YooAsset.Editor.PackSeparately);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(copyGroup, collector2);
        }

        // 卸载测试文件
        var unloadGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(testPackage, "UnloadGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "c8cebdb7cd80a4044b15a1522363bb86"; //TestRes4/Enemy目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(unloadGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "3ba9252bc0a278145a7bde0cf876cbcb"; //TestRes4/EnemyImage目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.DependAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(unloadGroup, collector2);

            var collector3 = new YooAsset.Editor.BundleCollector();
            collector3.CollectPath = "";
            collector3.CollectorGUID = "65cc88e038c61ec488be24b82c4e2937"; //TestRes4/EnemyMat目录
            collector3.CollectorType = YooAsset.Editor.ECollectorType.DependAssetCollector;
            collector3.PackRuleName = nameof(YooAsset.Editor.PackDirectory);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(unloadGroup, collector3);
        }
    }
    private static void CreateRawBundlePackageCollector()
    {
        // 创建RawBundlePackage
        var rawPackage = YooAsset.Editor.BundleCollectorSettingData.CreatePackage(TestConsts.RawBundlePackageName);
        rawPackage.EnableAddressable = true;
        rawPackage.AutoCollectShaders = true;
        rawPackage.IgnoreRuleName = "RawFileIgnoreRule";

        // 原生文件
        var rawFileGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(rawPackage, "RawFileGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "fddaaf9430e24344196cc82ac3d006b4"; //TestRes5/RawFiles目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackRawFile);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(rawFileGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "9378c52809165094cb532c1678355a3c"; //TestRes5/EncryptFiles目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackRawFile);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(rawFileGroup, collector2);
        }
    }
    private static void CreateArchiveBundlePackageCollector()
    {
        var archivePackage = YooAsset.Editor.BundleCollectorSettingData.CreatePackage(TestConsts.ArchiveBundlePackageName);
        archivePackage.EnableAddressable = true;
        archivePackage.AutoCollectShaders = false;
        archivePackage.IgnoreRuleName = "RawFileIgnoreRule";

        var archiveFileGroup = YooAsset.Editor.BundleCollectorSettingData.CreateGroup(archivePackage, "ArchiveFileGroup");
        {
            var collector1 = new YooAsset.Editor.BundleCollector();
            collector1.CollectPath = "";
            collector1.CollectorGUID = "c0444018376a7cd4ead6a671035617d6"; //TestRes6/ArchiveFiles目录
            collector1.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector1.PackRuleName = nameof(YooAsset.Editor.PackCollector);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(archiveFileGroup, collector1);

            var collector2 = new YooAsset.Editor.BundleCollector();
            collector2.CollectPath = "";
            collector2.CollectorGUID = "5bf4f68676550434291dbdee4da52f65"; //TestRes6/EncryptFiles目录
            collector2.CollectorType = YooAsset.Editor.ECollectorType.MainAssetCollector;
            collector2.PackRuleName = nameof(YooAsset.Editor.PackCollector);
            YooAsset.Editor.BundleCollectorSettingData.CreateCollector(archiveFileGroup, collector2);
        }
    }
#endif

    [Test]
    public void InitializeYooAssets()
    {
        // 初始化YooAsset
        YooAssets.Initialize();
    }
}