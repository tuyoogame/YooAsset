#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

/// <summary>
/// 提供 UI 面板监测示例的目录配置
/// </summary>
public static class UIPanelSettings
{
    /// <summary>
    /// 是否启用面板依赖监测
    /// </summary>
    public static bool EnablePanelMonitor = false;

    /// <summary>
    /// 面板文件夹GUID
    /// </summary>
    private const string UIPanelDirectoryGUID = "12d33f33f3a55224c9c747d7bffa1c68";

    /// <summary>
    /// 精灵文件夹GUID
    /// </summary>
    private const string UISpriteDirectoryGUID = "935d7f20c085cc141a3daf9cacfabfae";

    /// <summary>
    /// 图集文件夹GUID
    /// </summary>
    private const string UIAtlasDirectoryGUID = "c355c783476322b4cacac98c5e1b46d8";

    /// <summary>
    /// 面板资源目录
    /// </summary>
    /// <returns>面板资源目录路径</returns>
    public static string GetPanelDirectory()
    {
        string result = UnityEditor.AssetDatabase.GUIDToAssetPath(UIPanelDirectoryGUID);
        if (string.IsNullOrEmpty(result))
        {
            throw new System.IO.DirectoryNotFoundException($"Panel directory not found for GUID '{UIPanelDirectoryGUID}'.");
        }
        return result;
    }

    /// <summary>
    /// 精灵资源目录
    /// </summary>
    /// <returns>精灵资源目录路径</returns>
    public static string GetSpriteDirectory()
    {
        string result = UnityEditor.AssetDatabase.GUIDToAssetPath(UISpriteDirectoryGUID);
        if (string.IsNullOrEmpty(result))
        {
            throw new System.IO.DirectoryNotFoundException($"Sprite directory not found for GUID '{UISpriteDirectoryGUID}'.");
        }
        return result;
    }

    /// <summary>
    /// 图集资源目录
    /// </summary>
    /// <returns>图集资源目录路径</returns>
    public static string GetAtlasDirectory()
    {
        string result = UnityEditor.AssetDatabase.GUIDToAssetPath(UIAtlasDirectoryGUID);
        if (string.IsNullOrEmpty(result))
        {
            throw new System.IO.DirectoryNotFoundException($"Atlas directory not found for GUID '{UIAtlasDirectoryGUID}'.");
        }
        return result;
    }
}

/// <summary>
/// 在预制体保存时刷新 UI 面板依赖的图集清单
/// </summary>
public class UIPanelMonitor : UnityEditor.Editor
{
    [UnityEditor.InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        UnityEditor.SceneManagement.PrefabStage.prefabSaving += OnPrefabSaving;
    }

    static void OnPrefabSaving(GameObject go)
    {
        if (UIPanelSettings.EnablePanelMonitor == false)
            return;

        UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            string panelDirectory = UIPanelSettings.GetPanelDirectory();
            if (stage.assetPath.StartsWith(panelDirectory))
            {
                PanelManifest manifest = go.GetComponent<PanelManifest>();
                if (manifest == null)
                    manifest = go.AddComponent<PanelManifest>();

                RefreshPanelManifest(manifest);
            }
        }
    }

    /// <summary>
    /// 刷新面板清单
    /// </summary>
    private static void RefreshPanelManifest(PanelManifest manifest)
    {
        manifest.ReferencesAtlas.Clear();

        string spriteDirectory = UIPanelSettings.GetSpriteDirectory();
        string atlasDirectory = UIPanelSettings.GetAtlasDirectory();

        // 获取依赖的图集名称
        Transform root = manifest.transform;
        Image[] allImage = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < allImage.Length; i++)
        {
            Image image = allImage[i];
            if (image.sprite == null)
                continue;

            // 文件路径
            string spriteAssetPath = UnityEditor.AssetDatabase.GetAssetPath(image.sprite);

            // 跳过系统内置资源
            if (spriteAssetPath.Contains("_builtin_"))
                continue;

            // 跳过非图集精灵
            if (spriteAssetPath.StartsWith(spriteDirectory) == false)
                continue;

            string atlasAssetPath = GetAtlasPath(atlasDirectory, spriteAssetPath);
            SpriteAtlas spriteAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
            if (spriteAtlas == null)
            {
                throw new System.IO.FileNotFoundException($"SpriteAtlas not found: '{atlasAssetPath}'.");
            }
            else
            {
                if (manifest.ReferencesAtlas.Contains(spriteAtlas) == false)
                    manifest.ReferencesAtlas.Add(spriteAtlas);
            }
        }
    }

    /// <summary>
    /// 获取精灵所属图集
    /// </summary>
    private static string GetAtlasPath(string atlasDirectory, string assetPath)
    {
        string directory = Path.GetDirectoryName(assetPath);
        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        string atlasName = directoryInfo.Name;
        return $"{atlasDirectory}/{atlasName}.spriteatlas";
    }
}
#endif