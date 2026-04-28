using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using YooAsset.Editor;

/// <summary>
/// 收集资源包裹中材质产生的着色器变种
/// </summary>
public static class ShaderVariantCollector
{
    private enum ESteps
    {
        None,
        Prepare,
        CollectAllMaterial,
        CollectVariants,
        CollectSleeping,
        WaitingDone,
    }

    private const float WaitMilliseconds = 3000f;
    private const float SleepMilliseconds = 3000f;
    private static string _savePath;
    private static string _packageName;
    private static int _processMaxNum;
    private static Action _completedCallback;

    private static ESteps _steps = ESteps.None;
    private static Stopwatch _elapsedTime;
    private static List<string> _allMaterials;
    private static List<GameObject> _allSpheres = new List<GameObject>(1000);


    /// <summary>
    /// 启动着色器变种收集流程
    /// </summary>
    /// <param name="savePath">收集结果保存路径，扩展名必须为 .shadervariants。</param>
    /// <param name="packageName">参与收集的资源包裹名称</param>
    /// <param name="processMaxNum">每批处理的材质数量</param>
    /// <param name="completedCallback">收集完成后的回调</param>
    public static void Run(string savePath, string packageName, int processMaxNum, Action completedCallback)
    {
        if (_steps != ESteps.None)
            return;

        if (EditorSceneUtility.HasDirtyScenes())
        {
            UnityEngine.Debug.LogError("Unsaved scenes detected. Save all scenes before collecting shader variants.");
            return;
        }

        if (Path.HasExtension(savePath) == false)
            savePath = $"{savePath}.shadervariants";
        if (Path.GetExtension(savePath) != ".shadervariants")
            throw new System.ArgumentException("Shader variant file extension is invalid.", nameof(savePath));
        if (string.IsNullOrEmpty(packageName))
            throw new System.ArgumentNullException(nameof(packageName));
        if (processMaxNum <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(processMaxNum), "Process capacity must be greater than zero.");

        // Unity 对同名 ShaderVariantCollection 的刷新存在延迟，先删除旧资源再写入新结果。
        AssetDatabase.DeleteAsset(savePath);
        EditorFileUtility.CreateFileDirectory(savePath);
        _savePath = savePath;
        _packageName = packageName;
        _processMaxNum = processMaxNum;
        _completedCallback = completedCallback;

        // 聚焦到游戏窗口
        EditorWindowUtility.FocusUnityGameWindow();

        // 创建临时测试场景
        CreateTempScene();

        _steps = ESteps.Prepare;
        EditorApplication.update += EditorUpdate;
    }

    private static void EditorUpdate()
    {
        try
        {
            InternalUpdate();
        }
        catch (Exception ex)
        {
            Finish(false, ex);
        }
    }
    private static void InternalUpdate()
    {
        if (_steps == ESteps.None)
            return;

        if (_steps == ESteps.Prepare)
        {
            ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
            _steps = ESteps.CollectAllMaterial;
            return; // 等待一帧，让编辑器完成清理。
        }

        if (_steps == ESteps.CollectAllMaterial)
        {
            _allMaterials = GetAllMaterials();
            _steps = ESteps.CollectVariants;
            return; // 等待一帧，让材质列表收集结果稳定。
        }

        if (_steps == ESteps.CollectVariants)
        {
            int count = Mathf.Min(_processMaxNum, _allMaterials.Count);
            List<string> range = _allMaterials.GetRange(0, count);
            _allMaterials.RemoveRange(0, count);
            CollectVariants(range);

            if (_allMaterials.Count > 0)
            {
                _elapsedTime = Stopwatch.StartNew();
                _steps = ESteps.CollectSleeping;
            }
            else
            {
                _elapsedTime = Stopwatch.StartNew();
                _steps = ESteps.WaitingDone;
            }
        }

        if (_steps == ESteps.CollectSleeping)
        {
            if (ShaderUtil.anythingCompiling)
                return;

            if (_elapsedTime.ElapsedMilliseconds > SleepMilliseconds)
            {
                DestroyAllSpheres();
                _elapsedTime.Stop();
                _steps = ESteps.CollectVariants;
            }
        }

        if (_steps == ESteps.WaitingDone)
        {
            // Unity 需要等待若干帧后才能把当前变种集合写入资源文件。
            if (_elapsedTime.ElapsedMilliseconds > WaitMilliseconds)
            {
                _elapsedTime.Stop();
                _steps = ESteps.None;

                // 保存结果并创建清单
                ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_savePath);
                CreateManifest();

                Finish(true);
            }
        }
    }
    private static void CreateTempScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
    }
    private static List<string> GetAllMaterials()
    {
        // 获取所有打包的资源
        CollectResult collectResult = BundleCollectorSettingData.Setting.BeginCollect(_packageName, false, false);

        // 搜集所有材质球
        int progressValue = 0;
        HashSet<string> result = new HashSet<string>();
        foreach (var collectAssetInfo in collectResult.CollectAssets)
        {
            if (collectAssetInfo.AssetInfo.AssetType == typeof(UnityEngine.Material))
            {
                string assetPath = collectAssetInfo.AssetInfo.AssetPath;
                if (result.Contains(assetPath) == false)
                    result.Add(assetPath);
            }
            foreach(var dependAssetInfo in collectAssetInfo.DependAssets)
            {
                if (dependAssetInfo.AssetType == typeof(UnityEngine.Material))
                {
                    string assetPath = dependAssetInfo.AssetPath;
                    if (result.Contains(assetPath) == false)
                        result.Add(assetPath);
                }
            }
            EditorDialogUtility.DisplayProgressBar("Collect All Materials", ++progressValue, collectResult.CollectAssets.Count);
        }
        EditorDialogUtility.ClearProgressBar();

        // 返回结果
        return result.ToList();
    }
    private static void CollectVariants(List<string> materials)
    {
        Camera camera = Camera.main;
        if (camera == null)
            throw new System.InvalidOperationException("Main camera is missing.");

        // 设置主相机
        float aspect = camera.aspect;
        int totalMaterials = materials.Count;
        float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
        float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
        float halfHeight = Mathf.CeilToInt(height / 2f);
        float halfWidth = Mathf.CeilToInt(width / 2f);
        camera.orthographic = true;
        camera.orthographicSize = halfHeight;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        // 创建测试球体
        int xMax = (int)(width - 1);
        int x = 0, y = 0;
        int progressValue = 0;
        for (int i = 0; i < materials.Count; i++)
        {
            var material = materials[i];
            var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
            var go = CreateSphere(material, position, i);
            if (go != null)
                _allSpheres.Add(go);
            if (x == xMax)
            {
                x = 0;
                y++;
            }
            else
            {
                x++;
            }
            EditorDialogUtility.DisplayProgressBar("Render All Materials", ++progressValue, materials.Count);
        }
        EditorDialogUtility.ClearProgressBar();
    }
    private static GameObject CreateSphere(string assetPath, Vector3 position, int index)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            UnityEngine.Debug.LogWarning($"Material not found: '{assetPath}'.");
            return null;
        }

        var shader = material.shader;
        if (shader == null)
            return null;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.transform.position = position;
        go.name = $"Sphere_{index} | {material.name}";
        return go;
    }
    private static void DestroyAllSpheres()
    {
        foreach (var go in _allSpheres)
        {
            if (go != null)
                GameObject.DestroyImmediate(go);
        }
        _allSpheres.Clear();

        // 材质扫描会加载编辑器资源，收集结束后主动释放以降低内存占用。
        EditorUtility.UnloadUnusedAssetsImmediate(true);
    }
    private static void CreateManifest()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_savePath);
        if (svc != null)
        {
            var wrapper = ShaderVariantCollectionManifest.Extract(svc);
            string jsonData = JsonUtility.ToJson(wrapper, true);
            string savePath = _savePath.Replace(".shadervariants", ".json");
            File.WriteAllText(savePath, jsonData);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
    private static void Finish(bool success, Exception exception = null)
    {
        try
        {
            if (success)
                UnityEngine.Debug.Log("Shader variant collection completed.");
            else
                UnityEngine.Debug.LogError($"Shader variant collection failed: {exception}.");

            _completedCallback?.Invoke();
        }
        finally
        {
            Cleanup();
        }
    }
    private static void Cleanup()
    {
        EditorApplication.update -= EditorUpdate;
        EditorDialogUtility.ClearProgressBar();

        if (_elapsedTime != null)
        {
            _elapsedTime.Stop();
            _elapsedTime = null;
        }

        DestroyAllSpheres();
        _savePath = null;
        _packageName = null;
        _processMaxNum = 0;
        _completedCallback = null;
        _allMaterials = null;
        _steps = ESteps.None;
    }
}